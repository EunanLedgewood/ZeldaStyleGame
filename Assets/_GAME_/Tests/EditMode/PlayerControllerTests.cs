using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class Player_ControllerTests
{
    private GameObject playerObj;
    private Player_ControllerTest playerController; // Using test class instead
    private GameObject rbObj;
    private Rigidbody2D rb;
    private GameObject animatorObj;
    private Animator animator;
    private GameObject spriteRendererObj;
    private SpriteRenderer spriteRenderer;
    private GameObject pushableObj;
    private Rigidbody2D pushableRb;

    [SetUp]
    public void Setup()
    {
        // Create necessary objects
        playerObj = new GameObject("Player");
        rbObj = new GameObject("Rigidbody2D Component");
        rbObj.transform.SetParent(playerObj.transform);
        rb = rbObj.AddComponent<Rigidbody2D>();

        animatorObj = new GameObject("Animator Component");
        animatorObj.transform.SetParent(playerObj.transform);
        animator = animatorObj.AddComponent<Animator>();

        spriteRendererObj = new GameObject("SpriteRenderer Component");
        spriteRendererObj.transform.SetParent(playerObj.transform);
        spriteRenderer = spriteRendererObj.AddComponent<SpriteRenderer>();

        pushableObj = new GameObject("Pushable Object");
        pushableRb = pushableObj.AddComponent<Rigidbody2D>();

        // Add the Player_ControllerTest component
        playerController = playerObj.AddComponent<Player_ControllerTest>();

        // Set the dependencies manually
        playerController.SetDependenciesForTesting(rb, animator, spriteRenderer);

        // Setup pushable object reference
        playerController.SetPushableObjectForTesting(pushableObj.transform);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up
        Object.DestroyImmediate(playerObj);
        Object.DestroyImmediate(pushableObj);
    }

    [Test]
    public void ValidateDependencies_ReturnsTrueWhenAllDependenciesExist()
    {
        // Arrange - Skip validation flag is reset
        playerController.skipValidation = false;

        // Act
        bool result = playerController.ValidateDependencies();

        // Assert
        Assert.IsTrue(result, "ValidateDependencies should return true when all dependencies exist");
    }

    [Test]
    public void ValidateDependencies_SkipsValidationWhenFlagIsSet()
    {
        // Arrange
        playerController.skipValidation = true;

        // Destroy a dependency to create a validation error
        Object.DestroyImmediate(animator);

        // Act
        bool result = playerController.ValidateDependencies();

        // Assert
        Assert.IsTrue(result, "ValidateDependencies should always return true when skipValidation is true");
    }

    [Test]
    public void SetDependenciesForTesting_SetsSkipValidationFlag()
    {
        // Arrange
        playerController.skipValidation = false;

        // Act
        playerController.SetDependenciesForTesting(rb, animator, spriteRenderer);

        // Assert
        Assert.IsTrue(playerController.skipValidation, "skipValidation should be set to true by SetDependenciesForTesting");
    }

    [Test]
    public void LockMovement_CallsDelegate()
    {
        // Arrange
        bool delegateWasCalled = false;
        bool lockStatePassedToDelegate = false;

        playerController.OnLockMovement = (lockState) => {
            delegateWasCalled = true;
            lockStatePassedToDelegate = lockState;
        };

        // Act
        playerController.LockMovement(true);

        // Assert
        Assert.IsTrue(delegateWasCalled, "LockMovement delegate should be called");
        Assert.IsTrue(lockStatePassedToDelegate, "Correct lock state should be passed to delegate");
    }

    [Test]
    public void InternalLockMovement_SetsMovementLockStateAndStopsMovement()
    {
        // Arrange - apply velocity first
        rb.velocity = new Vector2(10f, 10f);

        // Act
        playerController.InternalLockMovement(true);

        // Assert
        Assert.AreEqual(Vector2.zero, rb.velocity, "Rigidbody velocity should be zero when movement is locked");

        // We'll use reflection to check the private field
        System.Reflection.FieldInfo isMovementLockedField = typeof(Player_ControllerTest).GetField("isMovementLocked",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        bool isMovementLocked = (bool)isMovementLockedField.GetValue(playerController);
        Assert.IsTrue(isMovementLocked, "isMovementLocked should be set to true");
    }

    [Test]
    public void Start_DisablesGravityForPushableObject()
    {
        // Arrange - setup a non-zero gravity scale
        pushableRb.gravityScale = 1.0f;

        // Act
        playerController.TestStart();

        // Assert
        Assert.AreEqual(0f, pushableRb.gravityScale, "Gravity scale for pushable object should be set to 0");
    }
}