using UnityEngine;

/// <summary>
/// 액티브 스킬의 타입별 실행 로직을 정의하는 인터페이스입니다.
/// </summary>
public interface IActiveSkillExecutor
{
    /// <summary>
    /// 스킬이 시전될 때 타입별로 실행되는 로직을 정의합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <c>Character</c></param>
    /// <param name="skillInstance">현재 사용되고 있는 <c>RuntimeSkill</c> 인스턴스</param>
    void OnSkillCast(CharacterBase caster, RuntimeSkill skillInstance);
}