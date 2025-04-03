using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;

public class BoxAndSlotTests
{
    private GameObject boxObject;
    private Box box;
    private GameObject slotObject;
    private Slot slot;

    [SetUp]
    public void Setup()
    {
        Debug.Log("=== SETTING UP BOX AND SLOT TEST ===");

        // Create a box GameObject with required components
        boxObject = new GameObject("TestBox");
        box = boxObject.AddComponent<Box>();
        BoxCollider2D boxCollider = boxObject.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true; // Make sure it's a trigger for OnTriggerEnter2D to work

        // Create a slot GameObject with required components
        slotObject = new GameObject("TestSlot");
        slot = slotObject.AddComponent<Slot>();
        BoxCollider2D slotCollider = slotObject.AddComponent<BoxCollider2D>();
        slotCollider.isTrigger = true;

        // Set debug mode on the slot to false to avoid editor-only calls
        SetPrivateField(slot, "debugMode", false);

        Debug.Log("Box and Slot GameObjects created with required components");
    }

    [TearDown]
    public void Teardown()
    {
        Debug.Log("=== TEARING DOWN BOX AND SLOT TEST ===");

        // Clear the static box collection to avoid test interference
        var resetMethod = typeof(Box).GetMethod("ResetAllBoxes",
                              BindingFlags.Public | BindingFlags.Static);
        resetMethod?.Invoke(null, null);

        Object.Destroy(boxObject);
        Object.Destroy(slotObject);

        Debug.Log("Test objects destroyed");
    }

    // Helper method to set private fields via reflection
    private void SetPrivateField<T>(object obj, string fieldName, T value)
    {
        var field = obj.GetType().GetField(fieldName,
                        BindingFlags.NonPublic |
                        BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
            Debug.Log($"Set field '{fieldName}' to {value}");
        }
        else
        {
            Debug.LogError($"Field '{fieldName}' not found on {obj.GetType().Name}");
        }
    }

