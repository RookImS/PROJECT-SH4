using UnityEngine;

/// <summary>
/// <c>OnCastAttackBuffEffect</c>는 스킬이 시전되었을 때 캐스터에게 공격력 버프를 부여하는 런타임 패시브 효과입니다.
/// <c>IRuntimePassiveEffect</c> 인터페이스를 구현하여 <c>OnSkillCast</c> 이벤트에 반응합니다.
/// </summary>
public class OnCastAttackBuffEffect : IRuntimePassiveEffect
{
    private AttackBuffStatusEffectData _attackBuffData;

    /// <summary>
    /// <c>OnCastAttackBuffEffect</c>의 새로운 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="attackBuffData">이 효과가 캐스터에게 부여할 공격력 버프 상태 효과 데이터입니다.</param>
    public OnCastAttackBuffEffect(AttackBuffStatusEffectData attackBuffData)
    {
        _attackBuffData = attackBuffData;
    }

    /// <summary>
    /// 스킬 시전 시 발동하는 로직을 정의합니다. 캐스터에게 정의된 공격력 버프 상태 효과를 부여합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <c>Character</c>입니다.</param>
    /// <param name="skillInstance">현재 사용되고 있는 <c>SkillInstance</c>입니다.</param>
    public void OnSkillCast(Character caster, SkillInstance skillInstance)
    {
        // 캐스터가 유효하고 공격력 버프 데이터가 할당되어 있다면 버프 효과를 적용합니다.
        if (caster != null && _attackBuffData != null)
        {
            // 실제 게임에서는 Character 클래스에 StatusEffectManager와 같은 컴포넌트가 있어서
            // caster.GetComponent<StatusEffectManager>().ApplyEffect(_attackBuffData); 와 같이 호출될 것입니다.
            // 여기서는 테스트를 위해 Character 클래스의 임시 메서드를 호출하고 Debug.Log로 대체합니다.
            caster.ApplyStatusEffect(_attackBuffData);
            Debug.Log($"'{skillInstance.activeSkillData.skillName}'이(가) 시전되어 '{caster.name}'에게 공격력 버프 '{_attackBuffData.effectName}'을(를) 부여합니다. (공격력 증가: {_attackBuffData.attackPowerIncrease}, 지속 시간: {_attackBuffData.duration}초)");
        }
    }

    /// <summary>
    /// 스킬이 적에게 성공적으로 적중했을 때 발동하는 로직을 정의합니다. 공격력 버프는 시전 시 발동하므로, 여기서는 아무것도 하지 않습니다.
    /// </summary>
    public void OnSkillHit(Character caster, Character target, SkillInstance skillInstance, float damageDealt)
    {
        // 공격력 버프는 시전 시 발동하므로, 적중 시에는 특별한 로직이 없습니다.
    }
}
