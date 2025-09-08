using System.Collections.Generic;
using UnityEngine;

public interface ISkillUser { 
    Transform Transform { get; }          // 위치/방향 참조(사거리·각도 계산)
    bool IsSkillAvailable { get; }               // 사망/침묵/기절/행동불가 등 상태이상 체크용
}