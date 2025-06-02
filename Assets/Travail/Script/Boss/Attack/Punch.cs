using System.Collections;
using UnityEngine;

public class Punch : MonoBehaviour
{
    [Header("Speeds")]
    public float chargingSpeed = 55f;
    public float returningSpeed = 20f;
    public float enterSpeed = 6f;
    public float followSpeedY = 5f;

    [Header("Durations")]
    public float chargeTime = 2f;
    public float stayDuration = 3f;
    public float followDuration = 1.5f;

    [Header("Spawn Positions")]
    public float leftSpawnX = -32f;
    public float rightSpawnX = 45f;
    public float enterDistance = 15f;

    [Header("Target Positions")]
    public float leftTargetX = -10f;
    public float rightTargetX = 23f;

    private Transform player;
    private Vector3 startPosition;
    private Vector3 entryPosition;
    private Vector3 targetPosition;
    private Vector3 returnPosition;
    private Vector3 initialScale;

    private bool isEntering = true;
    private bool isFollowing = false;
    private bool isCharging = false;
    private bool isPunching = false;
    private bool isReturning = false;

    private Shake cameraController;

    public AudioSource punchAudioSource;

    public AudioClip contact1;
    public AudioClip contact2;

    void Start()
    {
        punchAudioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        cameraController = Camera.main?.GetComponent<Shake>();

        initialScale = transform.localScale;

        bool fromLeft = Random.value > 0.5f;

        float spawnX = fromLeft ? leftSpawnX : rightSpawnX;
        float entryX = spawnX + (fromLeft ? enterDistance : -enterDistance);
        float finalTargetX = fromLeft ? rightTargetX : leftTargetX;

        float initialY = player.position.y;

        startPosition = new Vector3(spawnX, initialY, 0);
        entryPosition = new Vector3(entryX, initialY, 0);
        targetPosition = new Vector3(finalTargetX, initialY, 0);
        returnPosition = startPosition;

        transform.position = startPosition;

        if (!fromLeft)
        {
            transform.localScale = new Vector3(-initialScale.x, initialScale.y, initialScale.z);
        }
        else
        {
             transform.localScale = new Vector3(initialScale.x, initialScale.y, initialScale.z);
        }

        StartCoroutine(FollowYThenCharge());
    }

    void Update()
    {
        if (isEntering)
        {
            float stepX = enterSpeed * Time.deltaTime;
            Vector3 targetEntryPos = new Vector3(entryPosition.x, transform.position.y, 0);
            transform.position = Vector3.MoveTowards(transform.position, targetEntryPos, stepX);

            if (Mathf.Abs(transform.position.x - entryPosition.x) < 0.1f)
            {
                isEntering = false;
            }
        }
        else if (isPunching)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, chargingSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                punchAudioSource.PlayOneShot(contact1);
                punchAudioSource.PlayOneShot(contact2);
                isPunching = false;
                cameraController?.TriggerShake(0.15f, 0.5f);
                StartCoroutine(WaitAndReturn());
            }
        }
        else if (isReturning)
        {
            transform.position = Vector3.MoveTowards(transform.position, returnPosition, returningSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, returnPosition) < 0.1f)
            {
                Destroy(gameObject);
            }
        }
    }

    IEnumerator FollowYThenCharge()
    {
        isFollowing = true;
        float elapsed = 0f;

        while (elapsed < followDuration)
        {
            if (player == null) yield break;

            float stepY = followSpeedY * Time.deltaTime;
            float targetY = player.position.y;
            float newY = Mathf.MoveTowards(transform.position.y, targetY, stepY);
            transform.position = new Vector3(transform.position.x, newY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        isFollowing = false;

        Vector3 lockedPosition = transform.position;
        entryPosition = lockedPosition;
        targetPosition = new Vector3(targetPosition.x, lockedPosition.y, 0);
        returnPosition = new Vector3(returnPosition.x, lockedPosition.y, 0);
        StartCoroutine(ChargeAndPunch(lockedPosition));
    }

    IEnumerator ChargeAndPunch(Vector3 startChargePos)
    {
        isCharging = true;
        float elapsed = 0f;
        float shakeAmount = 0.1f;

        while (elapsed < chargeTime)
        {
            transform.position = startChargePos + (Vector3)Random.insideUnitCircle * shakeAmount;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startChargePos;
        isCharging = false;
        isPunching = true;
    }

    IEnumerator WaitAndReturn()
    {
        yield return new WaitForSeconds(stayDuration);
        isReturning = true;
    }
}