using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("HEALTH")]
    [SerializeField] private int maxHealth;
    [SerializeField] private float invincibilityDuration;
    [SerializeField] private float respawnDelay;
    [SerializeField] private float damageFlashDuration;
    [SerializeField] private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer;
    private Vector3 respawnPoint;
    private bool isDead = false;

    [Header("MOVE")]
    [SerializeField] private float _speedMove;
    [SerializeField] private float _speedDive;
    [SerializeField] private float momentum;

    [Header("JUMP")]
    [SerializeField] private float _minForceJump;
    [SerializeField] private float _maxForceJump;
    [SerializeField] private float _jumpHoldTime;
    private bool _hasJumpedSinceGrounded = false;
    private float _jumpStartTime;
    private bool _isJumping;

    [Header("WALL")]
    [SerializeField] private float WallJumpHorizontalForce;
    [SerializeField] private float WallJumpVerticalForce;
    [SerializeField] private float _wallSlideSpeed;
    private LayerMask _detectWall;
    private int _currentWallJumps;
    private bool _isWallSliding;
    private float _wallDirection;
    private float _timeToStopStick;
    private bool _isGrounded;
    private bool _isWallJumping;

    [Header("DASH")]
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _dashCooldown;
    [SerializeField] private KeyCode _dashKey = KeyCode.LeftControl;
    [SerializeField] private float _dashEndMomentumMultiplier;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private bool _isDashing;
    private Vector2 _dashDirection;
    private bool _canDash;
    private Rigidbody2D _myRgbd2D;
    private SpriteRenderer _spriteRend;
    private Collider2D _collider;

    [Header("PUNCH")]
    [SerializeField] private KeyCode _punchKey = KeyCode.F;
    [SerializeField] private float _punchRange = 1.5f;
    [SerializeField] private float _punchCooldown = 1f;
    [SerializeField] private string _destructibleTag = "Destructible";
    [SerializeField] private Transform _punchOrigin;
    [SerializeField] private LayerMask _punchableLayers;
    [SerializeField] private float _coneAngle = 90f;
    [SerializeField] private int _numberOfRaysInCone = 5;
    [SerializeField] private float _punchYOffset = 0f;
    [SerializeField] private float _punchActivationDelay = 0.5f;
    private float _lastPunchTime;
    private bool _isPunchCurrentlyActive = false;

    [Header("Sound")]
    public AudioSource playerAudioSource;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip punchSound;
    public AudioClip ouchSound;


    [Header("Other")]
    [SerializeField] private Animator _animator;
    [SerializeField] private bool infiniteDeath = false;
    private bool isSpawnLocked = false;
    private Shake cameraController;
    public GameObject deathpanel;




    void Awake()
    {
        playerAudioSource = GetComponent<AudioSource>();
        _myRgbd2D = GetComponent<Rigidbody2D>();
        _spriteRend = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        currentHealth = maxHealth;
        respawnPoint = transform.position;
        _detectWall = LayerMask.GetMask("Ground");
        _lastPunchTime = -_punchCooldown;
    }


    void Start()
    {
        deathpanel.SetActive(false);
        cameraController = Camera.main?.GetComponent<Shake>();
        StartCoroutine(SpawnLockCoroutine());
    }

    IEnumerator SpawnLockCoroutine()
    {
        isSpawnLocked = true;
        yield return new WaitForSeconds(0.7f);
        isSpawnLocked = false;
    }

    void Update()
    {
        if (isDead || isSpawnLocked) return;

        Move();
        WallSlide();
        HandleJumpInput();
        HandleDashInput();
        HandlePunchInput();

        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (_myRgbd2D.linearVelocity.y < 0)
        {
            _myRgbd2D.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        }
        else if (_myRgbd2D.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            _myRgbd2D.linearVelocity += Vector2.up * Physics2D.gravity.y * (2f - 1) * Time.deltaTime;
        }

        if (!_isGrounded && Input.GetKey(KeyCode.S))
        {
            _myRgbd2D.linearVelocity += Vector2.down * _speedDive * Time.deltaTime;
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

    void Move()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        float targetSpeed = moveInput * _speedMove;
        float currentSpeed = _myRgbd2D.linearVelocity.x;
        float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, momentum * Time.deltaTime);
        if (moveInput != 0)
        {
            _animator.SetBool("isRunning", true);
            _spriteRend.flipX = moveInput > 0;
        }
        else
        {
            _animator.SetBool("isRunning", false);
        }

        if (!_isDashing)
        {
            _myRgbd2D.linearVelocity = new Vector2(newSpeed, _myRgbd2D.linearVelocity.y);
        }
    }

    void StartJump()
    {
        _animator.SetBool("isJumping", true);
        _animator.SetBool("isFalling", false);
        _animator.SetBool("isGrounded", false);
        PlaySound(jumpSound);
        _jumpStartTime = Time.time;
        _isJumping = true;

        _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, 0);
    }

    void ContinueJump()
    {
        if (Time.time - _jumpStartTime < _jumpHoldTime)
        {

            float jumpForce = Mathf.Lerp(0, _maxForceJump, (Time.time - _jumpStartTime) / _jumpHoldTime);
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, jumpForce);

            if (_myRgbd2D.linearVelocity.y > 0)
            {
                _animator.SetBool("isJumping", true);
                _animator.SetBool("isFalling", false);
            }
            else
            {
                _animator.SetBool("isJumping", false);
                _animator.SetBool("isFalling", true);
            }
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

        _animator.SetBool("isJumping", false);
        _animator.SetBool("isFalling", true);
        _animator.SetBool("isGrounded", true);
    }

    void WallSlide()
    {
        _isWallSliding = false;

        Vector2 rightBoxPos = new Vector2(_spriteRend.bounds.max.x, _spriteRend.bounds.center.y);
        Vector2 leftBoxPos = new Vector2(_spriteRend.bounds.min.x, _spriteRend.bounds.center.y);
        Vector2 boxSize = new Vector2(0, _spriteRend.bounds.size.y * 0.9f);

        Collider2D rightWall = Physics2D.OverlapBox(rightBoxPos, boxSize, 0, _detectWall);
        Collider2D leftWall = Physics2D.OverlapBox(leftBoxPos, boxSize, 0, _detectWall);

        if (rightWall != null)
        {
            _isWallSliding = true;
            _wallDirection = -1;
        }
        else if (leftWall != null)
        {
            _isWallSliding = true;
            _wallDirection = 1;
        }

        if (_isWallSliding)
        {
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, Mathf.Clamp(_myRgbd2D.linearVelocity.y, -_wallSlideSpeed, float.MaxValue));
            _currentWallJumps = 0;
        }
    }

    void StartWallJump()
    {
        _currentWallJumps++;
        _jumpStartTime = Time.time;
        _isWallJumping = true;

        float horizontalForce = _wallDirection * 1;
        float verticalForce = 1;
        _myRgbd2D.linearVelocity = new Vector2(horizontalForce, verticalForce);
        _hasJumpedSinceGrounded = true;
        EndJump();
    }

    void ContinueWallJump()
    {
        if (Time.time - _jumpStartTime < _jumpHoldTime)
        {
            float horizontalForce = _wallDirection * Mathf.Lerp(1, WallJumpHorizontalForce, (Time.time - _jumpStartTime) / _jumpHoldTime);
            float verticalForce = Mathf.Lerp(1, WallJumpVerticalForce, (Time.time - _jumpStartTime) / _jumpHoldTime);

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
            _hasJumpedSinceGrounded = false;
            _canDash = true;
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
            if (_isWallSliding && (true || _currentWallJumps < 1))
            {
                if (_timeToStopStick <= 0)
                {
                    StartWallJump();
                }
            }
            else if (!_hasJumpedSinceGrounded)
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
        if (isSpawnLocked) return;
        if (Input.GetKeyDown(_dashKey) && _dashCooldownTimer <= 0 && !_isDashing && _canDash)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            if (horizontalInput == 0 && verticalInput == 0)
            {
                _dashDirection = _spriteRend.flipX ? Vector2.right : Vector2.left;
            }
            else
            {
                _dashDirection = new Vector2(horizontalInput, verticalInput).normalized;
            }

            StartDash(_dashDirection);
            _canDash = false;
        }
    }

    void StartDash(Vector2 direction)
    {
        cameraController?.TriggerShake(0.1f, 0.2f);
        _isDashing = true;
        PlaySound(dashSound);
        _dashTimer = _dashDuration;
        _dashDirection = direction;
        _dashCooldownTimer = _dashCooldown;
    }

    void EndDash()
    {
        _isDashing = false;
        _myRgbd2D.linearVelocity = _dashDirection * _dashSpeed * _dashEndMomentumMultiplier;
    }

    void HandlePunchInput()
    {
        if (Input.GetKeyDown(_punchKey) && Time.time >= _lastPunchTime + _punchCooldown)
        {
            PerformPunch();
            _lastPunchTime = Time.time;
        }
    }

    void PerformPunch()
    {
        if (isDead) return;
        if (_animator == null) _animator = GetComponent<Animator>();
        _animator.SetTrigger("punch");
        PlaySound(punchSound);
        StartCoroutine(ActivatePunchAfterDelay());
    }

    IEnumerator ActivatePunchAfterDelay()
    {
        yield return new WaitForSeconds(_punchActivationDelay);

        _isPunchCurrentlyActive = true;

        Vector2 baseOrigin = _punchOrigin != null ? (Vector2)_punchOrigin.position : (Vector2)transform.position;
        Vector2 effectiveOrigin = baseOrigin + new Vector2(0, _punchYOffset);

        float punchDirectionFactor = _spriteRend.flipX ? 1f : -1f;
        Vector2 mainDirection = Vector2.right * punchDirectionFactor;

        bool hasHitDestructibleThisPunch = false;

        float halfConeAngle = _coneAngle / 2f;
        float angleStep = (_numberOfRaysInCone > 1) ? _coneAngle / (_numberOfRaysInCone - 1) : 0;

        for (int i = 0; i < _numberOfRaysInCone; i++)
        {
            float currentAngle = -halfConeAngle + (i * angleStep);
            if (_numberOfRaysInCone == 1) currentAngle = 0;

            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 rayDirection = rotation * mainDirection;

            RaycastHit2D[] hits = Physics2D.RaycastAll(effectiveOrigin, rayDirection, _punchRange, _punchableLayers);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.CompareTag(_destructibleTag))
                {
                    DestructiblePlatform destructible = hit.collider.GetComponent<DestructiblePlatform>();
                    if (destructible != null)
                    {
                        destructible.OnPunchedByPlayer();
                        hasHitDestructibleThisPunch = true;
                        break;
                    }
                }
            }

            if (hasHitDestructibleThisPunch)
            {
                break;
            }
        }

        _isPunchCurrentlyActive = false;
    }

    void OnDrawGizmosSelected()
    {
        if (_spriteRend == null) _spriteRend = GetComponent<SpriteRenderer>();
        if (_spriteRend == null) return;

        Vector2 baseOrigin = _punchOrigin != null ? (Vector2)_punchOrigin.position : (Vector2)transform.position;
        Vector2 effectiveOrigin = baseOrigin + new Vector2(0, _punchYOffset);

        float punchDirectionFactor = _spriteRend.flipX ? 1f : -1f;
        Vector2 mainDirection = Vector2.right * punchDirectionFactor;

        if (_isPunchCurrentlyActive)
        {
            Gizmos.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
        }

        float halfConeAngle = _coneAngle / 2f;

        Quaternion rotLeft = Quaternion.Euler(0, 0, halfConeAngle);
        Quaternion rotRight = Quaternion.Euler(0, 0, -halfConeAngle);

        Vector2 edge1Dir = rotLeft * mainDirection;
        Vector2 edge2Dir = rotRight * mainDirection;

        Vector2 coneEdgePoint1 = effectiveOrigin + edge1Dir * _punchRange;
        Vector2 coneEdgePoint2 = effectiveOrigin + edge2Dir * _punchRange;

        Gizmos.DrawLine(effectiveOrigin, coneEdgePoint1);
        Gizmos.DrawLine(effectiveOrigin, coneEdgePoint2);
        Gizmos.DrawLine(coneEdgePoint1, coneEdgePoint2);

        if (_numberOfRaysInCone > 1)
        {
            float angleStep = _coneAngle / (_numberOfRaysInCone - 1);
            for (int i = 0; i < _numberOfRaysInCone; i++)
            {
                float currentAngle = -halfConeAngle + (i * angleStep);
                Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
                Vector2 rayDirection = rotation * mainDirection;
                Gizmos.DrawRay(effectiveOrigin, rayDirection * _punchRange);
            }
        }
        else if (_numberOfRaysInCone == 1)
        {
            Gizmos.DrawRay(effectiveOrigin, mainDirection * _punchRange);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead || isInvincible == true) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Die()
    {
        if (isInvincible) return;
        if (isDead) return;
        PlaySound(ouchSound);
        StartCoroutine(DieCoroutine());
    }

    private IEnumerator DieCoroutine()
    {
        isInvincible = true;
        deathpanel.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        isDead = true;
        _myRgbd2D.linearVelocity = Vector2.zero;
        _myRgbd2D.simulated = false;
        _collider.enabled = false;
        _isDashing = false;
        _isJumping = false;
        _isWallSliding = false;

        if (infiniteDeath == false)
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
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
        _hasJumpedSinceGrounded = false;
        _isGrounded = false;
        _dashCooldownTimer = 0;
    }

    private void PlaySound(AudioClip clipToPlay, float volume = 1.0f)
    {
        playerAudioSource.PlayOneShot(clipToPlay, volume);
    }
    
    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
}