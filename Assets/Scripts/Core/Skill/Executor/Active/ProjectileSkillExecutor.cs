using UnityEngine;

/// <summary>
/// 투사체(Projectile) 액티브 스킬의 실행 로직을 담당하는 클래스입니다.
/// </summary>
public class ProjectileSkillExecutor : IActiveSkillExecutor
{
    /// <summary>
    /// 투사체 스킬이 시전될 때 실행되는 로직을 정의합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <c>Character</c></param>
    /// <param name="skillInstance">현재 사용되고 있는 <c>RuntimeSkill</c> 인스턴스</param>
    public void OnSkillCast(Character caster, RuntimeSkill skillInstance)
    {
        var data = skillInstance.runtimeSkillData;
        Debug.Log($"투사체 발사! 속도: {data.currentProjectileSpeed}, 크기: {data.currentProjectileSize}, 사거리: {data.currentCoreStats.range}");
        // 실제 투사체 생성/이동 로직은 별도 구현 필요
    }
}