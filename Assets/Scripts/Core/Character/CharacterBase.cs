using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterBase : MonoBehaviour, IStatProvider
{
    [Header("SkillTest Data Setup")]
    public ActiveSkillData activeSkillToTest;
    public List<PassiveSkillData> passiveSkillsToApply;
    public CharacterBase targetCharacter;

    [SerializeField] private StatData _baseStatData; // 기본 스탯 데이터

    private StatSystem statSystem;
    public float this[StatType type] => TryGetStat(type, out var value) ? value : 0f;

    public StatusEffectManager StatusEffectManager;
    public CooldownManager CooldownManager;

    public List<RuntimeSkill> EquippedSkills { get; private set; }

    private Animator _animator;
    private Rigidbody _rigidBody;
    private Collider _collider;

    private bool _isDead = false;

    private void Awake()
    {
        statSystem = new StatSystem(_baseStatData);

        StatusEffectManager = GetComponent<StatusEffectManager>();
        CooldownManager = GetComponent<CooldownManager>();

        EquippedSkills = new List<RuntimeSkill>();

        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        this.statSystem.OnDeath += HandleDeath;

        // Test Methods
        if (activeSkillToTest != null)
            CreateSkillInstance();
    }

    private void OnDestroy()
    {
        if (statSystem != null)
            statSystem.OnDeath -= HandleDeath;
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        if (statSystem != null)
        {
            statSystem.TakeDamage(damage);
            Debug.Log($"{gameObject.name} 가 {damage} 만큼의 데미지를 입었습니다!");
            Debug.Log($"현재 체력 : {statSystem.CurrentStats.CurrentHealth}");
        }
    }

    private void HandleDeath()
    {
        if (_isDead) return;

        _isDead = true;

        _animator.SetTrigger("Death"); // 사망 애니메이션 트리거
        _rigidBody.isKinematic = true; // 물리 효과 비활성화
    }

    #region Stat Methods(IStatProvider 구현)
    public bool TryGetStat(StatType type, out float value)
    {
        return statSystem.FinalStats.TryGetValue(type, out value);
    }

    public void AddModifier(StatModifier modifier)
    {
        statSystem?.AddModifier(modifier);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        statSystem?.RemoveModifier(modifier);
    }

    public void AddModifiers(IEnumerable<StatModifier> modifiers)
    {
        foreach (StatModifier modifier in modifiers)
            statSystem?.AddModifier(modifier);
    }

    public void RemoveModifiers(IEnumerable<StatModifier> modifiers)
    {
        foreach (StatModifier modifier in modifiers)
            statSystem?.RemoveModifier(modifier);
    }
    #endregion

    public bool TryUseSkill(int skillIndex, CharacterBase target = null)
    {
        if (skillIndex < 0 || skillIndex >= EquippedSkills.Count)
        {
            Debug.LogWarning("잘못된 스킬 인덱스입니다.");
            return false;
        }
        var skill = EquippedSkills[skillIndex];
        if (CooldownManager.IsOnCooldown(skill.runtimeSkillData))
        {
            Debug.Log($"스킬 '{skill.activeSkillData.skillName}'은(는) 아직 쿨다운 중입니다.");
            return false;
        }
        skill.UseSkill(this, target);
        return true;
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