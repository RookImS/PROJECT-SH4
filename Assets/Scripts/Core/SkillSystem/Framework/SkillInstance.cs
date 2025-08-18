using UnityEngine;
using System.Collections.Generic;

public class SkillInstance
{
    /// <summary>
    /// 이 스킬 인스턴스가 기반하는 원본 <see cref="ActiveSkillData"/>에 대한 참조입니다.
    /// </summary>
    public ActiveSkillData activeSkillData;

    /// <summary>
    /// 모든 패시브 효과가 적용되어 최종 계산된 이 스킬의 현재 능력치를 담는 객체입니다.
    /// 실제 게임 플레이에 사용될 스킬의 최종 스탯을 나타냅니다.
    /// </summary>
    public RuntimeSkillData runtimeSkillData;

    /// <summary>
    /// 이 스킬 인스턴스에 부착된 원본 <see cref="PassiveSkillData"/>들의 참조 리스트입니다.
    /// 주로 내부 참조 및 디버깅 용도로 사용됩니다.
    /// </summary>
    private List<PassiveSkillData> _attachedPassiveDatas;

    /// <summary>
    /// <c>SkillInstance</c>의 새로운 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="activeData">이 스킬 인스턴스가 기반할 원본 <see cref="ActiveSkillData"/>입니다.</param>
    /// <param name="attachedPassiveDatas">이 스킬 인스턴스에 부착될 <see cref="PassiveSkillData"/>들의 리스트입니다.</param>
    /// <remarks>
    /// <para>1. <see cref="RuntimeSkillData"/>를 <paramref name="activeData"/>의 기본값으로 초기화합니다.</para>
    /// <para>2. 부착된 패시브 중 스탯 수정 패시브(<see cref="SkillStatModPassive"/>)를 찾아 스탯을 수정합니다.
    ///    이 과정은 <see cref="ApplyPreExecutionModifiers(RuntimeSkillData, List{PassiveSkillData})"/> 메서드를 통해 이루어집니다.</para>
    /// <para>3. 이벤트 기반 패시브의 런타임 효과 인스턴스를 생성하고 <see cref="RuntimeSkillData.attachedRuntimePassiveEffects"/> 리스트에 할당합니다.
    ///    이는 <see cref="IRuntimePassiveEffectFactory"/> 인터페이스를 통해 이루어집니다.</para>
    /// </remarks>
    public SkillInstance(ActiveSkillData activeData, List<PassiveSkillData> attachedPassiveDatas)
    {
        this.activeSkillData = activeData;
        this._attachedPassiveDatas = attachedPassiveDatas;

        // 1. RuntimeSkillData를 ActiveSkillData의 기본값으로 초기화
        this.runtimeSkillData = new RuntimeSkillData(activeData);

        // 2. 부착된 패시브 중 스탯 수정 패시브를 찾아 스탯을 수정
        ApplyPreExecutionModifiers(this.runtimeSkillData, attachedPassiveDatas);

        // 3. 이벤트 기반 패시브의 런타임 효과 인스턴스를 생성 및 할당
        this.runtimeSkillData.attachedRuntimePassiveEffects = new List<IRuntimePassiveEffect>();
        foreach (var passiveData in attachedPassiveDatas)
        {
            // PassiveSkillData가 IRuntimePassiveEffectFactory를 구현하도록 했으므로 캐스팅하여 사용
            if (passiveData is IRuntimePassiveEffectFactory factory)
            {
                IRuntimePassiveEffect runtimeEffect = factory.CreateRuntimeEffect();
                if (runtimeEffect != null)
                {
                    this.runtimeSkillData.attachedRuntimePassiveEffects.Add(runtimeEffect);
                }
            }
            // 참고: SkillStatModPassive는 CreateRuntimeEffect()에서 null을 반환하도록 설정했으므로
            // 여기서는 스탯 수정 런타임 효과가 추가되지 않습니다.
            // 만약 StatModifier도 런타임 효과가 필요하다면 해당 클래스의 CreateRuntimeEffect()를 수정해야 합니다.
        }
    }

