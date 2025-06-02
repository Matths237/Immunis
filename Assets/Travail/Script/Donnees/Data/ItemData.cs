using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlatformData", menuName = "Gameplay/Item/Data")]
[Serializable]

public class ItemData : BaseData
{

    public enum CATEGORY
    {
        COIN,
        LIFE,
        POWER,
    }

    public CATEGORY category;

    public int quantity;
    public float value; 
}
