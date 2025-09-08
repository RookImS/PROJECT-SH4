using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    private readonly Dictionary<int, float> _cooldowns = new();

    public event Action<int, float> OnCooldownStarted;
    public event Action<int> OnCooldownEnded;

    /// <summary>
    /// 스킬의 쿨타임을 시작합니다.
    /// </summary>
    /// <param name="skillId"></param>
    /// <param name="duration"></param>
    /// <returns>쿨타임이 시작되었는지 여부</returns>
    public bool StartCooldown(int skillId, float duration)
    {
        float endTime = Time.time + duration;

        if (IsOnCooldown(skillId)) return false;

        _cooldowns.Add(skillId, endTime);
        OnCooldownStarted?.Invoke(skillId, duration);
        StartCoroutine(CooldownCoroutine(skillId, duration));
        return true;
    }

    private IEnumerator CooldownCoroutine(int skillId, float duration)
    {
        yield return new WaitForSeconds(duration);
        _cooldowns.Remove(skillId);
        OnCooldownEnded?.Invoke(skillId);
    }

    /// <summary>
    /// 쿨타임 상태인지 확인.
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public bool IsOnCooldown(int skillId)
    {
        return _cooldowns.ContainsKey(skillId) && _cooldowns[skillId] > Time.time;
    }

    /// <summary>
    /// 남은 쿨타임 반환.
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public float GetRemainingTime(int skillId)
    {
        if (!_cooldowns.ContainsKey(skillId)) return 0f;
        return Mathf.Max(0, _cooldowns[skillId] - Time.time);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        _cooldowns.Clear();
    }
}
