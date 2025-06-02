using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Platform : MonoBehaviour
{
    public PlatformData platformData; 

    public float speedIncreasePerPhase = 0.2f; 

    [SerializeField] private float currentSpeed; 
    private float baseSpeedFromData; 
    private const string DESPAWN_TAG = "Despawn";
    private const string PLAYER_TAG = "Player";

    private Rigidbody2D rb;
    private BossHealth bossHealthCached; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (platformData != null)
        {
            baseSpeedFromData = platformData.fallSpeed;
            currentSpeed = baseSpeedFromData;
        }
        else
        {
            baseSpeedFromData = 1f; 
            currentSpeed = baseSpeedFromData;
        }
    }
    public void SubscribeToBossPhaseChanges(BossHealth boss, int initialPhase)
    {
        this.bossHealthCached = boss; 
        if (this.bossHealthCached != null)
        {
            this.bossHealthCached.onPhaseChanged.AddListener(OnBossPhaseChanged);
            OnBossPhaseChanged(initialPhase);
        }
    }


    void OnBossPhaseChanged(int newPhaseIndex)
    {
        currentSpeed = baseSpeedFromData + (newPhaseIndex * speedIncreasePerPhase);
        currentSpeed = Mathf.Max(0f, currentSpeed);
    }

    void FixedUpdate()
    {
        if (Mathf.Approximately(currentSpeed, 0f))
            return;

        Vector2 newPosition = rb.position + Vector2.down * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(DESPAWN_TAG))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PLAYER_TAG))
        {
            ContactPoint2D[] contacts = collision.contacts;
            bool landedOnTop = false;
            foreach (ContactPoint2D contact in contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    landedOnTop = true;
                    break;
                }
            }

            if (landedOnTop)
            {
                collision.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PLAYER_TAG))
        {
            if (collision.transform.parent == transform)
            {
                collision.transform.SetParent(null);
            }
        }
    }

    void OnDestroy()
    {
        if (bossHealthCached != null)
        {
            bossHealthCached.onPhaseChanged.RemoveListener(OnBossPhaseChanged);
        }
    }
}