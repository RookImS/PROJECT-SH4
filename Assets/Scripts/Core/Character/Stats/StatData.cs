using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New StatData", menuName = "Character/Stat Data")]
public class StatData : ScriptableObject
{
    [Header("Resources")]
    public float maxHealth;
    public float maxStamina;

    [Header("Offensive")]
    public float attackPower;
    public float attackRange;
    public float additionalDamageMultiplier;
    public float fixedAdditionalDamage;

    [Header("Defensive")]
    [Range(0f, 1f)] public float defense;

    [Header("Utility")]
    public float movementSpeed;
    public float rotationSpeed;
    public float attackSpeedMultiplier;

    [Header("Critical")]
    [Range(0f, 1f)] public float criticalHitChance; // 0.0 ~ 1.0 사이의 값 (0% ~ 100%)
    public float criticalDamageMultiplier;

    [Header("Skills & Effects")]
    [Range(0f, 1f)] public float cooldownReduction; // 0.0 ~ 1.0 사이의 값 (0% ~ 100%)
    [Range(0f, 1f)] public float statusResistance; // 0.0 ~ 1.0 사이의 값 (0% ~ 100%)

    public Dictionary<StatType, float> ToDictionary()
    {
        return new Dictionary<StatType, float>
        {
            { StatType.MaxHealth, maxHealth },
            { StatType.MaxStamina, maxStamina },
            { StatType.AttackPower, attackPower },
            { StatType.AttackRange, attackRange },
            { StatType.AdditionalDamageMultiplier, additionalDamageMultiplier },
            { StatType.FixedAdditionalDamage, fixedAdditionalDamage },
            { StatType.Defense, defense },
            { StatType.MovementSpeed, movementSpeed },
            { StatType.RotationSpeed, rotationSpeed },
            { StatType.AttackSpeedMultiplier, attackSpeedMultiplier },
            { StatType.CriticalHitChance, criticalHitChance },
            { StatType.CriticalDamageMultiplier, criticalDamageMultiplier },
            { StatType.CooldownReduction, cooldownReduction },
            { StatType.StatusResistance, statusResistance }
        };
    }
}
