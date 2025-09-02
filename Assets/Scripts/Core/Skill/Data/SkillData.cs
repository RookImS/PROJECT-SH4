using UnityEngine;

/// <summary>
/// 스킬의 기본적인 데이터
/// </summary>
public class SkillData : ScriptableObject
{
    [Header("General Skill Info")]
    [Tooltip("스킬 고유 ID")]
    public int skillId;
    [Tooltip("스킬 이름")]
    public string skillName;
    [Tooltip("스킬 설명")]
    [TextArea(3, 5)]
    public string description;
    [Tooltip("스킬 아이콘")]
    public Sprite skillIcon;
}
