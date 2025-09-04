using System.Collections.Generic;

public class StatSystem
{
    private StatData _baseStatData;
    public FinalStats FinalStats { get; private set; }
    public CurrentStats CurrentStats { get; private set; }

    private readonly List<StatModifier> _modifiers = new List<StatModifier>();

    public StatSystem(StatData baseStatData)
    {
        _baseStatData = baseStatData;

        FinalStats = new FinalStats();
        RecalculateFinalStats();
        CurrentStats = new CurrentStats(FinalStats);
    }

    public void RecalculateFinalStats()
    {
        // 1. 모든 최종 스탯을 기본 스탯으로 초기화
        FinalStats.maxHealth = _baseStatData.maxHealth;
        FinalStats.maxStamina = _baseStatData.maxStamina;
        FinalStats.attackPower = _baseStatData.attackPower;
        FinalStats.attackRange = _baseStatData.attackRange;
        FinalStats.additionalDamageMultiplier = _baseStatData.additionalDamageMultiplier;
        FinalStats.fixedAdditionalDamage = _baseStatData.fixedAdditionalDamage;
        FinalStats.defense = _baseStatData.defense;
        FinalStats.movementSpeed = _baseStatData.movementSpeed;
        FinalStats.rotationSpeed = _baseStatData.rotationSpeed;
        FinalStats.attackSpeedMultiplier = _baseStatData.attackSpeedMultiplier;
        FinalStats.criticalHitChance = _baseStatData.criticalHitChance;
        FinalStats.criticalDamageMultiplier = _baseStatData.criticalDamageMultiplier;
        FinalStats.cooldownReduction = _baseStatData.cooldownReduction;
        FinalStats.statusResistance = _baseStatData.statusResistance;

        // 2. 추후 여기에 _modifiers 리스트를 순회하며 스탯을 변경하는 로직 추가
        // 예: _modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
        //     foreach (var mod in _modifiers) { ... }

        CurrentStats?.UpdateMaxValues();
    }

    public void AddModifier(StatModifier modifier)
    {
        _modifiers.Add(modifier);
        RecalculateFinalStats();
    }

    public void RemoveModifier(StatModifier modifier)
    { 
        _modifiers.Remove(modifier);
        RecalculateFinalStats();
    }

    public void TakeDamage(float damage)
    {
        CurrentStats.TakeDamage(damage);
    }
}