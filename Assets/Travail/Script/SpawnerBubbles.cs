using UnityEngine;
using System.Collections; 
using System.Collections.Generic;

public class SpawnerBubbles : MonoBehaviour
{
    [Header("Référence Bulles")]
    public GameObject bubblePrefab;
    public Sprite[] bubbleSprites;

    [Header("Paramètres de Spawn")]
    public float minSpawnX = -5f;
    public float maxSpawnX = 5f;
    public float spawnY = -3f;
    public float spawnInterval = 0.5f;
    public int maxConcurrentBubbles = 30;

    private List<GameObject> activeBubbles = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true) 
        {

            activeBubbles.RemoveAll(item => item == null);

            if (activeBubbles.Count < maxConcurrentBubbles)
            {
                SpawnBubble();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnBubble()
    {
        float randomX = Random.Range(minSpawnX, maxSpawnX);
        Vector2 spawnPosition = new Vector2(randomX, spawnY);

        GameObject newBubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
        
        SpriteRenderer sr = newBubble.GetComponent<SpriteRenderer>();

        if (sr != null && bubbleSprites.Length > 0)
        {
            sr.sprite = bubbleSprites[Random.Range(0, bubbleSprites.Length)];
        }

        RisingBubble bubbleScript = newBubble.GetComponent<RisingBubble>();
        
        activeBubbles.Add(newBubble);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 p1 = new Vector3(minSpawnX, spawnY - 0.5f, 0);
        Vector3 p2 = new Vector3(maxSpawnX, spawnY - 0.5f, 0);
        Vector3 p3 = new Vector3(maxSpawnX, spawnY + 0.5f, 0);
        Vector3 p4 = new Vector3(minSpawnX, spawnY + 0.5f, 0);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
}