using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public int damageAmount = 1;
    public float damageInterval = 0.5f; 

    private float lastDamageTime = -1f; 
    private bool playerInside = false;
    private PlayerController playerController = null; 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            
            playerInside = true;
            playerController = other.GetComponent<PlayerController>();
            lastDamageTime = Time.time;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (playerInside && playerController != null) 
        {
            if (Time.time >= lastDamageTime + damageInterval)
            {
                ApplyDamage();
                lastDamageTime = Time.time;
            }
        }
        else if (other.CompareTag("Player"))
        {
             playerInside = true;
             playerController = other.GetComponent<PlayerController>();
             lastDamageTime = Time.time;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            playerController = null; 
        }
    }

    void ApplyDamage()
    {
        if (playerController != null)
        {
            playerController.TakeDamage(damageAmount);
        }
    }
}