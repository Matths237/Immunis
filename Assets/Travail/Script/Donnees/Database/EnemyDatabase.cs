using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Datas/Enemy", order = 1)]
public class EnemyDatabase : ScriptableObject
{
    [SerializeField] private List<EnemyData> data = new();

    public EnemyData GetData(int id, bool randomAllow = false)
    {
        if (randomAllow && (id < 0 || id >= data.Count))
            id = Random.Range(0, data.Count);
        else
            id = Mathf.Clamp(id, 0, data.Count - 1);

        return data[id];
    }
}