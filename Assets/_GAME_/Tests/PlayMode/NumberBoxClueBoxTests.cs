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
            Debug.Log($"Set field '{fieldName}' on {target.GetType().Name} to value: {value}");
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
            T value = (T)field.GetValue(target);
            Debug.Log($"Got field '{fieldName}' from {target.GetType().Name} with value: {value}");
            return value;
        }

        Debug.LogWarning($"Field '{fieldName}' not found in {target.GetType().Name}");
        return default(T);
    }

    // Helper method to safely invoke a method via reflection
    private object InvokeMethodSafely(object target, string methodName, object[] parameters = null)
    {
        MethodInfo method = target.GetType().GetMethod(methodName,
                               BindingFlags.NonPublic |
                               BindingFlags.Instance);

        if (method != null)
        {
            Debug.Log($"Invoking method '{methodName}' on {target.GetType().Name}");
            object result = method.Invoke(target, parameters);
            Debug.Log($"Method '{methodName}' invoked successfully");
            return result;
        }

        Debug.LogWarning($"Method '{methodName}' not found in {target.GetType().Name}");
        return null;
    }

    [SetUp]
    public void Setup()
    {
        Debug.Log("===== SETTING UP TEST ENVIRONMENT =====");

        // Set up NumberBox and dependencies
        numberBoxObject = new GameObject("NumberBox");
        numberBoxObject.AddComponent<BoxCollider2D>().isTrigger = true;
        numberBox = numberBoxObject.AddComponent<NumberBox>();
        Debug.Log("Created NumberBox GameObject and component");

        GameObject displayObject = new GameObject("NumberDisplay");
        displayObject.transform.SetParent(numberBoxObject.transform);
        numberDisplay = displayObject.AddComponent<TextMeshProUGUI>();
        Debug.Log("Created NumberDisplay with TextMeshProUGUI component");

        // Setup ClueBox and dependencies
        clueBoxObject = new GameObject("ClueBox");
        clueBox = clueBoxObject.AddComponent<ClueBox>();
        Debug.Log("Created ClueBox GameObject and component");

        GameObject clueDisplayObject = new GameObject("ClueDisplay");
        clueDisplayObject.transform.SetParent(clueBoxObject.transform);
        clueDisplay = clueDisplayObject.AddComponent<TextMeshProUGUI>();
        Debug.Log("Created ClueDisplay with TextMeshProUGUI component");

        // Create simple player object
        playerObject = new GameObject("Player");
        playerObject.tag = "Player";
        playerObject.AddComponent<BoxCollider2D>();
        Debug.Log("Created Player GameObject with BoxCollider2D");

        // Setup BridgeCodeArea and dependencies
        bridgeCodeAreaObject = new GameObject("BridgeCodeArea");
        bridgeCodeArea = bridgeCodeAreaObject.AddComponent<BridgeCodeArea>();
        Debug.Log("Created BridgeCodeArea GameObject and component");

        // Set references via reflection - using safe method that checks if field exists
        SetPrivateFieldSafely(numberBox, "numberDisplay", numberDisplay);
        SetPrivateFieldSafely(clueBox, "clueDisplay", clueDisplay);

        // Setup default values
        clueBox.boxIndex = 1;
        Debug.Log($"Set ClueBox boxIndex to {clueBox.boxIndex}");

        Debug.Log("===== TEST ENVIRONMENT SETUP COMPLETED =====");
    }

    [TearDown]
    public void Teardown()
    {
        Debug.Log("===== TEARING DOWN TEST ENVIRONMENT =====");

        Object.Destroy(numberBoxObject);
        Object.Destroy(playerObject);
        Object.Destroy(clueBoxObject);
        Object.Destroy(bridgeCodeAreaObject);

        Debug.Log("All test GameObjects destroyed");
        Debug.Log("===== TEST ENVIRONMENT TEARDOWN COMPLETED =====\n");
    }

    // NumberBox Tests
    [Test]
    public void NumberBox_InitializesWithDefaultValue()
    {
        Debug.Log("\n===== TEST: NumberBox_InitializesWithDefaultValue =====");

        // Log initial state
        Debug.Log($"Initial numberDisplay.text: {numberDisplay.text}");

        // Arrange & Act - Use the helper method to invoke Start
        InvokeMethodSafely(numberBox, "Start");

        // Get the current number after initialization
        int currentNumber = GetPrivateField<int>(numberBox, "currentNumber");

        // Log final state
        Debug.Log($"After Start() - currentNumber: {currentNumber}, numberDisplay.text: {numberDisplay.text}");

        // Assert - Verify text is "1" or check the field directly if text isn't updated
        bool testPassed = numberDisplay.text == "1";
        Debug.Log($"Assertion: numberDisplay.text equals '1'? {testPassed}");

        Assert.AreEqual("1", numberDisplay.text, "NumberBox should initialize with 1");

        Debug.Log("===== TEST COMPLETED SUCCESSFULLY =====");
    }

    [Test]
    public void NumberBox_IncreaseNumber_IncreasesValue()
    {
        Debug.Log("\n===== TEST: NumberBox_IncreaseNumber_IncreasesValue =====");

        // Arrange - Initialize number box
        InvokeMethodSafely(numberBox, "Start");

        // Log initial state
        int initialNumber = GetPrivateField<int>(numberBox, "currentNumber");
        Debug.Log($"Initial state - currentNumber: {initialNumber}, numberDisplay.text: {numberDisplay.text}");

        // Act - Call increase method
        Debug.Log("Calling IncreaseNumber method...");
        InvokeMethodSafely(numberBox, "IncreaseNumber");

        // Log final state
        int finalNumber = GetPrivateField<int>(numberBox, "currentNumber");
        Debug.Log($"Final state - currentNumber: {finalNumber}, numberDisplay.text: {numberDisplay.text}");

        // Assert
        bool testPassed = numberDisplay.text == "2";
        Debug.Log($"Assertion: numberDisplay.text equals '2'? {testPassed}");

        Assert.AreEqual("2", numberDisplay.text, "NumberBox value should increase to 2");

        Debug.Log("===== TEST COMPLETED SUCCESSFULLY =====");
    }

    [Test]
    public void NumberBox_DecreaseNumber_DecreasesValue()
    {
        Debug.Log("\n===== TEST: NumberBox_DecreaseNumber_DecreasesValue =====");

        // Arrange - Initialize number box
        InvokeMethodSafely(numberBox, "Start");

        // Log initial state
        int initialNumber = GetPrivateField<int>(numberBox, "currentNumber");
        Debug.Log($"After Start() - currentNumber: {initialNumber}, numberDisplay.text: {numberDisplay.text}");

        // Set initial value above 1
        Debug.Log("Setting currentNumber to 2...");
        SetPrivateFieldSafely(numberBox, "currentNumber", 2);

        // Update display
        Debug.Log("Updating display to reflect new value...");
        InvokeMethodSafely(numberBox, "UpdateDisplay");

        // Log state after setup
        Debug.Log($"After setup - currentNumber: {GetPrivateField<int>(numberBox, "currentNumber")}, numberDisplay.text: {numberDisplay.text}");

        // Act - Call decrease method
        Debug.Log("Calling DecreaseNumber method...");
        InvokeMethodSafely(numberBox, "DecreaseNumber");

        // Log final state
        int finalNumber = GetPrivateField<int>(numberBox, "currentNumber");
        Debug.Log($"Final state - currentNumber: {finalNumber}, numberDisplay.text: {numberDisplay.text}");

        // Assert
        bool testPassed = numberDisplay.text == "1";
        Debug.Log($"Assertion: numberDisplay.text equals '1'? {testPassed}");

        Assert.AreEqual("1", numberDisplay.text, "NumberBox value should decrease to 1");

        Debug.Log("===== TEST COMPLETED SUCCESSFULLY =====");
    }

    [Test]
    public void NumberBox_GetCurrentNumber_ReturnsCorrectValue()
    {
        Debug.Log("\n===== TEST: NumberBox_GetCurrentNumber_ReturnsCorrectValue =====");

        // Arrange
        int expectedValue = 5;
        Debug.Log($"Setting currentNumber to expected value: {expectedValue}");
        SetPrivateFieldSafely(numberBox, "currentNumber", expectedValue);

        // Act
        Debug.Log("Calling GetCurrentNumber method...");
        int actualValue = numberBox.GetCurrentNumber();
        Debug.Log($"GetCurrentNumber returned: {actualValue}");

        // Assert
        bool testPassed = expectedValue == actualValue;
        Debug.Log($"Assertion: actualValue equals expectedValue ({expectedValue})? {testPassed}");

        Assert.AreEqual(expectedValue, actualValue, "GetCurrentNumber should return the correct value");

        Debug.Log("===== TEST COMPLETED SUCCESSFULLY =====");
    }

    // BridgeCodeArea Tests
    [Test]
    public void BridgeCodeArea_CheckCode_CorrectCombination_Succeeds()
    {
        Debug.Log("\n===== TEST: BridgeCodeArea_CheckCode_CorrectCombination_Succeeds =====");

        // Arrange - Create number boxes and clue boxes
        Debug.Log("Creating number boxes and clue boxes...");
        NumberBox[] numberBoxes = new NumberBox[2];
        ClueBox[] clueBoxes = new ClueBox[2];

        for (int i = 0; i < 2; i++)
        {
            // Create NumberBox
            GameObject boxObj = new GameObject($"NumberBox_{i}");
            numberBoxes[i] = boxObj.AddComponent<NumberBox>();
            Debug.Log($"Created NumberBox_{i}");

            // Create ClueBox
            GameObject clueObj = new GameObject($"ClueBox_{i}");
            clueBoxes[i] = clueObj.AddComponent<ClueBox>();
            Debug.Log($"Created ClueBox_{i}");

            // Setup matching values
            clueBoxes[i].boxIndex = i + 1;
            clueBoxes[i].clueNumber = 5; // Same test value for both
            Debug.Log($"Set ClueBox_{i} boxIndex={i + 1}, clueNumber=5");

            // Set NumberBox value to match
            SetPrivateFieldSafely(numberBoxes[i], "currentNumber", 5); // Same as clue
            Debug.Log($"Set NumberBox_{i} currentNumber=5 to match the clue");
        }

        // Set up the BridgeCodeArea
        Debug.Log("Setting up BridgeCodeArea with boxes...");
        bridgeCodeArea.numberBoxes = numberBoxes;
        bridgeCodeArea.clueBoxes = clueBoxes;
        Debug.Log("Assigned number boxes and clue boxes to BridgeCodeArea");

        // Set up fields we'll check for success
        Debug.Log("Setting up gate and feedback panel...");
        GameObject bridgeGate = new GameObject("BridgeGate");
        GameObject feedbackPanel = new GameObject("FeedbackPanel");
        TextMeshProUGUI feedbackText = new GameObject("FeedbackText").AddComponent<TextMeshProUGUI>();
        Debug.Log("Created gate and feedback objects");

        SetPrivateFieldSafely(bridgeCodeArea, "bridgeGate", bridgeGate);
        SetPrivateFieldSafely(bridgeCodeArea, "feedbackPanel", feedbackPanel);
        SetPrivateFieldSafely(bridgeCodeArea, "feedbackText", feedbackText);

        // Act
        Debug.Log("Calling CheckCode method to test correct combination...");
        InvokeMethodSafely(bridgeCodeArea, "CheckCode");

        // Log results
        Debug.Log($"After CheckCode - bridgeGate.activeSelf: {bridgeGate.activeSelf}");
        Debug.Log($"After CheckCode - feedbackPanel.activeSelf: {feedbackPanel.activeSelf}");
        Debug.Log($"After CheckCode - feedbackText.text: {feedbackText.text}");

        // Assert
        Debug.Log("Checking assertions...");
        bool gateDeactivated = !bridgeGate.activeSelf;
        bool panelActive = feedbackPanel.activeSelf;
        bool textCorrect = feedbackText.text.Contains("Correct");

        Debug.Log($"Bridge gate deactivated? {gateDeactivated}");
        Debug.Log($"Feedback panel activated? {panelActive}");
        Debug.Log($"Feedback text contains 'Correct'? {textCorrect}");

        Assert.IsFalse(bridgeGate.activeSelf, "Bridge gate should be deactivated on correct code");
        Assert.IsTrue(feedbackPanel.activeSelf, "Feedback panel should be activated");
        Assert.IsTrue(feedbackText.text.Contains("Correct"), "Feedback text should indicate success");

        // Clean up
        Debug.Log("Cleaning up test objects...");
        foreach (var box in numberBoxes)
            Object.Destroy(box.gameObject);
        foreach (var clue in clueBoxes)
            Object.Destroy(clue.gameObject);
        Object.Destroy(bridgeGate);
        Object.Destroy(feedbackPanel);
        Object.Destroy(feedbackText.gameObject);

        Debug.Log("===== TEST COMPLETED SUCCESSFULLY =====");
    }

    [Test]
    public void BridgeCodeArea_CheckCode_IncorrectCombination_Fails()
    {
        Debug.Log("\n===== TEST: BridgeCodeArea_CheckCode_IncorrectCombination_Fails =====");

        // Arrange - Create number boxes and clue boxes
        Debug.Log("Creating number boxes and clue boxes...");
        NumberBox[] numberBoxes = new NumberBox[2];
        ClueBox[] clueBoxes = new ClueBox[2];

        for (int i = 0; i < 2; i++)
        {
            // Create NumberBox
            GameObject boxObj = new GameObject($"NumberBox_{i}");
            numberBoxes[i] = boxObj.AddComponent<NumberBox>();
            Debug.Log($"Created NumberBox_{i}");

            // Create ClueBox
            GameObject clueObj = new GameObject($"ClueBox_{i}");
            clueBoxes[i] = clueObj.AddComponent<ClueBox>();
            Debug.Log($"Created ClueBox_{i}");

            // Setup non-matching values
            clueBoxes[i].boxIndex = i + 1;
            clueBoxes[i].clueNumber = 5;
            Debug.Log($"Set ClueBox_{i} boxIndex={i + 1}, clueNumber=5");

            // Set NumberBox value to NOT match
            SetPrivateFieldSafely(numberBoxes[i], "currentNumber", 3); // Different from clue
            Debug.Log($"Set NumberBox_{i} currentNumber=3 (deliberately different from clue)");
        }

        // Set up the BridgeCodeArea
        Debug.Log("Setting up BridgeCodeArea with boxes...");
        bridgeCodeArea.numberBoxes = numberBoxes;
        bridgeCodeArea.clueBoxes = clueBoxes;
        Debug.Log("Assigned number boxes and clue boxes to BridgeCodeArea");

        // Fields for feedback
        Debug.Log("Setting up gate and feedback panel...");
        GameObject bridgeGate = new GameObject("BridgeGate");
        bridgeGate.SetActive(true);
        GameObject feedbackPanel = new GameObject("FeedbackPanel");
        TextMeshProUGUI feedbackText = new GameObject("FeedbackText").AddComponent<TextMeshProUGUI>();
        Debug.Log("Created gate and feedback objects");

        SetPrivateFieldSafely(bridgeCodeArea, "bridgeGate", bridgeGate);
        SetPrivateFieldSafely(bridgeCodeArea, "feedbackPanel", feedbackPanel);
        SetPrivateFieldSafely(bridgeCodeArea, "feedbackText", feedbackText);

        int initialLives = 3;
        SetPrivateFieldSafely(bridgeCodeArea, "livesRemaining", initialLives);
        Debug.Log($"Set livesRemaining to {initialLives}");

        // Act
        Debug.Log("Calling CheckCode method to test incorrect combination...");
        InvokeMethodSafely(bridgeCodeArea, "CheckCode");

        // Log results
        Debug.Log($"After CheckCode - bridgeGate.activeSelf: {bridgeGate.activeSelf}");
        Debug.Log($"After CheckCode - feedbackPanel.activeSelf: {feedbackPanel.activeSelf}");
        Debug.Log($"After CheckCode - feedbackText.text: {feedbackText.text}");

        // Get updated lives
        int remainingLives = GetPrivateField<int>(bridgeCodeArea, "livesRemaining");
        Debug.Log($"Lives remaining after incorrect attempt: {remainingLives} (started with {initialLives})");

        // Assert
        Debug.Log("Checking assertions...");
        bool gateStillActive = bridgeGate.activeSelf;
        bool panelActive = feedbackPanel.activeSelf;
        bool textWrong = feedbackText.text.Contains("WRONG");
        bool livesDecremented = remainingLives == initialLives - 1;

        Debug.Log($"Bridge gate still active? {gateStillActive}");
        Debug.Log($"Feedback panel activated? {panelActive}");
        Debug.Log($"Feedback text contains 'WRONG'? {textWrong}");
        Debug.Log($"Lives decremented? {livesDecremented} ({initialLives} -> {remainingLives})");

        Assert.IsTrue(bridgeGate.activeSelf, "Bridge gate should remain active on incorrect code");
        Assert.IsTrue(feedbackPanel.activeSelf, "Feedback panel should be activated");
        Assert.IsTrue(feedbackText.text.Contains("WRONG"), "Feedback text should indicate failure");

        // Also check lives were decremented
        Assert.AreEqual(initialLives - 1, remainingLives, "Lives should be decremented after incorrect attempt");

        // Clean up
        Debug.Log("Cleaning up test objects...");
        foreach (var box in numberBoxes)
            Object.Destroy(box.gameObject);
        foreach (var clue in clueBoxes)
            Object.Destroy(clue.gameObject);
        Object.Destroy(bridgeGate);
        Object.Destroy(feedbackPanel);
        Object.Destroy(feedbackText.gameObject);

        Debug.Log("===== TEST COMPLETED SUCCESSFULLY =====");
    }
}