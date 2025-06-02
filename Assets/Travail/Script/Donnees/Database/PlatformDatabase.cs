using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "PlatformDatabase", menuName = "Gameplay/Platform/Database")]
public class PlatformDatabase : ScriptableObject
{
    public List<PlatformData> platformTypes = new List<PlatformData>();

    private bool probabilitiesNormalized = false;
    private float totalProbability = 0f;

    private void OnValidate()
    {
        NormalizeProbabilities();
    }

    private void NormalizeProbabilities()
    {
        if (platformTypes == null || platformTypes.Count == 0)
        {
            totalProbability = 0;
            probabilitiesNormalized = false;
            return;
        }

        totalProbability = platformTypes.Sum(data => data.spawnProbability);

        if (totalProbability <= 0)
        {
            probabilitiesNormalized = false;
            return;
        }

        probabilitiesNormalized = true; 
    }

    public PlatformData ChoosePlatform()
    {
        if (!probabilitiesNormalized || totalProbability <= 0f || platformTypes.Count == 0)
        {
            NormalizeProbabilities();
            if (!probabilitiesNormalized || totalProbability <= 0f || platformTypes.Count == 0)
            {
                return null; 
            }
        }

        float randomNumber = Random.Range(0f, totalProbability);
        float cumulativeProbability = 0f;

        foreach (PlatformData platformData in platformTypes)
        {
            cumulativeProbability += platformData.spawnProbability;
            if (randomNumber <= cumulativeProbability)
            {
                if (platformData.platformPrefab == null)
                {
                    continue; 
                }
                return platformData;
            }
        }
        for (int i = platformTypes.Count - 1; i >= 0; i--)
        {
            if (platformTypes[i].spawnProbability > 0 && platformTypes[i].platformPrefab != null)
                return platformTypes[i];
        }

        return null; 
    }
}