using System.Collections.Generic;
using System;

public class StatSystem
{
    private readonly Dictionary<StatType, float> _baseStats = new();

    // 내부에서 소유하는 최종 스탯 딕셔너리(단일 소스)
    private readonly Dictionary<StatType, float> _finalStats = new();
    public IReadOnlyDictionary<StatType, float> FinalStats => _finalStats;

    public CurrentStats CurrentStats { get; private set; }

    private readonly List<StatModifier> _modifiers = new List<StatModifier>();

    // CurrentStats 이벤트를 구독하고 릴레이하는 방식으로 이벤트 전달
    public event Action<float> OnHealthChanged;
    public event Action<float> OnStaminaChanged;
    public event Action OnDeath;

    public StatSystem(StatData baseStatData)
    {
        _baseStats = baseStatData.ToDictionary();

        // 초기화: _finalStats 인스턴스는 재사용하여 참조 일관성을 유지
        foreach (var kv in _baseStats)
            _finalStats[kv.Key] = kv.Value;

        RecalculateFinalStats();
        CurrentStats = new CurrentStats(_finalStats);

        // 구독하고 릴레이하는 방식으로 이벤트 전달
        CurrentStats.OnHealthChanged += (hp) => OnHealthChanged?.Invoke(hp);
        CurrentStats.OnDeath += () => OnDeath?.Invoke();
        CurrentStats.OnStaminaChanged += (st) => OnStaminaChanged?.Invoke(st);
    }

    /// <summary>
    /// 스탯 변동이 있을 때마다 호출하여 최종 스탯을 재계산합니다.
    /// </summary>
    public void RecalculateFinalStats()
    {
        // 1. 모든 최종 스탯을 기본 스탯으로 초기화
        foreach (var stat in _baseStats)
        {
            _finalStats[stat.Key] = stat.Value;
        }

        // 2. _modifiers를 Order 기준으로 정렬
        _modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));

        Dictionary<StatType, float> percentAddTotals = new();

        // 3. 각 Modifier를 순서대로 적용
        foreach (var mod in _modifiers)
        {
            switch (mod.Type)
            {
                case ModifierType.Flat:
                    _finalStats[mod.StatType] += mod.Value;
                    break;

                case ModifierType.PercentAdd:
                    if (!percentAddTotals.ContainsKey(mod.StatType))
                        percentAddTotals[mod.StatType] = 0f;
                    percentAddTotals[mod.StatType] += mod.Value;
                    break;

                case ModifierType.PercentMult:
                    _finalStats[mod.StatType] *= 1 + mod.Value;
                    break;
            }
        }

        // 4. PercentAdd는 한 번에 적용
        foreach (var stats in percentAddTotals)
        {
            _finalStats[stats.Key] *= 1 + stats.Value;
        }

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

    public void AddModifiers(IEnumerable<StatModifier> modifiers)
    {
        _modifiers.AddRange(modifiers);
        RecalculateFinalStats();
    }

    public void RemoveModifiers(IEnumerable<StatModifier> modifiers)
    {
        foreach (var mod in modifiers)
            _modifiers.Remove(mod);
        RecalculateFinalStats();
    }

    public void TakeDamage(float damage)
    {
        CurrentStats.TakeDamage(damage);
    }

    public bool HasResource(StatType statType, float amount)
    {
        switch (statType)
        {
            case StatType.CurrentStamina:
                return CurrentStats.CurrentStamina >= amount;
            default:
                throw new NotImplementedException($"StatType '{statType}'에 대한 자원 확인이 구현되지 않았습니다.");
        }   
    }

    public void ConsumeResource(StatType statType, float amount)
    {
        switch (statType)
        {
            case StatType.CurrentStamina:
                CurrentStats.ConsumeStamina(amount);
                break;
            default:
                throw new NotImplementedException($"StatType '{statType}'에 대한 자원 소모가 구현되지 않았습니다.");
        }
    }
}