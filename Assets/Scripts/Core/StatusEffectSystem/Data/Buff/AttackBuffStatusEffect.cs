using UnityEngine;

/// <summary>
/// <c>AttackBuffStatusEffectData</c>는 공격력 버프 상태 효과의 구체적인 데이터를 정의하는 ScriptableObject입니다.
/// <c>StatusEffectData</c>를 상속받아 공통적인 상태 효과 정보를 포함합니다.
/// </summary>
/// <remarks>
/// Unity Editor에서 'SkillSystem/Status Effect Data/Attack Buff Effect' 메뉴를 통해 에셋을 생성할 수 있습니다.
/// </remarks>
[CreateAssetMenu(fileName = "001_AttackBuffStatusEffect", menuName = "StatusEffect/Buff/AttackPowerAdd")]
public class AttackBuffStatusEffectData : StatusEffectData
{
    [Header("Attack Buff Specifics")]
    [Tooltip("이 버프가 캐릭터의 공격력을 증가시킬 양입니다.")]
    public float attackPowerIncrease;

    // 필요에 따라 공격력 버프의 추가적인 속성 (예: 중첩 가능 여부, 비율 증가 등)을 여기에 정의할 수 있습니다.
}