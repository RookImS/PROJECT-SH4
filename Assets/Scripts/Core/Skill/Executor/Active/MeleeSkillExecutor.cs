using UnityEngine;

/// <summary>
/// 근접(Melee) 액티브 스킬의 실행 로직을 담당하는 클래스입니다.
/// </summary>
public class MeleeSkillExecutor : IActiveSkillExecutor
{
    /// <summary>
    /// 근접 스킬이 시전될 때 실행되는 로직을 정의합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <c>Character</c></param>
    /// <param name="skillInstance">현재 사용되고 있는 <c>RuntimeSkill</c> 인스턴스</param>
    public void OnSkillCast(CharacterBase caster, RuntimeSkill skillInstance)
    {
        var data = skillInstance.runtimeSkillData;
        Debug.Log($"근접 스킬 발동! 아크 각도: {data.currentMeleeArcAngle}, 사거리: {data.currentCoreStats.range}");
        // 실제 근접 공격 로직은 별도 구현 필요
    }
}