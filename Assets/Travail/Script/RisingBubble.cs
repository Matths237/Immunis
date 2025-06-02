using UnityEngine;

public class RisingBubble : MonoBehaviour
{
    public float riseSpeed = 1f;
    public float fadeDuration = 2f;
    public float lifetime = 3f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float timeAlive = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector2.up * riseSpeed * Time.deltaTime);

        timeAlive += Time.deltaTime;
        if (spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(originalColor.a, 0f, timeAlive / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
    }
}