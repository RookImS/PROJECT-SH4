using UnityEngine;
using System;

/// <summary>
/// 액티브 스킬 데이터 원형
/// </summary>
[CreateAssetMenu(fileName = "001_ActiveSkill", menuName = "Skill/Active")]
public class ActiveSkillData : SkillData, IActiveSkillExecutorFactory
{

    /* 액티브 스킬 핵심 스탯 */
    [Header("Core Attributes")]
    [Tooltip("스킬 핵심 스탯")]
    public CoreSkillStats coreStats; // 핵심 스탯들을 구조체로 그룹화

    /* 액티브 스킬 시각 효과 */
    [Header("Visuals")]
    [Tooltip("스킬 이펙트 프리팹")]
    public GameObject skillEffectPrefab;

    /* 액티브 스킬 공격 타입 */
    [Header("SkillAttackType")]
    [Tooltip("스킬 공격 타입 (투사체, 범위 공격, 근접 공격)")]
    public SkillAttackType skillAttackType;

    /* 공격 타입별 능력치 */
    [Header("Type-Specific Attributes (Conditional)")]

    /* Projectile (투사체) 타입 스킬 */
    [Header("Projectile Attributes")] 
    [Tooltip("투사체 스킬 사이즈")]
    public float baseProjectileSize;
    [Tooltip("투사체 스킬 이동 속도")]
    public float projectileSpeed;
    [Tooltip("관통 여부")]
    public bool canPierceTargets;
    [Tooltip("관통 가능한 적의 수(canPierceTargets가 true인 경우 유효)")]
    public int maxTargetsHit;

    /* AreaOfEffect (AOE) 타입 스킬 */
    [Header("AOE Attributes")]
    [Tooltip("범위형 스킬(AOE)의 반경")]
    public float baseAoeRadius;
    [Tooltip("AOE 실제 피해/효과가 적용되기까지의 지연 시간")]
    public float aoeDelay;
    [Tooltip("지속형 AOE 스킬의 효과 유지 시간(0 이상이면 장판형 스킬)")]
    public float aoeDuration;
    [Tooltip("지속형 AOE 스킬 피해 적용 간격(aoeDuration이 0보다 클 경우 유효)")]
    public float aoeHitInterval;

    /* Melee (근접) 타입 스킬 */
    [Header("Melee Attributes")]
    [Tooltip("근접 스킬 공격 각도 (부채꼴 형태, 0이면 일자형).")]
    [Range(0f, 360f)]
    public float baseMeleeArcAngle;

    public IActiveSkillExecutor CreateActiveSkillExecutor()
    {
        switch (skillAttackType)
        {
            case SkillAttackType.Projectile:
                return new ProjectileSkillExecutor();
            case SkillAttackType.AreaOfEffect:
                return new AoeSkillExecutor();
            case SkillAttackType.Melee:
                return new MeleeSkillExecutor();
            default:
                throw new NotImplementedException();
        }
    }
}
