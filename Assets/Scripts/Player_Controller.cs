using UnityEngine;

[SelectionBase]
public class Player_Controller : MonoBehaviour
{
    #region Editor Data
    [Header("Movement Attributes")]
    [SerializeField] float _moveSpeed = 50f;

    [Header("Dependencies")]
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] Animator _animator;
    [SerializeField] SpriteRenderer _spriteRenderer;
    #endregion

    #region Internal Data
    private Vector2 _moveDir = Vector2.zero;
    private Vector2 _lastMoveDir = Vector2.right; // Track last movement direction
    private PlayerAttack _playerAttack;

    // Animation parameter hashes for performance
    private readonly int _animSpeedX = Animator.StringToHash("SpeedX");
    private readonly int _animSpeedY = Animator.StringToHash("SpeedY");
    private readonly int _animSpeed = Animator.StringToHash("Speed");
    private readonly int _animAttackTrigger = Animator.StringToHash("Attack");
    private readonly int _animAttackDirectionX = Animator.StringToHash("AttackDirectionX");
    private readonly int _animAttackDirectionY = Animator.StringToHash("AttackDirectionY");
    #endregion

    #region Properties
    public Vector2 LastMoveDirection => _lastMoveDir;
    public bool IsMoving => _moveDir.sqrMagnitude > 0.01f;
    public Vector2 MoveDirection => _moveDir;
    #endregion

    #region Initialization
    void Start()
    {
        // Get attack component
        _playerAttack = GetComponent<PlayerAttack>();

        // Auto-assign components if not set
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Validate components
        if (_animator == null)
        {
            Debug.LogError("No Animator found! Please assign manually or add to child object.");
        }
    }
    #endregion

    #region Tick
    private void Update()
    {
        GatherInput();
        UpdateLastMoveDirection();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        MovementUpdate();
    }
    #endregion

    #region Input Logic
    private void GatherInput()
    {
        _moveDir.x = Input.GetAxisRaw("Horizontal");
        _moveDir.y = Input.GetAxisRaw("Vertical");
    }
    #endregion

    #region Movement Logic
    private void MovementUpdate()
    {
        // Calculate current speed (reduced if attacking)
        float currentSpeed = _moveSpeed;
        if (_playerAttack != null)
        {
            currentSpeed *= _playerAttack.GetMovementMultiplier();
        }

        // Apply movement
        _rb.linearVelocity = _moveDir.normalized * currentSpeed * Time.fixedDeltaTime;
    }

    private void UpdateLastMoveDirection()
    {
        // Only update last direction when actually moving
        if (IsMoving)
        {
            _lastMoveDir = _moveDir.normalized;
        }
    }
    #endregion

    #region Animation Logic
    private void UpdateAnimations()
    {
        if (_animator == null) return;

        // Set movement parameters for blend trees
        _animator.SetFloat(_animSpeedX, _moveDir.x);
        _animator.SetFloat(_animSpeedY, _moveDir.y);
        _animator.SetFloat(_animSpeed, _moveDir.magnitude);

        // Handle sprite flipping - completely disabled during attacks
        // The PlayerAttack script sets the facing direction and we maintain it
        if (_playerAttack == null || !_playerAttack.IsCurrentlyAttacking())
        {
            UpdateSpriteFlipping();
        }
        // During attack: No sprite flipping allowed - maintain attack direction
    }

    private void UpdateSpriteFlipping()
    {
        if (_spriteRenderer == null) return;

        // Use current movement direction, or last direction if not moving
        Vector2 directionForFlipping = IsMoving ? _moveDir : _lastMoveDir;

        if (Mathf.Abs(directionForFlipping.x) > 0.1f)
        {
            _spriteRenderer.flipX = directionForFlipping.x < 0;
        }
    }

    public void TriggerAttackAnimation(Vector2 attackDirection)
    {
        if (_animator == null) return;

        // Set attack direction parameters
        _animator.SetFloat(_animAttackDirectionX, attackDirection.x);
        _animator.SetFloat(_animAttackDirectionY, attackDirection.y);

        // Trigger attack animation
        _animator.SetTrigger(_animAttackTrigger);

        // Note: Sprite flipping is handled by PlayerAttack script
        // and UpdateSpriteFlipping is disabled during attacks
    }
    #endregion
}