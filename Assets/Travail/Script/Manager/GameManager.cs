using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public Animator animatorFadeOut;
    public AudioSource backgroundAudioSource;
    public AudioSource bossAudioSource;
    public Animator pauseLogoAnimator;

    private AudioLowPassFilter backgroundMusicLowPassFilter;
    public float normalCutoff = 22000f;
    public float muffledCutoff = 1000f;
    public float musicTransitionDuration = 0.5f;
    private Coroutine musicCutoffTransitionCoroutine;

    private bool gameHasEnded = false;
    [DoNotSerialize] public bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f;
        if (backgroundAudioSource != null)
        {
            backgroundMusicLowPassFilter = backgroundAudioSource.GetComponent<AudioLowPassFilter>();
            if (backgroundMusicLowPassFilter != null)
                backgroundMusicLowPassFilter.cutoffFrequency = normalCutoff;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameHasEnded)
        {
            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        if (pauseLogoAnimator != null)
            pauseLogoAnimator.SetTrigger("pause");
        Time.timeScale = 0f;
        MuffleBackgroundMusic();
    }

    private void ResumeGame()
    {
        if (pauseLogoAnimator != null)
            pauseLogoAnimator.SetTrigger("pause");
        isPaused = false;
        Time.timeScale = 1f;
        UnmuffleBackgroundMusic();
    }


    private void MuffleBackgroundMusic()
    {
        if (musicCutoffTransitionCoroutine != null) StopCoroutine(musicCutoffTransitionCoroutine);
        if (backgroundMusicLowPassFilter != null)
            musicCutoffTransitionCoroutine = StartCoroutine(TransitionMusicCutoff(muffledCutoff));
    }

    public void UnmuffleBackgroundMusic()
    {
        if (musicCutoffTransitionCoroutine != null) StopCoroutine(musicCutoffTransitionCoroutine);
        if (backgroundMusicLowPassFilter != null)
            musicCutoffTransitionCoroutine = StartCoroutine(TransitionMusicCutoff(normalCutoff));
    }

    private IEnumerator TransitionMusicCutoff(float targetCutoff)
    {
        float startCutoff = backgroundMusicLowPassFilter.cutoffFrequency;
        float time = 0;

        while (time < musicTransitionDuration)
        {
            backgroundMusicLowPassFilter.cutoffFrequency = Mathf.Lerp(startCutoff, targetCutoff, time / musicTransitionDuration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        backgroundMusicLowPassFilter.cutoffFrequency = targetCutoff;
        musicCutoffTransitionCoroutine = null;
    }

    public void HandleBossDefeated()
    {
        if (gameHasEnded) return;

        gameHasEnded = true;
        animatorFadeOut.SetTrigger("fadeOut");
        Time.timeScale = 0.4f;
        PlayerController player = FindObjectOfType<PlayerController>();
        player.SetInvincible(true);
        StartCoroutine(HandleWinSequence());
    }

    private IEnumerator HandleWinSequence()
    {
        yield return StartCoroutine(FadeOutMusic(3f));
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene("Win", LoadSceneMode.Single);
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        if (backgroundAudioSource == null && bossAudioSource == null) yield break;

        float startVolumeBackground = backgroundAudioSource != null ? backgroundAudioSource.volume : 0f;
        float startVolumeBoss = bossAudioSource != null ? bossAudioSource.volume : 0f;
        float time = 0f;

        while (time < duration)
        {
            if (backgroundAudioSource != null)
                backgroundAudioSource.volume = Mathf.Lerp(startVolumeBackground, 0f, time / duration);
            if (bossAudioSource != null)
                bossAudioSource.volume = Mathf.Lerp(startVolumeBoss, 0f, time / duration);

            time += Time.unscaledDeltaTime;
            yield return null;
        }
        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.volume = 0f;
            backgroundAudioSource.Stop();
        }
        if (bossAudioSource != null)
        {
            bossAudioSource.volume = 0f;
            bossAudioSource.Stop();
        }
    }
}