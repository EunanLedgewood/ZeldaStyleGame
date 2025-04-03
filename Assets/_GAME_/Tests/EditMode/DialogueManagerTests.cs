using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

[TestFixture]
public class DialogueManagerTests
{
    private GameObject dialogueManagerObj;
    private DialogueManager dialogueManager;
    private GameObject dialoguePanelObj;
    private GameObject dialogueTextObj;
    private GameObject npcImageObj;
    private GameObject nextButtonObj;

    private bool dialogueStartEventFired = false;
    private bool dialogueEndEventFired = false;
    private string[] dialogueLinesFromEvent = null;
    private Sprite spriteFromEvent = null;

    [SetUp]
    public void Setup()
    {
        Debug.Log("Setting up test environment");

        // Create objects
        dialogueManagerObj = new GameObject("DialogueManager");

        dialoguePanelObj = new GameObject("DialoguePanel");
        dialoguePanelObj.transform.SetParent(dialogueManagerObj.transform);

        dialogueTextObj = new GameObject("DialogueText");
        dialogueTextObj.transform.SetParent(dialoguePanelObj.transform);
        dialogueTextObj.AddComponent<TextMeshProUGUI>();

        npcImageObj = new GameObject("NPCImage");
        npcImageObj.transform.SetParent(dialoguePanelObj.transform);
        npcImageObj.AddComponent<Image>();

        nextButtonObj = new GameObject("NextButton");
        nextButtonObj.transform.SetParent(dialoguePanelObj.transform);
        nextButtonObj.AddComponent<Button>();

        // Add the DialogueManager component
        dialogueManager = dialogueManagerObj.AddComponent<DialogueManager>();

        // Set references via test method
        dialogueManager.SetTestReferences(
            dialoguePanelObj,
            dialogueTextObj.GetComponent<TextMeshProUGUI>(),
            npcImageObj.GetComponent<Image>(),
            nextButtonObj.GetComponent<Button>()
        );

        // Subscribe to events
        dialogueStartEventFired = false;
        dialogueEndEventFired = false;
        dialogueLinesFromEvent = null;
        spriteFromEvent = null;

        dialogueManager.OnDialogueStarted += () => {
            dialogueStartEventFired = true;
            Debug.Log("Dialogue started event fired");
        };
        dialogueManager.OnDialogueEnd += () => {
            dialogueEndEventFired = true;
            Debug.Log("Dialogue ended event fired");
        };
        dialogueManager.OnSetDialogueLines += (lines, sprite) => {
            dialogueLinesFromEvent = lines;
            spriteFromEvent = sprite;
            Debug.Log($"Dialogue lines set: {string.Join(", ", lines)}");
            if (sprite != null)
                Debug.Log("NPC sprite set: " + sprite.name);
            else
                Debug.Log("No NPC sprite set");
        };

        // Initialize (calls Start)
        dialogueManager.TestStart();
    }

    [TearDown]
    public void TearDown()
    {
        Debug.Log("Tearing down test environment");
        Object.DestroyImmediate(dialogueManagerObj);
    }

    [Test]
    public void SetDialogueLines_SetsLinesAndSprite()
    {
        Debug.Log("Running SetDialogueLines_SetsLinesAndSprite test");

        // Arrange
        string[] testLines = new string[] { "Line 1", "Line 2", "Line 3" };
        Sprite testSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);

        // Act
        dialogueManager.SetDialogueLines(testLines, testSprite);

