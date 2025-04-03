using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

[TestFixture]
public class GameManagerTests
{
    private GameObject gameManagerObj;
    private GameManager gameManager;
    private AudioSource musicSource;

    [SetUp]
    public void Setup()
    {
        // Create necessary objects
        gameManagerObj = new GameObject("GameManager");

        // Add the GameManager component
        gameManager = gameManagerObj.AddComponent<GameManager>();

        // Setup for test mode
        gameManager.InitializeForTesting();

        // Get the audio source (created by GameManager)
        musicSource = gameManagerObj.GetComponent<AudioSource>();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up
        Object.DestroyImmediate(gameManagerObj);
    }

    [Test]
    public void SetupBackgroundMusic_InitializesAudioSource()
    {
        // Arrange & Act - Setup is done in SetUp

        // Assert
        Assert.IsNotNull(musicSource, "Audio source should be created");
        Assert.IsTrue(musicSource.loop, "Music should be set to loop");
        Assert.AreEqual(0.5f, musicSource.volume, "Default volume should be 0.5");
    }

    [Test]
    public void PauseAndResumeMusic_WorksCorrectly()
    {
        // Arrange
        AudioClip testClip = AudioClip.Create("TestClip", 1000, 1, 44100, false);
        gameManager.SetMusicClipForTest(testClip);
        gameManager.PlayMusic();

        // Act
        gameManager.PauseMusic();
        bool isPaused = !musicSource.isPlaying;

        gameManager.ResumeMusic();
        bool isPlaying = musicSource.isPlaying;

        // Assert
        Assert.IsTrue(isPaused, "Music should be paused");
        Assert.IsTrue(isPlaying, "Music should be playing after resume");
    }

    [Test]
    public void SetMusicVolume_ChangesVolume()
    {
        // Arrange
        float initialVolume = musicSource.volume;
        float newVolume = 0.75f;

        // Act
        gameManager.SetMusicVolume(newVolume);

        // Assert
        Assert.AreEqual(newVolume, musicSource.volume, "Volume should be updated");
        Assert.AreNotEqual(initialVolume, musicSource.volume, "Volume should be different from initial");
    }

    [Test]
    public void SetMusicVolume_ClampsToValidRange()
    {
        // Arrange & Act
        gameManager.SetMusicVolume(1.5f);
        float tooHighResult = musicSource.volume;

        gameManager.SetMusicVolume(-0.5f);
        float tooLowResult = musicSource.volume;

        // Assert
        Assert.AreEqual(1.0f, tooHighResult, "Volume should be clamped to 1.0 maximum");
        Assert.AreEqual(0.0f, tooLowResult, "Volume should be clamped to 0.0 minimum");
    }

    [Test]
    public void PlayGameOverMusic_SetsGameOverState()
    {
        // Arrange
        AudioClip gameOverClip = AudioClip.Create("GameOverClip", 1000, 1, 44100, false);
        gameManager.SetGameOverMusicForTest(gameOverClip);

        // Act
        gameManager.PlayGameOverMusic();

        // Assert
        Assert.IsTrue(gameManager.isGameOver, "Game over state should be set");
        Assert.IsFalse(musicSource.loop, "Game over music should not loop");
        Assert.AreEqual(gameOverClip, musicSource.clip, "Game over music clip should be set");
    }

    [Test]
    public void RestoreBackgroundMusic_ResetsGameOverState()
    {
        // Arrange
        AudioClip bgClip = AudioClip.Create("BGClip", 1000, 1, 44100, false);
        gameManager.SetMusicClipForTest(bgClip);
        gameManager.PlayGameOverMusic();

        // Act
        gameManager.RestoreBackgroundMusic();

        // Assert
        Assert.IsFalse(gameManager.isGameOver, "Game over state should be reset");
        Assert.IsTrue(musicSource.loop, "Background music should loop");
        Assert.AreEqual(bgClip, musicSource.clip, "Background music clip should be restored");
    }

    [Test]
    public void CheckAllSlotsFilled_DoesNothingWhenGameIsOver()
    {
        // Arrange
        gameManager.isGameOver = true;
        bool invokeWasCalled = false;

        // Replace the Invoke method for testing
        gameManager.InvokeAction = (methodName, time) => {
            invokeWasCalled = true;
        };

        // Act
        gameManager.CheckAllSlotsFilled();

        // Assert
        Assert.IsFalse(invokeWasCalled, "Invoke should not be called when game is over");
    }
}