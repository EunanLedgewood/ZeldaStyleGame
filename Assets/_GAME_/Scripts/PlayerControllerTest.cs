using UnityEngine;

// This class is ONLY used in tests - completely separate from the actual Player_Controller
public class Player_ControllerTest : MonoBehaviour
{
    public bool skipValidation = false;
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

    [Header("Pushable Object")]
    [SerializeField] private float pushStrength = 5f;
    [SerializeField] private Transform _objectToPush;

    private Vector2 _moveDir = Vector2.zero;
    private Directions _facingDirection = Directions.RIGHT;

    // Delegate to allow method injection for testing
    public System.Action<bool> OnLockMovement;

    // Hashes for animator states
    private readonly int _animMoveRight = Animator.StringToHash("Anim_Player_Move_Right");
    private readonly int _animIdleRight = Animator.StringToHash("Anim_Player_Idle_Right");
    private readonly int _animMoveLeft = Animator.StringToHash("Anim_Player_Move_Left");
    private readonly int _animIdleLeft = Animator.StringToHash("Anim_Player_Idle_Left");
    private readonly int _animMoveUp = Animator.StringToHash("Anim_Player_Move_Up");
    private readonly int _animMoveDown = Animator.StringToHash("Anim_Player_Move_Down");
    private readonly int _animIdleUp = Animator.StringToHash("Anim_Player_Idle_Up");
    private readonly int _animIdleDown = Animator.StringToHash("Anim_Player_Idle_Down");

    // For testing - allows calling Start manually
    public void TestStart()
    {
        Start();
    }

    // For testing - set pushable object
    public void SetPushableObjectForTesting(Transform pushable)
    {
        _objectToPush = pushable;
    }

    private void Awake()
    {
        // Skip everything if skipping validation
        if (skipValidation)
        {
            return;
        }

        // Try to find components automatically if not manually assigned
        FindAndAssignComponents();
        ValidateDependencies();

        // Default to internal method if no delegate is set
        OnLockMovement = InternalLockMovement;
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
    }

    // Public method to validate dependencies (useful for unit testing)
    public bool ValidateDependencies()
    {
        // Skip validation if needed
        if (skipValidation)
        {
            return true;
        }

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

        // Automatically skip validation when this method is called
        skipValidation = true;
    }

    private void Start()
    {
        // Ensure gravity scale is set to 0 for pushable objects to prevent them from falling at the start
        if (_objectToPush != null && _objectToPush.GetComponent<Rigidbody2D>())
        {
            _objectToPush.GetComponent<Rigidbody2D>().gravityScale = 0; // Disable gravity for pushable objects
        }
    }

    // This is a test class - we don't need Update/FixedUpdate as we'll test methods directly

    // Public method that uses the delegate
    public void LockMovement(bool lockMovement)
    {
        OnLockMovement?.Invoke(lockMovement);
    }

    // Internal method for locking movement
    public void InternalLockMovement(bool lockMovement)
    {
        isMovementLocked = lockMovement;
        if (lockMovement && _rb != null)
        {
            _rb.velocity = Vector2.zero;
        }
        Debug.Log($"LockMovement called. Movement Locked: {lockMovement}");
    }
}