        // Assert
        Assert.AreEqual(testLines, dialogueLinesFromEvent, "Dialogue lines should be passed to the event");
        Assert.AreEqual(testSprite, spriteFromEvent, "NPC sprite should be passed to the event");
        Assert.IsTrue(npcImageObj.activeSelf, "NPC image should be active");
    }

    [Test]
    public void SetDialogueLines_HandlesNullSprite()
    {
        Debug.Log("Running SetDialogueLines_HandlesNullSprite test");

        // Arrange
        string[] testLines = new string[] { "Line 1", "Line 2", "Line 3" };

        // Act
        dialogueManager.SetDialogueLines(testLines, null);

        // Assert
        Assert.AreEqual(testLines, dialogueLinesFromEvent, "Dialogue lines should be passed to the event");
        Assert.IsNull(spriteFromEvent, "NPC sprite should be null");
        Assert.IsFalse(npcImageObj.activeSelf, "NPC image should be inactive");
    }

    [Test]
    public void StartDialogue_ShowsPanelAndFiresEvent()
    {
        Debug.Log("Running StartDialogue_ShowsPanelAndFiresEvent test");

        // Arrange
        string[] testLines = new string[] { "Line 1", "Line 2", "Line 3" };
        dialogueManager.SetDialogueLines(testLines, null);
        dialoguePanelObj.SetActive(false);
        nextButtonObj.SetActive(false);

        // Act
        dialogueManager.StartDialogue();

        // Assert
        Assert.IsTrue(dialogueStartEventFired, "Dialogue started event should fire");
        Assert.IsTrue(dialoguePanelObj.activeSelf, "Dialogue panel should be visible");
        Assert.IsTrue(nextButtonObj.activeSelf, "Next button should be visible");
        Assert.AreEqual(testLines[0], dialogueManager.GetCurrentTextForTest(), "First line should be displayed");
    }

    [Test]
    public void StartDialogue_HandlesEmptyLines()
    {
        Debug.Log("Running StartDialogue_HandlesEmptyLines test");

        // Arrange - don't set any dialogue lines

        // Act
        dialogueManager.StartDialogue();

        // Assert
        Assert.IsFalse(dialogueStartEventFired, "Dialogue started event should not fire");
        Assert.IsFalse(dialoguePanelObj.activeSelf, "Dialogue panel should not be visible");
    }

    [Test]
    public void DisplayNextLine_AdvancesToNextLine()
    {
        Debug.Log("Running DisplayNextLine_AdvancesToNextLine test");

        // Arrange
        string[] testLines = new string[] { "Line 1", "Line 2", "Line 3" };
        dialogueManager.SetDialogueLines(testLines, null);
        dialogueManager.StartDialogue();

        // Act
        dialogueManager.DisplayNextLine();

        // Assert
        Assert.AreEqual(testLines[1], dialogueManager.GetCurrentTextForTest(), "Second line should be displayed");
        Assert.IsTrue(dialoguePanelObj.activeSelf, "Dialogue panel should still be visible");
    }

    [Test]
    public void DisplayNextLine_EndsDialogueAtLastLine()
    {
        Debug.Log("Running DisplayNextLine_EndsDialogueAtLastLine test");

        // Arrange
        string[] testLines = new string[] { "Line 1", "Line 2" };
        dialogueManager.SetDialogueLines(testLines, null);
        dialogueManager.StartDialogue();

        // Act - advance to last line
        dialogueManager.DisplayNextLine();
        // Act - try to advance past the end
        dialogueManager.DisplayNextLine();

        // Assert
        Assert.IsTrue(dialogueEndEventFired, "Dialogue end event should fire");
        Assert.IsFalse(dialoguePanelObj.activeSelf, "Dialogue panel should be hidden");
        Assert.IsFalse(nextButtonObj.activeSelf, "Next button should be hidden");
    }

    [Test]
    public void EndDialogue_HidesPanelAndFiresEvent()
    {
        Debug.Log("Running EndDialogue_HidesPanelAndFiresEvent test");

        // Arrange
        string[] testLines = new string[] { "Line 1", "Line 2", "Line 3" };
        dialogueManager.SetDialogueLines(testLines, null);
        dialogueManager.StartDialogue();

        // Act
        dialogueManager.EndDialogue();

        // Assert
        Assert.IsTrue(dialogueEndEventFired, "Dialogue end event should fire");
        Assert.IsFalse(dialoguePanelObj.activeSelf, "Dialogue panel should be hidden");
        Assert.IsFalse(nextButtonObj.activeSelf, "Next button should be hidden");
    }
}