    /// <summary>
    /// 스킬 인스턴스 생성 시점에 호출되어, 부착된 모든 스탯 수정 패시브들을 <see cref="RuntimeSkillData"/>에 적용합니다.
    /// </summary>
    /// <param name="runtimeData">스탯 수정이 적용될 대상 <see cref="RuntimeSkillData"/> 객체입니다.</param>
    /// <param name="passiveDatas">스킬에 부착된 모든 <see cref="PassiveSkillData"/> 리스트입니다.</param>
    /// <remarks>
    /// 이 메서드는 설계서의 "3.1.b) 이후 부착된 모든 속성 수정 패시브들이 ApplyPreExecutionModifiers 메서드를 통해 이 RuntimeSkillData의 값들을 수정한다."에 해당합니다.
    /// </remarks>
    private void ApplyPreExecutionModifiers(RuntimeSkillData runtimeData, List<PassiveSkillData> passiveDatas)
    {
        foreach (var passiveData in passiveDatas)
        {
            // SkillStatModPassive 타입인지 확인하여 스탯 수정 로직을 적용합니다.
            if (passiveData is SkillStatModPassive skillStatModPassive)
            {
                foreach (var modEntry in skillStatModPassive.statModifications)
                {
                    // RuntimeSkillData의 ApplyStatModifier 메서드를 호출하여 스탯 수정을 위임합니다.
                    runtimeData.ApplyStatModifier(modEntry);
                }
            }
            // 다른 유형의 패시브 (예: EventTriggeredPassiveData)에 대한 초기 수정 로직이 있다면 여기에 추가될 수 있습니다.
        }
    }

    // ApplyStatModification 메서드는 RuntimeSkillData로 이동했습니다.
    // ApplyValue 메서드도 RuntimeSkillData로 이동했습니다.

    /// <summary>
    /// 캐릭터가 특정 스킬 인스턴스를 사용하고자 할 때 호출됩니다.
    /// </summary>
    /// <param name="caster">스킬을 시전하는 <see cref="Character"/>입니다.</param>
    /// <param name="target">스킬의 대상 <see cref="Character"/> (옵션, 기본값 null).</param>
    /// <remarks>
    /// <para>이 메서드는 설계서의 "2.3.2. 핵심 로직: UseSkill()"에 해당합니다.</para>
    /// <para>쿨다운을 적용하고, 스킬 시전 이벤트를 발동시키며, 스킬 타입에 따른 실제 공격 로직을 호출합니다.</para>
    /// </remarks>
    public void UseSkill(Character caster, Character target = null)
    {
        // 쿨다운 적용 (캐스터의 CooldownManager를 통해)
        // Note: Character.StartCooldown should ideally return something or take a callback for actual cooldown management.
        caster.StartCooldown(runtimeSkillData.currentCoreStats.cooldown);
        Debug.Log($"캐릭터 '{caster.name}'가 스킬 '{activeSkillData.skillName}' 사용 시도. 쿨다운: {runtimeSkillData.currentCoreStats.cooldown}초.");

        // 스킬 시전 이벤트 발동 (모든 런타임 패시브 효과에 OnSkillCast 호출)
        foreach (var runtimeEffect in runtimeSkillData.attachedRuntimePassiveEffects)
        {
            runtimeEffect.OnSkillCast(caster, this);
        }

        // 실제 공격 로직 실행 (SkillExecutionManager에서 담당하는 것이 더 적합)
        Debug.Log($"'{activeSkillData.skillName}' 스킬 발동!");

        // 예시: SkillAttackType에 따른 더미 로직 분기
        if (activeSkillData.skillAttackType == SkillAttackType.Projectile)
        {
            Debug.Log($"투사체 발사! 속도: {runtimeSkillData.currentProjectileSpeed}, 크기: {runtimeSkillData.currentProjectileSize}, 사거리: {runtimeSkillData.currentCoreStats.range}");
            // 실제 투사체 생성 및 이동 로직 (SkillExecutionManager에서 구현될 예정)
            // 투사체가 적중했다고 가정하고 OnSkillHit 호출 (임시)
            // if (target != null) OnSkillHit(caster, target, CalculateFinalDamage(caster, target));
        }
        else if (activeSkillData.skillAttackType == SkillAttackType.AreaOfEffect)
        {
            Debug.Log($"AOE 스킬 발동! 반경: {runtimeSkillData.currentAoeRadius}, 지연 시간: {runtimeSkillData.currentAoeDelay}, 사거리: {runtimeSkillData.currentCoreStats.range}");
            // 실제 AOE 이펙트 생성 및 범위 판정 로직 (SkillExecutionManager에서 구현될 예정)
        }
        else if (activeSkillData.skillAttackType == SkillAttackType.Melee)
        {
Debug.Log($"근접 스킬 발동! 애니메이션: {runtimeSkillData.currentAnimationTriggerName}, 아크 각도: {runtimeSkillData.currentMeleeArcAngle}, 사거리: {runtimeSkillData.currentCoreStats.range}");
            // 실제 근접 공격 판정 로직 (SkillExecutionManager에서 구현될 예정)
        }
    }

