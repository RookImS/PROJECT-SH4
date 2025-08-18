using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusEffectData", menuName = "SkillSystem/Status Effect Data")]
public class StatusEffectData : ScriptableObject
{
    [Header("Status Effect Info")]
    [Tooltip("상태 효과의 이름입니다.")]
    public string effectName;
    [Tooltip("UI에 표시될 상태 효과 아이콘입니다.")]
    public Sprite effectIcon;
    [Tooltip("상태 효과에 대한 상세 설명입니다.")]
    [TextArea(3, 5)]
    public string description;
    [Tooltip("상태 효과의 지속 시간입니다 (초). 0이면 영구적입니다.")]
    public float duration;
}