using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Base Stats")]
    public string characterName = "Default Character";
    public float Health = 100f;
    public float AttackPower = 100f;
    public float DefenseReductionRate = 0f;
    public float BonusDamageMultiplier = 0f;

    [Header("Critical Stats")]
    [Range(0f, 1f)]
    public float CriticalChance = 0f;
    public float CriticalDamageMultiplier = 1.5f;

    public void StartCooldown(float duration)
    {
        Debug.Log($"캐릭터 '{characterName}'의 쿨다운이 {duration}초 시작되었습니다.");
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
        Debug.Log($"{this.name}이(가) {amount} 피해를 받아 남은 체력: {this.Health}");
        if (this.Health <= 0)
        {
            Debug.Log($"{this.name}이(가) 쓰러졌습니다!");
        }
    }
}


