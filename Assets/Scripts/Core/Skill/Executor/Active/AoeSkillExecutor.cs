using UnityEngine;

/// <summary>
/// 범위형(AOE) 액티브 스킬의 실행 로직을 담당하는 클래스입니다.
/// </summary>
public class AoeSkillExecutor : IActiveSkillExecutor
{
    /// <summary>
    /// AOE 스킬이 시전될 때 실행되는 로직을 정의합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <c>Character</c></param>
    /// <param name="skillInstance">현재 사용되고 있는 <c>RuntimeSkill</c> 인스턴스</param>
    public void OnSkillCast(CharacterBase caster, RuntimeSkill skillInstance)
    {
        var data = skillInstance.runtimeSkillData;
        Debug.Log($"AOE 스킬 발동! 반경: {data.currentAoeRadius}, 지연 시간: {data.currentAoeDelay}, 사거리: {data.currentCoreStats.range}");
        // 실제 AOE 효과 생성/적용 로직은 별도 구현 필요
    }
}