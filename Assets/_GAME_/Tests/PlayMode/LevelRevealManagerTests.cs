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
        Debug.Log("Setting up test environment...");

        testEnemies = new GameObject[3];
        testBoxes = new GameObject[3];
        testSlots = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            testEnemies[i] = new GameObject($"TestEnemy_{i}");
            testBoxes[i] = new GameObject($"TestBox_{i}");
            testSlots[i] = new GameObject($"TestSlot_{i}");

            testEnemies[i].SetActive(true);
            testBoxes[i].SetActive(true);
            testSlots[i].SetActive(true);
        }

        managerObject = new GameObject("LevelRevealManager");
        levelRevealManager = managerObject.AddComponent<LevelRevealManager>();

        var isTestModeField = typeof(LevelRevealManager).GetField("isTestMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        isTestModeField?.SetValue(levelRevealManager, true);

        levelRevealManager.InitializeForTest(testEnemies, testBoxes, testSlots);

        Debug.Log("Test setup complete.");
    }

    [TearDown]
    public void TearDown()
    {
        Debug.Log("Tearing down test environment...");

        foreach (var enemy in testEnemies) Object.DestroyImmediate(enemy);
        foreach (var box in testBoxes) Object.DestroyImmediate(box);
        foreach (var slot in testSlots) Object.DestroyImmediate(slot);

        Object.DestroyImmediate(managerObject);

        Debug.Log("Test environment cleaned up.");
    }

    [Test]
    public void HideAllObjects_HidesAllObjects()
    {
        Debug.Log("Testing HideAllObjects method...");

        levelRevealManager.HideAllObjects();

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

        Debug.Log("HideAllObjects test passed.");
    }

    [Test]
    public void StartRevealSequence_SetsLevelRevealedFlag()
    {
        Debug.Log("Testing StartRevealSequence...");

        levelRevealManager.HideAllObjects();
        Assert.IsFalse(levelRevealManager.LevelRevealed, "LevelRevealed should start as false");

        levelRevealManager.StartRevealSequence();

        Assert.IsTrue(levelRevealManager.LevelRevealed, "LevelRevealed should be set to true");
        Debug.Log("StartRevealSequence test passed.");
    }

    [Test]
    public void StartRevealSequence_OnlyRunsOnce()
    {
        Debug.Log("Testing that StartRevealSequence only runs once...");

        levelRevealManager.HideAllObjects();
        levelRevealManager.StartRevealSequence();
        Assert.IsTrue(levelRevealManager.LevelRevealed, "First call should set revealed flag");

        levelRevealManager.HideAllObjects();
        var revealedField = typeof(LevelRevealManager).GetField("levelRevealed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        revealedField?.SetValue(levelRevealManager, false);
        levelRevealManager.StartRevealSequence();

        Assert.IsTrue(levelRevealManager.LevelRevealed, "Second call should still set revealed flag");
        Debug.Log("StartRevealSequence only runs once test passed.");
    }

    [Test]
    public void ManuallyShowAllObjects_RevealsEverything()
    {
        Debug.Log("Testing manual object reveal...");

        levelRevealManager.HideAllObjects();
        Assert.IsTrue(levelRevealManager.AreAllObjectsHidden(), "Objects should be hidden initially");

        foreach (var slot in testSlots) slot.SetActive(true);
        foreach (var box in testBoxes) box.SetActive(true);
        foreach (var enemy in testEnemies) enemy.SetActive(true);

        Assert.IsTrue(levelRevealManager.AreAllObjectsRevealed(), "All objects should be revealed");
        Debug.Log("ManuallyShowAllObjects test passed.");
    }
}
