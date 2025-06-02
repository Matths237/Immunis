using UnityEngine;

public class PlatformExplosionAttack : MonoBehaviour
{
    public string platformTag = "InfectionPlatform";
    public float delayBetweenPlatformExplosions = 0.2f;
    public bool activateAllAtOnce = true;

    private GameObject[] platformsToExplode;

    void OnEnable()
    {
        platformsToExplode = GameObject.FindGameObjectsWithTag(platformTag);

        if (platformsToExplode.Length == 0)
        {
            return;
        }

        if (activateAllAtOnce)
        {
            for (int i = 0; i < platformsToExplode.Length; i++)
            {
                if (platformsToExplode[i] != null)
                {
                    InfectionExplosion platformScript = platformsToExplode[i].GetComponent<InfectionExplosion>();
                    if (platformScript != null)
                    {
                        bool playSound = (i == 0);
                        platformScript.TriggerPlatformExplosion(playSound);
                    }
                }
            }
        }
        else
        {
            StartCoroutine(ExplodePlatformsSequentially());
        }
    }

    System.Collections.IEnumerator ExplodePlatformsSequentially()
    {
        for (int i = 0; i < platformsToExplode.Length; i++)
        {
            if (platformsToExplode[i] != null)
            {
                InfectionExplosion platformScript = platformsToExplode[i].GetComponent<InfectionExplosion>();
                if (platformScript != null)
                {
                    bool playSound = (i == 0);
                    platformScript.TriggerPlatformExplosion(playSound);
                }

                if (delayBetweenPlatformExplosions > 0)
                {
                    yield return new WaitForSeconds(delayBetweenPlatformExplosions);
                }
            }
        }
    }
}