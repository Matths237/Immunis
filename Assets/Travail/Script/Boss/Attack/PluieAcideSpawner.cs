using UnityEngine;

public class PluieAcideSpawner : MonoBehaviour
{
    public GameObject acidPrefab;
    public float spawnRate = 2f;
    [Range(0f, 1f)]
    public float screenWidthPercentage = 0.7f;
    public float minHorizontalSpacing = 2f;
    public float spawnHeightOffset = 1.1f;

    private float nextSpawnTime = 0f;
    private float screenWorldWidth;
    private float lastSpawnX;
    private bool isInitialized = false;

    void InitializeIfNeeded()
    {
        if (isInitialized) return;

        if (acidPrefab == null)
        {
            this.enabled = false; 
            return;
        }

        if (Camera.main == null)
        {
            this.enabled = false;
            return;
        }

        float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        screenWorldWidth = 2f * halfWidth * screenWidthPercentage;
        spawnRate = Mathf.Max(0.01f, Mathf.Abs(spawnRate));
        lastSpawnX = Camera.main.transform.position.x;
        nextSpawnTime = Time.time + 1f / spawnRate; 
        isInitialized = true;
    }

    void OnEnable()
    {
        InitializeIfNeeded();
        if (isInitialized)
        {
            nextSpawnTime = Time.time + (1f / spawnRate) * Random.Range(0.5f, 1f);
        }
    }

    void Update()
    {
        if (!isInitialized) return;


        if (Time.time >= nextSpawnTime)
        {
            SpawnAcidDrop();
            nextSpawnTime = Time.time + 1f / spawnRate;
        }
    }

    void SpawnAcidDrop()
    {
        if (acidPrefab == null || !isInitialized) return;

        float randomX;
        int attempts = 0;
        const int maxAttempts = 20;
        float cameraLeftEdge = Camera.main.transform.position.x - (screenWorldWidth / 2f);
        float cameraRightEdge = Camera.main.transform.position.x + (screenWorldWidth / 2f);

        if (minHorizontalSpacing > 0f)
        {
            do
            {
                randomX = Random.Range(cameraLeftEdge, cameraRightEdge);
                attempts++;
            } while (attempts > 1 && Mathf.Abs(randomX - lastSpawnX) < minHorizontalSpacing && attempts <= maxAttempts);
        }
        else
        {
            randomX = Random.Range(cameraLeftEdge, cameraRightEdge);
        }

        Vector3 spawnPositionViewport = new Vector3(0.5f, spawnHeightOffset, Mathf.Abs(Camera.main.transform.position.z - transform.position.z));
        Vector3 spawnPositionWorld = Camera.main.ViewportToWorldPoint(spawnPositionViewport);
        spawnPositionWorld.x = randomX;
        spawnPositionWorld.z = 0;

        Instantiate(acidPrefab, spawnPositionWorld, Quaternion.identity);
        lastSpawnX = randomX;
    }
}