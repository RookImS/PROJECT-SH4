using UnityEngine;

/// <summary>
/// <c>PoisonStatusEffectData</c>는 독 상태 효과의 구체적인 데이터를 정의하는 ScriptableObject입니다.
/// <c>StatusEffectData</c>를 상속받아 공통적인 상태 효과 정보를 포함합니다.
/// </summary>
/// <remarks>
/// Unity Editor에서 'SkillSystem/Status Effect Data/Poison Effect' 메뉴를 통해 에셋을 생성할 수 있습니다.
/// </remarks>
[CreateAssetMenu(fileName = "101_PoisonStatusEffect", menuName = "StatusEffect/Debuff/PoisonStatus")]
public class PoisonStatusEffectData : StatusEffectData
{
    [Header("Poison Specifics")]
    [Tooltip("독 효과가 대상에게 입힐 초당 피해량입니다.")]
    public float damagePerSecond;
    [Tooltip("독 효과가 적용될 총 틱 수입니다.")]
    public int totalTicks;
    [Tooltip("각 독 틱 사이의 간격 (초)입니다. 예를 들어 1초면 1초마다 피해를 입힙니다.")]
    public float tickInterval;

    // 필요에 따라 독 효과의 추가적인 속성 (예: 중첩 가능 여부, 해제 조건 등)을 여기에 정의할 수 있습니다.
}