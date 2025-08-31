using System;
using UnityEngine;
using System.Collections.Generic;


public class ExampleManager1 : MonoBehaviour
{
    private IEntityListner<ExampleEntity> listener;

    private void Awake()
    {
        listener = EntityListener<ExampleEntity>.Create(new List<Action<ExampleEntity>>{ Print });
        listener.Subscribe(EntityBroadcaster<ExampleEntity>.Observable);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDestroy()
    {
        listener?.Dispose();
    }

    private void Print(ExampleEntity entity)
    {
        Debug.Log($"[{GetType().Name}]");
        Debug.Log($"GameObject: {entity.Go.name}");
        Debug.Log($"IntValue: {entity.IntValue}");
        Debug.Log($"FloatValue: {entity.FloatValue}");
        Debug.Log($"StringValue: {entity.StringValue}");
    }
}
