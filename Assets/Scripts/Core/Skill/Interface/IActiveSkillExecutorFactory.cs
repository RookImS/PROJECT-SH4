using UnityEngine;

/// <summary>
/// 액티브 스킬 타입에 따라 실행자(<c>IActiveSkillExecutor</c>)를 생성하는 팩토리 인터페이스입니다.
/// </summary>
public interface IActiveSkillExecutorFactory
{
    /// <summary>
    /// 액티브 스킬 타입에 맞는 실행자 인스턴스를 생성하여 반환합니다.
    /// </summary>
    /// <returns>생성된 <c>IActiveSkillExecutor</c> 인스턴스</returns>
    IActiveSkillExecutor CreateActiveSkillExecutor();
}