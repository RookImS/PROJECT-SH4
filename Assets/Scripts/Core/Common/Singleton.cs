/// <summary>
/// <c>T</c>형식의 싱글톤 객체를 구현하는 클래스입니다.<br/>
/// <example><c>class A : Singleton&lt;A&gt;</c>와 같은 형태로 사용하면 A클래스를 싱글톤 패턴 클래스로 사용할 수 있습니다.</example>
/// </summary>
/// <typeparam name="T">싱글톤으로 만들고 싶은 클래스</typeparam>
public class Singleton<T> where T : class, new()
{
    private static readonly T _instance = new();

    protected Singleton() { }

    /// <summary>
    /// 싱글톤 인스턴스에 접근할 수 있는 프로퍼티로 필요시 public으로 재정의해서 사용합니다.
    /// </summary>
    protected static T Instance => _instance;
}