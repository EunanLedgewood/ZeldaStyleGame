using System.Collections;
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
        // Create player GameObject
        playerObject = new GameObject("Player");
        playerObject.tag = "Player";

        // Add Rigidbody2D for knockback effects
        playerRigidbody = playerObject.AddComponent<Rigidbody2D>();

        // Add audio source if needed by Player_Health
        playerObject.AddComponent<AudioSource>();

        // Add health component
        playerHealth = playerObject.AddComponent<Player_Health>();

        // Setup heart objects
        GameObject[] hearts = new GameObject[3];
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i] = new GameObject($"Heart{i}");
        }

        // Set max health and other fields
        SetPrivateField(playerHealth, "maxHealth", 3);
        SetPrivateField(playerHealth, "currentHealth", 3);
        SetPrivateField(playerHealth, "heartObjects", hearts);
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
        // Arrange - Set up initial health
        int initialHealth = 3;
        SetPrivateField(playerHealth, "currentHealth", initialHealth);

        // Act - Deal damage
        playerHealth.TakeDamage(1, Vector2.zero);

        // Assert - Verify health was reduced
        int currentHealth = GetPrivateField<int>(playerHealth, "currentHealth");
        Assert.AreEqual(initialHealth - 1, currentHealth, "Health should be reduced by 1");
    }
}