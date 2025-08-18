using UnityEngine;
using System.Collections.Generic;

/* 액티브 스킬의 핵심 스탯을 수정하는 패시브 스킬 데이터 */
[CreateAssetMenu(fileName = "101_StatModfierPassive", menuName = "Skill/Passive/StatModfier")]
public class SkillStatModPassive : PassiveSkillData
{
    /* 패시브 스킬이 수정할 액티브 스킬 핵심 스탯 수정 목록 (수정스탯-방식-값) */
    [Header("Skill Stat Modification Details")]
    [Tooltip("수정스탯-방식-수치")]
    public List<SkillStatModEntry> statModifications = new List<SkillStatModEntry>();

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
    public override IRuntimePassiveEffect CreateRuntimeEffect()
    {
        return null;
    }

}