    // Helper method to get private fields via reflection
    private T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName,
                        BindingFlags.NonPublic |
                        BindingFlags.Instance);
        if (field != null)
        {
            return (T)field.GetValue(obj);
        }
        else
        {
            Debug.LogError($"Field '{fieldName}' not found on {obj.GetType().Name}");
            return default(T);
        }
    }

    // Test for matching box and slot colors
    [UnityTest]
    public IEnumerator MatchingBoxAndSlot_BoxDestroyed()
    {
        Debug.Log("=== STARTING MatchingBoxAndSlot_BoxDestroyed TEST ===");

        // Arrange - Set matching colors
        string testColor = "Red";
        box.boxColor = testColor;
        slot.slotColor = testColor;

        Debug.Log($"Box color set to: {box.boxColor}");
        Debug.Log($"Slot color set to: {slot.slotColor}");

        // Call Start() on both objects to initialize them properly
        var boxStartMethod = typeof(Box).GetMethod("Start",
                                 BindingFlags.NonPublic |
                                 BindingFlags.Instance);
        boxStartMethod?.Invoke(box, null);

        var slotStartMethod = typeof(Slot).GetMethod("Start",
                                  BindingFlags.NonPublic |
                                  BindingFlags.Instance);
        slotStartMethod?.Invoke(slot, null);

        // Create a mock GameManager to avoid null reference issues
        GameObject gmObject = new GameObject("GameManager");
        GameManager mockGM = gmObject.AddComponent<GameManager>();
        typeof(GameManager).GetField("instance", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, mockGM);

        Debug.Log("Initialized components and created mock GameManager");

        // Act - Call OnTriggerEnter2D directly with the slot's collider
        var triggerEnterMethod = typeof(Box).GetMethod("OnTriggerEnter2D",
                                     BindingFlags.NonPublic |
                                     BindingFlags.Instance);

        Debug.Log("Triggering OnTriggerEnter2D with slot's collider");
        triggerEnterMethod?.Invoke(box, new object[] { slotObject.GetComponent<Collider2D>() });

        // Wait a frame to allow for any delayed destruction
        yield return null;

        // Assert - Box should be inactive and scheduled for destruction
        Debug.Log($"Box active state after trigger: {boxObject.activeInHierarchy}");

        Assert.IsFalse(boxObject.activeInHierarchy, "Box should be deactivated when it enters a matching slot");

        // Check if slot is filled
        bool isFilled = slot.IsFilled();
        Debug.Log($"Slot filled state: {isFilled}");

        Assert.IsTrue(isFilled, "Slot should be marked as filled");

        // Clean up mock GameManager
        Object.Destroy(gmObject);

        Debug.Log("=== TEST COMPLETED SUCCESSFULLY ===");
    }

    // Test for non-matching box and slot colors
    [UnityTest]
    public IEnumerator NonMatchingBoxAndSlot_BoxRemains()
    {
        Debug.Log("=== STARTING NonMatchingBoxAndSlot_BoxRemains TEST ===");

        // Arrange - Set different colors
        box.boxColor = "Red";
        slot.slotColor = "Blue";

        Debug.Log($"Box color set to: {box.boxColor}");
        Debug.Log($"Slot color set to: {slot.slotColor}");

        // Call Start() on both objects to initialize them properly
        var boxStartMethod = typeof(Box).GetMethod("Start",
                                 BindingFlags.NonPublic |
                                 BindingFlags.Instance);
        boxStartMethod?.Invoke(box, null);

        var slotStartMethod = typeof(Slot).GetMethod("Start",
                                  BindingFlags.NonPublic |
                                  BindingFlags.Instance);
        slotStartMethod?.Invoke(slot, null);

        Debug.Log("Initialized components");

        // Act - Call OnTriggerEnter2D directly with the slot's collider
        var triggerEnterMethod = typeof(Box).GetMethod("OnTriggerEnter2D",
                                     BindingFlags.NonPublic |
                                     BindingFlags.Instance);

        Debug.Log("Triggering OnTriggerEnter2D with slot's collider");
        triggerEnterMethod?.Invoke(box, new object[] { slotObject.GetComponent<Collider2D>() });

        // Wait a frame to make sure no delayed effects occur
        yield return null;

        // Assert - Box should still be active
        Debug.Log($"Box active state after trigger: {boxObject.activeInHierarchy}");

        Assert.IsTrue(boxObject.activeInHierarchy, "Box should remain active when it enters a non-matching slot");

        // Check if slot is still empty
        bool isFilled = slot.IsFilled();
        Debug.Log($"Slot filled state: {isFilled}");

        Assert.IsFalse(isFilled, "Slot should remain empty");

        Debug.Log("=== TEST COMPLETED SUCCESSFULLY ===");
    }

    // Test for box reset functionality
    [UnityTest]
    public IEnumerator ResetBox_ReturnsToOriginalPosition()
    {
        Debug.Log("=== STARTING ResetBox_ReturnsToOriginalPosition TEST ===");

        // Arrange - Set up box and call Start to register it
        Vector3 originalPosition = new Vector3(0, 0, 0);
        boxObject.transform.position = originalPosition;

        Debug.Log($"Original box position: {originalPosition}");

        // Call Start to register the box
        var startMethod = typeof(Box).GetMethod("Start",
                              BindingFlags.NonPublic |
                              BindingFlags.Instance);
        startMethod?.Invoke(box, null);

        // Move the box to a new position
        Vector3 newPosition = new Vector3(5, 5, 0);
        boxObject.transform.position = newPosition;

        Debug.Log($"Moved box to new position: {newPosition}");

        // Act - Call the reset method
        Box.ResetAllBoxes();

        // Wait a frame to ensure everything processes
        yield return null;

        // Assert - Box should be back at original position
        Vector3 currentPosition = boxObject.transform.position;
        Debug.Log($"Box position after reset: {currentPosition}");

        Assert.AreEqual(originalPosition, currentPosition, "Box should return to its original position after reset");

        Debug.Log("=== TEST COMPLETED SUCCESSFULLY ===");
    }
}
