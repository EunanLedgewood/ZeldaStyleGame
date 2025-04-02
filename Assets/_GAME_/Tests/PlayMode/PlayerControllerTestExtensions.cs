using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

// This static class provides extensions for testing Player_Controller
public static class PlayerControllerTestExtensions
{
    // Creates a test Player_Controller with validation errors suppressed
    public static Player_Controller AddTestPlayerController(this GameObject gameObject)
    {
        // Set up log assertion to ignore the expected errors
        LogAssert.ignoreFailingMessages = true;

        // Add the component (this will call Awake which will log errors, but we're ignoring them)
        var controller = gameObject.AddComponent<Player_Controller>();

        // Restore normal log behavior
        LogAssert.ignoreFailingMessages = false;

        return controller;
    }
}