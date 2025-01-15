using UnityEngine;

public class Slot : MonoBehaviour
{
    public Sprite slotSprite; // Assign this sprite in the Inspector

    // Check if the square's sprite matches the slot's sprite
    public bool IsCorrectSprite(Sprite squareSprite)
    {
        return squareSprite == slotSprite;
    }
}
