using System;

namespace Sh4
{
    /// <summary>
    /// 이 타입으로 생성된 객체를 <see cref="Dictionary{TKey, TValue}"/>의 Key로 사용하면, 
    /// 그 객체를 만들때 사용된 <typeparamref name="T"/> 타입 원본 인스턴스가 Key로 쓰이는 것으로 간주됩니다.<br/>
    /// 또한, 이 타입의 객체는 원본 인스턴스를 약하게 참조하고 있어서, 객체의 참조가 원본 인스턴스에 대한 GC의 작동에 영향을 주지 않습니다.
    /// </summary>
    /// <typeparam name="T">약하게 참조할 원본 인스턴스의 타입</typeparam>
    /// <remarks>
    /// <typeparamref name="T"/> 타입은 이 클래스로 암시적 변환할 수 있습니다.<br/>
    /// 이 클래스는 <typeparamref name="T"/> 타입으로 명시적 변활할 수 있습니다.
    /// </remarks>
    public class WeakKey<T> where T : class
    {
#nullable enable
        // 필드
        private readonly WeakReference<T> _weakKey;
        private readonly int _hashCode;

        // 생성자
#pragma warning disable CS8618 // null을 허용하지 않는 필드는 생성자를 종료할 때 null이 아닌 값을 포함해야 합니다. 'required' 한정자를 추가하거나 nullable로 선언하는 것이 좋습니다.
        private WeakKey() { }
#pragma warning restore CS8618 // null을 허용하지 않는 필드는 생성자를 종료할 때 null이 아닌 값을 포함해야 합니다. 'required' 한정자를 추가하거나 nullable로 선언하는 것이 좋습니다.

        public WeakKey(T key)
        {
            _weakKey = new(key);
            _hashCode = key.GetHashCode();
        }

        public static implicit operator WeakKey<T>(T key) => new WeakKey<T>(key);

        public static explicit operator T(WeakKey<T> key) => key.TryGetTarget(out T target) ? target : throw new NullReferenceException("Explicit operator for T in WeakKey<T> can only be used where the referenced instance is not null.");

        // 메서드
        /// <summary>
        /// 약하게 참조 중인 원본 인스턴스를 가져옵니다.
        /// </summary>
        /// <param name="target">
        /// 원본 인스턴스를 참조할 객체<br/>
        /// 원본 인스턴스를 참조할 수 있는 상태가 아니라면 이 매개변수는 비초기화 상태로 남습니다.
        /// </param>
        /// <returns>원본 인스턴스를 가져오는 것에 성공하면 <see langword="true"/>, 실패하면 <see langword="false"/></returns>
        public bool TryGetTarget(out T target) => _weakKey.TryGetTarget(out target);

        public override int GetHashCode() => _hashCode;

        public override bool Equals(object obj)
        {
            if (obj is WeakKey<T> other)
            {
                if (_weakKey.TryGetTarget(out var thisTarget) && other.TryGetTarget(out var otherTarget))
                {
                    return ReferenceEquals(thisTarget, otherTarget);
                }
            }

            return false;
        }
    }
}