using UnityEngine;

/// <summary>
/// 런타임에 패시브 효과가 반응할 이벤트를 정의하는 공통 인터페이스
/// </summary>
public interface IRuntimePassiveEffect
{
    /// <summary>
    /// 스킬이 시전될 때 발동하는 로직을 정의한다.
    /// 스킬 시전 시점에 특정 패시브 효과가 발동해야 할 때 구현된다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <c>Character</c></param>
    /// <param name="skillInstance">현재 사용되고 있는 <c>SkillInstance</c></param>
    void OnSkillCast(Character caster, SkillInstance skillInstance);

    /// <summary>
    /// 스킬이 적에게 성공적으로 적중했을 때 발동하는 로직을 정의한다.
    /// 스킬의 공격이 대상에게 피해를 입히는 시점에 특정 패시브 효과가 발동해야 할 때 구현된다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <c>Character</c></param>
    /// <param name="target">스킬에 적중된 <c>Character</c> 대상</param>
    /// <param name="skillInstance">현재 사용되고 있는 <c>SkillInstance</c></param>
    /// <param name="damageDealt">대상에게 실제로 가해진 최종 피해량</param>
    void OnSkillHit(Character caster, Character target, SkillInstance skillInstance, float damageDealt);
}

/// <summary>
/// 런타임 패시브 효과 인스턴스를 생성하는 팩토리 인터페이스
/// </summary>
public interface IRuntimePassiveEffectFactory
{

    /// <summary>
    /// 이 팩토리로부터 실제 런타임 패시브 효과 (<c>IRuntimePassiveEffect</c>) 인스턴스를 생성하여 반환한다.
    /// 스킬 인스턴스에 독립적인 런타임 패시브 로직을 부여할 때 사용된다.
    /// </summary>
    /// <returns>
    /// 생성된 <c>IRuntimePassiveEffect</c> 인스턴스.
    /// 만약 해당 패시브가 런타임에 특정한 로직을 수행할 필요가 없다면 <c>null</c>을 반환한다.
    /// </returns>
    IRuntimePassiveEffect CreateRuntimeEffect();
}