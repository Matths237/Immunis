using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [Header("Database des Plateformes")]
    public PlatformDatabase platformDatabase;

    [Header("Parametres de Spawn")]
    public float spawnAreaWidth = 10f;
    public float spawnXPosition = 0f;
    public float spawnYPosition = -2f;

    [Header("Logique de Spawn")]
    public float baseSpawnRate = 1f;
    public float spawnRateIncreasePerPhase = 0.2f;
    public float minHorizontalSpacing = 2.5f;
    public int recentPositionsToTrack = 3;

    [Header("Parametres des Plateformes Coeur")]
    public GameObject heartPlatformPrefab;
    public float heartPlatformInitialDelay = 30f;
    public float heartPlatformDestroyedCooldown = 30f;
    public float heartPlatformMissedRespawnDelay = 7f;

    [Header("Ref du Boss pour les Phases")]
    public BossHealth bossHealth;

    private float currentRegularSpawnRate;
    private float nextRegularSpawnTime = 0f;
    private List<float> _recentSpawnXPositions = new List<float>();

    private float _nextHeartPlatformSpawnTime;
    private GameObject _activeHeartPlatformInstance = null;

    void Start()
    {
        currentRegularSpawnRate = Mathf.Max(0.1f, baseSpawnRate);

        bossHealth.onPhaseChanged.AddListener(HandleBossPhaseChange);
        HandleBossPhaseChange(bossHealth.GetCurrentPhase());

        _nextHeartPlatformSpawnTime = Time.time + heartPlatformInitialDelay;
    }

    void Update()
    {
        if (Time.time >= nextRegularSpawnTime && currentRegularSpawnRate > 0)
        {
            SpawnRegularPlatform();
            nextRegularSpawnTime = Time.time + 1f / currentRegularSpawnRate;
        }

        HandleHeartPlatformSpawning();
    }

    void SpawnRegularPlatform()
    {
        PlatformData selectedPlatformData = platformDatabase.ChoosePlatform();
        InstantiatePlatform(selectedPlatformData.platformPrefab, true);
    }

    void HandleHeartPlatformSpawning()
    {
        if (_activeHeartPlatformInstance != null && _activeHeartPlatformInstance.gameObject == null)
        {
            _activeHeartPlatformInstance = null;
            _nextHeartPlatformSpawnTime = Time.time + heartPlatformMissedRespawnDelay;
        }
        
        if (_activeHeartPlatformInstance == null && Time.time >= _nextHeartPlatformSpawnTime)
        {
            _activeHeartPlatformInstance = InstantiatePlatform(heartPlatformPrefab, true); 
        }
    }

    GameObject InstantiatePlatform(GameObject platformPrefabToSpawn, bool applySpacingLogic)
    {
        float halfWidth = spawnAreaWidth / 2f;
        float minX = spawnXPosition - halfWidth;
        float maxX = spawnXPosition + halfWidth;
        float finalSpawnX = 0f;

        if (applySpacingLogic)
        {
            bool positionFound = false;
            while (!positionFound)
            {
                finalSpawnX = Random.Range(minX, maxX);
                bool tooClose = false;
                foreach (float recentX in _recentSpawnXPositions)
                {
                    if (Mathf.Abs(finalSpawnX - recentX) < minHorizontalSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (!tooClose)
                {
                    positionFound = true;
                }
            }
        }
        else
        {
            finalSpawnX = Random.Range(minX, maxX);
        }

        Vector3 spawnPosition = new Vector3(finalSpawnX, spawnYPosition, 0f);
        GameObject newPlatformObject = Instantiate(platformPrefabToSpawn, spawnPosition, Quaternion.identity);

        Platform platformComponent = newPlatformObject.GetComponent<Platform>();
        if (platformComponent != null)
        {
            platformComponent.SubscribeToBossPhaseChanges(bossHealth, bossHealth.GetCurrentPhase());
        }
        
        if (applySpacingLogic)
        {
             _recentSpawnXPositions.Add(finalSpawnX);
            if (_recentSpawnXPositions.Count > recentPositionsToTrack)
            {
                _recentSpawnXPositions.RemoveAt(0);
            }
        }
        
        return newPlatformObject;
    }

    public void NotifyHeartPlatformDestroyed()
    {
        _activeHeartPlatformInstance = null;
        _nextHeartPlatformSpawnTime = Time.time + heartPlatformDestroyedCooldown;
    }

    void HandleBossPhaseChange(int newPhaseIndex)
    {
        currentRegularSpawnRate = baseSpawnRate + (newPhaseIndex * spawnRateIncreasePerPhase);
        currentRegularSpawnRate = Mathf.Max(0.1f, currentRegularSpawnRate);
    }

    void OnDestroy()
    {
        bossHealth.onPhaseChanged.RemoveListener(HandleBossPhaseChange);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 spawnAreaCenterPos = new Vector3(spawnXPosition, spawnYPosition, 0f);
        Vector3 spawnAreaSize = new Vector3(spawnAreaWidth, 0.5f, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnAreaCenterPos, spawnAreaSize);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            foreach (float xPos in _recentSpawnXPositions)
            {
                Gizmos.DrawSphere(new Vector3(xPos, spawnYPosition, 0f), 0.3f);
            }
        }
    }
}