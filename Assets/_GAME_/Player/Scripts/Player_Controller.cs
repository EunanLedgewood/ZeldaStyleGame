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

    private Vector2 _moveDir = Vector2.zero;
    private Directions _facingDirection = Directions.RIGHT;

    // Hashes for animator states
    private readonly int _animMoveRight = Animator.StringToHash("Anim_Player_Move_Right");
    private readonly int _animIdleRight = Animator.StringToHash("Anim_Player_Idle_Right");
    private readonly int _animMoveUp = Animator.StringToHash("Anim_Player_Move_Up");
    private readonly int _animMoveDown = Animator.StringToHash("Anim_Player_Move_Down");
    private readonly int _animIdleUp = Animator.StringToHash("Anim_Player_Idle_Up"); // If you have an idle up animation
    private readonly int _animIdleDown = Animator.StringToHash("Anim_Player_Idle_Down"); // If you have an idle down animation

    private void Awake()
    {
        if (_rb == null) Debug.LogError("Rigidbody2D is not assigned in Player_Controller!");
        if (_animator == null) Debug.LogError("Animator is not assigned in Player_Controller!");
        if (_spriteRenderer == null) Debug.LogError("SpriteRenderer is not assigned in Player_Controller!");
    }

    private void Update()
    {
        if (!isMovementLocked)
        {
            GatherInput();
            CalculateFacingDirection();
            UpdateAnimation();
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
        // Update facing direction based on movement
        if (_moveDir.x > 0)
        {
            _facingDirection = Directions.RIGHT;
        }
        else if (_moveDir.x < 0)
        {
            _facingDirection = Directions.LEFT;
        }

        // If the movement is vertical, the facing direction is either up or down
        if (_moveDir.y > 0)
        {
            _facingDirection = Directions.UP;
        }
        else if (_moveDir.y < 0)
        {
            _facingDirection = Directions.DOWN;
        }
    }

    private void UpdateAnimation()
    {
        if (_spriteRenderer == null || _animator == null) return;

        // Flip sprite for left direction (already works based on the X-axis)
        _spriteRenderer.flipX = _facingDirection == Directions.LEFT;

        // Handle movement animations
        if (_moveDir.sqrMagnitude > 0)
        {
            // Horizontal Movement (Right)
            if (_moveDir.x > 0)
            {
                _animator.CrossFade(_animMoveRight, 0);
            }
            // Vertical Movement (Up)
            else if (_moveDir.y > 0)
            {
                _animator.CrossFade(_animMoveUp, 0);
            }
            // Vertical Movement (Down)
            else if (_moveDir.y < 0)
            {
                _animator.CrossFade(_animMoveDown, 0);
            }
        }
        else
        {
            // Idle animations
            if (_facingDirection == Directions.RIGHT)
            {
                _animator.CrossFade(_animIdleRight, 0);
            }
            else if (_facingDirection == Directions.UP)
            {
                _animator.CrossFade(_animIdleUp, 0);
            }
            else if (_facingDirection == Directions.DOWN)
            {
                _animator.CrossFade(_animIdleDown, 0);
            }
            else // Default Idle (Left-facing idle if needed)
            {
                _animator.CrossFade(_animIdleRight, 0); // Adjust to idle state for left if needed
            }
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
