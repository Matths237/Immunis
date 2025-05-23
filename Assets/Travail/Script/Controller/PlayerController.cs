using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("HEALTH")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private float damageFlashDuration = 0.1f;
    [SerializeField] private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer;
    private Vector3 respawnPoint;
    private bool isDead = false;

    [Header("MOVE")]
    [SerializeField] private float _speedMove = 8;
    [SerializeField] private float _runSpeedMultiplier = 1.5f;
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;
    [SerializeField] private float momentum;

    [Header("JUMP")]
    [SerializeField] private float _minForceJump = 12;
    [SerializeField] private float _maxForceJump = 14;
    [SerializeField] private float _jumpHoldTime = 0.2f;
    [SerializeField] private int _limitJump = 1;

    private int _currentJump;
    private bool _hasJumpedSinceGrounded = false;
    private float _jumpStartTime;
    private bool _isJumping;

    [Header("WALL")]
    [SerializeField] private LayerMask _detectWall;
    [SerializeField] private float distanceDW = 0f;
    [SerializeField] private float _minWallJumpHorizontalForce = 5f;
    [SerializeField] private float _maxWallJumpHorizontalForce = 5f;
    [SerializeField] private float _minWallJumpVerticalForce = 4f;
    [SerializeField] private float _maxWallJumpVerticalForce = 20f;
    [SerializeField] private float _wallSlideSpeed = 4f;
    [SerializeField] private float _wallStickTime = 0.1f;
    [SerializeField] private bool _infiniteWallJumps = true;
    [SerializeField] private int _maxWallJumps = 3;
    private int _currentWallJumps;
    private bool _isWallSliding;
    private float _wallDirection;
    private float _timeSinceLastWallJump;
    private float _timeToStopStick;
    private bool _isGrounded;
    private bool _isWallJumping;

    [Header("DASH")]
    [SerializeField] private float _dashSpeed = 17f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1f;
    [SerializeField] private KeyCode _dashKey = KeyCode.LeftControl;
    [SerializeField] private float _dashEndMomentumMultiplier = 0.4f;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private bool _isDashing;
    private Vector2 _dashDirection;

    private Rigidbody2D _myRgbd2D;
    private SpriteRenderer _spriteRend;
    private Collider2D _collider;
    private bool _isSprinting;

    [Header("EFFECTS")]
    [SerializeField] private CameraController cameraController;

    [Header("COLOR")]
    [SerializeField] private Color _sprintColor = Color.red;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _dashColor = Color.blue;
    [SerializeField] private Color damageColor = Color.magenta;
    [SerializeField] private Color deathColor = Color.black;

    void Awake()
    {
        _myRgbd2D = GetComponent<Rigidbody2D>();
        _spriteRend = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        currentHealth = maxHealth;
        respawnPoint = transform.position;
        _spriteRend.color = _normalColor;
    }

    void Update()
    {
        Move();
        WallSlide();
        HandleJumpInput();
        HandleDashInput();

        if (_timeSinceLastWallJump > 0)
        {
            _timeSinceLastWallJump -= Time.deltaTime;
        }

        if (_timeToStopStick > 0)
        {
            _timeToStopStick -= Time.deltaTime;
        }

        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }

        if (_isDashing)
        {
            _spriteRend.color = _dashColor;
        } else {
            UpdateColor(); 
        }
    }

    void FixedUpdate()
    {
        if (_myRgbd2D.linearVelocity.y < 0)
        {
            _myRgbd2D.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime; //fixeddeltatime
        }
        else if (_myRgbd2D.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            _myRgbd2D.linearVelocity += Vector2.up * Physics2D.gravity.y * (2f - 1) * Time.deltaTime;
        }

        if (_isDashing)
        {
            _myRgbd2D.linearVelocity = _dashDirection * _dashSpeed;
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0)
            {
                EndDash();
            }
        }
    }


    void UpdateColor()
    {
        if (_isDashing)
        {
            _spriteRend.color = _dashColor;
        }
        else if (_isSprinting)
        {
             _spriteRend.color = _sprintColor;
        }
        else
        {
             _spriteRend.color = _normalColor;
        }
    }

    void Move()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        _isSprinting = Input.GetKey(_runKey);
        float targetSpeed = moveInput * _speedMove * (_isSprinting ? _runSpeedMultiplier : 1);
        float currentSpeed = _myRgbd2D.linearVelocity.x;
        float accelerationRate = Mathf.Abs(moveInput) > 0.01f ? momentum : momentum;
        float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.deltaTime);

        if (moveInput != 0)
            _spriteRend.flipX = moveInput < 0;

        if (_timeSinceLastWallJump > 0)
        {
            newSpeed *= 0.7f;
        }

        if (!_isDashing)
        {
            _myRgbd2D.linearVelocity = new Vector2(newSpeed, _myRgbd2D.linearVelocity.y);
        }
    }

    void StartJump()
    {
        _jumpStartTime = Time.time;
        _isJumping = true;
        _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, _minForceJump);
        _currentJump++;
    }

    void ContinueJump()
    {
        if (Time.time - _jumpStartTime < _jumpHoldTime)
        {
            float jumpForce = Mathf.Lerp(_minForceJump, _maxForceJump, (Time.time - _jumpStartTime) / _jumpHoldTime);
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, jumpForce);
        }
        else
        {
            EndJump();
        }
    }

    void EndJump()
    {
        _isJumping = false;
        if (_myRgbd2D.linearVelocity.y > 0)
        {
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, _myRgbd2D.linearVelocity.y * 0.5f);
        }
    }

    void WallSlide()
    {
        _isWallSliding = false;

        Vector2 rightBoxPos = new Vector2(_spriteRend.bounds.max.x + distanceDW / 2, _spriteRend.bounds.center.y);
        Vector2 leftBoxPos = new Vector2(_spriteRend.bounds.min.x - distanceDW / 2, _spriteRend.bounds.center.y);
        Vector2 boxSize = new Vector2(distanceDW, _spriteRend.bounds.size.y * 0.9f);

        Collider2D rightWall = Physics2D.OverlapBox(rightBoxPos, boxSize, 0, _detectWall);
        Collider2D leftWall = Physics2D.OverlapBox(leftBoxPos, boxSize, 0, _detectWall);

        if (rightWall != null)
        {
            _isWallSliding = true;
            if (rightWall.tag != "Platform_eau")
                _wallDirection = -1;
            else {
                _wallDirection = 0;
            }
        }
        else if (leftWall != null)
        {
            _isWallSliding = true;
            if (leftWall.tag != "Platform_eau")
                _wallDirection = 1;
            else {
                _wallDirection = 0;
            }
        }

        if (_isWallSliding)
        {
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, Mathf.Clamp(_myRgbd2D.linearVelocity.y, -_wallSlideSpeed, float.MaxValue));
            _currentJump = 0; 
            _currentWallJumps = 0; 
        }
    }

    void StartWallJump()
    {
        _timeSinceLastWallJump = 0.2f;
        _timeToStopStick = _wallStickTime;
        _currentWallJumps++;

        _jumpStartTime = Time.time;
        _isWallJumping = true;

        float horizontalForce = _wallDirection * _minWallJumpHorizontalForce;
        float verticalForce = _minWallJumpVerticalForce;
        _myRgbd2D.linearVelocity = new Vector2(horizontalForce, verticalForce);
        _currentJump++;
        _hasJumpedSinceGrounded = true;
        EndJump();
    }

    void ContinueWallJump()
    {
        if (Time.time - _jumpStartTime < _jumpHoldTime)
        {
            float horizontalForce = _wallDirection * Mathf.Lerp(_minWallJumpHorizontalForce, _maxWallJumpHorizontalForce, (Time.time - _jumpStartTime) / _jumpHoldTime);
            float verticalForce = Mathf.Lerp(_minWallJumpVerticalForce, _maxWallJumpVerticalForce, (Time.time - _jumpStartTime) / _jumpHoldTime);

            _myRgbd2D.linearVelocity = new Vector2(horizontalForce, verticalForce);
        }
        else
        {
            EndWallJump();
        }
    }

    void EndWallJump()
    {
        _isWallJumping = false;
        if (_myRgbd2D.linearVelocity.y > 0)
        {
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, _myRgbd2D.linearVelocity.y * 0.5f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _isGrounded = true;
            _currentJump = 0;
            _currentWallJumps = 0;
            _hasJumpedSinceGrounded = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _isGrounded = false;
        }
    }

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_isWallSliding && (_infiniteWallJumps || _currentWallJumps < _maxWallJumps))
            {
                if (_timeToStopStick <= 0)
                {
                    StartWallJump();
                }
            }
            else if (_currentJump < _limitJump)
            {
                StartJump();
                _hasJumpedSinceGrounded = true;
            }
        }

        if (Input.GetKey(KeyCode.Space) && _isJumping)
        {
            ContinueJump();
        }

        if (Input.GetKey(KeyCode.Space) && _isWallJumping)
        {
            ContinueWallJump();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            EndJump();
            EndWallJump();
        }
    }

    void HandleDashInput()
    {
        if (Input.GetKeyDown(_dashKey) && _dashCooldownTimer <= 0 && !_isDashing)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            if (horizontalInput == 0 && verticalInput == 0)
            {
                _dashDirection = _spriteRend.flipX ? Vector2.left : Vector2.right;
            }
            else
            {
                _dashDirection = new Vector2(horizontalInput, verticalInput).normalized;
            }

            StartDash(_dashDirection);
        }
    }

    void StartDash(Vector2 direction)
    {
        _isDashing = true;
        _dashTimer = _dashDuration;
        _dashDirection = direction;
        _dashCooldownTimer = _dashCooldown;
        _spriteRend.color = _dashColor;

        if (cameraController != null)
        {
            cameraController.TriggerShake();
        }
    }

    void EndDash()
    {
        _isDashing = false;
        _myRgbd2D.linearVelocity = _dashDirection * _dashSpeed * _dashEndMomentumMultiplier;
        UpdateColor(); 
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        StartCoroutine(DamageFlashCoroutine());
        StartCoroutine(InvincibilityCoroutine()); 

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

     private IEnumerator DamageFlashCoroutine()
    {
        Color originalColor = _spriteRend.color;
        _spriteRend.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration);

        if (!isDead) 
        {
            UpdateColor();
        }
    }

     private IEnumerator InvincibilityCoroutine()
     {
         yield return new WaitForSeconds(invincibilityDuration);
         isInvincible = false;
         if (!isDead)
         {
             UpdateColor(); 
         }
     }

    public int CurrentHealth
    {
        get { return currentHealth; }
        private set { currentHealth = Mathf.Clamp(value, 0, maxHealth); }
    }

    public Vector3 RespawnPoint
    {
        get { return respawnPoint; }
        set { respawnPoint = value; }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        _myRgbd2D.linearVelocity = Vector2.zero;
        _myRgbd2D.simulated = false;
        _collider.enabled = false;
        _isDashing = false;
        _isJumping = false;
        _isWallJumping = false;
        _isWallSliding = false;

        _spriteRend.color = deathColor;

        StopAllCoroutines();

        Invoke(nameof(Respawn), respawnDelay);
    }

    void Respawn()
    {
        transform.position = respawnPoint;
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;
        invincibilityTimer = 0;
        _myRgbd2D.simulated = true;
        _collider.enabled = true;
        _myRgbd2D.linearVelocity = Vector2.zero;
        _currentJump = 0;
        _currentWallJumps = 0;
        _hasJumpedSinceGrounded = false;
        _isGrounded = false;
        _spriteRend.color = _normalColor;
        _dashCooldownTimer = 0;
        transform.SetParent(null);
        _myRgbd2D.position = respawnPoint;
    }
}