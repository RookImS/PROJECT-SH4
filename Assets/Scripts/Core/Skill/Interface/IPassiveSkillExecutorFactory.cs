using UnityEngine;

/// <summary>
/// 런타임 패시브 스킬 인스턴스를 생성하는 팩토리 인터페이스입니다.
/// </summary>
public interface IPassiveSkillExecutorFactory
{
    /// <summary>
    /// 실제 런타임 패시브 효과(<c>IPassiveSkillExecutor</c>) 인스턴스를 생성하여 반환합니다.
    /// </summary>
    /// <returns>
    /// 생성된 <c>IPassiveSkillExecutor</c> 인스턴스.
    /// 만약 해당 패시브가 런타임에 특정한 로직을 수행할 필요가 없다면 <c>null</c>을 반환합니다.
    /// </returns>
    IPassiveSkillExecutor CreatePassiveSkillExecutor();
}