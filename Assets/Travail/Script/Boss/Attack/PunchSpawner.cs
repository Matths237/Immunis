using UnityEngine;

public class PunchSpawner : MonoBehaviour
{
    public GameObject punchPrefab;

    void OnEnable()
    {
        Instantiate(punchPrefab); 
    }
}