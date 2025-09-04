using UnityEngine;

public enum MonsterRole    { Chaser, Kiter, Bruiser, Bomber }
public enum AttackKind     { None, Melee, Ranged, AoE, Suicide }

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("Identity")]
    public int id;
    public string name;
    public GameObject prefab;
    
    [Header("Basic Stats")]
    public int contactDamage;
    public float speed;
    
    [Header("Combat Stats")]
    public float range;
    public float attackDamage;
    public float attackRate;
    
    [Header("Resources")]
    public GameObject projectilePrefab;
}
