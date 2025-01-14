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

    private readonly int _animMoveRight = Animator.StringToHash("Anim_Player_Move_Right");
    private readonly int _animIdleRight = Animator.StringToHash("Anim_Player_Idle_Right");

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
        _moveDir.x = Input.GetAxisRaw("Horizontal");
        _moveDir.y = Input.GetAxisRaw("Vertical");
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
        if (_moveDir.x > 0) _facingDirection = Directions.RIGHT;
        else if (_moveDir.x < 0) _facingDirection = Directions.LEFT;
    }

    private void UpdateAnimation()
    {
        if (_spriteRenderer == null || _animator == null) return;

        _spriteRenderer.flipX = _facingDirection == Directions.LEFT;

        if (_moveDir.sqrMagnitude > 0)
        {
            _animator.CrossFade(_animMoveRight, 0);
        }
        else
        {
            _animator.CrossFade(_animIdleRight, 0);
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
