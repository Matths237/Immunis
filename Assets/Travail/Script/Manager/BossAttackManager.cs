using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class BossAttackPattern
{

    public string attackName;
    public MonoBehaviour attackController;
    public float attackDuration = 5f;
    public bool canBeChained = true;
    public bool isEnabled = true;
    public int requiredPhase = 0;
    public int maxAllowedPhase = 99;
}

public class BossAttackManager : MonoBehaviour
{
    public List<BossAttackPattern> attackPatterns = new List<BossAttackPattern>();
    public float minRestTime = 2f;
    public float maxRestTime = 5f;

    [Header("Dependencies")]
    [SerializeField] private BossHealth bossHealthScript;

    private int currentPhase = 0;
    private bool isAttacking = false;
    private Coroutine currentAttackRoutine = null;
    private MonoBehaviour activeAttackController = null;
    [SerializeField] Animator bossAnimator;
    private Shake cameraController;
    [SerializeField] private float timeBeforeBegining;
    public AudioSource bossAudioSource;

    public AudioClip screamNextPhase1;
    public AudioClip screamNextPhase2;
    public AudioClip screamAttack;
    public AudioClip screamAttackNoMouth;

    void Start()
    {
        bossAudioSource = GetComponent<AudioSource>();
        cameraController = Camera.main?.GetComponent<Shake>();
        bossAnimator = GetComponent<Animator>();
        if (bossHealthScript == null)
        {
            bossHealthScript = GetComponent<BossHealth>();
            if (bossHealthScript == null)
            {
                this.enabled = false;
                return;
            }
        }

        currentPhase = bossHealthScript.GetCurrentPhase();

        if (attackPatterns.Count == 0)
        {
            this.enabled = false;
            return;
        }

        foreach (var pattern in attackPatterns)
        {
            if (pattern.attackController != null)
                pattern.attackController.enabled = false;
            else
                pattern.isEnabled = false;
        }

        currentAttackRoutine = StartCoroutine(AttackCycle());
    }

    IEnumerator AttackCycle()
    {
        yield return new WaitForSeconds(timeBeforeBegining);
        while (true)
        {
            if (currentPhase == 0 || currentPhase == 1)
            {
                PlaySound(screamAttackNoMouth);
            }
            else
            {
                PlaySound(screamAttack);
            }
            if (!isAttacking)
            {
                BossAttackPattern nextAttack = ChooseNextAttack();
                bossAnimator.SetTrigger("isScreaming");
                cameraController?.TriggerShake(0.7f, 0.2f);
                yield return new WaitForSeconds(1f);
                if (nextAttack != null && nextAttack.attackController != null && nextAttack.isEnabled)
                {
                    isAttacking = true;
                    activeAttackController = nextAttack.attackController;
                    activeAttackController.enabled = true;

                    yield return new WaitForSeconds(nextAttack.attackDuration);

                    if (activeAttackController != null && activeAttackController.enabled)
                        activeAttackController.enabled = false;

                    activeAttackController = null;

                    if (!nextAttack.canBeChained || Random.value > 0.7f)
                    {
                        float restTime = Random.Range(minRestTime, maxRestTime);
                        yield return new WaitForSeconds(restTime);
                    }
                    isAttacking = false;
                }
                else
                {
                    yield return new WaitForSeconds(Mathf.Max(1f, minRestTime));
                    isAttacking = false;
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    BossAttackPattern ChooseNextAttack()
    {
        List<BossAttackPattern> availableAttacks = attackPatterns
            .Where(p => p != null &&
                        p.isEnabled &&
                        p.attackController != null &&
                        this.currentPhase >= p.requiredPhase &&
                        this.currentPhase <= p.maxAllowedPhase)
            .ToList();

        if (availableAttacks.Count == 0)
            return null;

        int randomIndex = Random.Range(0, availableAttacks.Count);
        return availableAttacks[randomIndex];
    }

    public void SetPhase(int newPhase)
    {
        if (this.currentPhase != newPhase)
        {
            StartCoroutine(SetPhaseCoroutine(newPhase));
        }
    }

    IEnumerator SetPhaseCoroutine(int newPhase)
    {
        this.currentPhase = newPhase;
        PlaySound(screamNextPhase1);
        PlaySound(screamNextPhase2);
        bossAnimator.SetTrigger("nextPhase");
        cameraController?.TriggerShake(0.7f, 0.2f);
        PlaySound(screamAttack);


        if (isAttacking && activeAttackController != null)
        {
            activeAttackController.enabled = false;
            activeAttackController = null;
        }
        isAttacking = false;
        yield return new WaitForSeconds(2.5f);
    }

    public void StopAllAttacks()
    {
        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
            currentAttackRoutine = null;
        }

        if (activeAttackController != null)
        {
            activeAttackController.enabled = false;
            activeAttackController = null;
        }
        isAttacking = false;

        foreach (var pattern in attackPatterns)
        {
            if (pattern.attackController != null && pattern.attackController.enabled)
                pattern.attackController.enabled = false;
        }
    }

    public void RestartAttackCycle()
    {
        StopAllAttacks();
        if (bossHealthScript != null)
        {
            currentPhase = bossHealthScript.GetCurrentPhase();
        }
        else
        {
            currentPhase = 0;
        }

        if (this.isActiveAndEnabled)
        {
            currentAttackRoutine = StartCoroutine(AttackCycle());
        }
    }
    
    private void PlaySound(AudioClip clipToPlay, float volume = 1.0f)
    {
        bossAudioSource.PlayOneShot(clipToPlay, volume);
    }
}