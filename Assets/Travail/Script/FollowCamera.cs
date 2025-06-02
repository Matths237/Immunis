using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTarget; 

    [Header("Camera Settings")]
    [Range(0.01f, 1.0f)]
    public float smoothFactor = 0.1f; 
    public Vector3 offset = new Vector3(0, 2, -10); 

    private float initialYPosition; 
    private float fixedZPosition;

    public AudioSource explosionAudioSource;

    void Start()
    {
        initialYPosition = transform.position.y;
        fixedZPosition = transform.position.z; 
        explosionAudioSource = GetComponent<AudioSource>();
        InfectionExplosion.globalExplosionAudioSource = explosionAudioSource;
    }

    void LateUpdate() 
    {
        float targetX = playerTarget.position.x + offset.x;
        float targetY = Mathf.Max(playerTarget.position.y + offset.y, initialYPosition);
        Vector3 desiredPosition = new Vector3(targetX, targetY, fixedZPosition);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothFactor);
        transform.position = smoothedPosition;
    }

    void OnDrawGizmosSelected()
    {
        if (playerTarget != null)
        {
            Gizmos.color = Color.blue;
            Vector3 targetPosWithOffset = playerTarget.position + offset;
            Gizmos.DrawLine(playerTarget.position, targetPosWithOffset);
            Gizmos.DrawSphere(targetPosWithOffset, 0.2f);
        }

        Gizmos.color = Color.red;
        float lineLength = 10f; 
        Vector3 lineStart = new Vector3(transform.position.x - lineLength / 2, initialYPosition, transform.position.z);
        Vector3 lineEnd = new Vector3(transform.position.x + lineLength / 2, initialYPosition, transform.position.z);
        if (Application.isPlaying) 
        {
            Gizmos.DrawLine(lineStart, lineEnd);
        }
    }
}