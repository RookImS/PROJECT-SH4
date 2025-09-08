using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffectData : ScriptableObject
{
    [Header("Status Effect Info")]
    [Tooltip("상태 효과의 ID")]
    public int effectId;
    [Tooltip("상태 효과의 이름")]
    public string effectName;
    [Tooltip("UI에 표시될 상태 효과 아이콘")]
    public Sprite effectIcon;
    [Tooltip("상태 효과에 대한 상세 설명")]
    [TextArea(3, 5)]
    public string description;
    [Tooltip("상태 효과의 지속 시간 (초). -1이면 영구적")]
    public float duration;
    [Tooltip("변경될 스탯 목록")]
    public List<StatModifier> statModifiers;
}