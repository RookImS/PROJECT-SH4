using System;
using UnityEngine;

public class ObserverTest : MonoBehaviour
{
    event EventHandler<ExampleEntity> TestEvent;
    event Action CompleteEvent;
    float timer = 0.0f;
    bool[] completed = { false, false };

    private void Awake()
    {
        TestEvent += EntityBroadcaster<ExampleEntity>.Access.BroadcastHandler;
        CompleteEvent += EntityBroadcaster<ExampleEntity>.Access.CompletedHandler;
    }

    private void Start()
    {
        TestEvent?.Invoke(this, new ExampleEntity(transform.gameObject, 3, 5.0f, "Test1"));
    }

    private void Update()
    {
        if (!completed[1])
        {
            timer += Time.deltaTime;
            if (!completed[0] && timer >= 3.0f)
            {
                // CompleteEvent?.Invoke();
                TestEvent?.Invoke(this, new ExampleEntity(transform.gameObject, 1, 3.0f, "Test2"));
                completed[0] = true;
            }

            if(timer >= 9.0f)
            {
                TestEvent?.Invoke(this, new ExampleEntity(transform.gameObject, 5, 3.0f, "Test3"));
                completed[1] = true;
            }
        }
    }
}