using UnityEngine;
using System.Collections;

public class InfectionExplosion : MonoBehaviour 
{
    public AudioSource explosionAudioSource;
    public AudioClip raiseSound;
    public AudioClip boomSound1;
    public AudioClip boomSound2;
    [SerializeField] private Animator animator;

    public static InfectionExplosion master;
    public static AudioSource globalExplosionAudioSource;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        explosionAudioSource = GetComponent<AudioSource>();
        if (master == null)
        {
            master = this;
        }
    }

    public void TriggerPlatformExplosion(bool playSound = false)
    {
        StartCoroutine(TriggerPlatformExplosionCoroutine(playSound));
    }

    private IEnumerator TriggerPlatformExplosionCoroutine(bool playSound)
    {
        if (playSound && globalExplosionAudioSource != null)
        {
            globalExplosionAudioSource.PlayOneShot(raiseSound);
        }
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("Explode");
        if (playSound && globalExplosionAudioSource != null)
        {
            yield return new WaitForSeconds(1.2f);
            globalExplosionAudioSource.PlayOneShot(boomSound1);
            yield return new WaitForSeconds(0.3f);
            globalExplosionAudioSource.PlayOneShot(boomSound2);
        }
    }
}