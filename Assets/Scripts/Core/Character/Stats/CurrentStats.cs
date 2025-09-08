using System;
using System.Collections.Generic;

public class CurrentStats
{
    private readonly Dictionary<StatType, float> _finalStats;

    private float _currentHealth;
    private float _currentStamina;

    /// <summary>
    /// Property로 0 ~ Max 범위로 자동 클램핑 및 이벤트 호출
    /// </summary>
    public float CurrentHealth
    {
        get => _currentHealth;
        private set
        {
            float max = GetFinal(StatType.MaxHealth);
            float clamped = Math.Clamp(value, 0f, max);
            if (clamped == _currentHealth) return;

            float prev = _currentHealth;
            _currentHealth = clamped;

            float delta = clamped - prev;
            OnStatChanged?.Invoke(StatType.CurrentStamina, clamped, delta);
            OnHealthChanged?.Invoke(clamped);

            if (prev > 0f && clamped <= 0f)
                OnDeath?.Invoke();
        }
    }

    public float CurrentStamina
    {
        get => _currentStamina;
        private set
        {
            float max = GetFinal(StatType.MaxStamina);
            float clamped = Math.Clamp(value, 0f, max);
            if (clamped == _currentStamina) return;

            float prev = _currentStamina;
            _currentStamina = clamped;

            float delta = clamped - prev;
            OnStatChanged?.Invoke(StatType.CurrentStamina, clamped, delta);
            OnStaminaChanged?.Invoke(clamped);
        }
    }

    // 상태 변경을 알리기 위한 이벤트
    public event Action<StatType, float, float> OnStatChanged; // (statType, newValue, delta)
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;
    public event Action<float> OnStaminaChanged;

    public CurrentStats(Dictionary<StatType, float> finalStats)
    {
        _finalStats = finalStats;
        _currentHealth = GetFinal(StatType.MaxHealth);
        _currentStamina = GetFinal(StatType.MaxStamina);
    }

    private float GetFinal(StatType stat)
    {
        if (_finalStats.TryGetValue(stat, out var v)) return v;
        return 0f;
    }

    #region HealthLogic
    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        CurrentHealth = CurrentHealth - damage; // uses property setter
    }

    public void RestoreHp(float amount)
    {
        if (amount <= 0f) return;
        CurrentHealth = CurrentHealth + amount; // uses property setter
    }
    #endregion

    #region StaminaLogic
    public void ConsumeStamina(float amount)
    {
        if (amount <= 0f) return;
        CurrentStamina = CurrentStamina - amount; // uses property setter
    }

    public void RestoreStamina(float amount)
    {
        if (amount <= 0f) return;
        CurrentStamina = CurrentStamina + amount; // uses property setter
    }
    #endregion

    /// <summary>
    /// 최대치가 변경되었을 때 현재 값을 보정하는 함수
    /// </summary>
    public void UpdateMaxValues()
    {
        CurrentHealth = _currentHealth;
        CurrentStamina = _currentStamina;
    }
}