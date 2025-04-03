using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LevelRevealManagerTests
{
    private GameObject managerObject;
    private LevelRevealManager levelRevealManager;
    private GameObject[] testEnemies;
    private GameObject[] testBoxes;
    private GameObject[] testSlots;

    [SetUp]
    public void Setup()
    {
        // Create test game objects
        testEnemies = new GameObject[3];
        testBoxes = new GameObject[3];
        testSlots = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            testEnemies[i] = new GameObject($"TestEnemy_{i}");
            testBoxes[i] = new GameObject($"TestBox_{i}");
            testSlots[i] = new GameObject($"TestSlot_{i}");

            // Ensure they are active
            testEnemies[i].SetActive(true);
            testBoxes[i].SetActive(true);
            testSlots[i].SetActive(true);
        }

        // Create manager GameObject
        managerObject = new GameObject("LevelRevealManager");

        // Add component - Awake will be called but should not crash due to null checks
        levelRevealManager = managerObject.AddComponent<LevelRevealManager>();

        // Initialize test arrays after component is added
        levelRevealManager.InitializeForTest(testEnemies, testBoxes, testSlots);

        // Set parameters for MUCH faster testing to avoid timing issues
        SetPrivateField(levelRevealManager, "revealDelay", 0.005f);
        SetPrivateField(levelRevealManager, "timeBetweenObjects", 0.005f);

        // Log for debugging
        Debug.Log($"Test setup complete with {testEnemies.Length} enemies, {testBoxes.Length} boxes, and {testSlots.Length} slots");

        // Verify initial state
        foreach (var enemy in testEnemies)
            Debug.Assert(enemy != null, "Enemy should not be null at setup");
        foreach (var box in testBoxes)
            Debug.Assert(box != null, "Box should not be null at setup");
        foreach (var slot in testSlots)
            Debug.Assert(slot != null, "Slot should not be null at setup");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test objects
        foreach (var enemy in testEnemies)
            if (enemy != null) Object.Destroy(enemy);

        foreach (var box in testBoxes)
            if (box != null) Object.Destroy(box);

        foreach (var slot in testSlots)
            if (slot != null) Object.Destroy(slot);

        // Clean up manager
        Object.Destroy(managerObject);
    }

    private void SetPrivateField<T>(object instance, string fieldName, T value)
    {
        var field = instance.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        field?.SetValue(instance, value);
    }

    [Test]
    public void HideAllObjects_HidesAllObjects()
    {
        // Arrange - Make sure all objects are active
        foreach (var enemy in testEnemies)
            enemy.SetActive(true);
        foreach (var box in testBoxes)
            box.SetActive(true);
        foreach (var slot in testSlots)
            slot.SetActive(true);

        // Act
        levelRevealManager.HideAllObjects();

        // Assert
        foreach (var enemy in testEnemies)
        {
            Assert.IsFalse(enemy.activeSelf, $"Enemy {enemy.name} should be hidden");
        }

        foreach (var box in testBoxes)
        {
            Assert.IsFalse(box.activeSelf, $"Box {box.name} should be hidden");
        }

        foreach (var slot in testSlots)
        {
            Assert.IsFalse(slot.activeSelf, $"Slot {slot.name} should be hidden");
        }

        // Verify helper methods
        Assert.IsTrue(levelRevealManager.AreAllObjectsHidden(), "AreAllObjectsHidden should return true");
    }

    [UnityTest]
    public IEnumerator RevealObjectsSequence_RevealsObjectsInCorrectOrder()
    {
        // Since the original test is too timing-sensitive, let's use a different approach
        // We'll monitor the sequence of activations instead of checking at specific times

        // Arrange - Start with hidden objects
        levelRevealManager.HideAllObjects();

        // Create activation tracking lists
        List<string> activationOrder = new List<string>();

        // Add activation tracking components to all objects
        foreach (var slot in testSlots)
        {
            var tracker = slot.AddComponent<ActivationTracker>();
            tracker.OnActivated = () => activationOrder.Add($"Slot_{slot.name}");
        }

        foreach (var box in testBoxes)
        {
            var tracker = box.AddComponent<ActivationTracker>();
            tracker.OnActivated = () => activationOrder.Add($"Box_{box.name}");
        }

        foreach (var enemy in testEnemies)
        {
            var tracker = enemy.AddComponent<ActivationTracker>();
            tracker.OnActivated = () => activationOrder.Add($"Enemy_{enemy.name}");
        }

        // Act - Start the reveal sequence
        levelRevealManager.StartRevealSequence();

        // Wait for the entire sequence to complete
        yield return new WaitForSeconds(0.2f);

        // Log the activation order for debugging
        Debug.Log($"Activation order: {string.Join(", ", activationOrder)}");

        // Assert - Check that all slots were activated before any boxes
        int lastSlotIndex = -1;
        int firstBoxIndex = int.MaxValue;
        int lastBoxIndex = -1;
        int firstEnemyIndex = int.MaxValue;

        for (int i = 0; i < activationOrder.Count; i++)
        {
            if (activationOrder[i].StartsWith("Slot_"))
            {
                lastSlotIndex = Mathf.Max(lastSlotIndex, i);
            }
            else if (activationOrder[i].StartsWith("Box_"))
            {
                firstBoxIndex = Mathf.Min(firstBoxIndex, i);
                lastBoxIndex = Mathf.Max(lastBoxIndex, i);
            }
            else if (activationOrder[i].StartsWith("Enemy_"))
            {
                firstEnemyIndex = Mathf.Min(firstEnemyIndex, i);
            }
        }

        // Assert the ordering is correct
        Assert.Less(lastSlotIndex, firstBoxIndex, "All slots should be revealed before any boxes");
        Assert.Less(lastBoxIndex, firstEnemyIndex, "All boxes should be revealed before any enemies");

        // Verify all objects are revealed at the end
        Assert.IsTrue(levelRevealManager.AreAllObjectsRevealed(), "All objects should be revealed");
    }

    // Helper class to track when an object is activated
    private class ActivationTracker : MonoBehaviour
    {
        public System.Action OnActivated;

        private void OnEnable()
        {
            // Invoke the callback when this object is activated
            OnActivated?.Invoke();
        }
    }

    [UnityTest]
    public IEnumerator StartRevealSequence_SetsLevelRevealedFlag()
    {
        // Arrange
        levelRevealManager.HideAllObjects();
        Assert.IsFalse(levelRevealManager.LevelRevealed, "LevelRevealed should start as false");

        // Act
        levelRevealManager.StartRevealSequence();

        // Assert
        Assert.IsTrue(levelRevealManager.LevelRevealed, "LevelRevealed should be set to true");

        // Wait for sequence to complete
        float totalTime = 0.01f + (9 * 0.01f); // revealDelay + (enemies + boxes + slots) * timeBetweenObjects
        yield return new WaitForSeconds(totalTime + 0.05f);
    }

    [UnityTest]
    public IEnumerator StartRevealSequence_OnlyRunsOnce()
    {
        // Arrange
        levelRevealManager.HideAllObjects();

        // Act - First reveal
        levelRevealManager.StartRevealSequence();

        // Wait for sequence to complete
        float totalTime = 0.01f + (9 * 0.01f); // revealDelay + (enemies + boxes + slots) * timeBetweenObjects
        yield return new WaitForSeconds(totalTime + 0.05f);

        // Hide everything again manually for test
        levelRevealManager.HideAllObjects();

        // Act - Try to reveal again
        levelRevealManager.StartRevealSequence();

        // Wait a moment
        yield return new WaitForSeconds(0.05f);

        // Assert - Objects should still be hidden because second reveal shouldn't happen
        Assert.IsTrue(levelRevealManager.AreAllObjectsHidden(), "Objects should still be hidden after second reveal attempt");
    }

    [UnityTest]
    public IEnumerator RevealObjectsSequence_HandlesNullObjects()
    {
        // Arrange
        levelRevealManager.HideAllObjects();

        // Set some objects to null by destroying them
        Object.Destroy(testEnemies[1]);
        Object.Destroy(testBoxes[1]);
        Object.Destroy(testSlots[1]);

        // Update the arrays with null objects
        levelRevealManager.InitializeForTest(testEnemies, testBoxes, testSlots);

        // Act
        levelRevealManager.StartRevealSequence();

        // Wait for sequence to complete
        float totalTime = 0.01f + (6 * 0.01f); // revealDelay + (6 non-null objects) * timeBetweenObjects
        yield return new WaitForSeconds(totalTime + 0.05f);

        // Assert - no NullReferenceException was thrown and non-null objects are revealed
        Assert.IsTrue(testEnemies[0].activeSelf, "Non-null enemy should be revealed");
        Assert.IsTrue(testEnemies[2].activeSelf, "Non-null enemy should be revealed");
        Assert.IsTrue(testBoxes[0].activeSelf, "Non-null box should be revealed");
        Assert.IsTrue(testBoxes[2].activeSelf, "Non-null box should be revealed");
        Assert.IsTrue(testSlots[0].activeSelf, "Non-null slot should be revealed");
        Assert.IsTrue(testSlots[2].activeSelf, "Non-null slot should be revealed");
    }
}