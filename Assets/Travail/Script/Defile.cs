using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ScrollingElementConfig
{
    public string label = "Element";

    [Header("GameObject")]
    public GameObject elementPrefab;
    public float elementHeight; 

    [Header("Placement")]
    public float xPosition = 0f; // Si relatif, entre -0.5 (gauche) et 0.5 (droite)
    public bool xPositionIsRelative = false; // Nouveau : true = relatif à la largeur de la caméra
    public int initialCount;

    [Header("Vitesse")]
    public float baseScrollSpeed = 5f;
    public float scrollSpeedIncrease = 0.5f;

    [HideInInspector]
    public List<Transform> instances = new List<Transform>();

    [HideInInspector]
    public float currentScrollSpeed;
}

public class Defile : MonoBehaviour
{
    [Header("Configurations des Éléments à Faire Défiler")]
    public List<ScrollingElementConfig> elementConfigurations = new List<ScrollingElementConfig>();

    [Header("Boss Reference")]
    public BossHealth bossHealth;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            this.enabled = false;
            return;
        }

        foreach (ScrollingElementConfig config in elementConfigurations)
        {
            config.instances.Clear();

            for (int i = 0; i < config.initialCount; i++)
            {
                GameObject newElement = Instantiate(config.elementPrefab, transform);
                newElement.transform.position = new Vector3(GetActualXPosition(config), i * config.elementHeight, 0);
                config.instances.Add(newElement.transform);
            }

            if (bossHealth == null)
            {
                config.currentScrollSpeed = config.baseScrollSpeed;
                config.currentScrollSpeed = Mathf.Max(0.1f, config.currentScrollSpeed); 
            }
        }

        if (bossHealth != null)
        {
            bossHealth.onPhaseChanged.AddListener(HandleBossPhaseChange);
            HandleBossPhaseChange(bossHealth.GetCurrentPhase()); 
        }
    }

    void Update()
    {
        float cameraBottomY = mainCamera.transform.position.y - mainCamera.orthographicSize;

        foreach (ScrollingElementConfig config in elementConfigurations)
        {
            if (config.instances.Count == 0 || config.currentScrollSpeed <= 0) continue;

            float repositionThreshold = cameraBottomY - config.elementHeight;

            foreach (Transform element in config.instances)
            {
                element.position += Vector3.down * config.currentScrollSpeed * Time.deltaTime;

                if (element.position.y < repositionThreshold)
                {
                    float highestYForThisType = GetHighestElementY(config.instances);
                    element.position = new Vector3(GetActualXPosition(config), highestYForThisType + config.elementHeight, element.position.z);
                }
            }
        }
    }

    private float GetActualXPosition(ScrollingElementConfig config)
    {
        if (!config.xPositionIsRelative) return config.xPosition;

        float halfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        return mainCamera.transform.position.x + config.xPosition * 2f * halfWidth;
    }

    float GetHighestElementY(List<Transform> specificElements)
    {
        float highest = float.MinValue;
        if (specificElements.Count == 0) return 0;

        foreach (Transform element in specificElements)
        {
            if (element.position.y > highest)
            {
                highest = element.position.y;
            }
        }
        return highest;
    }

    void HandleBossPhaseChange(int newPhaseIndex)
    {
        foreach (ScrollingElementConfig config in elementConfigurations)
        {
            config.currentScrollSpeed = config.baseScrollSpeed + (newPhaseIndex * config.scrollSpeedIncrease);
            config.currentScrollSpeed = Mathf.Max(0.1f, config.currentScrollSpeed);
        }
    }

    void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.onPhaseChanged.RemoveListener(HandleBossPhaseChange);
        }
    }
}