using UnityEngine;

public class EmergencyFix : MonoBehaviour
{
    void Start()
    {
        // Fix time scale if it's zero
        if (Time.timeScale == 0)
        {
            Debug.LogError("TIME SCALE WAS ZERO! Setting to 1");
            Time.timeScale = 1.0f;
        }

        // Reset any test flags
        if (typeof(Player_Controller).GetField("IsTestMode",
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static) != null)
        {
            //Debug.Log("Resetting Player_Controller.IsTestMode");
            //Player_Controller.IsTestMode = false;
        }

        // Find and check the player controller
        var player = GameObject.FindObjectOfType<Player_Controller>();
        if (player != null)
        {
            Debug.Log("Found Player_Controller: " + player.name);

            // Check for Rigidbody2D
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log($"Player Rigidbody2D: bodyType={rb.bodyType}, simulated={rb.simulated}");

                // Ensure physics simulation is enabled
                rb.simulated = true;

                // Make sure it's not kinematic
                if (rb.bodyType == RigidbodyType2D.Kinematic)
                {
                    Debug.LogError("Player Rigidbody was Kinematic! Setting to Dynamic");
                    rb.bodyType = RigidbodyType2D.Dynamic;
                }
            }
            else
            {
                Debug.LogError("PLAYER LACKS RIGIDBODY2D!");
            }
        }

        // Check LevelRevealManager
        var levelReveal = GameObject.FindObjectOfType<LevelRevealManager>();
        if (levelReveal != null)
        {
            Debug.Log("Found LevelRevealManager");
        }
    }
}