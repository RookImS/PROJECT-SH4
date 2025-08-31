using System;
using System.Collections.Generic;
using UnityEngine;
using Sh4;

public class ExampleManager2 : MonoBehaviour
{
    private IEntityListner<ExampleEntity> listener;
    float timer = 0.0f;
    bool completed = false;

    private void Awake()
    {
        listener = EntityListener<ExampleEntity>.Create(new List<Action<ExampleEntity>>{ PrintEntity });
        listener.Subscribe(EntityBroadcaster<ExampleEntity>.Observable);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!completed)
        {
            timer += Time.deltaTime;
            if (timer >= 6.0f)
            {
                //listener.Unsubscribe();
                Destroy(this.gameObject);
                completed = true;
            }
        }
    }

    private void OnDestroy()
    {
        listener?.Dispose();
    }
    private void PrintEntity(ExampleEntity entity)
    {
        Debug.Log($"[{GetType().Name}] entity: {entity.Go.name}, {entity.IntValue}, {entity.FloatValue}, {entity.StringValue}");
    }
}
