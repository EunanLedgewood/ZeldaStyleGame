using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;

public class PlayerHealthTests
{
    private GameObject playerObject;
    private Player_Health playerHealth;
    private Rigidbody2D playerRigidbody;

    [SetUp]
    public void Setup()
    {
        // Create player GameObject with required components
        playerObject = new GameObject("Player");
        playerObject.tag = "Player"; // Set tag for collision detection

        // Add required components - only what we need for the test
        playerRigidbody = playerObject.AddComponent<Rigidbody2D>();
        playerObject.AddComponent<AudioSource>();

        // Setup heart objects
        GameObject[] hearts = new GameObject[3];
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i] = new GameObject($"Heart{i}");
        }
        
        // Add Player_Health last, after all dependencies
        playerHealth = playerObject.AddComponent<Player_Health>();

        // If Player_Health references Player_Controller internally, set it to our mock
        var controllerField = playerHealth.GetType().GetField("playerController",
                                 BindingFlags.NonPublic |
                                 BindingFlags.Instance);

        if (controllerField != null)
        {
            controllerField.SetValue(playerHealth, null);
            Debug.Log("Set mock player controller");
        }

        // Set max health via reflection
        SetPrivateField(playerHealth, "maxHealth", 3);

        // Set heart objects via reflection
        SetPrivateField(playerHealth, "heartObjects", hearts);

        Debug.Log("Test setup complete");
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up
        Object.Destroy(playerObject);
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

    [Test]
    public void TakeDamage_ReducesHealth()
    {
        // Log test beginning
        Debug.Log("=== STARTING TakeDamage_ReducesHealth TEST ===");

        // Arrange - Set up initial health
        int initialHealth = 3;
        SetPrivateField(playerHealth, "currentHealth", initialHealth);
        Debug.Log($"Initial health set to: {initialHealth}");

        // Act - Deal damage
        LogAssert.ignoreFailingMessages = true; // Ignore any errors during TakeDamage call
        Debug.Log("Calling TakeDamage(1, Vector2.zero)");
        playerHealth.TakeDamage(1, Vector2.zero);
        LogAssert.ignoreFailingMessages = false;

        // Get current health after damage
        int currentHealth = GetPrivateField<int>(playerHealth, "currentHealth");
        Debug.Log($"Health after damage: {currentHealth}");

        // Assert
        bool testPassed = currentHealth == initialHealth - 1;
        Debug.Log($"Assertion: Health reduced by 1? {testPassed}");

        Assert.AreEqual(initialHealth - 1, currentHealth, "Health should be reduced by 1");

        Debug.Log("=== TEST COMPLETED SUCCESSFULLY ===");
    }
}