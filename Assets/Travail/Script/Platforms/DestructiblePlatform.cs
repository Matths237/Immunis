using UnityEngine;

public class DestructiblePlatform : MonoBehaviour
{
    public int damageToBoss = 50;
    [SerializeField] Animator animator;

    private BossHealth bossHealth;
    private Spawner spawnerInstance;
    private Shake cameraController;
    public AudioSource destructibleAudioSource;

    public AudioClip destructionSound;
    public AudioClip bpmSound;

    void Start()
    {
        destructibleAudioSource.PlayOneShot(bpmSound);
        destructibleAudioSource = GetComponent<AudioSource>();
        cameraController = Camera.main?.GetComponent<Shake>();
        GameObject bossGameObject = GameObject.FindGameObjectWithTag("Boss");
        if (bossGameObject != null)
        {
            bossHealth = bossGameObject.GetComponent<BossHealth>();
        }

        spawnerInstance = FindObjectOfType<Spawner>();
    }

    public void OnPunchedByPlayer()
    {
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(damageToBoss);
        }

        if (spawnerInstance != null)
        {
            spawnerInstance.NotifyHeartPlatformDestroyed();
        }
        destructibleAudioSource.PlayOneShot(destructionSound, 0.8f);
        animator.SetTrigger("isBroken");
        cameraController?.TriggerShake(0.1f, 0.5f);
    }
}