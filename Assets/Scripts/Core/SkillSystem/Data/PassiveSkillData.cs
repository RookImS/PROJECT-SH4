using UnityEngine;

/* 패시브 스킬의 기본적인 데이터 */
public class PassiveSkillData : SkillData, IRuntimePassiveEffectFactory
{
    /* 패시브 스킬 데이터*/
    [Header("Passive Specifics")]
    /* 패시브에 의해 부여되는 상태 효과 (버프 & 디버프) 데이터*/
    [Header("Status Effect (Buff & Debuff)")]
    [Tooltip("상태이상(버프 또는 디버프) 데이터")]
    StatusEffectData statusEffectToApply;

    /*
        # 함수
        IRuntimePassiveEffect CreateRuntimeEffect()
        
        # 주요 로직
        스킬 인스턴스에 독립적인 런타임 패시브 효과 로직을 부여한다.
        실제 런타임 패시브 효과 (IRuntimePassiveEffect) 인스턴스를 생성하여 반환한다.

        # 매개변수
    
        # 반환값
        이 패시브 데이터에 대한 고유한 런타임 효과 인스턴스 반환
        (고유한 효과가 없다면 null)
    */
    public virtual IRuntimePassiveEffect CreateRuntimeEffect()
    {
        return null;
    }
}
