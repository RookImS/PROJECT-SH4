[System.Serializable]
public enum ModifierType
{
    Flat = 100,       // 합연산
    PercentAdd = 200, // 퍼센트 합연산 
    PercentMult = 300 // 퍼센트 곱연산 
}

[System.Serializable]
public struct StatModifier
{
    public StatType StatType;
    public float Value;
    public ModifierType Type;

    public int Order => (int)Type;

    public StatModifier(StatType statType, float value, ModifierType type)
    {
        StatType = statType;
        Value = value;
        Type = type;
    }
}