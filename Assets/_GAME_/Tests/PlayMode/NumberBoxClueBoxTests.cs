using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using System.Reflection;

public class NumberBoxClueBoxTests
{
    // NumberBox tests
    private GameObject numberBoxObject;
    private NumberBox numberBox;
    private TextMeshProUGUI numberDisplay;
    private GameObject playerObject;

    // ClueBox tests
    private GameObject clueBoxObject;
    private ClueBox clueBox;
    private TextMeshProUGUI clueDisplay;

    // BridgeCodeArea tests
    private GameObject bridgeCodeAreaObject;
    private BridgeCodeArea bridgeCodeArea;

    // Helper method to safely set a private field via reflection
    private bool SetPrivateFieldSafely<T>(object target, string fieldName, T value)
    {
        FieldInfo field = target.GetType().GetField(fieldName,
                             BindingFlags.NonPublic |
                             BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(target, value);
            return true;
        }

        Debug.LogWarning($"Field '{fieldName}' not found in {target.GetType().Name}");
        return false;
    }

    // Helper method to safely get a private field via reflection
    private T GetPrivateField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName,
                             BindingFlags.NonPublic |
                             BindingFlags.Instance);

        if (field != null)
        {
            return (T)field.GetValue(target);
        }

        Debug.LogWarning($"Field '{fieldName}' not found in {target.GetType().Name}");
        return default(T);
    }

    [SetUp]
    public void Setup()
    {
        // Set up NumberBox and dependencies
        numberBoxObject = new GameObject("NumberBox");
        numberBoxObject.AddComponent<BoxCollider2D>().isTrigger = true;
        numberBox = numberBoxObject.AddComponent<NumberBox>();

        GameObject displayObject = new GameObject("NumberDisplay");
        displayObject.transform.SetParent(numberBoxObject.transform);
        numberDisplay = displayObject.AddComponent<TextMeshProUGUI>();

        // Setup ClueBox and dependencies
        clueBoxObject = new GameObject("ClueBox");
        clueBox = clueBoxObject.AddComponent<ClueBox>();

        GameObject clueDisplayObject = new GameObject("ClueDisplay");
        clueDisplayObject.transform.SetParent(clueBoxObject.transform);
        clueDisplay = clueDisplayObject.AddComponent<TextMeshProUGUI>();

        // Create simple player object
        playerObject = new GameObject("Player");
        playerObject.tag = "Player";
        playerObject.AddComponent<BoxCollider2D>();

        // Setup BridgeCodeArea and dependencies
        bridgeCodeAreaObject = new GameObject("BridgeCodeArea");
        bridgeCodeArea = bridgeCodeAreaObject.AddComponent<BridgeCodeArea>();

        // Set references via reflection - using safe method that checks if field exists
        SetPrivateFieldSafely(numberBox, "numberDisplay", numberDisplay);
        SetPrivateFieldSafely(clueBox, "clueDisplay", clueDisplay);

        // Setup default values
        clueBox.boxIndex = 1;
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(numberBoxObject);
        Object.Destroy(playerObject);
        Object.Destroy(clueBoxObject);
        Object.Destroy(bridgeCodeAreaObject);
    }

    // NumberBox Tests
    [Test]
    public void NumberBox_InitializesWithDefaultValue()
    {
        // Arrange & Act - Use the safe method to invoke Start
        var startMethod = typeof(NumberBox).GetMethod("Start",
                               BindingFlags.NonPublic |
                               BindingFlags.Instance);

        if (startMethod != null)
        {
            startMethod.Invoke(numberBox, null);
        }
        else
        {
            Debug.LogWarning("Start method not found in NumberBox");
        }

        // Assert - Verify text is "1" or check the field directly if text isn't updated
        Assert.AreEqual("1", numberDisplay.text, "NumberBox should initialize with 1");
    }

    [Test]
    public void NumberBox_IncreaseNumber_IncreasesValue()
    {
        // Arrange - Initialize number box
        var startMethod = typeof(NumberBox).GetMethod("Start",
                               BindingFlags.NonPublic |
                               BindingFlags.Instance);

        if (startMethod != null)
        {
            startMethod.Invoke(numberBox, null);
        }

        // Act - Call increase method
        var increaseMethod = typeof(NumberBox).GetMethod("IncreaseNumber",
                                 BindingFlags.NonPublic |
                                 BindingFlags.Instance);

        if (increaseMethod != null)
        {
            increaseMethod.Invoke(numberBox, null);
        }
        else
        {
            Debug.LogWarning("IncreaseNumber method not found in NumberBox");
        }

        // Assert
        Assert.AreEqual("2", numberDisplay.text, "NumberBox value should increase to 2");
    }

    [Test]
    public void NumberBox_DecreaseNumber_DecreasesValue()
    {
        // Arrange - Initialize number box
        var startMethod = typeof(NumberBox).GetMethod("Start",
                               BindingFlags.NonPublic |
                               BindingFlags.Instance);

        if (startMethod != null)
        {
            startMethod.Invoke(numberBox, null);
        }

        // Set initial value above 1
        SetPrivateFieldSafely(numberBox, "currentNumber", 2);

        // Update display
        var updateDisplayMethod = typeof(NumberBox).GetMethod("UpdateDisplay",
                                      BindingFlags.NonPublic |
                                      BindingFlags.Instance);

        if (updateDisplayMethod != null)
        {
            updateDisplayMethod.Invoke(numberBox, null);
        }

        // Act - Call decrease method
        var decreaseMethod = typeof(NumberBox).GetMethod("DecreaseNumber",
                                 BindingFlags.NonPublic |
                                 BindingFlags.Instance);

        if (decreaseMethod != null)
        {
            decreaseMethod.Invoke(numberBox, null);
        }

        // Assert
        Assert.AreEqual("1", numberDisplay.text, "NumberBox value should decrease to 1");
    }

    [Test]
    public void NumberBox_GetCurrentNumber_ReturnsCorrectValue()
    {
        // Arrange
        int expectedValue = 5;
        SetPrivateFieldSafely(numberBox, "currentNumber", expectedValue);

        // Act
        int actualValue = numberBox.GetCurrentNumber();

        // Assert
        Assert.AreEqual(expectedValue, actualValue, "GetCurrentNumber should return the correct value");
    }

    // BridgeCodeArea Tests
    [Test]
    public void BridgeCodeArea_CheckCode_CorrectCombination_Succeeds()
    {
        // Arrange - Create number boxes and clue boxes
        NumberBox[] numberBoxes = new NumberBox[2];
        ClueBox[] clueBoxes = new ClueBox[2];

        for (int i = 0; i < 2; i++)
        {
            // Create NumberBox
            GameObject boxObj = new GameObject($"NumberBox_{i}");
            numberBoxes[i] = boxObj.AddComponent<NumberBox>();

            // Create ClueBox
            GameObject clueObj = new GameObject($"ClueBox_{i}");
            clueBoxes[i] = clueObj.AddComponent<ClueBox>();

            // Setup matching values
            clueBoxes[i].boxIndex = i + 1;
            clueBoxes[i].clueNumber = 5; // Same test value for both

            // Set NumberBox value to match
            SetPrivateFieldSafely(numberBoxes[i], "currentNumber", 5); // Same as clue
        }

        // Set up the BridgeCodeArea
        bridgeCodeArea.numberBoxes = numberBoxes;
        bridgeCodeArea.clueBoxes = clueBoxes;

        // Set up fields we'll check for success
        GameObject bridgeGate = new GameObject("BridgeGate");
        GameObject feedbackPanel = new GameObject("FeedbackPanel");
        TextMeshProUGUI feedbackText = new GameObject("FeedbackText").AddComponent<TextMeshProUGUI>();

        SetPrivateFieldSafely(bridgeCodeArea, "bridgeGate", bridgeGate);
        SetPrivateFieldSafely(bridgeCodeArea, "feedbackPanel", feedbackPanel);
        SetPrivateFieldSafely(bridgeCodeArea, "feedbackText", feedbackText);

        // Act
        var checkCodeMethod = typeof(BridgeCodeArea).GetMethod("CheckCode",
                                  BindingFlags.NonPublic |
                                  BindingFlags.Instance);

        if (checkCodeMethod != null)
        {
            checkCodeMethod.Invoke(bridgeCodeArea, null);
        }

        // Assert
        Assert.IsFalse(bridgeGate.activeSelf, "Bridge gate should be deactivated on correct code");
        Assert.IsTrue(feedbackPanel.activeSelf, "Feedback panel should be activated");
        Assert.IsTrue(feedbackText.text.Contains("Correct"), "Feedback text should indicate success");

        // Clean up
        foreach (var box in numberBoxes)
            Object.Destroy(box.gameObject);
        foreach (var clue in clueBoxes)
            Object.Destroy(clue.gameObject);
        Object.Destroy(bridgeGate);
        Object.Destroy(feedbackPanel);
        Object.Destroy(feedbackText.gameObject);
    }

    [Test]
    public void BridgeCodeArea_CheckCode_IncorrectCombination_Fails()
    {
        // Arrange - Create number boxes and clue boxes
        NumberBox[] numberBoxes = new NumberBox[2];
        ClueBox[] clueBoxes = new ClueBox[2];

        for (int i = 0; i < 2; i++)
        {
            // Create NumberBox
            GameObject boxObj = new GameObject($"NumberBox_{i}");
            numberBoxes[i] = boxObj.AddComponent<NumberBox>();

            // Create ClueBox
            GameObject clueObj = new GameObject($"ClueBox_{i}");
            clueBoxes[i] = clueObj.AddComponent<ClueBox>();

            // Setup non-matching values
            clueBoxes[i].boxIndex = i + 1;
            clueBoxes[i].clueNumber = 5;

            // Set NumberBox value to NOT match
            SetPrivateFieldSafely(numberBoxes[i], "currentNumber", 3); // Different from clue
        }

        // Set up the BridgeCodeArea
        bridgeCodeArea.numberBoxes = numberBoxes;
        bridgeCodeArea.clueBoxes = clueBoxes;

        // Fields for feedback
        GameObject bridgeGate = new GameObject("BridgeGate");
        bridgeGate.SetActive(true);
        GameObject feedbackPanel = new GameObject("FeedbackPanel");
        TextMeshProUGUI feedbackText = new GameObject("FeedbackText").AddComponent<TextMeshProUGUI>();

        SetPrivateFieldSafely(bridgeCodeArea, "bridgeGate", bridgeGate);
        SetPrivateFieldSafely(bridgeCodeArea, "feedbackPanel", feedbackPanel);
        SetPrivateFieldSafely(bridgeCodeArea, "feedbackText", feedbackText);
        SetPrivateFieldSafely(bridgeCodeArea, "livesRemaining", 3);

        // Act
        var checkCodeMethod = typeof(BridgeCodeArea).GetMethod("CheckCode",
                                  BindingFlags.NonPublic |
                                  BindingFlags.Instance);

        if (checkCodeMethod != null)
        {
            checkCodeMethod.Invoke(bridgeCodeArea, null);
        }

        // Assert
        Assert.IsTrue(bridgeGate.activeSelf, "Bridge gate should remain active on incorrect code");
        Assert.IsTrue(feedbackPanel.activeSelf, "Feedback panel should be activated");
        Assert.IsTrue(feedbackText.text.Contains("WRONG"), "Feedback text should indicate failure");

        // Also check lives were decremented
        int lives = GetPrivateField<int>(bridgeCodeArea, "livesRemaining");
        Assert.AreEqual(2, lives, "Lives should be decremented after incorrect attempt");

        // Clean up
        foreach (var box in numberBoxes)
            Object.Destroy(box.gameObject);
        foreach (var clue in clueBoxes)
            Object.Destroy(clue.gameObject);
        Object.Destroy(bridgeGate);
        Object.Destroy(feedbackPanel);
        Object.Destroy(feedbackText.gameObject);
    }
}