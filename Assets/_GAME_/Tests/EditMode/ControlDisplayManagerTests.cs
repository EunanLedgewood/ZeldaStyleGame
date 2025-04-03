using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

[TestFixture]
public class ControlsDisplayManagerTests
{
    private GameObject controlsManagerObj;
    private ControlsDisplayManager controlsManager;
    private GameObject canvasObj;
    private GameObject pauseMenuPanelObj;
    private GameObject controlsContainerObj;
    private GameObject controlEntryPrefabObj;
    private GameObject closeButtonObj;

    [SetUp]
    public void Setup()
    {
        // Create required objects
        canvasObj = new GameObject("Canvas");
        pauseMenuPanelObj = new GameObject("PauseMenuPanel");
        pauseMenuPanelObj.transform.SetParent(canvasObj.transform);

        controlsManagerObj = new GameObject("ControlsDisplayManager");
        controlsManagerObj.transform.SetParent(canvasObj.transform);

        controlsContainerObj = new GameObject("ControlsContainer");
        controlsContainerObj.transform.SetParent(controlsManagerObj.transform);

        controlEntryPrefabObj = CreateControlEntryPrefab();

        closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(controlsManagerObj.transform);
        closeButtonObj.AddComponent<Button>();

        // Add the component
        controlsManager = controlsManagerObj.AddComponent<ControlsDisplayManager>();

        // Set references
        controlsManager.SetTestReferences(
            controlsContainerObj.transform,
            controlEntryPrefabObj,
            closeButtonObj.GetComponent<Button>()
        );
    }

    private GameObject CreateControlEntryPrefab()
    {
        GameObject prefab = new GameObject("ControlEntryPrefab");

        GameObject actionTextObj = new GameObject("ActionText");
        actionTextObj.transform.SetParent(prefab.transform);
        actionTextObj.AddComponent<TextMeshProUGUI>();

        GameObject keyTextObj = new GameObject("KeyText");
        keyTextObj.transform.SetParent(prefab.transform);
        keyTextObj.AddComponent<TextMeshProUGUI>();

        GameObject descriptionTextObj = new GameObject("DescriptionText");
        descriptionTextObj.transform.SetParent(prefab.transform);
        descriptionTextObj.AddComponent<TextMeshProUGUI>();

        return prefab;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(canvasObj);
    }

    [Test]
    public void DefaultControls_AreAddedWhenNoControlsExist()
    {
        // Act
        controlsManager.TestAwake();

        // Assert
        List<ControlInfo> controls = controlsManager.GetControlsForTest();
        Assert.IsNotEmpty(controls, "Default controls should be added");
        Assert.AreEqual(3, controls.Count, "Should have 3 default controls");

        // Verify specific default controls
        Assert.AreEqual("Movement", controls[0].actionName);
        Assert.AreEqual("WASD / Arrow Keys", controls[0].keyName);

        Assert.AreEqual("Interact", controls[1].actionName);
        Assert.AreEqual("E", controls[1].keyName);

        Assert.AreEqual("Pause", controls[2].actionName);
        Assert.AreEqual("ESC", controls[2].keyName);
    }

    [Test]
    public void AddControl_AddsNewControlToList()
    {
        // Arrange
        controlsManager.TestAwake(); // Initialize default controls
        int initialCount = controlsManager.GetControlsForTest().Count;

        // Act
        controlsManager.AddControl("Jump", "Space", "Make your character jump");

        // Assert
        List<ControlInfo> controls = controlsManager.GetControlsForTest();
        Assert.AreEqual(initialCount + 1, controls.Count, "Control should be added to the list");

        // Verify the added control
        ControlInfo addedControl = controls[controls.Count - 1];
        Assert.AreEqual("Jump", addedControl.actionName);
        Assert.AreEqual("Space", addedControl.keyName);
        Assert.AreEqual("Make your character jump", addedControl.description);
    }

    [Test]
    public void ReturnToPauseMenu_HidesControlsAndShowsPauseMenu()
    {
        // Arrange
        controlsManager.TestAwake();
        pauseMenuPanelObj.SetActive(false);
        controlsManagerObj.SetActive(true);

        // Act
        controlsManager.ReturnToPauseMenu();

        // Assert
        Assert.IsFalse(controlsManagerObj.activeSelf, "Controls panel should be hidden");
        Assert.IsTrue(pauseMenuPanelObj.activeSelf, "Pause menu should be visible");
    }
}