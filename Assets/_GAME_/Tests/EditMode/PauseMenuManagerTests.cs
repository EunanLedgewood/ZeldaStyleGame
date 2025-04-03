using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

[TestFixture]
public class PauseMenuManagerTests
{
    private GameObject pauseMenuManagerObj;
    private PauseMenuManager pauseMenuManager;
    private GameObject pauseMenuPanelObj;
    private GameObject volumeSliderObj;
    private Slider volumeSlider;
    private GameObject gameManagerObj;
    private GameManager gameManager;

    [SetUp]
    public void Setup()
    {
        // Create GameManager first (it will be needed by PauseMenuManager)
        gameManagerObj = new GameObject("GameManager");
        gameManager = gameManagerObj.AddComponent<GameManager>();
        gameManager.InitializeForTesting();
        GameManager.instance = gameManager;

        // Create PauseMenuManager objects
        pauseMenuManagerObj = new GameObject("PauseMenuManager");
        pauseMenuPanelObj = new GameObject("PauseMenuPanel");
        pauseMenuPanelObj.transform.SetParent(pauseMenuManagerObj.transform);

        volumeSliderObj = new GameObject("VolumeSlider");
        volumeSliderObj.transform.SetParent(pauseMenuManagerObj.transform);
        volumeSlider = volumeSliderObj.AddComponent<Slider>();

        // Add the PauseMenuManager component
        pauseMenuManager = pauseMenuManagerObj.AddComponent<PauseMenuManager>();

        // Set references for testing
        pauseMenuManager.SetTestReferences(pauseMenuPanelObj, volumeSlider);

        // Initialize UI state
        pauseMenuManager.TestStart();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up
        Object.DestroyImmediate(pauseMenuManagerObj);
        Object.DestroyImmediate(gameManagerObj);

        // Reset static reference
        GameManager.instance = null;
    }

    [Test]
    public void Start_InitializesPauseMenuHidden()
    {
        // Already executed in setup

        // Assert
        Assert.IsFalse(pauseMenuPanelObj.activeSelf, "Pause menu should be hidden at start");
        Assert.IsFalse(pauseMenuManager.GetIsPausedForTest(), "Game should not be paused at start");
    }

    [Test]
    public void TogglePause_ShowsMenuAndPausesGame()
    {
        // Act
        pauseMenuManager.TogglePause();

        // Assert
        Assert.IsTrue(pauseMenuPanelObj.activeSelf, "Pause menu should be shown when paused");
        Assert.IsTrue(pauseMenuManager.GetIsPausedForTest(), "Game should be paused");
        Assert.AreEqual(0f, Time.timeScale, "Time scale should be 0 when paused");
    }

    [Test]
    public void TogglePause_HidesMenuAndResumesGame()
    {
        // Arrange
        pauseMenuManager.TogglePause(); // First toggle to pause
        Assert.IsTrue(pauseMenuManager.GetIsPausedForTest(), "Game should be paused initially");

        // Act
        pauseMenuManager.TogglePause(); // Second toggle to unpause

        // Assert
        Assert.IsFalse(pauseMenuPanelObj.activeSelf, "Pause menu should be hidden when unpaused");
        Assert.IsFalse(pauseMenuManager.GetIsPausedForTest(), "Game should be unpaused");
        Assert.AreEqual(1f, Time.timeScale, "Time scale should be 1 when unpaused");
    }

    [Test]
    public void ResumeGame_UnpausesGame()
    {
        // Arrange
        pauseMenuManager.TogglePause(); // Pause the game
        Assert.IsTrue(pauseMenuManager.GetIsPausedForTest(), "Game should be paused initially");

        // Act
        pauseMenuManager.ResumeGame();

        // Assert
        Assert.IsFalse(pauseMenuPanelObj.activeSelf, "Pause menu should be hidden");
        Assert.IsFalse(pauseMenuManager.GetIsPausedForTest(), "Game should be unpaused");
        Assert.AreEqual(1f, Time.timeScale, "Time scale should be 1");
    }

    [Test]
    public void OnVolumeChanged_UpdatesGameManagerVolume()
    {
        // Arrange
        float testVolume = 0.75f;
        AudioSource musicSource = gameManagerObj.GetComponent<AudioSource>();
        Assert.IsNotNull(musicSource, "Music source should exist");
        float initialVolume = musicSource.volume;

        // Act
        pauseMenuManager.OnVolumeChanged(testVolume);

        // Assert
        Assert.AreEqual(testVolume, musicSource.volume, "Volume should be updated");
        Assert.AreNotEqual(initialVolume, musicSource.volume, "Volume should be different from initial");
    }

    [Test]
    public void QuitGame_StopsMusic()
    {
        // Arrange
        bool musicStopped = false;

        // Set up the test action BEFORE calling QuitGame
        gameManager.TestStopMusicAction = () => {
            musicStopped = true;
        };

        // Act
        pauseMenuManager.QuitGame();

        // Assert
        Assert.IsTrue(musicStopped, "Music should be stopped when quitting");

        // Clean up the test action
        gameManager.TestStopMusicAction = null;
    }
}