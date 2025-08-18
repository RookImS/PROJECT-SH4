using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 패시브 효과가 적용된 최종 스킬 능력치를 담는 런타임 데이터 클래스
/// </summary>
public class RuntimeSkillData
{
    /// <summary>
    /// 원본 액티브 스킬 데이터 참조
    /// </summary>
    public ActiveSkillData baseActiveData;

    /// <summary>
    /// 적용된 스탯 보정 목록
    /// </summary>
    public List<SkillStatModEntry> appliedStatModifiers;

    /// <summary>
    /// 부착된 런타임 패시브 효과 목록
    /// </summary>
    public List<IRuntimePassiveEffect> attachedRuntimePassiveEffects;

    /// <summary>
    /// 최종 계산된 스킬 핵심 스탯
    /// </summary>
    public CoreSkillStats currentCoreStats;

    // 아래 값들은 패시브에 의해 직접 수정되지 않는 고정 특성 (투사체, AOE 스킬 관련)
    public float currentProjectileSpeed;      // 투사체 속도
    public bool currentCanPierceTargets;      // 관통 여부
    public int currentMaxTargetsHit;          // 관통 가능한 적 수
    public float currentAoeDelay;             // AOE 지연 시간
    public float currentAoeDuration;          // AOE 지속 시간
    public float currentAoeHitInterval;       // AOE 타격 간격

    // EffectMagnitude가 적용된 최종 크기/반경/각도
    public float currentProjectileSize;       // 투사체 크기
    public float currentAoeRadius;            // AOE 반경
    public float currentMeleeArcAngle;        // 근접 공격 각도

    
    /// <summary>
    /// 생성자. 원본 액티브 스킬 데이터로 초기화
    /// </summary>
    /// <param name="activeData">원본 액티브 스킬 데이터</param>
    public RuntimeSkillData(ActiveSkillData activeData)
    {
        baseActiveData = activeData;
        if (baseActiveData == null)
        {
            Debug.LogError("RuntimeSkillData 생성 시 baseActiveData가 null입니다.");
            return;
        }

        // 적용된 스탯 보정 목록 및 부착된 런타임 패시브 효과 목록초기화
        appliedStatModifiers = new List<SkillStatModEntry>();
        attachedRuntimePassiveEffects = new List<IRuntimePassiveEffect>();
        
        // 핵심 스탯 초기화
        currentCoreStats = activeData.coreStats;  

        // 고정 특성 초기화
        currentProjectileSpeed = activeData.projectileSpeed;
        currentCanPierceTargets = activeData.canPierceTargets;
        currentMaxTargetsHit = activeData.maxTargetsHit;
        currentAoeDelay = activeData.aoeDelay;
        currentAoeDuration = activeData.aoeDuration;
        currentAoeHitInterval = activeData.aoeHitInterval;

        // 파생 스탯 초기화
        UpdateDerivedEffectMagnitudes();
    }

