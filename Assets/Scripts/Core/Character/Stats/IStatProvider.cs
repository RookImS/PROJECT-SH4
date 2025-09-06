using System.Collections.Generic;

/// <summary>
/// 스탯 제공자 인터페이스
/// </summary>
public interface IStatProvider
{
    // IReadOnlyDictionary<StatType, float> FinalStats { get; }
    bool TryGetStat(StatType type, out float value);
    float this[StatType type] { get; }

    // Stat modifier 관련 API
    void AddModifier(StatModifier modifier);
    void RemoveModifier(StatModifier modifier);

    // 여러 modifier 한 번에 추가/제거
    void AddModifiers(IEnumerable<StatModifier> modifiers);
    void RemoveModifiers(IEnumerable<StatModifier> modifiers);
}
