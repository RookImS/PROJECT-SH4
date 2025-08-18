using UnityEngine;
using System.Collections.Generic;

public class RuntimeSkillData
{
    // 핵심 능력치 (CoreSkillStats 구조체로 묶음)
    public CoreSkillStats currentCoreStats; // 패시브가 적용된 최종 핵심 스탯

    public ActiveSkillData baseActiveData; // 원본 ActiveSkillData 참조

    // 기타 타입별 현재 스탯들 (개별 유지) - 이 스탯들은 패시브에 의해 직접 수정되지 않음
    public float currentProjectileSpeed;
    public bool currentCanPierceTargets;
    public int currentMaxTargetsHit;
    public float currentAoeDelay;
    public float currentAoeDuration;
    public float currentAoeHitInterval;
    public string currentAnimationTriggerName;

    // EffectMagnitude가 적용된 최종 타입별 크기/반경/각도 값 (계산된 속성)
    /// <summary>
    /// 투사체 스킬의 최종 크기입니다. <see cref="SkillAttackType.Projectile"/>일 때 유효합니다.
    /// <c>baseActiveData.baseProjectileSize</c>에 <c>currentCoreStats.effectMagnitude</c>를 곱하여 결정됩니다.
    /// </summary>
    public float currentProjectileSize;

    /// <summary>
    /// AOE 스킬의 최종 반경입니다. <see cref="SkillAttackType.AreaOfEffect"/>일 때 유효합니다.
    /// <c>baseActiveData.baseAoeRadius</c>에 <c>currentCoreStats.effectMagnitude</c>를 곱하여 결정됩니다.
    /// </summary>
    public float currentAoeRadius;

    /// <summary>
    /// 근접 스킬의 최종 공격 아크 각도입니다. <see cref="SkillAttackType.Melee"/>일 때 유효합니다.
    /// <c>baseActiveData.baseMeleeArcAngle</c>에 <c>currentCoreStats.effectMagnitude</c>를 곱하여 결정됩니다.
    /// </summary>
    public float currentMeleeArcAngle;


    public List<IRuntimePassiveEffect> attachedRuntimePassiveEffects;

    /// <summary>
    /// <c>RuntimeSkillData</c>의 새로운 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="activeData">이 런타임 스킬 데이터가 기반할 <see cref="ActiveSkillData"/> 원본입니다.</param>
    /// <remarks>
    /// 생성 시, <paramref name="activeData"/>의 기본값으로 <c>currentCoreStats</c>를 직접 복사하고,
    /// 기타 타입별 스탯들을 초기화합니다.
    /// </remarks>
    public RuntimeSkillData(ActiveSkillData activeData)
    {
        baseActiveData = activeData;

        // ActiveSkillData의 coreStats 값을 RuntimeSkillData의 currentCoreStats로 직접 복사합니다.
        // CoreSkillStats는 struct(값 타입)이므로 이 할당은 깊은 복사(deep copy)를 수행합니다.
        currentCoreStats = activeData.coreStats;

        // 기타 타입별 스탯 초기화 (ActiveSkillData에서 직접 복사)
        currentProjectileSpeed = activeData.projectileSpeed;
        currentCanPierceTargets = activeData.canPierceTargets;
        currentMaxTargetsHit = activeData.maxTargetsHit;
        currentAoeDelay = activeData.aoeDelay;
        currentAoeDuration = activeData.aoeDuration;
        currentAoeHitInterval = activeData.aoeHitInterval;
        currentAnimationTriggerName = activeData.animationTriggerName;

        // EffectMagnitude에 의해 결정되는 최종 크기/반경/각도 초기화
        // baseActiveData의 기본값에 currentCoreStats.effectMagnitude를 곱하여 계산
        currentProjectileSize = activeData.baseProjectileSize * currentCoreStats.effectMagnitude;
        currentAoeRadius = activeData.baseAoeRadius * currentCoreStats.effectMagnitude;
        currentMeleeArcAngle = activeData.baseMeleeArcAngle * currentCoreStats.effectMagnitude;

        attachedRuntimePassiveEffects = new List<IRuntimePassiveEffect>();
    }

