using System.Collections.Generic;
using System.Linq;

namespace Sh4
{
    /// <summary>
    /// <see cref="Dictionary{TKey, TValue}"/>와 동일한 기능을 제공하지만 Key를 <see cref="WeakKey{T}"/>로 관리합니다.<br/>
    /// 이를 통해 Key로 사용된 인스턴스에 대한 참조가 모두 유실되어 이에 연결된 Value에 접근할 수 없거나, 
    /// 반대로 Key에 의해 생긴 참조로 사용이 종료된 인스턴스를 GC가 정리하지 못하는 상황을 방지합니다.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class WeakKeyDictionary<TKey, TValue> : Dictionary<WeakKey<TKey>, TValue> where TKey : class
    {
#nullable enable
        /// <summary>
        /// Key 전체를 검사해 참조가 유실된 인스턴스가 Key로 사용되는 경우, 
        /// 해당 요소를 제거하고 그 Key에 연결된 Value를 돌려줍니다.
        /// </summary>
        /// <param name="missingValues">참조가 유실된 Key들에 연결된 Value들을 담는 객체</param>
        /// <returns>참조가 유실된 Key가 있으면 <see langword="true"/>, 그렇지 않으면 <see langword="false"/></returns>
        public bool CullMissingKey(out List<TValue> missingValues)
        {
            bool hasMissing = false;

            WeakKey<TKey>[] keys = Keys.ToArray();
            TValue[]? values = null;
            List<WeakKey<TKey>> missingKeys = new();
            missingValues = new();

            for (int i = 0; i < keys.Length; i++)
            {
                bool isMissing = !keys[i].TryGetTarget(out TKey target);

                if (!isMissing && target is UnityEngine.Object unityObj)
                {
                    isMissing = !unityObj;
                }

                if (isMissing)
                {
                    if (!hasMissing)
                    {
                        hasMissing = true;
                        values = Values.ToArray();
                    }

#pragma warning disable CS8602 // null 가능 참조에 대한 역참조입니다.
                    missingKeys.Add(keys[i]);
                    missingValues.Add(values[i]);
#pragma warning restore CS8602 // null 가능 참조에 대한 역참조입니다.
                }
            }

            foreach (var key in missingKeys)
            {
                Remove(key);
            }

            return hasMissing;
        }
    }
}