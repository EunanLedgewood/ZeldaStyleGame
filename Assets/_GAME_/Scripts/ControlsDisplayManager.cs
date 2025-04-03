using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class ControlInfo
{
    public string actionName;
    public string keyName;
    public string description;
}

public class ControlsDisplayManager : MonoBehaviour
{
    [Header("Control Entries")]
    [SerializeField] private List<ControlInfo> controls = new List<ControlInfo>();

    [Header("UI References")]
    [SerializeField] private Transform controlsContainer;
    [SerializeField] private GameObject controlEntryPrefab;
    [SerializeField] private Button closeButton;

    private GameObject pauseMenuPanel;

    // Added for testing
    public void SetTestReferences(Transform container, GameObject prefab, Button button)
    {
        controlsContainer = container;
        controlEntryPrefab = prefab;
        closeButton = button;
    }

    // For testing - expose the controls list
    public List<ControlInfo> GetControlsForTest()
    {
        return controls;
    }

    // Expose Awake for testing
    public void TestAwake()
    {
        Awake();
    }

    private void Awake()
    {
        // Set up default controls if none exist
        if (controls.Count == 0)
        {
            SetupDefaultControls();
        }

        // Find the pause menu panel
        pauseMenuPanel = transform.parent.Find("PauseMenuPanel")?.gameObject;

        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ReturnToPauseMenu);
        }
        else
        {
            Debug.LogWarning("Close button reference is missing in ControlsDisplayManager");
        }
    }

    private void Start()
    {
        // Generate the controls UI
        GenerateControlsUI();
    }

    public void ReturnToPauseMenu()
    {
        // Hide controls panel
        gameObject.SetActive(false);

        // Show pause menu if available
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    private void GenerateControlsUI()
    {
        // Clear any existing controls
        foreach (Transform child in controlsContainer)
        {
            Destroy(child.gameObject);
        }

        // Create the controls UI from our list
        foreach (ControlInfo control in controls)
        {
            GameObject entryObj = Instantiate(controlEntryPrefab, controlsContainer);

            // Try to find the text components in the prefab
            Transform actionTransform = entryObj.transform.Find("ActionText");
            Transform keyTransform = entryObj.transform.Find("KeyText");
            Transform descriptionTransform = entryObj.transform.Find("DescriptionText");

            // Set action name
            if (actionTransform != null)
            {
                TextMeshProUGUI actionText = actionTransform.GetComponent<TextMeshProUGUI>();
                if (actionText != null)
                {
                    actionText.text = control.actionName + ":";
                }
            }

            // Set key name
            if (keyTransform != null)
            {
                TextMeshProUGUI keyText = keyTransform.GetComponent<TextMeshProUGUI>();
                if (keyText != null)
                {
                    keyText.text = control.keyName;
                }
            }

            // Set description
            if (descriptionTransform != null)
            {
                TextMeshProUGUI descriptionText = descriptionTransform.GetComponent<TextMeshProUGUI>();
                if (descriptionText != null)
                {
                    descriptionText.text = control.description;
                }
            }
        }
    }

    private void SetupDefaultControls()
    {
        controls.Add(new ControlInfo
        {
            actionName = "Movement",
            keyName = "WASD / Arrow Keys",
            description = "Move your character around the game world"
        });

        controls.Add(new ControlInfo
        {
            actionName = "Interact",
            keyName = "E",
            description = "Pick up boxes and interact with objects"
        });

        controls.Add(new ControlInfo
        {
            actionName = "Pause",
            keyName = "ESC",
            description = "Pause the game and open menu"
        });
    }

    // Public method to add a new control at runtime
    public void AddControl(string action, string key, string description)
    {
        controls.Add(new ControlInfo
        {
            actionName = action,
            keyName = key,
            description = description
        });

        // Regenerate the UI
        GenerateControlsUI();
    }
}