using UnityEngine;

[System.Serializable]   
public enum StatType
{
    // 리소스 (Resources)
    MaxHealth,
    MaxStamina,

    // 공격 능력 (Offensive)
    AttackPower,
    AttackRange,
    AdditionalDamageMultiplier,
    FixedAdditionalDamage,

    // 방어 능력 (Defensive)
    Defense,

    // 유틸리티 (Utility)
    MovementSpeed,
    RotationSpeed,
    AttackSpeedMultiplier,

    // 치명타 (Critical)
    CriticalHitChance,
    CriticalDamageMultiplier,

    // 스킬 및 효과 (Skills & Effects)
    CooldownReduction,
    StatusResistance
}
