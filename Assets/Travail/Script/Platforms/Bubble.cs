using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] Animator animator;
    private bool playerHasLanded = false;
    public AudioSource bubbleAudioSource;

    public AudioClip popSound;
    void Start()
    {
        bubbleAudioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerHasLanded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerHasLanded)
            {
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null && playerRb.linearVelocity.y > 0.1f) 
                {
                    bubbleAudioSource.PlayOneShot(popSound);
                    animator.SetTrigger("explode");
                }
            }
        }
    }

    void Awake()
    {
        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
            rb2d.isKinematic = true;
        }
    }
}