public class CurrentStats
{
    private readonly FinalStats _finalStats;

    public float Health { get; private set; }
    public float Stamina { get; private set; }

    public CurrentStats(FinalStats finalStats)
    {
        _finalStats = finalStats;
        Health = _finalStats.maxHealth;
        Stamina = _finalStats.maxStamina;
    }

    #region HealthLogic
    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;
    }

    public void RestoreHp(float amount)
    {
        Health += amount;
        if (Health > _finalStats.maxHealth) Health = _finalStats.maxHealth;
    }
    #endregion

    #region StaminaLogic
    // 스태미너 관련 로직
    public void SpendStamina(float amount)
    {
        Stamina -= amount;
        if (Stamina < 0) Stamina = 0;
    }

    public void RestoreStamina(float amount)
    {
        Stamina += amount;
        if (Stamina > _finalStats.maxStamina) Stamina = _finalStats.maxStamina;
    }
    #endregion

    // 최대치가 변경되었을 때 현재 값을 보정하는 함수
    public void UpdateMaxValues()
    {
        if (Health > _finalStats.maxHealth) Health = _finalStats.maxHealth;
        if (Stamina > _finalStats.maxStamina) Stamina = _finalStats.maxStamina;
    }
}