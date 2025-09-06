using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillSystemTestCode : MonoBehaviour
{

    [Header("Test Data Setup")]
    public ActiveSkillData activeSkillToTest;
    public List<PassiveSkillData> passiveSkillsToApply;
    public CharacterBase casterCharacter;
    public CharacterBase targetCharacter;

    public void Start()
    {
        TestSkillSystem();
    }

    [ContextMenu("Test Skill System")]
    public void TestSkillSystem()
    {
        Debug.Log("--- Skill System Test Started ---");

        if (activeSkillToTest == null || casterCharacter == null || targetCharacter == null)
        {
            Debug.LogError("Required test data is not assigned. Please assign ActiveSkill, Caster, and Target.");
            return;
        }

        // 1. SkillInstance 생성 테스트
        Debug.Log("\n--- 1. SkillInstance Creation Test ---");
        RuntimeSkill skillInstance = new RuntimeSkill(activeSkillToTest, passiveSkillsToApply);
        Debug.Log($"SkillInstance '{skillInstance.activeSkillData.skillName}' created successfully.");

        // 2. RuntimeSkillData 초기화 및 스탯 확인
        Debug.Log("\n--- 2. RuntimeSkillData Init & Stat Check Test ---");
        PrintRuntimeSkillDataStatus(skillInstance.runtimeSkillData);
          

        // 3. UseSkill 동작 테스트
        Debug.Log("\n--- 3. UseSkill Execution Test ---");
        skillInstance.UseSkill(casterCharacter, targetCharacter);
        
        // 4. CalculateFinalDamage 및 OnSkillHit 동작 테스트
        Debug.Log("\n--- 4. Damage Calculation & OnSkillHit Test ---");
        float calculatedDamage = skillInstance.CalculateFinalDamage(casterCharacter, targetCharacter);
        Debug.Log($"Calculated Final Damage to {targetCharacter.gameObject.name}: {calculatedDamage}");
        skillInstance.OnSkillHit(casterCharacter, targetCharacter, calculatedDamage); // OnSkillHit 강제 호출

        
        Debug.Log("--- Skill System Test Finished ---");
    }
 

    public void PrintRuntimeSkillDataStatus(RuntimeSkillData runtimeSkillData)
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
}
