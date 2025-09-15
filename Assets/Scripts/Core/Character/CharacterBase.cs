using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(StatusEffectManager))]
public class CharacterBase : MonoBehaviour, IStatProvider, ISkillUser
{
    [SerializeField] private StatData _baseStatData; // 기본 스탯 데이터

    private StatSystem statSystem;
    public float this[StatType type] => TryGetStat(type, out var value) ? value : 0f;
    public float CurrentHealth => statSystem?.CurrentStats.CurrentHealth ?? 0f;
    public float CurrentStamina => statSystem?.CurrentStats.CurrentStamina ?? 0f;

    public StatusEffectManager StatusEffectManager { get; private set; }
    public CooldownManager CooldownManager { get; private set; }
    public SkillCaster SkillCaster { get; private set; }

    public bool IsSkillAvailable => !_isDead;
    public Transform Transform => this.transform;

    private Animator _animator;
    private CharacterController _characterController;   // 캐릭터 이동을 위한 CharacterController
    private Collider _collider;                         // 캐릭터의 피격 처리를 위한 Collider  

    private bool _isDead = false;

    private void Awake()
    {
        statSystem = new StatSystem(_baseStatData);

        StatusEffectManager = GetComponent<StatusEffectManager>();
        CooldownManager = GetComponent<CooldownManager>();
        SkillCaster = GetComponent<SkillCaster>();

        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _collider = GetComponent<Collider>();

        this.statSystem.OnDeath += HandleDeath;
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
            Debug.Log($"{gameObject.name} 의 현재 체력 : {statSystem.CurrentStats.CurrentHealth}");
        }
    }

    private void HandleDeath()
    {
        if (_isDead) return;

        _isDead = true;

        _animator.SetTrigger("Death"); // 사망 애니메이션 트리거
        _characterController.enabled = false; // 캐릭터 컨트롤러 비활성화
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
        statSystem?.AddModifiers(modifiers);
    }

    public void RemoveModifiers(IEnumerable<StatModifier> modifiers)
    {
        statSystem?.RemoveModifiers(modifiers);
    }

    public bool HasResource(StatType statType, float amount)
    {
        return statSystem.HasResource(statType, amount);
    }

    public void ConsumeResource(StatType statType, float amount)
    {
         statSystem.ConsumeResource(statType, amount);
    }
    #endregion
}