using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Player_Controller : MonoBehaviour
{
    private bool isMovementLocked = false;

    private enum Directions
    {
        UP, DOWN, LEFT, RIGHT
    }

    [Header("Movement Attributes")]
    [SerializeField] private float _moveSpeed = 100f;

    [Header("Dependencies")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private AudioSource _audioSource;

    [Header("Pushable Object")]
    [SerializeField] private float pushStrength = 5f;
    [SerializeField] private Transform _objectToPush;

    [Header("Dance Floor Integration")]
    [SerializeField] private bool enableDanceFloorEffects = true;
    [SerializeField] private AudioClip tileWarningSound;
    [SerializeField] private AudioClip tileDangerSound;
    [SerializeField] private float nervousAnimationSpeed = 1.5f; // Animation speed when on warning tile

    private Vector2 _moveDir = Vector2.zero;
    private Directions _facingDirection = Directions.RIGHT;
    private DanceFloorTile _currentTile = null; // Reference to the tile player is standing on
    private float _defaultAnimationSpeed = 1f; // Store the default animation speed

    // Hashes for animator states
    private readonly int _animMoveRight = Animator.StringToHash("Anim_Player_Move_Right");
    private readonly int _animIdleRight = Animator.StringToHash("Anim_Player_Idle_Right");
    private readonly int _animMoveLeft = Animator.StringToHash("Anim_Player_Move_Left");
    private readonly int _animIdleLeft = Animator.StringToHash("Anim_Player_Idle_Left");
    private readonly int _animMoveUp = Animator.StringToHash("Anim_Player_Move_Up");
    private readonly int _animMoveDown = Animator.StringToHash("Anim_Player_Move_Down");
    private readonly int _animIdleUp = Animator.StringToHash("Anim_Player_Idle_Up");
    private readonly int _animIdleDown = Animator.StringToHash("Anim_Player_Idle_Down");

    private void Awake()
    {
        // Try to find components automatically if not manually assigned
        FindAndAssignComponents();
        ValidateDependencies();
    }

    // Method to automatically find and assign components
    private void FindAndAssignComponents()
    {
        // If Rigidbody2D is not assigned, try to find it on this object or its children
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
            {
                _rb = GetComponentInChildren<Rigidbody2D>();
            }
        }

        // If Animator is not assigned, look for it on this object or its children
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                // Try to find Animator on children
                _animator = GetComponentInChildren<Animator>();

                // If still not found, look specifically in the Character child
                if (_animator == null)
                {
                    Transform characterChild = transform.Find("Character");
                    if (characterChild != null)
                    {
                        _animator = characterChild.GetComponent<Animator>();
                    }
                }
            }
        }

        // If SpriteRenderer is not assigned, look for it on this object or its children
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

                // If still not found, look specifically in the Character child
                if (_spriteRenderer == null)
                {
                    Transform characterChild = transform.Find("Character");
                    if (characterChild != null)
                    {
                        _spriteRenderer = characterChild.GetComponent<SpriteRenderer>();
                    }
                }
            }
        }

        // If AudioSource is not assigned, try to find it
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }
    }

    // Public method to validate dependencies (useful for unit testing)
    public bool ValidateDependencies()
    {
        bool hasErrors = false;

        if (_rb == null)
        {
            Debug.LogError("Rigidbody2D is not assigned in Player_Controller!");
            hasErrors = true;
        }

        if (_animator == null)
        {
            Debug.LogError("Animator is not assigned in Player_Controller!");
            hasErrors = true;
        }

        if (_spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer is not assigned in Player_Controller!");
            hasErrors = true;
        }

        return !hasErrors;
    }

    // Public method to manually set dependencies for testing
    public void SetDependenciesForTesting(Rigidbody2D rb, Animator animator, SpriteRenderer spriteRenderer)
    {
        _rb = rb;
        _animator = animator;
        _spriteRenderer = spriteRenderer;
    }

    private void Start()
    {
        // Store default animation speed
        if (_animator != null)
        {
            _defaultAnimationSpeed = _animator.speed;
        }

        // Ensure gravity scale is set to 0 for pushable objects to prevent them from falling at the start
        if (_objectToPush != null && _objectToPush.GetComponent<Rigidbody2D>())
        {
            _objectToPush.GetComponent<Rigidbody2D>().gravityScale = 0; // Disable gravity for pushable objects
        }
    }

    private void Update()
    {
        if (!isMovementLocked)
        {
            GatherInput();
            CalculateFacingDirection();
            UpdateAnimation();
            TryPushObject();
        }
    }

    private void FixedUpdate()
    {
        if (!isMovementLocked)
        {
            MovementUpdate();
        }
    }

    private void GatherInput()
    {
        _moveDir.x = Input.GetAxisRaw("Horizontal"); // Left/Right movement
        _moveDir.y = Input.GetAxisRaw("Vertical"); // Up/Down movement
    }

    private void MovementUpdate()
    {
        if (_rb != null)
        {
            _rb.velocity = _moveDir.normalized * _moveSpeed * Time.fixedDeltaTime;
        }
    }

    private void CalculateFacingDirection()
    {
        // If moving right
        if (_moveDir.x > 0)
        {
            _facingDirection = Directions.RIGHT;
        }
        // If moving left
        else if (_moveDir.x < 0)
        {
            _facingDirection = Directions.LEFT;
        }
        // If moving up
        else if (_moveDir.y > 0)
        {
            _facingDirection = Directions.UP;
        }
        // If moving down
        else if (_moveDir.y < 0)
        {
            _facingDirection = Directions.DOWN;
        }
    }

    private void UpdateAnimation()
    {
        if (_spriteRenderer == null || _animator == null) return;

        // If player is moving
        if (_moveDir.sqrMagnitude > 0)
        {
            if (_facingDirection == Directions.RIGHT) // Right movement
            {
                _animator.CrossFade(_animMoveRight, 0);
            }
            else if (_facingDirection == Directions.LEFT) // Left movement
            {
                _animator.CrossFade(_animMoveLeft, 0);
            }
            else if (_facingDirection == Directions.UP) // Up movement
            {
                _animator.CrossFade(_animMoveUp, 0);
            }
            else if (_facingDirection == Directions.DOWN) // Down movement
            {
                _animator.CrossFade(_animMoveDown, 0);
            }
        }
        // If player is idle
        else
        {
            if (_facingDirection == Directions.RIGHT) // Idle Right
            {
                _animator.CrossFade(_animIdleRight, 0);
            }
            else if (_facingDirection == Directions.LEFT) // Idle Left
            {
                _animator.CrossFade(_animIdleLeft, 0);  // Use idle animation for right-facing, no flip needed
            }
            else if (_facingDirection == Directions.UP) // Idle Up
            {
                _animator.CrossFade(_animIdleUp, 0);
            }
            else if (_facingDirection == Directions.DOWN) // Idle Down
            {
                _animator.CrossFade(_animIdleDown, 0);
            }
        }
    }

    private void TryPushObject()
    {
        // Only push the object if it's set and in range
        if (_objectToPush != null)
        {
            Vector3 objectOffset = _objectToPush.position - transform.position; // Offset between player and object

            // If the object is close enough to the player, we attach it
            if (objectOffset.magnitude < 1f)
            {
                _objectToPush.position = transform.position + objectOffset; // Keep the relative offset intact
            }

            // Apply movement based on player's movement (drag the object)
            _objectToPush.position += (Vector3)_moveDir * pushStrength * Time.deltaTime; // Move object with player
        }
    }

    public void LockMovement(bool lockMovement)
    {
        isMovementLocked = lockMovement;
        if (lockMovement && _rb != null)
        {
            _rb.velocity = Vector2.zero;
        }
        Debug.Log($"LockMovement called. Movement Locked: {lockMovement}");
    }

    // Dance Floor integration methods

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we're on a dance floor tile
        if (enableDanceFloorEffects)
        {
            DanceFloorTile tile = other.GetComponent<DanceFloorTile>();
            if (tile != null)
            {
                _currentTile = tile;

                // Subscribe to the tile's state change event
                tile.OnTileStateChanged += OnTileStateChanged;

                // Check the current state of the tile
                CheckTileState(tile.GetCurrentState());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if we're leaving a dance floor tile
        if (enableDanceFloorEffects && _currentTile != null)
        {
            DanceFloorTile tile = other.GetComponent<DanceFloorTile>();
            if (tile != null && tile == _currentTile)
            {
                // Unsubscribe from the tile's state change event
                tile.OnTileStateChanged -= OnTileStateChanged;

                // Reset animation speed
                if (_animator != null)
                {
                    _animator.speed = _defaultAnimationSpeed;
                }

                _currentTile = null;
            }
        }
    }

    // Handle tile state changes
    private void OnTileStateChanged(DanceFloorTile.TileState newState)
    {
        CheckTileState(newState);
    }

    private void CheckTileState(DanceFloorTile.TileState state)
    {
        if (!enableDanceFloorEffects) return;

        switch (state)
        {
            case DanceFloorTile.TileState.Warning:
                // Player is on a warning tile - look nervous!
                if (_animator != null)
                {
                    _animator.speed = nervousAnimationSpeed;
                }

                // Play warning sound
                if (_audioSource != null && tileWarningSound != null)
                {
                    _audioSource.PlayOneShot(tileWarningSound);
                }
                break;

            case DanceFloorTile.TileState.Danger:
                // Player is on a danger tile!
                // Play danger sound
                if (_audioSource != null && tileDangerSound != null)
                {
                    _audioSource.PlayOneShot(tileDangerSound);
                }
                break;

            default:
                // Reset animation speed for other states
                if (_animator != null)
                {
                    _animator.speed = _defaultAnimationSpeed;
                }
                break;
        }
    }

    public bool IsMovementLocked()
    {
        return isMovementLocked;
    }

    // Added this method to get the player's facing direction
    public Vector2 GetFacingDirection()
    {
        Vector2 direction = Vector2.right; // Default facing right

        switch (_facingDirection)
        {
            case Directions.RIGHT:
                direction = Vector2.right;
                break;
            case Directions.LEFT:
                direction = Vector2.left;
                break;
            case Directions.UP:
                direction = Vector2.up;
                break;
            case Directions.DOWN:
                direction = Vector2.down;
                break;
        }

        return direction;
    }
}