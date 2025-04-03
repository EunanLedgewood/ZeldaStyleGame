using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class NPCTests
{
    private GameObject npcObject;
    private NPC npc;
    private GameObject playerObject;
    private Player_Controller playerController;
    private GameObject dialogueManagerObject;
    private DialogueManager dialogueManager;
    private bool playerMovementLocked = false;
    private bool dialogueStarted = false;
    private bool dialogueLinesSet = false;
    private Canvas testCanvas;

    [SetUp]
    public void Setup()
    {
        Debug.Log("=== SETTING UP NPC TEST ===");

        // Reset input simulator
        InputSimulator.Reset();

        // Set test mode flag before creating any Player_Controller instances
        Player_Controller.IsTestMode = true;

        // Create a test canvas for UI elements
        GameObject canvasObj = new GameObject("TestCanvas");
        testCanvas = canvasObj.AddComponent<Canvas>();
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create NPC GameObject with required components
        npcObject = new GameObject("TestNPC");
        BoxCollider2D npcCollider = npcObject.AddComponent<BoxCollider2D>();
        npcCollider.isTrigger = true; // Make it a trigger for interaction
        npc = npcObject.AddComponent<NPC>();

        // Create player GameObject
        playerObject = new GameObject("Player");
        playerObject.tag = "Player"; // Important for tag checking
        BoxCollider2D playerCollider = playerObject.AddComponent<BoxCollider2D>();

        // Create a real Player_Controller - validation will be skipped due to IsTestMode flag
        playerController = playerObject.AddComponent<Player_Controller>();

        // Add a component to intercept LockMovement calls
        var playerBehavior = playerObject.AddComponent<PlayerControllerTestBehavior>();
        playerBehavior.playerController = playerController;
        playerBehavior.onLockMovement = (isLocked) => {
            playerMovementLocked = isLocked;
            Debug.Log($"LockMovement called with: {isLocked}");
        };

        // Set the OnLockMovement delegate
        playerController.OnLockMovement = (bool lockMovement) => {
            Debug.Log($"Test Setup LockMovement: {lockMovement}");
            playerController.InternalLockMovement(lockMovement);
            playerBehavior.LockMovement(lockMovement);
        };

        // Create dialogueManager and UI elements
        SetupDialogueManager();

        // Setup the references in NPC
        npc.dialogueManager = dialogueManager;
        npc.playerController = playerController;

        // Set a test input wrapper
        npc.SetInputWrapper(new TestInputWrapper());

        // Verify references were set correctly
        if (npc.dialogueManager == null)
        {
            Debug.LogError("DialogueManager reference not set in NPC");
            Assert.Fail("DialogueManager reference is null");
        }

        if (npc.playerController == null)
        {
            Debug.LogError("PlayerController reference not set in NPC");
            Assert.Fail("PlayerController reference is null");
        }

        // Set up test data via reflection
        string[] testDialogueLines = new string[] { "Test line 1", "Test line 2" };
        Sprite testSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);

        SetPrivateField(npc, "dialogueLines", testDialogueLines);
        SetPrivateField(npc, "npcImage", testSprite);

        // Additional debug logging for field setting
        Debug.Log($"Dialogue Lines set: {testDialogueLines.Length}");
        Debug.Log($"NPC Image set: {testSprite != null}");

        Debug.Log("Test setup complete");
    }

    private void SetupDialogueManager()
    {
        // Create dialogueManager GameObject
        dialogueManagerObject = new GameObject("DialogueManager");

        // First create all the UI elements
        GameObject panelObj = CreateUIElement("DialoguePanel", testCanvas.transform);
        GameObject textObj = CreateUIElement("DialogueText", panelObj.transform);
        GameObject imageObj = CreateUIElement("NPCImage", panelObj.transform);
        GameObject buttonObj = CreateUIElement("NextButton", panelObj.transform);

        // Add required components
        TextMeshProUGUI dialogueText = textObj.AddComponent<TextMeshProUGUI>();
        Image npcImage = imageObj.AddComponent<Image>();
        Button nextButton = buttonObj.AddComponent<Button>();

        // Make panel initially inactive
        panelObj.SetActive(false);

        // Create a DialogueManagerTracker
        DialogueManagerTracker tracker = dialogueManagerObject.AddComponent<DialogueManagerTracker>();

        // Create the real DialogueManager (after creating all UI elements)
        dialogueManager = dialogueManagerObject.AddComponent<DialogueManager>();

        // Now assign the UI references directly through the serialized field using serialized property
        var serializedObject = new UnityEditor.SerializedObject(dialogueManager);

        var panelProperty = serializedObject.FindProperty("dialoguePanel");
        panelProperty.objectReferenceValue = panelObj;

        var textProperty = serializedObject.FindProperty("dialogueText");
        textProperty.objectReferenceValue = dialogueText;

        var imageProperty = serializedObject.FindProperty("npcImageSlot");
        imageProperty.objectReferenceValue = npcImage;

        var buttonProperty = serializedObject.FindProperty("nextButton");
        buttonProperty.objectReferenceValue = nextButton;

        serializedObject.ApplyModifiedProperties();

        // Wire up the tracker to the dialogue manager events
        dialogueManager.OnSetDialogueLines += tracker.SetDialogueLines;
        dialogueManager.OnDialogueStarted += tracker.StartDialogue;

        // Initialize the tracker with references
        tracker.dialogueManager = dialogueManager;
    }

    // Helper method to create UI elements
    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(100, 100);
        return obj;
    }

    [TearDown]
    public void Teardown()
    {
        Debug.Log("=== TEARING DOWN NPC TEST ===");

        // Reset the test mode flag
        Player_Controller.IsTestMode = false;

        // Reset test state
        dialogueStarted = false;
        dialogueLinesSet = false;
        playerMovementLocked = false;

        // Clean up
        UnityEngine.Object.Destroy(npcObject);
        UnityEngine.Object.Destroy(playerObject);
        UnityEngine.Object.Destroy(dialogueManagerObject);
        UnityEngine.Object.Destroy(testCanvas.gameObject);
        InputPatch.Restore();

        Debug.Log("Test cleanup complete");
    }

    private void SetPrivateField<T>(object obj, string fieldName, T value)
    {
        try
        {
            // Try to get the field using reflection
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Public);

            if (field != null)
            {
                field.SetValue(obj, value);
                Debug.Log($"Successfully set field '{fieldName}' to {value}");
                return;
            }

            // If direct field access fails, try property
            var property = obj.GetType().GetProperty(fieldName,
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Public);

            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
                Debug.Log($"Successfully set property '{fieldName}' to {value}");
                return;
            }

            // If both fail, use backing field approach
            var backingFieldName = $"<{fieldName}>k__BackingField";
            var backingField = obj.GetType().GetField(
                backingFieldName,
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

            if (backingField != null)
            {
                backingField.SetValue(obj, value);
                Debug.Log($"Successfully set backing field '{backingFieldName}' to {value}");
                return;
            }

            // If all methods fail, log all available fields
            var allFields = obj.GetType().GetFields(
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Public
            );

            string availableFields = string.Join(", ",
                allFields.Select(f => $"{f.Name} ({f.FieldType.Name})"));

            Debug.LogError($"Field '{fieldName}' not found on {obj.GetType().Name}. " +
                           $"Available fields: {availableFields}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error setting field '{fieldName}': {ex.Message}");
        }
    }

    // Corresponding GetPrivateField method
    private T GetPrivateField<T>(object obj, string fieldName)
    {
        try
        {
            // Try to get the field using reflection
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Public);

            if (field != null)
            {
                return (T)field.GetValue(obj);
            }

            // If direct field access fails, try property
            var property = obj.GetType().GetProperty(fieldName,
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Public);

            if (property != null && property.CanRead)
            {
                return (T)property.GetValue(obj);
            }

            // If both fail, use backing field approach
            var backingFieldName = $"<{fieldName}>k__BackingField";
            var backingField = obj.GetType().GetField(
                backingFieldName,
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

            if (backingField != null)
            {
                return (T)backingField.GetValue(obj);
            }

            // If all methods fail, log all available fields
            var allFields = obj.GetType().GetFields(
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Public
            );

            string availableFields = string.Join(", ",
                allFields.Select(f => $"{f.Name} ({f.FieldType.Name})"));

            Debug.LogError($"Field '{fieldName}' not found on {obj.GetType().Name}. " +
                           $"Available fields: {availableFields}");

            return default(T);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting field '{fieldName}': {ex.Message}");
            return default(T);
        }
    }

    [UnityTest]
    public IEnumerator PlayerEntersNPCRange_SetsPlayerNearby()
    {
        Debug.Log("=== STARTING PlayerEntersNPCRange_SetsPlayerNearby TEST ===");

        // Explicitly set playerIsNearby to false at the start of the test
        SetPrivateField(npc, "PlayerIsNearby", false);

        // Verify initial state
        bool initialPlayerNearby = GetPrivateField<bool>(npc, "PlayerIsNearby");
        Debug.Log($"Initial playerIsNearby state: {initialPlayerNearby}");

        // Act - Simulate player entering NPC trigger zone
        var triggerEnterMethod = typeof(NPC).GetMethod("OnTriggerEnter2D",
                                     BindingFlags.NonPublic |
                                     BindingFlags.Instance);

        Debug.Log("Simulating player entering NPC range");
        LogAssert.Expect(LogType.Log, "Player entered NPC interaction range.");
        triggerEnterMethod.Invoke(npc, new object[] { playerObject.GetComponent<Collider2D>() });

        // Wait a frame to ensure everything processes
        yield return null;

        // Assert
        bool finalPlayerNearby = GetPrivateField<bool>(npc, "PlayerIsNearby");
        Debug.Log($"Final playerIsNearby state: {finalPlayerNearby}");

        Assert.IsFalse(initialPlayerNearby, "playerIsNearby should start as false");
        Assert.IsTrue(finalPlayerNearby, "playerIsNearby should be true after player enters range");

        Debug.Log("=== TEST COMPLETED SUCCESSFULLY ===");
    }

    [UnityTest]
    public IEnumerator PlayerExitsNPCRange_UnsetsPlayerNearby()
    {
        Debug.Log("=== STARTING PlayerExitsNPCRange_UnsetsPlayerNearby TEST ===");

        // Arrange - Set player as nearby first
        SetPrivateField(npc, "PlayerIsNearby", true);
        bool initialPlayerNearby = GetPrivateField<bool>(npc, "PlayerIsNearby");
        Debug.Log($"Initial PlayerIsNearby state: {initialPlayerNearby}");

        // Act - Simulate player exiting NPC trigger zone
        var triggerExitMethod = typeof(NPC).GetMethod("OnTriggerExit2D",
                                    BindingFlags.NonPublic |
                                    BindingFlags.Instance);

        Debug.Log("Simulating player exiting NPC range");
        LogAssert.Expect(LogType.Log, "Player exited NPC interaction range.");
        triggerExitMethod.Invoke(npc, new object[] { playerObject.GetComponent<Collider2D>() });

        // Wait a frame to ensure everything processes
        yield return null;

        // Assert
        bool finalPlayerNearby = GetPrivateField<bool>(npc, "PlayerIsNearby");
        Debug.Log($"Final PlayerIsNearby state: {finalPlayerNearby}");

        Assert.IsTrue(initialPlayerNearby, "PlayerIsNearby should start as true");
        Assert.IsFalse(finalPlayerNearby, "PlayerIsNearby should be false after player exits range");

        Debug.Log("=== TEST COMPLETED SUCCESSFULLY ===");
    }
    [UnityTest]
    public IEnumerator PressE_WhenNearNPC_StartsDialogue()
    {
        Debug.Log("=== STARTING PressE_WhenNearNPC_StartsDialogue TEST ===");

        // Get the tracker components
        var dialogueManagerTracker = dialogueManagerObject.GetComponent<DialogueManagerTracker>();
        var playerControllerTracker = playerObject.GetComponent<PlayerControllerTestBehavior>();

        // Arrange - Set player as nearby
        SetPrivateField(npc, "PlayerIsNearby", true);

        // Reset test flags
        dialogueStarted = false;
        dialogueLinesSet = false;
        playerMovementLocked = false;

        // Monkey patch Input to simulate E key press
        InputSimulator.SetKeyDownResponse(KeyCode.E, true);

        // Find the Update method more specifically
        var updateMethod = typeof(NPC).GetMethod("Update",
            BindingFlags.NonPublic |
            BindingFlags.Instance);

        if (updateMethod == null)
        {
            Assert.Fail("Could not find Update method in NPC class");
        }

        // Invoke the Update method to trigger dialogue
        updateMethod.Invoke(npc, null);

        // Wait a frame to ensure everything processes
        yield return null;

        // Restore input state
        InputSimulator.SetKeyDownResponse(KeyCode.E, false);

        // Explicitly check the tracked events
        dialogueStarted = GetPrivateField<bool>(npc, "DialogueStarted");

        // Check dialogue manager tracker
        if (dialogueManagerTracker != null)
        {
            dialogueLinesSet = dialogueManagerTracker.DialogueLinesSet;
            Debug.Log($"Dialogue Lines Set by Tracker: {dialogueLinesSet}");
        }

        // Check player controller tracker
        if (playerControllerTracker != null)
        {
            playerMovementLocked = playerControllerTracker.MovementLocked;
            Debug.Log($"Movement Locked from Tracker: {playerMovementLocked}");
        }

        // Assert
        Debug.Log($"Dialogue started: {dialogueStarted}");
        Debug.Log($"Dialogue lines set: {dialogueLinesSet}");
        Debug.Log($"Player movement locked: {playerMovementLocked}");

        Assert.IsTrue(dialogueStarted, "Dialogue should start when E is pressed near NPC");
        Assert.IsTrue(dialogueLinesSet, "Dialogue lines should be set when E is pressed near NPC");
        Assert.IsTrue(playerMovementLocked, "Player movement should be locked when dialogue starts");

        Debug.Log("=== TEST COMPLETED SUCCESSFULLY ===");
    }

    [UnityTest]
    public IEnumerator PressE_WhenNotNearNPC_DoesNothing()
    {
        Debug.Log("=== STARTING PressE_WhenNotNearNPC_DoesNothing TEST ===");

        // Get the tracker components
        var dialogueManagerTracker = dialogueManagerObject.GetComponent<DialogueManagerTracker>();
        var playerControllerTracker = playerObject.GetComponent<PlayerControllerTestBehavior>();

        // Arrange - Ensure player is not nearby
        SetPrivateField(npc, "PlayerIsNearby", false);

        // Reset test flags
        dialogueStarted = false;
        dialogueLinesSet = false;
        playerMovementLocked = false;

        // Monkey patch Input to simulate E key press
        InputSimulator.SetKeyDownResponse(KeyCode.E, true);

        // Find the Update method more specifically
        var updateMethod = typeof(NPC).GetMethod("Update",
            BindingFlags.NonPublic |
            BindingFlags.Instance);

        if (updateMethod == null)
        {
            Assert.Fail("Could not find Update method in NPC class");
        }

        // Invoke the Update method to process input
        updateMethod.Invoke(npc, null);

        // Wait a frame to ensure everything processes
        yield return null;

        // Restore input state
        InputSimulator.SetKeyDownResponse(KeyCode.E, false);

        // Explicitly check the tracked events
        dialogueStarted = GetPrivateField<bool>(npc, "DialogueStarted");

        // Check dialogue manager tracker
        if (dialogueManagerTracker != null)
        {
            dialogueLinesSet = dialogueManagerTracker.DialogueLinesSet;
            Debug.Log($"Dialogue Lines Set by Tracker: {dialogueLinesSet}");
        }

        // Check player controller tracker
        if (playerControllerTracker != null)
        {
            playerMovementLocked = playerControllerTracker.MovementLocked;
            Debug.Log($"Movement Locked from Tracker: {playerMovementLocked}");
        }

        // Assert
        Debug.Log($"Dialogue started: {dialogueStarted}");
        Debug.Log($"Dialogue lines set: {dialogueLinesSet}");
        Debug.Log($"Player movement locked: {playerMovementLocked}");

        Assert.IsFalse(dialogueStarted, "Dialogue should not start when E is pressed away from NPC");
        Assert.IsFalse(dialogueLinesSet, "Dialogue lines should not be set when E is pressed away from NPC");
        Assert.IsFalse(playerMovementLocked, "Player movement should not be locked when E is pressed away from NPC");

        Debug.Log("=== TEST COMPLETED SUCCESSFULLY ===");
    }

    public class DialogueManagerTracker : MonoBehaviour
    {
        public DialogueManager dialogueManager;
        public bool DialogueLinesSet { get; private set; } = false;
        public bool DialogueStarted { get; private set; } = false;

        public void SetDialogueLines(string[] lines, Sprite npcSprite)
        {
            DialogueLinesSet = true;
            Debug.Log($"Tracker: Dialogue lines set with {lines.Length} lines");
        }

        public void StartDialogue()
        {
            DialogueStarted = true;
            Debug.Log("Tracker: Dialogue started");
        }

        void Start()
        {
            // Additional setup if needed
        }
    }

    public class PlayerControllerTestBehavior : MonoBehaviour
    {
        public Player_Controller playerController;
        public System.Action<bool> onLockMovement;

        public bool MovementLocked { get; private set; } = false;

        public void LockMovement(bool lockMovement)
        {
            Debug.Log($"PlayerControllerTestBehavior: LockMovement called with {lockMovement}");
            MovementLocked = lockMovement;
            onLockMovement?.Invoke(lockMovement);
        }

        void Start()
        {
            StartCoroutine(MonitorLockMovementCalls());
        }

        IEnumerator MonitorLockMovementCalls()
        {
            while (true)
            {
                yield return null;
            }
        }
    }

    public static class InputSimulator
    {
        private static Dictionary<KeyCode, bool> keyStates = new Dictionary<KeyCode, bool>();

        public static void Reset()
        {
            keyStates.Clear();
        }

        public static void SetKeyDownResponse(KeyCode key, bool isDown)
        {
            keyStates[key] = isDown;
            Debug.Log($"InputSimulator: Set {key} to {isDown}");
        }

        public static bool GetKeyDown(KeyCode key)
        {
            bool isDown = keyStates.ContainsKey(key) && keyStates[key];
            Debug.Log($"InputSimulator: GetKeyDown({key}) returned {isDown}");
            return isDown;
        }
    }

    // Add this right after the InputSimulator class
    public class TestInputWrapper : IInputWrapper
    {
        public bool GetKeyDown(KeyCode key)
        {
            return InputSimulator.GetKeyDown(key);
        }
    }

    // Patch for Input.GetKeyDown
    public static class InputPatch
    {
        private static Func<KeyCode, bool> originalGetKeyDown;

        public static void Initialize()
        {
            // Store the original method
            originalGetKeyDown = (Func<KeyCode, bool>)Delegate.CreateDelegate(
                typeof(Func<KeyCode, bool>),
                typeof(Input).GetMethod("GetKeyDown",
                    BindingFlags.Public |
                    BindingFlags.Static,
                    null,
                    new[] { typeof(KeyCode) },
                    null)
            );
        }

        public static bool GetKeyDown(KeyCode key)
        {
            // Use InputSimulator if a simulation is set up, otherwise use original method
            return InputSimulator.GetKeyDown(key);
        }

        public static void Restore()
        {
            // Restore original behavior if needed
            InputSimulator.Reset();
        }
    }

    // Updated Input Simulation Method
    private void MonkeyPatchInput(KeyCode key, bool value)
{
    // Store our key state in the Input simulator
    InputSimulator.SetKeyDownResponse(key, value);

    // Modify Input.GetKeyDown method to return our simulated value
    typeof(Input)
        .GetMethod("GetKeyDown", BindingFlags.Public | BindingFlags.Static)
        .Invoke(null, new object[] { key });

    // Debug log
    Debug.Log($"Simulating key {key} is {(value ? "pressed" : "released")}");
}

private void PrintNPCClassMethods()
    {
        var methods = typeof(NPC).GetMethods(
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static);

        Debug.Log("NPC Class Methods:");
        foreach (var method in methods)
        {
            Debug.Log($"Method: {method.Name}, " +
                      $"Public: {method.IsPublic}, " +
                      $"Private: {method.IsPrivate}");
        }
    }
}