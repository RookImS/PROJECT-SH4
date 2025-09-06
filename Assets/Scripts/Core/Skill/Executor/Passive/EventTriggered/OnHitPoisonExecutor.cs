using UnityEngine;

/// <summary>
/// <c>OnHitPoisonEffect</c>는 스킬이 대상에게 적중했을 때 독 상태 효과를 부여하는 런타임 패시브 효과입니다.
/// <c>IRuntimePassiveEffect</c> 인터페이스를 구현하여 <c>OnSkillHit</c> 이벤트에 반응합니다.
/// </summary>
public class OnHitPoisonExecutor : IPassiveSkillExecutor
{
    private PoisonStatusEffectData _poisonEffectData;

    /// <summary>
    /// <c>OnHitPoisonEffect</c>의 새로운 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="poisonEffectData">이 효과가 타겟에게 부여할 독 상태 효과 데이터입니다.</param>
    public OnHitPoisonExecutor(PoisonStatusEffectData poisonEffectData)
    {
        _poisonEffectData = poisonEffectData;
    }

    /// <summary>
    /// 스킬 시전 시 발동하는 로직을 정의합니다. 독 효과는 적중 시 발동하므로, 여기서는 아무것도 하지 않습니다.
    /// </summary>
    public void OnSkillCast(CharacterBase caster, RuntimeSkill skillInstance)
    {
        // 독 효과는 스킬 적중 시 발동하므로, 시전 시에는 특별한 로직이 없습니다.
    }

    /// <summary>
    /// 스킬이 적에게 성공적으로 적중했을 때 발동하는 로직을 정의합니다.
    /// 타겟에게 정의된 독 상태 효과를 부여합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <c>Character</c>입니다.</param>
    /// <param name="target">스킬에 적중된 <c>Character</c> 대상입니다.</param>
    /// <param name="skillInstance">현재 사용되고 있는 <c>SkillInstance</c>입니다.</param>
    /// <param name="damageDealt">대상에게 실제로 가해진 최종 피해량입니다.</param>
    public void OnSkillHit(CharacterBase caster, CharacterBase target, RuntimeSkill skillInstance, float damageDealt)
    {
        // 타겟이 유효하고 독 효과 데이터가 할당되어 있다면 독 효과를 적용합니다.
        if (target != null && _poisonEffectData != null)
        {
            // 실제 게임에서는 Character 클래스에 StatusEffectManager와 같은 컴포넌트가 있어서
            // target.GetComponent<StatusEffectManager>().ApplyEffect(_poisonEffectData); 와 같이 호출될 것입니다.
            // 여기서는 테스트를 위해 Character 클래스의 임시 메서드를 호출하고 Debug.Log로 대체합니다.
            target.StatusEffectManager.ApplyStatusEffect(_poisonEffectData);
            Debug.Log($"'{skillInstance.activeSkillData.skillName}'이(가) '{target.name}'에게 적중하여 독 효과 '{_poisonEffectData.effectName}'을(를) 부여합니다. (초당 피해: {_poisonEffectData.damagePerSecond}, 지속 시간: {_poisonEffectData.duration}초)");
        }
    }
}