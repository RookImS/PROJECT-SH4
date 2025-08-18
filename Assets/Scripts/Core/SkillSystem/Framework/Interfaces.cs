using UnityEngine;

public interface IRuntimePassiveEffect
{
    /*
        # 함수
        void OnSkillCast(Character caster, SkillInstance skillInstance)
        
        # 주요 로직
        스킬이 시전될 때 발동하는 로직 정의
        스킬 시전 시점에 특정 패시브 효과가 발동해야 할 때 구현된다.

        # 매개변수
        caster: 스킬을 시전한 Character.
        skillInstance: 현재 사용되고 있는 SkillInstance
    
        # 반환값
    */
    void OnSkillCast(Character caster, SkillInstance skillInstance);

    /*
        # 함수
        void OnSkillHit(Character caster, Character target,
        SkillInstance skillInstance, float damageDealt)
        
        # 주요 로직
        스킬이 적에게 성공적으로 적중했을 때 발동하는 로직
        스킬의 공격이 대상에게 피해를 입히는 시점에 특정 패시브 효과가 발동해야 할 때 구현된다.

        # 매개변수
        caster: 스킬을 시전한 Character
        target: 스킬에 적중된 Character
        skillInstance: 현재 사용되고 있는 SkillInstance
        damageDealt: 대상에게 실제로 가해진 최종 피해량

        # 반환값
    */
    void OnSkillHit(Character caster, Character target, SkillInstance skillInstance, float damageDealt);
}


public interface IRuntimePassiveEffectFactory
{

    /*
        # 함수
        IRuntimePassiveEffect CreateRuntimeEffect()
        
        # 주요 로직
        스킬 인스턴스에 독립적인 런타임 패시브 효과 로직을 부여한다.
        실제 런타임 패시브 효과 (IRuntimePassiveEffect) 인스턴스를 생성하여 반환한다.

        # 매개변수
    
        # 반환값
        이 패시브 데이터에 대한 고유한 런타임 효과 인스턴스 반환
        (고유한 효과가 없다면 null)
    */
    IRuntimePassiveEffect CreateRuntimeEffect();
}