    /// <summary>
    /// 단일 스탯 수정 항목(<see cref="SkillStatModEntry"/>)을 이 <c>RuntimeSkillData</c>에 적용합니다.
    /// </summary>
    /// <param name="modEntry">적용할 스탯 수정 항목입니다.</param>
    /// <remarks>
    /// 이 메서드는 <see cref="SkillStatType"/>에 따라 적절한 스탯 필드를 찾아 지정된 <see cref="SkillStatModType"/> 방식으로 값을 수정합니다.
    /// <c>EffectMagnitude</c>가 변경되면 종속된 최종 타입별 크기/반경/각도 스탯들도 함께 업데이트됩니다.
    /// </remarks>
    public void ApplyStatModifier(SkillStatModEntry modEntry)
    {
        switch (modEntry.skillStatType)
        {
            // CoreSkillStats 내의 float 스탯들
            case SkillStatType.BaseDamage:
                ApplyValue(ref currentCoreStats.baseDamage, modEntry.value, modEntry.skillStatModType);
                break;
            case SkillStatType.Cooldown:
                ApplyValue(ref currentCoreStats.cooldown, modEntry.value, modEntry.skillStatModType);
                break;
            case SkillStatType.CastTime:
                ApplyValue(ref currentCoreStats.castTime, modEntry.value, modEntry.skillStatModType);
                break;
            case SkillStatType.ResourceCost:
                ApplyValue(ref currentCoreStats.resourceCost, modEntry.value, modEntry.skillStatModType);
                break;
            case SkillStatType.CharacterAttackPowerMultiplier:
                ApplyValue(ref currentCoreStats.characterAttackPowerMultiplier, modEntry.value, modEntry.skillStatModType);
                break;
            case SkillStatType.FlatDamageAdded:
                ApplyValue(ref currentCoreStats.flatDamageAdded, modEntry.value, modEntry.skillStatModType);
                break;
            case SkillStatType.BaseCriticalChance:
                ApplyValue(ref currentCoreStats.baseCriticalChance, modEntry.value, modEntry.skillStatModType);
                break;
            case SkillStatType.BaseCriticalDamageMultiplier:
                ApplyValue(ref currentCoreStats.baseCriticalDamageMultiplier, modEntry.value, modEntry.skillStatModType);
                break;
            case SkillStatType.Range: // Range 스탯 수정 로직
                ApplyValue(ref currentCoreStats.range, modEntry.value, modEntry.skillStatModType);
                break;
            case SkillStatType.EffectMagnitude: // EffectMagnitude 스탯 수정 로직
                ApplyValue(ref currentCoreStats.effectMagnitude, modEntry.value, modEntry.skillStatModType);
                // EffectMagnitude가 변경되면, 종속된 최종 타입별 스탯들도 업데이트
                currentProjectileSize = baseActiveData.baseProjectileSize * currentCoreStats.effectMagnitude;
                currentAoeRadius = baseActiveData.baseAoeRadius * currentCoreStats.effectMagnitude;
                currentMeleeArcAngle = baseActiveData.baseMeleeArcAngle * currentCoreStats.effectMagnitude;
                break;

            // CoreSkillStats 내의 int 스탯들
            case SkillStatType.HitCount:
                ApplyValue(ref currentCoreStats.hitCount, modEntry.value, modEntry.skillStatModType);
                break;

            default:
                Debug.LogWarning($"Unhandled SkillStatType: {modEntry.skillStatType}. This stat is not designed to be modified by generic stat modification passives.");
                break;
        }
    }

    /// <summary>
    /// float 타입 스탯에 지정된 값과 수정 방식을 적용합니다.
    /// </summary>
    /// <param name="targetStat">수정될 대상 스탯 (참조로 전달되어 직접 수정됨).</param>
    /// <param name="modifierValue">적용할 수정 값.</param>
    /// <param name="modType">수정 방식 (<see cref="SkillStatModType"/>).</param> // SkillModType -> SkillStatModType 변경
    private void ApplyValue(ref float targetStat, float modifierValue, SkillStatModType modType) // SkillModType -> SkillStatModType 변경
    {
        switch (modType)
        {
            case SkillStatModType.Additive: // SkillModType -> SkillStatModType 변경
                targetStat += modifierValue;
                break;
            case SkillStatModType.Multiplicative: // SkillModType -> SkillStatModType 변경
                // modifierValue가 0.1이면 1.1배, -0.1이면 0.9배 적용
                targetStat *= (1f + modifierValue);
                break;
            case SkillStatModType.Override: // SkillModType -> SkillStatModType 변경
                targetStat = modifierValue;
                break;
        }
    }

    /// <summary>
    /// int 타입 스탯에 지정된 값과 수정 방식을 적용합니다.
    /// </summary>
    /// <param name="targetStat">수정될 대상 스탯 (참조로 전달되어 직접 수정됨).</param>
    /// <param name="modifierValue">적용할 수정 값.</param>
    /// <param name="modType">수정 방식 (<see cref="SkillStatModType"/>).</param> // SkillModType -> SkillStatModType 변경
    private void ApplyValue(ref int targetStat, float modifierValue, SkillStatModType modType) // SkillModType -> SkillStatModType 변경
    {
        switch (modType)
        {
            case SkillStatModType.Additive: // SkillModType -> SkillStatModType 변경
                targetStat += Mathf.RoundToInt(modifierValue); // 정수로 반올림하여 적용
                break;
            case SkillStatModType.Multiplicative: // SkillModType -> SkillStatModType 변경
                // int 타입도 float과 동일한 비율로 적용 후 정수로 반올림
                targetStat = Mathf.RoundToInt(targetStat * (1f + modifierValue));
                break;
            case SkillStatModType.Override: // SkillModType -> SkillStatModType 변경
                targetStat = Mathf.RoundToInt(modifierValue);
                break;
        }
    }
}