    /// <summary>
    /// 모든 패시브 스탯 보정을 적용해 최종 스탯을 재계산한다.
    /// </summary>
    /// <param name="groupedModEntries">스탯 타입별 보정 목록</param>
    public void RecalculateAllStats(Dictionary<SkillStatType, List<SkillStatModEntry>> groupedModEntries)
    {
        currentCoreStats = baseActiveData.coreStats; // 원본으로 초기화

        foreach (SkillStatType statType in Enum.GetValues(typeof(SkillStatType)))
        {
            if (!groupedModEntries.TryGetValue(statType, out List<SkillStatModEntry> mods))
                continue;

            // 보정 타입별로 분류
            List<float> additiveMods = new List<float>();
            List<float> multiplicativeMods = new List<float>();
            List<float> overrideMods = new List<float>();

            foreach (var mod in mods)
            {
                if (mod.skillStatModType == SkillStatModType.Override)
                    overrideMods.Add(mod.value);
                else if (mod.skillStatModType == SkillStatModType.Additive)
                    additiveMods.Add(mod.value);
                else if (mod.skillStatModType == SkillStatModType.Multiplicative)
                    multiplicativeMods.Add(mod.value);
            }

            // 스탯별로 재계산
            switch (statType)
            {
                case SkillStatType.BaseDamage:
                    RecalculateSingleStat(ref currentCoreStats.baseDamage, baseActiveData.coreStats.baseDamage, additiveMods, multiplicativeMods, overrideMods, statType);
                    break;
                case SkillStatType.Cooldown:
                    RecalculateSingleStat(ref currentCoreStats.cooldown, baseActiveData.coreStats.cooldown, additiveMods, multiplicativeMods, overrideMods, statType);
                    break;
                case SkillStatType.CastTime:
                    RecalculateSingleStat(ref currentCoreStats.castTime, baseActiveData.coreStats.castTime, additiveMods, multiplicativeMods, overrideMods, statType);
                    break;
                case SkillStatType.ResourceCost:
                    RecalculateSingleStat(ref currentCoreStats.resourceCost, baseActiveData.coreStats.resourceCost, additiveMods, multiplicativeMods, overrideMods, statType);
                    break;
                case SkillStatType.CharacterAttackPowerMultiplier:
                    RecalculateSingleStat(ref currentCoreStats.characterAttackPowerMultiplier, baseActiveData.coreStats.characterAttackPowerMultiplier, additiveMods, multiplicativeMods, overrideMods, statType);
                    break;
                case SkillStatType.FlatDamageAdded:
                    RecalculateSingleStat(ref currentCoreStats.flatDamageAdded, baseActiveData.coreStats.flatDamageAdded, additiveMods, multiplicativeMods, overrideMods, statType);
                    break;
                case SkillStatType.BaseCriticalChance:
                    RecalculateSingleStat(ref currentCoreStats.baseCriticalChance, baseActiveData.coreStats.baseCriticalChance, additiveMods, multiplicativeMods, overrideMods, statType);
                    break;
                case SkillStatType.BaseCriticalDamageMultiplier:
                    RecalculateSingleStat(ref currentCoreStats.baseCriticalDamageMultiplier, baseActiveData.coreStats.baseCriticalDamageMultiplier, additiveMods, multiplicativeMods, overrideMods, statType);
                    break;
                case SkillStatType.Range:
                    RecalculateSingleStat(ref currentCoreStats.range, baseActiveData.coreStats.range, additiveMods, multiplicativeMods, overrideMods, statType);
                    break;
                case SkillStatType.EffectMagnitude:
                    RecalculateSingleStat(ref currentCoreStats.effectMagnitude, baseActiveData.coreStats.effectMagnitude, additiveMods, multiplicativeMods, overrideMods, statType);
                    UpdateDerivedEffectMagnitudes(); // EffectMagnitude 변경 시 파생 스탯도 갱신
                    break;
                case SkillStatType.HitCount:
                    RecalculateSingleIntStat(ref currentCoreStats.hitCount, baseActiveData.coreStats.hitCount, additiveMods, multiplicativeMods, overrideMods, statType);
                    break;
                default:
                    Debug.LogWarning($"Unknown or unhandled modifiable stat type in RecalculateAllStats: {statType}.");
                    break;
            }
        }
    }

