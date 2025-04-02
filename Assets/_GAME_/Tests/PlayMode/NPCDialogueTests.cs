using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;

public class NPCDialogueTests
{
    private GameObject npcObject;
    private NPC npc;
    private GameObject playerObject;
    private Player_Controller playerController;
    private GameObject dialogueManagerObject;
    private DialogueManager dialogueManager;

    private string[] testDialogueLines = new string[] { "Test line 1", "Test line 2" };
    private Sprite testSprite;

    [SetUp]
    public void Setup()
    {
        // Create test sprite
        testSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);

        // Create player GameObject
        playerObject = new GameObject("Player");
        playerObject.tag = "Player";

        // Add required components for Player_Controller
        Rigidbody2D rb = playerObject.AddComponent<Rigidbody2D>();

        // Expect the validation error logs
        LogAssert.Expect(LogType.Error, "Animator is not assigned in Player_Controller!");
        LogAssert.Expect(LogType.Error, "SpriteRenderer is not assigned in Player_Controller!");

        // Create Player_Controller
        playerController = playerObject.AddComponent<Player_Controller>();

        // Create DialogueManager GameObject
        dialogueManagerObject = new GameObject("DialogueManager");
        dialogueManager = dialogueManagerObject.AddComponent<DialogueManager>();

        // Create NPC GameObject
        npcObject = new GameObject("NPC");
        npcObject.AddComponent<BoxCollider2D>().isTrigger = true;
        npc = npcObject.AddComponent<NPC>();

        // Set references via direct assignment
        npc.dialogueManager = dialogueManager;
        npc.playerController = playerController;

        // Set dialogue lines via reflection
        var linesField = typeof(NPC).GetField("dialogueLines",
                             BindingFlags.NonPublic |
                             BindingFlags.Instance);
        var imageField = typeof(NPC).GetField("npcImage",
                             BindingFlags.NonPublic |
                             BindingFlags.Instance);

        linesField.SetValue(npc, testDialogueLines);
        imageField.SetValue(npc, testSprite);
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(npcObject);
        Object.Destroy(playerObject);
        Object.Destroy(dialogueManagerObject);
    }

    [Test]
    public void OnTriggerEnter_SetsPlayerNearby()
    {
        // Arrange
        var playerIsNearbyField = typeof(NPC).GetField("playerIsNearby",
                                      BindingFlags.NonPublic |
                                      BindingFlags.Instance);

        // Initial state should be false
        bool initialState = (bool)playerIsNearbyField.GetValue(npc);

        // Act - Trigger the OnTriggerEnter2D method
        var triggerEnterMethod = typeof(NPC).GetMethod("OnTriggerEnter2D",
                                     BindingFlags.NonPublic |
                                     BindingFlags.Instance);
        triggerEnterMethod.Invoke(npc, new object[] { playerObject.AddComponent<BoxCollider2D>() });

        // Get updated state
        bool updatedState = (bool)playerIsNearbyField.GetValue(npc);

        // Assert
        Assert.IsFalse(initialState, "playerIsNearby should initially be false");
        Assert.IsTrue(updatedState, "playerIsNearby should be true after player enters trigger");
    }

    [Test]
    public void OnTriggerExit_UnsetsPlayerNearby()
    {
        // Arrange
        var playerIsNearbyField = typeof(NPC).GetField("playerIsNearby",
                                      BindingFlags.NonPublic |
                                      BindingFlags.Instance);

        // Set initial state to true
        playerIsNearbyField.SetValue(npc, true);

        // Act - Trigger the OnTriggerExit2D method
        var triggerExitMethod = typeof(NPC).GetMethod("OnTriggerExit2D",
                                    BindingFlags.NonPublic |
                                    BindingFlags.Instance);
        triggerExitMethod.Invoke(npc, new object[] { playerObject.AddComponent<BoxCollider2D>() });

        // Get updated state
        bool updatedState = (bool)playerIsNearbyField.GetValue(npc);

        // Assert
        Assert.IsFalse(updatedState, "playerIsNearby should be false after player exits trigger");
    }

    // Mock classes for testing
    private class MockDialogueManager : DialogueManager
    {
        // Parameterless constructor to prevent any initialization issues
        public MockDialogueManager()
        {
            // Optional: Add any minimal setup needed
        }

        public System.Action<string[], Sprite> OnSetDialogueLines;
        public System.Action OnStartDialogue;

        public new void SetDialogueLines(string[] lines, Sprite sprite)
        {
            OnSetDialogueLines?.Invoke(lines, sprite);
        }

        public new void StartDialogue()
        {
            OnStartDialogue?.Invoke();
        }
    }

    private class MockPlayerController : Player_Controller
    {
        public System.Action<bool> OnLockMovement;

        public new void LockMovement(bool lockMovement)
        {
            OnLockMovement?.Invoke(lockMovement);
        }
    }

    // Input simulator for testing
    private static class InputSimulator
    {
        // Field to control mock response
        private static bool _mockKeyDownResponse = false;

        // Method to set the mock response
        public static void SetKeyDownResponse(bool response)
        {
            _mockKeyDownResponse = response;
        }

        // Method that mimics Unity's Input.GetKeyDown
        public static bool GetKeyDown(KeyCode key)
        {
            return _mockKeyDownResponse;
        }
    }
}