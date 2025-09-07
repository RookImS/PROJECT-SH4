using System;
using System.Reflection;

namespace Sh4
{
    /// <summary>
    /// 상속한 클래스의 싱글톤 객체를 구현하는 클래스입니다.<br/>
    /// <example><c>class A : Singleton&lt;A&gt;</c>와 같은 형태로 사용하면 A클래스를 싱글톤 패턴 클래스로 사용할 수 있습니다.</example><br/>
    /// 이때, 상속한 클래스는 반드시 매개변수가 없는 <see langword="private"/>생성자를 가지고 있어야 합니다.
    /// </summary>
    /// <typeparam name="T">싱글톤 패턴으로 만들고 싶은 클래스</typeparam>
    public class Singleton<T> where T : class
    {
        private static readonly T _instance = (T)typeof(T)
            .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, 
                            CallingConventions.HasThis, new Type[0], null)
            .Invoke(null);

        protected Singleton() { }

        /// <summary>
        /// 싱글톤 인스턴스에 접근할 수 있는 프로퍼티로 필요시 <c>public</c>으로 재정의해서 사용합니다.
        /// </summary>
        protected static T Instance => _instance;
    }
}