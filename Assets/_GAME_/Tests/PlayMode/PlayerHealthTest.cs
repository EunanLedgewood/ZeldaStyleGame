using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerHealthTests
{
    private GameObject playerObject;
    private Player_Health playerHealth;
    private Rigidbody2D playerRigidbody;
    private Player_Controller playerController;

    [SetUp]
    public void Setup()
    {
        // Create player GameObject with required components
        playerObject = new GameObject("Player");
        playerObject.tag = "Player"; // Set tag for collision detection

        // Add required components
        playerHealth = playerObject.AddComponent<Player_Health>();
        playerRigidbody = playerObject.AddComponent<Rigidbody2D>();
        playerController = playerObject.AddComponent<Player_Controller>();
        playerObject.AddComponent<AudioSource>();

        // Setup heart objects
        GameObject[] hearts = new GameObject[3];
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i] = new GameObject($"Heart{i}");
        }

        // Set max health via reflection
        var maxHealthField = typeof(Player_Health).GetField("maxHealth",
                                  System.Reflection.BindingFlags.NonPublic |
                                  System.Reflection.BindingFlags.Instance);
        maxHealthField.SetValue(playerHealth, 3);

        // Set heart objects via reflection
        var heartsField = typeof(Player_Health).GetField("heartObjects",
                               System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Instance);
        heartsField.SetValue(playerHealth, hearts);
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up
        Object.Destroy(playerObject);
    }

    [Test]
    public void PlayerHealth_Initialization_SetsMaxHealth()
    {
        // Arrange - Already done in setup

        // Act - Call Start method explicitly since it won't be called in tests
        var startMethod = typeof(Player_Health).GetMethod("Start",
                               System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Instance);
        startMethod.Invoke(playerHealth, null);

        // Get current health via reflection
        var currentHealthField = typeof(Player_Health).GetField("currentHealth",
                                     System.Reflection.BindingFlags.NonPublic |
                                     System.Reflection.BindingFlags.Instance);
        int currentHealth = (int)currentHealthField.GetValue(playerHealth);

        // Assert
        Assert.AreEqual(3, currentHealth, "Initial health should be set to max health (3)");
    }

    [UnityTest]
    public IEnumerator TakeDamage_ReducesHealth()
    {
        // Arrange
        var startMethod = typeof(Player_Health).GetMethod("Start",
                               System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Instance);
        startMethod.Invoke(playerHealth, null);

        // Act
        playerHealth.TakeDamage(1, Vector2.zero);

        // Wait a frame for processing
        yield return null;

        // Get current health via reflection
        var currentHealthField = typeof(Player_Health).GetField("currentHealth",
                                     System.Reflection.BindingFlags.NonPublic |
                                     System.Reflection.BindingFlags.Instance);
        int currentHealth = (int)currentHealthField.GetValue(playerHealth);

        // Assert
        Assert.AreEqual(2, currentHealth, "Health should be reduced by 1");
    }

    [UnityTest]
    public IEnumerator TakeDamage_AppliesKnockback()
    {
        // Arrange
        var startMethod = typeof(Player_Health).GetMethod("Start",
                               System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Instance);
        startMethod.Invoke(playerHealth, null);

        // Remember initial position
        Vector2 initialPosition = playerObject.transform.position;

        // Act
        playerHealth.TakeDamage(1, new Vector2(-10, 0)); // Damage coming from left

        // Wait a frame for physics to apply
        yield return null;

        // Assert
        Assert.AreNotEqual(initialPosition, (Vector2)playerObject.transform.position,
                          "Player should be knocked back");

        // The player should be knocked to the right (positive x) since damage came from left
        Assert.Greater(playerRigidbody.velocity.x, 0,
                      "Player should be knocked in opposite direction of damage source");
    }

    [UnityTest]
    public IEnumerator Die_LocksPlayerMovement()
    {
        // Arrange
        var startMethod = typeof(Player_Health).GetMethod("Start",
                               System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Instance);
        startMethod.Invoke(playerHealth, null);

        // Act - Deal fatal damage
        playerHealth.TakeDamage(3, Vector2.zero);

        // Wait a frame for death sequence
        yield return null;

        // Check if player controller is locked via reflection
        var movementLockedField = typeof(Player_Controller).GetField("isMovementLocked",
                                       System.Reflection.BindingFlags.NonPublic |
                                       System.Reflection.BindingFlags.Instance);
        bool isLocked = (bool)movementLockedField.GetValue(playerController);

        // Assert
        Assert.IsTrue(isLocked, "Player movement should be locked on death");
    }

    [Test]
    public void ResetHealth_RestoresFullHealth()
    {
        // Arrange
        var startMethod = typeof(Player_Health).GetMethod("Start",
                               System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Instance);
        startMethod.Invoke(playerHealth, null);

        // Manually set health to 1
        var currentHealthField = typeof(Player_Health).GetField("currentHealth",
                                     System.Reflection.BindingFlags.NonPublic |
                                     System.Reflection.BindingFlags.Instance);
        currentHealthField.SetValue(playerHealth, 1);

        // Act
        playerHealth.ResetHealth();

        // Get updated health
        int currentHealth = (int)currentHealthField.GetValue(playerHealth);

        // Assert
        Assert.AreEqual(3, currentHealth, "Health should be reset to maximum");
    }
}
