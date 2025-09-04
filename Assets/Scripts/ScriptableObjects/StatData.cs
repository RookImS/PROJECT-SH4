using UnityEngine;

[CreateAssetMenu(fileName = "New StatData", menuName = "Character/Stat Data")]
public class StatData : ScriptableObject
{
    [Header("리소스 (Resources)")]
    public float maxHealth;
    public float maxStamina;

    [Header("공격 능력 (Offensive)")]
    public float attackPower;
    public float attackRange;
    public float additionalDamageMultiplier;
    public float fixedAdditionalDamage;
    
    [Header("방어 능력 (Defensive)")]
    public float defense;

    [Header("유틸리티 (Utility)")]
    public float movementSpeed;
    public float rotationSpeed;
    public float attackSpeedMultiplier;

    [Header("치명타 (Critical)")]
    [Range(0f, 1f)] public float criticalHitChance; // 0.0 ~ 1.0 사이의 값 (0% ~ 100%)
    public float criticalDamageMultiplier;

    [Header("스킬 및 효과 (Skills & Effects)")]
    [Range(0f, 1f)] public float cooldownReduction; // 0.0 ~ 1.0 사이의 값 (0% ~ 100%)
    [Range(0f, 1f)] public float statusResistance; // 0.0 ~ 1.0 사이의 값 (0% ~ 100%)
}
