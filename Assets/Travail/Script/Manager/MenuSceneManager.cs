using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuSceneManager : MonoBehaviour
{
    public AudioSource backgroundAudioSource;
    public GameObject panel;
    public AudioSource audioSource;
    public AudioClip ClickedSound1;
    public AudioClip ClickedSound2;
    public AudioClip ClickedSound3;
    public AudioClip ClickedButtonSound;
    public AudioClip ClickedNextPageSound;
    public AudioClip ClickedAntePageSound;
    public Animator rulesAnimator;
    public Animator panelAnimator;
    

    private AudioLowPassFilter backgroundMusicLowPassFilter;
    public float normalCutoff = 22000f;
    public float muffledCutoff = 1000f;
    public float musicTransitionDuration = 0.5f;
    private Coroutine musicCutoffTransitionCoroutine;

    private bool isRulesMenuOpen = false;
    private bool isOptionsMenuOpen = false;

    void Start()
    {
        Time.timeScale = 1f;
        panel.SetActive(true);
        audioSource = GetComponent<AudioSource>();
        backgroundMusicLowPassFilter = backgroundAudioSource.GetComponent<AudioLowPassFilter>();
        backgroundMusicLowPassFilter.cutoffFrequency = normalCutoff;
        isRulesMenuOpen = false; 
        isOptionsMenuOpen = false;
    }

    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        PlaySound(ClickedButtonSound);
        yield return StartCoroutine(FadeOutMusic(1f));
        PlaySound(ClickedSound1);
        yield return new WaitForSeconds(0.3f);
        PlaySound(ClickedSound2);
        panelAnimator.SetTrigger("fadeOut");
        yield return new WaitForSeconds(2.5f);
        PlaySound(ClickedSound3);
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void ReturnMenuGame()
    {
        StartCoroutine(ReturnMenuGameCoroutine());
    }

    private IEnumerator ReturnMenuGameCoroutine()
    {
        panelAnimator.SetTrigger("fadeOut");
        yield return StartCoroutine(FadeOutMusic(3f));
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = backgroundAudioSource.volume;
        float time = 0f;

        while (time < duration)
        {
            backgroundAudioSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        backgroundAudioSource.volume = 0f;
        backgroundAudioSource.Stop();
    }

    public void RuleGame()
    {
        PlaySound(ClickedButtonSound);
        rulesAnimator.SetTrigger("selected");
        isRulesMenuOpen = !isRulesMenuOpen;
        UpdateMusicMuffleState();
    }

    public void NextPage()
    {
        PlaySound(ClickedNextPageSound);
        rulesAnimator.SetTrigger("nextPage");
    }
    
    public void AntePage()
    {
        PlaySound(ClickedAntePageSound);
        rulesAnimator.SetTrigger("antePage");
    }

    private void UpdateMusicMuffleState()
    {
        if (isRulesMenuOpen || isOptionsMenuOpen)
        {
            MuffleBackgroundMusic();
        }
        else
        {
            UnmuffleBackgroundMusic();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void PlaySound(AudioClip clipToPlay, float volume = 1.0f)
    {
        audioSource.PlayOneShot(clipToPlay, volume);
    }

    private void MuffleBackgroundMusic()
    {
        if (musicCutoffTransitionCoroutine != null) StopCoroutine(musicCutoffTransitionCoroutine);
        musicCutoffTransitionCoroutine = StartCoroutine(TransitionMusicCutoff(muffledCutoff));
    }

    private void UnmuffleBackgroundMusic()
    {
        if (musicCutoffTransitionCoroutine != null) StopCoroutine(musicCutoffTransitionCoroutine);
        musicCutoffTransitionCoroutine = StartCoroutine(TransitionMusicCutoff(normalCutoff));
    }

    IEnumerator TransitionMusicCutoff(float targetCutoff)
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
}