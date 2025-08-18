public enum ModifierType
{
    Flat = 100,       // 합연산
    PercentAdd = 200, // 퍼센트 합연산 
    PercentMult = 300 // 퍼센트 곱연산 
}

public readonly struct StatModifier
{
    public readonly float Value;
    public readonly ModifierType Type;

    public StatModifier(float value, ModifierType type)
    {
        Value = value;
        Type = type;
    }
}