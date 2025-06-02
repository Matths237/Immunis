using UnityEngine;

public class Collectible : MonoBehaviour
{

    [Header("General Settings")]
    [SerializeField] private string playerTag = "Player"; 
    [SerializeField] private int damageAmount = 10; 


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                player.TakeDamage(damageAmount);

                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"L'objet {other.name} avec le tag '{playerTag}' n'a pas de composant PlayerController (ou PlayerHealth). L'acide ne peut pas infliger de dégâts.");
            }
        }
    }
    public int GetDamageAmount() => damageAmount;
}