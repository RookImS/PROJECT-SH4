using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 액티브/패시브 조합으로 생성되는 실제 사용 가능한 스킬 인스턴스
/// </summary>
public class RutimeSkill
{
    /// <summary>
    /// 원본 액티브 스킬 데이터 참조
    /// </summary>
    public ActiveSkillData activeSkillData;

    // 부착된 패시브 데이터 리스트(내부 참조/디버깅용)
    private List<PassiveSkillData> _attachedPassiveDatas;

    /// <summary>
    /// 패시브 스킬이 적용된 최종 스킬 능력치
    /// </summary>
    public RuntimeSkillData runtimeSkillData;

    
    /// <summary>
    /// 생성자. 액티브/패시브 조합으로 인스턴스 생성한다.
    /// </summary>
    public RutimeSkill(ActiveSkillData activeData, List<PassiveSkillData> attachedPassiveDatas)
    {
        activeSkillData = activeData;
        _attachedPassiveDatas = attachedPassiveDatas;

        // 1. 런타임 스킬 데이터 초기화
        runtimeSkillData = new RuntimeSkillData(activeData);

        // 2. 스탯 수정 패시브 적용
        ApplyPreExecutionModifiers(runtimeSkillData, attachedPassiveDatas);

        // 3. 이벤트 기반 패시브 런타임 스킬 생성 및 할당
        runtimeSkillData.attachedRuntimePassiveSkills = new List<IRuntimePassiveSkill>();
        foreach (var passiveData in attachedPassiveDatas)
        {
            if (passiveData is IRuntimePassiveSkillFactory factory)
            {
                IRuntimePassiveSkill runtimeSkill = factory.CreateRuntimeSkill();
                if (runtimeSkill != null)
                    runtimeSkillData.attachedRuntimePassiveSkills.Add(runtimeSkill);
            }
        }       
    }

    /// <summary>
    /// 부착된 모든 스탯 수정 패시브를 런타임 데이터에 적용한다.
    /// </summary>
    /// <param name="runtimeData">적용 대상 런타임 데이터</param>
    /// <param name="passiveDatas">부착된 패시브 리스트</param>
    private void ApplyPreExecutionModifiers(RuntimeSkillData runtimeData, List<PassiveSkillData> passiveDatas)
    {
        // 스탯 타입별로 보정 항목 그룹화
        Dictionary<SkillStatType, List<SkillStatModEntry>> groupedModEntries = new Dictionary<SkillStatType, List<SkillStatModEntry>>();
        runtimeData.appliedStatModifiers.Clear();

        foreach (var passiveData in passiveDatas)
        {
            // 스탯 수정 패시브만 처리
            if (passiveData is SkillStatModPassive skillStatModPassive)
            {
                foreach (var modEntry in skillStatModPassive.statModifications)
                {
                    if (!groupedModEntries.ContainsKey(modEntry.skillStatType))
                        groupedModEntries.Add(modEntry.skillStatType, new List<SkillStatModEntry>());
                    groupedModEntries[modEntry.skillStatType].Add(modEntry);
                    runtimeData.appliedStatModifiers.Add(modEntry);
                }
            }
        }

        // 최종 스탯 재계산
        runtimeData.RecalculateAllStats(groupedModEntries);
    }

