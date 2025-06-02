using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReturnMenu : MonoBehaviour
{

    public Animator panelAnimator;
    public Animator pauseAnimator;
    public AudioSource backgroundAudioSource;
    public PlayerController playerController;
    public GameManager gameManager;





    public void ReturnMenuGame()
    {
        StartCoroutine(ReturnMenuGameCoroutine());
    }

    private IEnumerator ReturnMenuGameCoroutine()
    {
        gameManager.UnmuffleBackgroundMusic();
        playerController.SetInvincible(true);
        pauseAnimator.SetTrigger("pause");
        panelAnimator.SetTrigger("fadeOut");
        Time.timeScale = 1f;
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
}
