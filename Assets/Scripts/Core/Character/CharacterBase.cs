using System;
using UnityEngine;

public class CharacterBase : MonoBehaviour, IStatProvider
{
    public event Action OnDeath;

    StatSystem statSystem;

    public FinalStats FinalStats => statSystem.FinalStats;

    private Animator _animator;
    private Rigidbody _rigidBody;
    private Collider _collider;

    private void Awake()
    {
        // StatData는 외부에서 주입받거나, 스크립트에서 직접 설정할 수 있습니다.
        StatData baseStatData = new StatData
        {
            maxHealth = 100f,
            maxStamina = 50f,
            attackPower = 10f,
            attackRange = 1.5f,
            additionalDamageMultiplier = 0.2f,
            fixedAdditionalDamage = 5f,
            defense = 5f,
            movementSpeed = 3f,
            rotationSpeed = 10f,
            attackSpeedMultiplier = 1.0f,
            criticalHitChance = 0.1f,
            criticalDamageMultiplier = 1.5f,
            cooldownReduction = 0.1f,
            statusResistance = 0.05f
        };

        statSystem = new StatSystem(baseStatData);
        _animator = GetComponentInChildren<Animator>();
        _rigidBody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        this.OnDeath += HandleDeath;
    }

    public void TakeDamage(float damage)
    {
        if (statSystem != null)
        {
            statSystem.TakeDamage(damage);
            Debug.Log($"{gameObject.name} 가 {damage} 만큼의 데미지를 입었습니다!");
            Debug.Log($"현재 체력 : {statSystem.CurrentStats.Health}");
        }

        if (statSystem.CurrentStats.Health <= 0)
        {
            // OnDeath 이벤트를 구독한 모든 이들에게 "나 죽었다!라고송니다.
            OnDeath?.Invoke();
        }
    }

    private void HandleDeath()
    {
        _animator.SetTrigger("Death");
    }
}