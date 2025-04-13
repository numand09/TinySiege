using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "RTS/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public Sprite icon;
    public GameObject prefab;
    public int hp;
    public int damage;
    public float moveSpeed = 3f;      
    public float attackCooldown = 1f; 
}