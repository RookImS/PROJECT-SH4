using UnityEngine;
using System.Collections.Generic; // List를 사용하지 않아도 되지만, 일반적으로 필요할 수 있어 포함

/// <summary>
/// <c>OnCastAttackBuffPassiveData</c>는 스킬 시전 시 캐스터에게 공격력 버프 상태 효과를 부여하는 패시브 스킬의 데이터입니다.
/// <c>PassiveSkillData</c>를 상속받으며, <c>IRuntimePassiveSkillFactory</c>를 구현합니다.
/// </summary>
/// <remarks>
/// Unity Editor에서 'SkillSystem/Passive Skill Data/On Cast Attack Buff' 메뉴를 통해 에셋을 생성할 수 있습니다.
/// 이 패시브 데이터 에셋은 <c>AttackBuffStatusEffectData</c>를 참조하여 어떤 공격력 버프 효과를 적용할지 결정합니다.
/// </remarks>
[CreateAssetMenu(fileName = "001_AttackPowerBuff", menuName = "Skill/Passive/Event-Triggered/AttackPowerBuff")]
public class OnCastAttackBuffPassiveData : PassiveSkillData, IPassiveSkillExecutorFactory
{
    [Header("Attack Buff Passive Specifics")]
    [Tooltip("이 패시브가 스킬 시전 시 캐스터에게 적용할 공격력 버프 상태 효과 데이터입니다.")]
    public AttackBuffStatusEffectData attackBuffData;

    /// <summary>
    /// 이 팩토리로부터 실제 런타임 패시브 효과 인스턴스를 생성하여 반환합니다.
    /// <c>OnCastAttackBuffExecutor</c> 인스턴스를 생성하여 반환하며, 공격력 버프 데이터를 전달합니다.
    /// </summary>
    /// <returns>생성된 <c>OnCastAttackBuffExecutor</c> 인스턴스입니다. <c>attackBuffData</c>가 할당되지 않은 경우 <c>null</c>을 반환합니다.</returns>
    public IPassiveSkillExecutor CreatePassiveSkillExecutor()
    {
        if (attackBuffData == null)
        {
            Debug.LogWarning("OnCastAttackBuffPassiveData: No AttackBuffStatusEffectData assigned. Returning null runtime effect.");
            return null;
        }
        // 스킬 시전 시 공격력 버프를 부여할 실제 런타임 효과 인스턴스를 생성하여 반환합니다.
        return new OnCastAttackBuffExecutor(attackBuffData);
    }
}