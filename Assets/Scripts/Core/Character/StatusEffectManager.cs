using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    /// <summary>
    /// 현재 캐릭터에게 적용된 상태 효과의 정보를 관리합니다. 
    /// </summary>
    private class ActiveEffect
    {
        public StatusEffectData data;
        public float remaining;
        public Coroutine runner;
        public int stackCount = 1;
        public List<StatModifier> appliedModifiers = new();
    }

    private readonly Dictionary<int, ActiveEffect> _activeEffects = new();

    private IStatProvider _statProvider;

    private void Awake()
    {
        _statProvider = GetComponent<IStatProvider>();
    }

    /// <summary>
    /// 상태 효과를 캐릭터에 적용.
    /// </summary>
    /// <param name="effectData"></param>
    public void ApplyStatusEffect(StatusEffectData effectData)
    {
        Debug.Log($"캐릭터 '{gameObject.name}'에게 상태 효과 '{effectData.effectName}' 적용 시도.");


        if (effectData == null)
        {
            Debug.LogWarning("적용하려는 상태 효과 데이터가 null입니다.");
            return;
        }

        if (_activeEffects.TryGetValue(effectData.effectId, out var existingEffect))
        {
            Debug.Log($"상태 효과 '{effectData.effectName}'가 이미 적용되어 있습니다. 지속 시간을 갱신합니다.");
            existingEffect.remaining = effectData.duration;
            if (existingEffect.runner != null)
            {
                StopCoroutine(existingEffect.runner);
            }
            existingEffect.runner = StartCoroutine(RunEffect(existingEffect));
            return;
        }

        var newEffect = new ActiveEffect
        {
            data = effectData,
            remaining = effectData.duration,
            stackCount = 1,
            appliedModifiers = new List<StatModifier>(effectData.statModifiers)
        };

        newEffect.runner = StartCoroutine(RunEffect(newEffect));
        _activeEffects.Add(effectData.effectId, newEffect);
    }

    /// <summary>
    /// 상태 효과의 지속 시간을 관리하고, 효과 시작 및 종료 시 필요한 로직을 처리
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    private IEnumerator RunEffect(ActiveEffect effect)
    {
        Debug.Log($"상태 효과 '{effect.data.effectName}'가 적용되었습니다.");

        // 효과 시작 시점에 필요한 로직 추가 (예: 버프 적용)
        if (_statProvider == null)
        {
            Debug.LogWarning("IStatProvider 컴포넌트를 찾을 수 없습니다. 버프 적용 실패.");
            yield break;
        }

        foreach (var mod in effect.appliedModifiers)
        {
            _statProvider.AddModifier(mod);
        }

        while (effect.remaining > 0f)
        {
            effect.remaining -= Time.deltaTime;
            yield return null;
        }

        EndStatusEffect(effect);
    }

    /// <summary>
    /// 상태 효과를 강제로 제거합니다.
    /// </summary>
    /// <param name="effectId"></param>
    public void RemoveStatusEffect(int effectId)
    {
        if (_activeEffects.TryGetValue(effectId, out var effect))
        {
            StopCoroutine(effect.runner);
            EndStatusEffect(effect);
        }
    }

    private void EndStatusEffect(ActiveEffect effect)
    {
        // 효과 종료 시점에 필요한 로직 추가 (예: 버프 제거)
        if (_statProvider == null)
        {
            Debug.LogWarning("IStatProvider 컴포넌트를 찾을 수 없습니다. 버프 제거 실패.");
            return;
        }

        foreach (var mod in effect.appliedModifiers)
            _statProvider.RemoveModifier(mod);

        Debug.Log($"상태 효과 '{effect.data.effectName}'가 종료되었습니다.");
        _activeEffects.Remove(effect.data.effectId);
    }
}
