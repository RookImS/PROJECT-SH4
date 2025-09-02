using UnityEngine;
using System;


/* 액티브 스킬 핵심 스탯 */
[System.Serializable]
public struct CoreSkillStats
{
    public float baseDamage; // 기본 피해량
    public float cooldown; // 재사용 대기시간
    public float castTime; // 시전 시간
    public float resourceCost; // 자원 소모량
    public float characterAttackPowerMultiplier; // 캐릭터 공격력 계수
    public float flatDamageAdded; // 추가 고정 피해량 (방어무시)
    public float baseCriticalChance; // 기본 치명타 확률
    public float baseCriticalDamageMultiplier; // 기본 치명타 피해 배율
    public int hitCount;            // 스킬 타수
    public float effectMagnitude;   // 스킬 효과의 공통 크기/반경/각도 배율 (1.0 = 기본)
    public float range;             // 스킬 발동 유효 거리
}

/* 수정 가능한 스킬 스텟 목록 */
public enum SkillStatType
{
    // CoreSkillStats에 포함된 스탯 중 패시브로 수정 가능한 것들
    BaseDamage,                     // 기본 피해량
    Cooldown,                       // 재사용 대기시간
    CastTime,                       // 시전 시간
    ResourceCost,                   // 자원 소모량
    CharacterAttackPowerMultiplier, // 캐릭터 공격력 계수
    FlatDamageAdded,                // 고정 추가 피해량
    BaseCriticalChance,             // 기본 치명타 확률
    BaseCriticalDamageMultiplier,   // 기본 치명타 피해 배율
    HitCount,                       // 스킬 타수
    EffectMagnitude,                // 스킬 효과의 크기/반경/각도를 조절하는 공통 배율
    Range,                          // 스킬 발동 유효 거리
}

/* 스킬 스텟 수정 방식 */
public enum SkillStatModType
{
    Additive,       // 값을 더하는 방식 (예: +50 데미지, -2sec 쿨다운)
    Multiplicative, // 값을 비율로 곱하는 방식 (예: +0.1은 1.1배 / -0.1는 0.9배로 계산)
    Override,       // 값을 완전히 덮어쓰는 방식
}

/* 실제 스텟에 사용되는 구조체 (수정스탯-방식-값) */
[System.Serializable]
public struct SkillStatModEntry
{
    public SkillStatType skillStatType; // 수정 스탯
    public SkillStatModType skillStatModType; // 수정 방식
    public float value; // 수정 값
}

/* 액티브 공격 타입 목록 */
public enum SkillAttackType
{
    Projectile, // 투사체
    AreaOfEffect, // 범위
    Melee, // 근접
}