using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    private readonly Dictionary<int, float> _cooldowns = new Dictionary<int, float>();

    /// <summary>
    /// 스킬의 쿨타임을 시작합니다.
    /// </summary>
    /// <param name="runtimeSkillData"></param>
    public void StartCooldown(RuntimeSkillData runtimeSkillData)
    {
        int skillId = runtimeSkillData.baseActiveData.skillId;
        float duration = runtimeSkillData.currentCoreStats.cooldown;
        float endTime = Time.time + duration;

        if (_cooldowns.ContainsKey(skillId))
        {
            Debug.LogWarning($"{runtimeSkillData.baseActiveData.skillName}가 쿨타임 상태입니다.");
            return;
        }

        _cooldowns[skillId] = endTime;
        StartCoroutine(CooldownCoroutine(runtimeSkillData));
    }

    private IEnumerator CooldownCoroutine(RuntimeSkillData runtimeSkillData)
    {
        yield return new WaitForSeconds(runtimeSkillData.currentCoreStats.cooldown);
        _cooldowns.Remove(runtimeSkillData.baseActiveData.skillId);
        Debug.Log($"{runtimeSkillData.baseActiveData.skillName}의 쿨타임 종료.");
    }

    /// <summary>
    /// 쿨타임 상태인지 확인.
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public bool IsOnCooldown(RuntimeSkillData runtimeSkillData)
    {
        int skillId = runtimeSkillData.baseActiveData.skillId;
        return _cooldowns.ContainsKey(skillId) && _cooldowns[skillId] > Time.time;
    }

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
}
