using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 액티브 스킬의 핵심 스탯을 수정하는 패시브 스킬 데이터
/// </summary>
[CreateAssetMenu(fileName = "101_StatModfierPassive", menuName = "Skill/Passive/StatModfier")]
public class SkillStatModPassive : PassiveSkillData
{
    /* 패시브 스킬이 수정할 액티브 스킬 핵심 스탯 수정 목록 (수정스탯-방식-값) */
    [Header("Skill Stat Modification Details")]
    [Tooltip("수정스탯-방식-수치")]
    public List<SkillStatModEntry> statModifications = new List<SkillStatModEntry>();

}
