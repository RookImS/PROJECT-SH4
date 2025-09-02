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

    /// <summary>
    /// 임시 상태 효과 적용 메서드입니다. 실제 게임에서는 <c>StatusEffectManager</c>와 같은
    /// 전용 컴포넌트가 상태 효과의 실제 로직을 담당하게 될 것입니다.
    /// </summary>
    /// <param name="effectData">적용할 <c>StatusEffectData</c> 객체입니다.</param>
    public void ApplyStatusEffect(StatusEffectData effectData)
    {
        Debug.Log($"캐릭터 '{characterName}'에게 상태 효과 '{effectData.effectName}' 적용 시도.");
        // 여기서 effectData의 타입에 따라 실제 버프/디버프 로직을 시작할 수 있습니다.
        // 예를 들어:
        // if (effectData is PoisonStatusEffectData poisonData) { /* 독 효과 로직 */ }
        // else if (effectData is AttackBuffStatusEffectData buffData) { /* 버프 효과 로직 */ }
    }
}


