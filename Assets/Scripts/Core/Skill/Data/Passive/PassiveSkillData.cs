using UnityEngine;

/// <summary>
/// 패시브 스킬의 기본적인 데이터
/// </summary>
public abstract class PassiveSkillData : SkillData
{
    [Header("Passive Specifics")]
    /* 패시브에 의해 부여되는 상태 효과 (버프 & 디버프) 데이터*/
    [Header("Status Effect (Buff & Debuff)")]
    [Tooltip("상태이상(버프 또는 디버프) 데이터")]
    StatusEffectData statusEffectToApply;

}
