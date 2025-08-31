using UnityEngine;
using Sh4;

public class ExampleEntity : Entity
{
    // 생성자
    public ExampleEntity(GameObject gameObject, int i, float f, string str) 
    {
        Debug.Log($"[{GetType().Name}] Created");
        Go = gameObject;
        IntValue = i;
        FloatValue = f;
        StringValue = str;
    }

    // 프로퍼티
    public GameObject Go { get; }
    public int IntValue { get; }
    public float FloatValue { get; }
    public string StringValue { get; }
}
