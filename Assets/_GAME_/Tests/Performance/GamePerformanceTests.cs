using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GamePerformanceTests
{
    [UnityTest, Performance]
    public IEnumerator TestSceneLoadTime()
    {
        // Measure scene loading performance
        yield return Measure.Method(() =>
        {
            SceneManager.LoadScene("Balance2"); // Replace with your actual scene name
        })
        .WarmupCount(3)
        .MeasurementCount(10)
        .IterationsPerMeasurement(5)
        .Run();
    }

    [UnityTest, Performance]
    public IEnumerator TestPlayerMovementPerformance()
    {
        // Load the scene
        yield return SceneManager.LoadSceneAsync("Balance2");

        // Find the player controller
        var playerController = Object.FindObjectOfType<Player_Controller>();
        Assert.IsNotNull(playerController, "Player Controller not found in scene");

        yield return Measure.Method(() =>
        {
            // Simulate player movement for performance testing
            playerController.SetDependenciesForTesting(
                playerController.GetComponent<Rigidbody2D>(),
                playerController.GetComponent<Animator>(),
                playerController.GetComponent<SpriteRenderer>()
            );
        })
        .WarmupCount(5)
        .MeasurementCount(20)
        .IterationsPerMeasurement(10)
        .Run();
    }

    [UnityTest, Performance]
    public IEnumerator TestEnemyArcherPerformance()
    {
        // Load the scene
        yield return SceneManager.LoadSceneAsync("Balance2");

        // Find an enemy archer
        var enemyArcher = Object.FindObjectOfType<Enemy_Archer>();
        Assert.IsNotNull(enemyArcher, "Enemy Archer not found in scene");

        yield return Measure.Method(() =>
        {
            // Test arrow shooting performance
            enemyArcher.ShootArrow();
        })
        .WarmupCount(3)
        .MeasurementCount(15)
        .IterationsPerMeasurement(5)
        .Run();
    }
}