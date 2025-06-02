using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Gameplay/Item/Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> data = new();

    public ItemData GetData(int id, bool randomAllow = false)
    {
        if (randomAllow && (id < 0 || id >= data.Count))
            id = Random.Range(0, data.Count);
        else
            id = Mathf.Clamp(id, 0, data.Count - 1);

        return data[id];
    }
}