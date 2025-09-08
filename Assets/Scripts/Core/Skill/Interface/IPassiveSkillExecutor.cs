using UnityEngine;

/// <summary>
/// 런타임에 패시브 스킬이 반응할 이벤트를 정의하는 공통 인터페이스입니다.
/// </summary>
public interface IPassiveSkillExecutor
{
    /// <summary>
    /// 스킬이 시전될 때 발동하는 로직을 정의합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <c>Character</c></param>
    /// <param name="skillInstance">현재 사용되고 있는 <c>RuntimeSkill</c></param>
    void OnSkillCast(ISkillUser caster, RuntimeSkill skillInstance);

    /// <summary>
    /// 스킬이 적에게 성공적으로 적중했을 때 발동하는 로직을 정의합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <c>Character</c></param>
    /// <param name="target">스킬에 적중된 <c>Character</c> 대상</param>
    /// <param name="skillInstance">현재 사용되고 있는 <c>RuntimeSkill</c></param>
    /// <param name="damageDealt">대상에게 실제로 가해진 최종 피해량</param>
    void OnSkillHit(ISkillUser caster, CharacterBase target, RuntimeSkill skillInstance, float damageDealt);
}