    /// <summary>
    /// 이 메서드는 스킬의 공격이 대상에게 성공적으로 적중했을 때 호출됩니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 <see cref="Character"/>입니다.</param>
    /// <param name="target">스킬에 적중된 <see cref="Character"/> 대상입니다.</param>
    /// <param name="damageDealt">대상에게 실제로 가해진 최종 피해량입니다.</param>
    /// <remarks>
    /// <para>이 메서드는 설계서의 "2.3.2. 핵심 로직: OnSkillHit()"에 해당합니다.</para>
    /// <para>주로 타격 시 발동하는 패시브 효과를 처리하는 역할을 합니다.</para>
    /// </remarks>
    public void OnSkillHit(Character caster, Character target, float damageDealt)
    {
        Debug.Log($"'{activeSkillData.skillName}' 스킬이 '{target.name}'에게 적중! 예상 피해: {damageDealt}");

        // 타격 이벤트 발동 (모든 런타임 패시브 효과에 OnSkillHit 호출)
        foreach (var runtimeEffect in runtimeSkillData.attachedRuntimePassiveEffects)
        {
            runtimeEffect.OnSkillHit(caster, target, this, damageDealt);
        }

        // 실제 피해 적용 로직 (예: target.TakeDamage(damageDealt);)
        // Debug.Log($"{target.name}에게 {damageDealt} 피해를 입혔습니다.");
    }

    /// <summary>
    /// 스킬이 대상에게 입힐 최종 피해량을 계산합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전하는 <see cref="Character"/>입니다.</param>
    /// <param name="target">피해를 받을 대상 <see cref="Character"/>입니다.</param>
    /// <returns>스킬이 대상에게 입힐 최종 피해량입니다.</returns>
    /// <remarks>
    /// <para>이 메서드는 설계서의 "2.3.2. 핵심 로직: CalculateFinalDamage()"에 해당합니다.</para>
    /// <para>캐스터의 공격력과 대상의 방어력, 그리고 현재 스킬 인스턴스의 모든 공격 관련 속성을 종합하여 계산합니다.</para>
    /// <para>계산은 다음 순서로 이루어집니다:</para>
    /// <para>1. 스킬 기본 및 계수 대미지 계산</para>
    /// <para>2. 대상 방어력에 의한 피해 감소 적용</para>
    /// <para>3. 고정 추가 대미지 합산</p>
    /// <para>4. 캐릭터 추가 피해 적용</para>
    /// <para>5. 최종 치명타 확률 결정 및 6. 최종 치명타 피해 배율 결정 및 적용</para>
    /// </remarks>
    public float CalculateFinalDamage(Character caster, Character target)
    {
        float finalDamage = 0f;

        // 1단계: 스킬 기본 및 계수 대미지 계산
        finalDamage = runtimeSkillData.currentCoreStats.baseDamage + (caster.AttackPower * runtimeSkillData.currentCoreStats.characterAttackPowerMultiplier);
        Debug.Log($"1단계 기본+계수 데미지: {finalDamage}");

        // 2단계: 대상 방어력에 의한 피해 감소 적용
        finalDamage *= (1.0f - target.DefenseReductionRate);
        Debug.Log($"2단계 방어력 적용 후 데미지: {finalDamage}");

        // 3단계: 고정 추가 대미지 합산
        // (설계서에 캐릭터의 고정 추가 대미지 필드가 없으므로 스킬의 고정 추가 대미지만 적용)
        finalDamage += runtimeSkillData.currentCoreStats.flatDamageAdded;
        Debug.Log($"3단계 고정 추가 데미지 합산 후: {finalDamage}");

        // 4단계: 캐릭터 추가 피해 적용
        finalDamage *= (1 + caster.BonusDamageMultiplier);
        Debug.Log($"4단계 캐릭터 추가 피해 계수 적용 후: {finalDamage}");

        // 5단계: 최종 치명타 확률 결정 및 6단계: 최종 치명타 피해 배율 결정 및 적용
        // 스킬 고유 치명타 확률과 캐릭터 치명타 확률 합산
        float combinedCritChance = runtimeSkillData.currentCoreStats.baseCriticalChance + caster.CriticalChance;
        float combinedCritDamageMultiplier = runtimeSkillData.currentCoreStats.baseCriticalDamageMultiplier + (caster.CriticalDamageMultiplier - 1.0f); // 캐릭터 기본 배율이 1.5면 0.5가 더해지도록

        // 랜덤하게 치명타 발생 여부 결정
        bool isCriticalHit = UnityEngine.Random.value < combinedCritChance;
        Debug.Log($"최종 치명타 확률: {combinedCritChance * 100}%, 치명타 발생: {isCriticalHit}");

        if (isCriticalHit)
        {
            finalDamage *= combinedCritDamageMultiplier;
            Debug.Log($"6단계 치명타 발생! 최종 치명타 피해 배율 {combinedCritDamageMultiplier} 적용 후 데미지: {finalDamage}");
        }

        return finalDamage;
    }
}