    /// <summary>
    /// 캐릭터가 스킬을 사용할 때 호출한다.
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">대상(옵션)</param>
    public void UseSkill(Character caster, Character target = null)
    {
        // 쿨다운 적용
        caster.StartCooldown(runtimeSkillData.currentCoreStats.cooldown);
        Debug.Log($"캐릭터 '{caster.name}'가 스킬 '{activeSkillData.skillName}' 사용 시도. 쿨다운: {runtimeSkillData.currentCoreStats.cooldown}초.");

        // 모든 런타임 패시브 스킬에 OnSkillCast 호출
        foreach (var runtimeSkill in runtimeSkillData.attachedRuntimePassiveSkills)
            runtimeSkill.OnSkillCast(caster, this); // 스킬을 사용했을 떄 발동되어야하는 패시브 스킬을 처리하는건고

        // 실제 공격 로직(더미)
        Debug.Log($"'{activeSkillData.skillName}' 스킬 발동!");

        
        // 공격 타입별 더미 분기 (실제 액티브 스킬이 처리해야되는 부분)
        if (activeSkillData.skillAttackType == SkillAttackType.Projectile)
        {
            Debug.Log($"투사체 발사! 속도: {runtimeSkillData.currentProjectileSpeed}, 크기: {runtimeSkillData.currentProjectileSize}, 사거리: {runtimeSkillData.currentCoreStats.range}");
            // 실제 투사체 생성/이동 로직은 별도 구현
        }
        else if (activeSkillData.skillAttackType == SkillAttackType.AreaOfEffect)
        {
            Debug.Log($"AOE 스킬 발동! 반경: {runtimeSkillData.currentAoeRadius}, 지연 시간: {runtimeSkillData.currentAoeDelay}, 사거리: {runtimeSkillData.currentCoreStats.range}");
            // 실제 AOE 효과 생성/적용 로직은 별도 구현
        }
        else if (activeSkillData.skillAttackType == SkillAttackType.Melee)
        {
            Debug.Log($"근접 스킬 발동! 아크 각도: {runtimeSkillData.currentMeleeArcAngle}, 사거리: {runtimeSkillData.currentCoreStats.range}");
            //실제 근접 공격 로직은 별도 구현
        }
        
    }

  

    /// <summary>
    /// 스킬이 대상에게 적중했을 때 호출한다.
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">대상</param>
    /// <param name="damageDealt">실제 피해량</param>
    public void OnSkillHit(Character caster, Character target, float damageDealt)
    {
        Debug.Log($"'{activeSkillData.skillName}' 스킬이 '{target.name}'에게 적중! 예상 피해: {damageDealt}");

        // 모든 런타임 패시브 스킬에 OnSkillHit 호출
        foreach (var runtimeSkill in runtimeSkillData.attachedRuntimePassiveSkills)
            runtimeSkill.OnSkillHit(caster, target, this, damageDealt);

        // 실제 피해 적용 로직은 별도 구현
    }

    /// <summary>
    /// 스킬의 최종 피해량 계산한다.
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">대상</param>
    /// <returns>최종 피해량</returns>
    public float CalculateFinalDamage(Character caster, Character target)
    {
        float finalDamage = 0f;

        // 1. 기본+계수 데미지
        finalDamage = runtimeSkillData.currentCoreStats.baseDamage + (caster.AttackPower * runtimeSkillData.currentCoreStats.characterAttackPowerMultiplier);
        Debug.Log($"1단계 기본+계수 데미지: {finalDamage}");

        // 2. 방어력 적용
        finalDamage *= (1.0f - target.DefenseReductionRate);
        Debug.Log($"2단계 방어력 적용 후 데미지: {finalDamage}");

        // 3. 고정 추가 데미지
        finalDamage += runtimeSkillData.currentCoreStats.flatDamageAdded;
        Debug.Log($"3단계 고정 추가 데미지 합산 후: {finalDamage}");

        // 4. 캐릭터 추가 피해 계수
        finalDamage *= (1 + caster.BonusDamageMultiplier);
        Debug.Log($"4단계 캐릭터 추가 피해 계수 적용 후: {finalDamage}");

        // 5~6. 치명타 확률/배율 적용
        float combinedCritChance = runtimeSkillData.currentCoreStats.baseCriticalChance + caster.CriticalChance;
        float combinedCritDamageMultiplier = runtimeSkillData.currentCoreStats.baseCriticalDamageMultiplier + (caster.CriticalDamageMultiplier - 1.0f);
        bool isCriticalHit = UnityEngine.Random.value < combinedCritChance;
        Debug.Log($"최종 치명타 확률: {combinedCritChance * 100}%, 치명타 발생: {isCriticalHit}");

        if (isCriticalHit)
        {
            finalDamage *= combinedCritDamageMultiplier;
            Debug.Log($"6단계 치명타 발생! 최종 치명타 피해 배율 {combinedCritDamageMultiplier} 적용 후 데미지: {finalDamage}");
        }

        return finalDamage;
    }
}