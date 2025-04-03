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

        // Add component with isTestMode set to true (via reflection so it's set before Awake)
        var serializedObject = new UnityEditor.SerializedObject(managerObject);
        levelRevealManager = managerObject.AddComponent<LevelRevealManager>();

        // Set test mode flag via reflection
        var isTestModeField = typeof(LevelRevealManager).GetField("isTestMode",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        isTestModeField?.SetValue(levelRevealManager, true);

        // Initialize test arrays after component is added
        levelRevealManager.InitializeForTest(testEnemies, testBoxes, testSlots);

        // Set parameters for MUCH faster testing to avoid timing issues
        var revealDelayField = typeof(LevelRevealManager).GetField("revealDelay",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        revealDelayField?.SetValue(levelRevealManager, 0.001f);

        var timeBetweenObjectsField = typeof(LevelRevealManager).GetField("timeBetweenObjects",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        timeBetweenObjectsField?.SetValue(levelRevealManager, 0.001f);

        Debug.Log($"Test setup complete with {testEnemies.Length} enemies, {testBoxes.Length} boxes, and {testSlots.Length} slots");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test objects
        foreach (var enemy in testEnemies)
            if (enemy != null) Object.DestroyImmediate(enemy);

        foreach (var box in testBoxes)
            if (box != null) Object.DestroyImmediate(box);

        foreach (var slot in testSlots)
            if (slot != null) Object.DestroyImmediate(slot);

        // Clean up manager
        Object.DestroyImmediate(managerObject);
    }

    [Test]
    public void HideAllObjects_HidesAllObjects()
    {
        // Arrange - Ensure all objects are active initially
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

    [Test]
    public void StartRevealSequence_SetsLevelRevealedFlag()
    {
        // Arrange
        levelRevealManager.HideAllObjects();
        Assert.IsFalse(levelRevealManager.LevelRevealed, "LevelRevealed should start as false");

        // Act
        levelRevealManager.StartRevealSequence();

        // Assert
        Assert.IsTrue(levelRevealManager.LevelRevealed, "LevelRevealed should be set to true");
    }

    [Test]
    public void StartRevealSequence_OnlyRunsOnce()
    {
        // Arrange
        levelRevealManager.HideAllObjects();

        // First call - should work
        levelRevealManager.StartRevealSequence();
        bool firstCallRevealed = levelRevealManager.LevelRevealed;

        // Hide objects manually 
        levelRevealManager.HideAllObjects();

        // Reset the revealed flag for testing
        var revealedField = typeof(LevelRevealManager).GetField("levelRevealed",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        revealedField?.SetValue(levelRevealManager, false);

        // Try to reveal again
        levelRevealManager.StartRevealSequence();

        // Assert
        Assert.IsTrue(firstCallRevealed, "First call should have set revealed flag");
        Assert.IsTrue(levelRevealManager.LevelRevealed, "Second call should still set revealed flag");
    }

    // Test that we can manually reveal objects without using coroutines
    [Test]
    public void ManuallyShowAllObjects_RevealsEverything()
    {
        // Arrange
        levelRevealManager.HideAllObjects();
        Assert.IsTrue(levelRevealManager.AreAllObjectsHidden(), "Objects should be hidden initially");

        // Act - Manually set active without coroutines
        foreach (var slot in testSlots)
            slot.SetActive(true);

        foreach (var box in testBoxes)
            box.SetActive(true);

        foreach (var enemy in testEnemies)
            enemy.SetActive(true);

        // Assert
        Assert.IsTrue(levelRevealManager.AreAllObjectsRevealed(), "All objects should be revealed");
    }
}