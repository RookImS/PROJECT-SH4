using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillCaster : MonoBehaviour
{
    // TODO: 테스트용, 추후 InventoryManager 등에서 관리
    [Header("SkillTest Data Setup")]
    public ActiveSkillData activeSkillToTest;
    public List<PassiveSkillData> passiveSkillsToApply;
    public CharacterBase targetCharacter;

    public List<RuntimeSkill> EquippedSkills { get; private set; } = new();

    private ISkillUser _user;
    private CooldownManager _cooldownManager;

    private void Awake()
    {
        _user = GetComponent<ISkillUser>();
        _cooldownManager = GetComponent<CooldownManager>();

        _cooldownManager.OnCooldownEnded += (skillId) =>
        {
            var skill = EquippedSkills.FirstOrDefault(s => s.activeSkillData.skillId == skillId);
            if (skill != null)
            {
                Debug.Log($"스킬 '{skill.activeSkillData.skillName}'의 쿨다운이 종료되었습니다.");
            }
        };

        // Test Methods
        // TODO: 추후 InventoryManager 등에서 관리
        if (activeSkillToTest != null)
            CreateSkillInstance();
    }

    /// <summary>
    /// 스킬 사용 시도
    /// </summary>
    /// <param name="skillIndex"></param>
    /// <returns></returns>
    public bool TryUseSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= EquippedSkills.Count)
        {
            Debug.LogWarning("잘못된 스킬 인덱스입니다.");
            return false;
        }

        var skill = EquippedSkills[skillIndex];
        var data = skill.runtimeSkillData;

        // 1. 쿨다운 체크
        if (_cooldownManager.IsOnCooldown(skill.activeSkillData.skillId))
        {
            Debug.Log($"스킬 '{skill.activeSkillData.skillName}'은(는) 아직 쿨다운 중입니다.");
            Debug.Log($"남은 쿨다운 시간: {_cooldownManager.GetRemainingTime(skill.activeSkillData.skillId)}초");
            return false;
        }

        // 2. 스킬 사용 가능 상태 체크 (예: 스턴, 침묵, 사망 등)
        if (!_user.IsSkillAvailable) return false;

        // 3. 자원 체크 및 소모
        if (_user is IStatProvider statProvider)
        {
            float cost = skill.runtimeSkillData.currentCoreStats.resourceCost;
            if (!statProvider.HasResource(StatType.CurrentStamina, cost))
            {
                Debug.Log($"스킬 '{skill.activeSkillData.skillName}' 시전 실패. 자원이 부족합니다.");
                return false;
            }

            statProvider.ConsumeResource(StatType.CurrentStamina, cost);
        }

        float castTime = data.currentCoreStats.castTime;
        float finalCooldown = data.currentCoreStats.cooldown;

        // 4. 쿨다운 감소 적용
        if (_user is IStatProvider _statProvider)
        {
            finalCooldown *= (1f - Mathf.Clamp01(_statProvider[StatType.CooldownReduction]));
        }

        Debug.Log($"캐릭터 '{gameObject.name}'가 스킬 '{skill.activeSkillData.skillName}' 사용 시도. 쿨다운:{finalCooldown}초.");

        // 5. 시전 시간 처리
        if (castTime > 0f)
        {
            StartCoroutine(ExecuteSkillAfterCast(castTime, finalCooldown, skill));
            return true;
        }

        // 즉시 시전
        ExecuteSkill(skill, finalCooldown);

        return true;
    }

    private IEnumerator ExecuteSkillAfterCast(float castTime, float cooldown, RuntimeSkill skill)
    {
        Debug.Log($"스킬 '{skill.activeSkillData.skillName}' 시전 시작. 시전 시간: {castTime}초.");
        float elapsed = 0f;
        while (elapsed < castTime)
        {
            // 캐스팅 도중 상태이상 발생시 중단(사망, 스턴 등)
            if (!_user.IsSkillAvailable) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }
        ExecuteSkill(skill, cooldown);
    }

    private void ExecuteSkill(RuntimeSkill skill, float cooldown)
    {
        Debug.Log($"스킬 '{skill.activeSkillData.skillName}' 시전 완료. 쿨다운 시작.");

        int skillId = skill.activeSkillData.skillId;

        // 쿨다운 적용
        if (!_cooldownManager.StartCooldown(skillId, cooldown))
        {
            Debug.Log($"스킬 '{skill.activeSkillData.skillName}'은(는) 아직 쿨다운 중입니다.");
            Debug.Log($"남은 쿨다운 시간: {_cooldownManager.GetRemainingTime(skillId)}초");
            return;
        }

        // Test 용으로 targetCharacter 사용
        skill.UseSkill(_user, targetCharacter);
        // 데미지 적용 (더미)
        float calculatedDamage = skill.CalculateFinalDamage(_user as IStatProvider, targetCharacter);
        skill.OnSkillHit(_user, targetCharacter, calculatedDamage);
    }

    #region Test Methods
    /// <summary>
    /// debug 용으로 스킬 장착, 추후 InventoryManager 등에서 관리
    /// </summary>
    private void CreateSkillInstance()
    {
        // 1. SkillInstance 생성 테스트
        Debug.Log("\n--- 1. SkillInstance Creation Test ---");
        RuntimeSkill skillInstance = new RuntimeSkill(activeSkillToTest, passiveSkillsToApply);
        Debug.Log($"SkillInstance '{skillInstance.activeSkillData.skillName}' created successfully.");

        EquippedSkills.Add(skillInstance);

        // 2. RuntimeSkillData 초기화 및 스탯 확인
        Debug.Log("\n--- 2. RuntimeSkillData Init & Stat Check Test ---");
        PrintRuntimeSkillDataStatus(skillInstance.runtimeSkillData);
    }

    private void PrintRuntimeSkillDataStatus(RuntimeSkillData runtimeSkillData)
    {
        if (runtimeSkillData == null)
        {
            Debug.LogError("RuntimeSkillData가 null입니다.");
            return;
        }

        Debug.Log("=== RuntimeSkillData 스탯 및 패시브 적용 상태 ===");

        // CoreStats 출력
        Debug.Log("--- currentCoreStats ---");
        Debug.Log($"baseDamage: {runtimeSkillData.currentCoreStats.baseDamage}");
        Debug.Log($"cooldown: {runtimeSkillData.currentCoreStats.cooldown}");
        Debug.Log($"castTime: {runtimeSkillData.currentCoreStats.castTime}");
        Debug.Log($"resourceCost: {runtimeSkillData.currentCoreStats.resourceCost}");
        Debug.Log($"characterAttackPowerMultiplier: {runtimeSkillData.currentCoreStats.characterAttackPowerMultiplier}");
        Debug.Log($"flatDamageAdded: {runtimeSkillData.currentCoreStats.flatDamageAdded}");
        Debug.Log($"baseCriticalChance: {runtimeSkillData.currentCoreStats.baseCriticalChance}");
        Debug.Log($"baseCriticalDamageMultiplier: {runtimeSkillData.currentCoreStats.baseCriticalDamageMultiplier}");
        Debug.Log($"hitCount: {runtimeSkillData.currentCoreStats.hitCount}");
        Debug.Log($"effectMagnitude: {runtimeSkillData.currentCoreStats.effectMagnitude}");
        Debug.Log($"range: {runtimeSkillData.currentCoreStats.range}");

        // 파생 스탯 출력
        Debug.Log("--- Derived Stats ---");
        Debug.Log($"currentProjectileSize: {runtimeSkillData.currentProjectileSize}");
        Debug.Log($"currentAoeRadius: {runtimeSkillData.currentAoeRadius}");
        Debug.Log($"currentMeleeArcAngle: {runtimeSkillData.currentMeleeArcAngle}");

        // 적용된 스탯 보정 출력
        Debug.Log("--- Applied Stat Modifiers ---");
        if (runtimeSkillData.appliedStatModifiers != null && runtimeSkillData.appliedStatModifiers.Any())
        {
            foreach (var mod in runtimeSkillData.appliedStatModifiers)
            {
                Debug.Log($"  {mod.skillStatType} | {mod.skillStatModType} | {mod.value}");
            }
        }
        else
        {
            Debug.Log("  적용된 스탯 보정 없음.");
        }

        // 런타임 패시브 효과 출력
        Debug.Log("--- Attached Runtime Passive Effects ---");
        if (runtimeSkillData.attachedPassiveSkillExecutors != null && runtimeSkillData.attachedPassiveSkillExecutors.Any())
        {
            foreach (var effect in runtimeSkillData.attachedPassiveSkillExecutors)
            {
                Debug.Log($"  {effect.GetType().Name}");
            }
        }
        else
        {
            Debug.Log("  런타임 패시브 효과 없음.");
        }

        Debug.Log("=== RuntimeSkillData 상태 출력 끝 ===");
    }
    #endregion
}
