using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

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

        // Set references via reflection
        var displayField = typeof(NumberBox).GetField("numberDisplay",
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Instance);
        displayField.SetValue(numberBox, numberDisplay);

        var clueDisplayField = typeof(ClueBox).GetField("clueDisplay",
                                   System.Reflection.BindingFlags.NonPublic |
                                   System.Reflection.BindingFlags.Instance);
        clueDisplayField.SetValue(clueBox, clueDisplay);

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
        // Arrange & Act
        var startMethod = typeof(NumberBox).GetMethod("Start",
                               System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Instance);
        startMethod.Invoke(numberBox, null);

        // Assert
        Assert.AreEqual("1", numberDisplay.text, "NumberBox should initialize with 1");
    }

    [Test]
    public void NumberBox_IncreaseNumber_IncreasesValue()
    {
        // Arrange
        var startMethod = typeof(NumberBox).GetMethod("Start",
                               System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Instance);
        startMethod.Invoke(numberBox, null);

        // Act
        var increaseMethod = typeof(NumberBox).GetMethod("IncreaseNumber",
                                 System.Reflection.BindingFlags.NonPublic |
                                 System.Reflection.BindingFlags.Instance);
        increaseMethod.Invoke(numberBox, null);

        // Assert
        Assert.AreEqual("2", numberDisplay.text, "NumberBox value should increase to 2");
    }

    [Test]
    public void NumberBox_DecreaseNumber_DecreasesValue()
    {
        // Arrange
        var startMethod = typeof(NumberBox).GetMethod("Start",
                               System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Instance);
        startMethod.Invoke(numberBox, null);

        // Set initial value above 1
        var currentNumberField = typeof(NumberBox).GetField("currentNumber",
                                     System.Reflection.BindingFlags.NonPublic |
                                     System.Reflection.BindingFlags.Instance);
        currentNumberField.SetValue(numberBox, 2);

        // Update display
        var updateDisplayMethod = typeof(NumberBox).GetMethod("UpdateDisplay",
                                      System.Reflection.BindingFlags.NonPublic |
                                      System.Reflection.BindingFlags.Instance);
        updateDisplayMethod.Invoke(numberBox, null);

        // Act
        var decreaseMethod = typeof(NumberBox).GetMethod("DecreaseNumber",
                                 System.Reflection.BindingFlags.NonPublic |
                                 System.Reflection.BindingFlags.Instance);
        decreaseMethod.Invoke(numberBox, null);

        // Assert
        Assert.AreEqual("1", numberDisplay.text, "NumberBox value should decrease to 1");
    }

    [Test]
    public void NumberBox_GetCurrentNumber_ReturnsCorrectValue()
    {
        // Arrange
        int expectedValue = 5;
        var currentNumberField = typeof(NumberBox).GetField("currentNumber",
                                     System.Reflection.BindingFlags.NonPublic |
                                     System.Reflection.BindingFlags.Instance);
        currentNumberField.SetValue(numberBox, expectedValue);

        // Act
        int actualValue = numberBox.GetCurrentNumber();

        // Assert
        Assert.AreEqual(expectedValue, actualValue, "GetCurrentNumber should return the correct value");
    }

    [UnityTest]
    public IEnumerator NumberBox_PlayerEntersRange_ActivatesInteraction()
    {
        // Arrange
        var playerIsNearbyField = typeof(NumberBox).GetField("playerIsNearby",
                                      System.Reflection.BindingFlags.NonPublic |
                                      System.Reflection.BindingFlags.Instance);
        // Initial state should be false
        playerIsNearbyField.SetValue(numberBox, false);

        // Act - Call OnTriggerEnter2D
        var triggerEnterMethod = typeof(NumberBox).GetMethod("OnTriggerEnter2D",
                                     System.Reflection.BindingFlags.NonPublic |
                                     System.Reflection.BindingFlags.Instance);
        triggerEnterMethod.Invoke(numberBox, new object[] { playerObject.GetComponent<Collider2D>() });

        // Wait a frame
        yield return null;

        // Assert
        bool playerIsNearby = (bool)playerIsNearbyField.GetValue(numberBox);
        Assert.IsTrue(playerIsNearby, "Player should be marked as nearby when entering trigger");
    }

    // ClueBox Tests
    [Test]
    public void ClueBox_Start_GeneratesRandomNumber()
    {
        // Arrange
        var originalValue = clueBox.clueNumber;

        // Act
        var startMethod = typeof(ClueBox).GetMethod("Start",
                               System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Instance);
        startMethod.Invoke(clueBox, null);

        // Assert
        Assert.IsTrue(clueBox.clueNumber >= 1 && clueBox.clueNumber <= 9,
                     "ClueBox should generate a number between 1 and 9");
        Assert.AreEqual(clueBox.clueNumber.ToString(), clueDisplay.text,
                      "ClueBox display should show the generated number");
    }

    // BridgeCodeArea Tests
    [Test]
    public void BridgeCodeArea_CheckCode_CorrectCombination_Succeeds()
    {
        // Arrange
        // Create number boxes and clue boxes
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

            // Set NumberBox value to match (using reflection)
            var currentNumberField = typeof(NumberBox).GetField("currentNumber",
                                         System.Reflection.BindingFlags.NonPublic |
                                         System.Reflection.BindingFlags.Instance);
            currentNumberField.SetValue(numberBoxes[i], 5); // Same as clue
        }

        // Set up the BridgeCodeArea
        bridgeCodeArea.numberBoxes = numberBoxes;
        bridgeCodeArea.clueBoxes = clueBoxes;

        // Set up fields we'll check for success
        GameObject bridgeGate = new GameObject("BridgeGate");
        GameObject feedbackPanel = new GameObject("FeedbackPanel");
        TextMeshProUGUI feedbackText = new GameObject("FeedbackText").AddComponent<TextMeshProUGUI>();

        var bridgeGateField = typeof(BridgeCodeArea).GetField("bridgeGate",
                                  System.Reflection.BindingFlags.NonPublic |
                                  System.Reflection.BindingFlags.Instance);
        var feedbackPanelField = typeof(BridgeCodeArea).GetField("feedbackPanel",
                                     System.Reflection.BindingFlags.NonPublic |
                                     System.Reflection.BindingFlags.Instance);
        var feedbackTextField = typeof(BridgeCodeArea).GetField("feedbackText",
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance);

        bridgeGateField.SetValue(bridgeCodeArea, bridgeGate);
        feedbackPanelField.SetValue(bridgeCodeArea, feedbackPanel);
        feedbackTextField.SetValue(bridgeCodeArea, feedbackText);

        // Act
        var checkCodeMethod = typeof(BridgeCodeArea).GetMethod("CheckCode",
                                  System.Reflection.BindingFlags.NonPublic |
                                  System.Reflection.BindingFlags.Instance);
        checkCodeMethod.Invoke(bridgeCodeArea, null);

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
        // Arrange
        // Create number boxes and clue boxes
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
            var currentNumberField = typeof(NumberBox).GetField("currentNumber",
                                         System.Reflection.BindingFlags.NonPublic |
                                         System.Reflection.BindingFlags.Instance);
            currentNumberField.SetValue(numberBoxes[i], 3); // Different from clue
        }

        // Set up the BridgeCodeArea
        bridgeCodeArea.numberBoxes = numberBoxes;
        bridgeCodeArea.clueBoxes = clueBoxes;

        // Fields for feedback
        GameObject bridgeGate = new GameObject("BridgeGate");
        bridgeGate.SetActive(true);
        GameObject feedbackPanel = new GameObject("FeedbackPanel");
        TextMeshProUGUI feedbackText = new GameObject("FeedbackText").AddComponent<TextMeshProUGUI>();

        var bridgeGateField = typeof(BridgeCodeArea).GetField("bridgeGate",
                                  System.Reflection.BindingFlags.NonPublic |
                                  System.Reflection.BindingFlags.Instance);
        var feedbackPanelField = typeof(BridgeCodeArea).GetField("feedbackPanel",
                                     System.Reflection.BindingFlags.NonPublic |
                                     System.Reflection.BindingFlags.Instance);
        var feedbackTextField = typeof(BridgeCodeArea).GetField("feedbackText",
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance);
        var livesField = typeof(BridgeCodeArea).GetField("livesRemaining",
                              System.Reflection.BindingFlags.NonPublic |
                              System.Reflection.BindingFlags.Instance);

        bridgeGateField.SetValue(bridgeCodeArea, bridgeGate);
        feedbackPanelField.SetValue(bridgeCodeArea, feedbackPanel);
        feedbackTextField.SetValue(bridgeCodeArea, feedbackText);
        livesField.SetValue(bridgeCodeArea, 3);

        // Act
        var checkCodeMethod = typeof(BridgeCodeArea).GetMethod("CheckCode",
                                  System.Reflection.BindingFlags.NonPublic |
                                  System.Reflection.BindingFlags.Instance);
        checkCodeMethod.Invoke(bridgeCodeArea, null);

        // Assert
        Assert.IsTrue(bridgeGate.activeSelf, "Bridge gate should remain active on incorrect code");
        Assert.IsTrue(feedbackPanel.activeSelf, "Feedback panel should be activated");
        Assert.IsTrue(feedbackText.text.Contains("WRONG"), "Feedback text should indicate failure");

        // Also check lives were decremented
        int lives = (int)livesField.GetValue(bridgeCodeArea);
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