    /// <summary>
    /// float 스탯에 합연산, 곱연산, 덮어쓰기를 적용해 최종값 계산
    /// </summary>
    private void RecalculateSingleStat(ref float targetStat, float baseValue, List<float> additiveMods, List<float> multiplicativeMods, List<float> overrideMods, SkillStatType statType)
    {
        // 합연산 적용
        float calculatedValue = baseValue;
        foreach (float mod in additiveMods)
            calculatedValue += mod;

        // 곱연산 적용
        float multiplicativeFactor = 1.0f;
        foreach (float mod in multiplicativeMods)
            multiplicativeFactor *= (1.0f + mod);
        calculatedValue *= multiplicativeFactor;

        // 덮어쓰기 중 가장 유리한 값 선택
        float? bestOverrideValue = null;
        if (overrideMods != null && overrideMods.Any())
        {
            bestOverrideValue = overrideMods[0];
            for (int i = 1; i < overrideMods.Count; i++)
            {
                if (IsValueBetter(statType, bestOverrideValue.Value, overrideMods[i]))
                    bestOverrideValue = overrideMods[i];
            }
        }

        // 최종값 결정
        if (bestOverrideValue.HasValue)
        {
            if (IsValueBetter(statType, calculatedValue, bestOverrideValue.Value))
                targetStat = bestOverrideValue.Value;
            else
                targetStat = calculatedValue;
        }
        else
        {
            targetStat = calculatedValue;
        }
    }

    /// <summary>
    /// int 스탯에 합연산, 곱연산, 덮어쓰기를 적용해 최종값 계산
    /// </summary>
    private void RecalculateSingleIntStat(ref int targetStat, int baseValue, List<float> additiveMods, List<float> multiplicativeMods, List<float> overrideMods, SkillStatType statType)
    {
        // float으로 계산 후 int로 변환
        float calculatedFloatValue = baseValue;
        foreach (float mod in additiveMods)
            calculatedFloatValue += mod;

        float multiplicativeFactor = 1.0f;
        foreach (float mod in multiplicativeMods)
            multiplicativeFactor *= (1.0f + mod);
        calculatedFloatValue *= multiplicativeFactor;

        float? bestOverrideValue = null;
        if (overrideMods != null && overrideMods.Any())
        {
            bestOverrideValue = overrideMods[0];
            for (int i = 1; i < overrideMods.Count; i++)
            {
                if (IsValueBetter(statType, bestOverrideValue.Value, overrideMods[i]))
                    bestOverrideValue = overrideMods[i];
            }
        }

        if (bestOverrideValue.HasValue)
        {
            if (IsValueBetter(statType, calculatedFloatValue, bestOverrideValue.Value))
                targetStat = Mathf.RoundToInt(bestOverrideValue.Value);
            else
                targetStat = Mathf.RoundToInt(calculatedFloatValue);
        }
        else
        {
            targetStat = Mathf.RoundToInt(calculatedFloatValue);
        }
    }

    /// <summary>
    /// EffectMagnitude에 따라 파생 스탯(크기, 반경, 각도) 갱신
    /// </summary>
    private void UpdateDerivedEffectMagnitudes()
    {
        currentProjectileSize = baseActiveData.baseProjectileSize * currentCoreStats.effectMagnitude;
        currentAoeRadius = baseActiveData.baseAoeRadius * currentCoreStats.effectMagnitude;
        currentMeleeArcAngle = baseActiveData.baseMeleeArcAngle * currentCoreStats.effectMagnitude;
    }

    /// <summary>
    /// statType 기준으로 newValue가 currentValue보다 유리한지 판단
    /// </summary>
    private bool IsValueBetter(SkillStatType statType, float currentValue, float newValue)
    {
        switch (statType)
        {
            // 높을수록 좋은 스탯
            case SkillStatType.BaseDamage:
            case SkillStatType.CharacterAttackPowerMultiplier:
            case SkillStatType.FlatDamageAdded:
            case SkillStatType.BaseCriticalChance:
            case SkillStatType.BaseCriticalDamageMultiplier:
            case SkillStatType.EffectMagnitude:
            case SkillStatType.Range:
            case SkillStatType.HitCount:
                return newValue > currentValue;
            // 낮을수록 좋은 스탯
            case SkillStatType.Cooldown:
            case SkillStatType.CastTime:
            case SkillStatType.ResourceCost:
                return newValue < currentValue;
            default:
                Debug.LogWarning($"IsValueBetter: Unknown or unhandled SkillStatType for comparison: {statType}");
                return false;
        }
    }
}