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

    [Header("Pushable Object")]
    [SerializeField] private float pushStrength = 5f; // Adjusted push strength for controlled movement
    [SerializeField] private Transform _objectToPush; // Reference to the object to be pushed

    private Vector2 _moveDir = Vector2.zero;
    private Directions _facingDirection = Directions.RIGHT;

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
        if (_rb == null) Debug.LogError("Rigidbody2D is not assigned in Player_Controller!");
        if (_animator == null) Debug.LogError("Animator is not assigned in Player_Controller!");
        if (_spriteRenderer == null) Debug.LogError("SpriteRenderer is not assigned in Player_Controller!");
    }

    private void Start()
    {
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
}
