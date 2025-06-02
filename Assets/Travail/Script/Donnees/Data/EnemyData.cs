using System;
using UnityEngine;


[Serializable]
public class EnemyData : BaseData
{
    [HeaderAttribute("SETUP")]


    [HeaderAttribute("STATS")]
    public float health;
    public float speed;
    public float damage;

    [HeaderAttribute("ATTACK")]
    public float detectionDistance;
    public float attackDistance;
    public float attackCooldown;
    public float projectileSpeed;
    public int attackStyle;
    public GameObject projectilePrefab;

    [HeaderAttribute("STATE")]
    public float durationIDLE;
    public float durationMOVE;
}