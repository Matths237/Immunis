using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 300;
    public int currentHealth;
    [SerializeField] private GameObject lifeBoss;
    private Animator lifeBossAnimator;

    [Header("Boss Phases")]
    public List<int> phaseHealth = new List<int> { 200, 100 };

    [SerializeField] private int currentPhase = 0;

    [System.Serializable]
    [HideInInspector] public class PhaseChangeEvent : UnityEvent<int> { }
    [HideInInspector] public PhaseChangeEvent onPhaseChanged;
    public UnityEvent onBossDefeated;

    public AudioSource bossAudioSource;
    public AudioClip screamDieSound;

    void Start()
    {
        lifeBossAnimator = lifeBoss.GetComponent<Animator>();
        currentHealth = maxHealth;
        currentPhase = 0;
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;
        lifeBossAnimator.SetTrigger("hurted");

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        CheckForPhaseChange();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void CheckForPhaseChange()
    {
        if (currentPhase < phaseHealth.Count)
        {
            int nextThreshold = phaseHealth[currentPhase];
            if (currentHealth <= nextThreshold)
            {
                currentPhase++;
                onPhaseChanged.Invoke(currentPhase);

                BossAttackManager attackManager = GetComponent<BossAttackManager>();
                if (attackManager != null)
                {
                    attackManager.SetPhase(currentPhase);
                }
            }
        }
    }

    public int GetCurrentPhase()
    {
        return currentPhase;
    }

    void Die()
    {
        BossAttackManager attackManager = GetComponent<BossAttackManager>();
        if (attackManager != null)
        {
            attackManager.enabled = false;
        }
        lifeBossAnimator.SetTrigger("die");
        PlaySound(screamDieSound);
        onBossDefeated.Invoke();
    }

    private void PlaySound(AudioClip clipToPlay, float volume = 1.0f)
    {
        bossAudioSource.PlayOneShot(clipToPlay, volume);
    }
}