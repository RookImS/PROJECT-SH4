using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <see cref="EntityListener{T}"/>의 사용자 인터페이스입니다. 사용 후 반드시 <see cref="IDisposable.Dispose"/>를 호출하여 리소스를 해제해야 합니다.
/// </summary>
/// <typeparam name="T"><see cref="EntityListener{T}"/>, <see cref="EntityBroadcaster{T}"/> 객체가 서로 주고 받을 데이터의 형식</typeparam>
public interface IEntityListner<T> : IDisposable where T : Entity
{
    /// <summary>
    /// <see cref="EntityBroadcaster{T}"/> 객체를 구독합니다.
    /// </summary>
    /// <param name="broadcaster">구독할 <see cref="EntityBroadcaster{T}"/> 객체</param>
    public void Subscribe(IObservable<T> broadcaster);

    /// <summary>
    /// <see cref="EntityBroadcaster{T}"/> 객체를 구독 취소합니다.
    /// </summary>
    public void Unsubscribe();

    /// <summary>
    /// <see cref="EntityListener{T}.OnNext(T)"/>가 사용될 때, 작동할 메서드를 갱신합니다.
    /// </summary>
    /// <param name="nextActions"></param>
    public void RenewNextAction(List<Action<T>> nextActions);

    /// <summary>
    /// <see cref="EntityListener{T}.OnCompleted"/>가 사용될 때, 작동할 메서드를 갱신합니다.
    /// </summary>
    public void RenewCompletedAction(List<Action> completedActions);
}

/// <summary>
/// <typeparamref name="T"/>형식 데이터에 대한 알림을 보내주는 <see cref="EntityBroadcaster{T}"/> 객체를 구독할 수 있고, 
/// 각 알림에 대해 적합한 기능을 수행하는 객체에 대한 클래스입니다.
/// </summary>
/// <typeparam name="T"><see cref="EntityListener{T}"/>, <see cref="EntityBroadcaster{T}"/> 객체가 서로 주고 받을 데이터의 형식</typeparam>
public class EntityListener<T> : IEntityListner<T>, IObserver<T> where T : Entity
{
#nullable enable
    // 필드
    private IDisposable? _cancellation;

    // 이벤트
    private event Action<T>? NextAction;
    private event Action? CompletedAction;

    // 생성자
    private EntityListener()
    {
    }

    // 메서드
    /// <summary>
    /// <see cref="EntityListener{T}"/> 객체를 생성하고, 해당 객체를 사용할 때 사용되는 인터페이스를 반환합니다.
    /// </summary>
    /// <param name="nextActions"><see cref="OnNext(T)"/>가 사용될 때, 작동할 메서드</param>
    /// <param name="completedActions"><see cref="OnCompleted"/>가 사용될 때, 작동할 메서드</param>
    /// <returns>생성된 객체의 <see cref="IEntityListner{T}"/>형식 인터페이스</returns>
    public static IEntityListner<T> Create(List<Action<T>> nextActions, List<Action>? completedActions = null)
    {
        var listener = new EntityListener<T>();

        foreach (var action in nextActions)
            listener.NextAction += action;

        if (completedActions is not null)
        {
            foreach (var action in completedActions)
                listener.CompletedAction += action;
        }
        
        return listener;
    }

    #region IEntityListner
    /// <summary>
    /// <see cref="EntityBroadcaster{T}"/> 객체를 구독합니다.<br/>
    /// <see cref="ExecuteInSubscribe"/>로 구독 도중에 해야할 일을 구현할 수 있습니다.
    /// </summary>
    /// <param name="broadcaster">구독할 <see cref="EntityBroadcaster{T}"/> 객체</param>
    public void Subscribe(IObservable<T> broadcaster)
    {
        _cancellation = broadcaster.Subscribe(this);

        if(_cancellation is null)
            throw new InvalidOperationException($"[{GetType().Name}]이미 구독 중인 EntityBroadcaster 객체입니다.");

        ExecuteInSubscribe();
    }

    /// <summary>
    /// <see cref="EntityBroadcaster{T}"/> 객체를 구독 취소합니다.<br/>
    /// <see cref="ExecuteInUnsubscribe"/>로 구독 취소 중에 해야할 일을 구현할 수 있습니다.
    /// </summary>
    public void Unsubscribe()
    {
        ExecuteInUnsubscribe();

        _cancellation?.Dispose();
        _cancellation = null;
    }

    public void RenewNextAction(List<Action<T>> nextActions)
    {
        NextAction = null;
        foreach (var action in nextActions)
            NextAction += action;
    }

    public void RenewCompletedAction(List<Action> completedActions)
    {
        CompletedAction = null;
        foreach (var action in completedActions)
            CompletedAction += action;
    }
    #endregion

    /// <summary>
    /// <see cref="Subscribe(EntityBroadcaster{T})"/> 중에 해야할 일을 구현합니다.
    /// </summary>
    protected virtual void ExecuteInSubscribe()
    {
        Debug.Log($"[{GetType().Name}] Subscribed");
    }

    /// <summary>
    /// <see cref="Unsubscribe"/> 중에 해야할 일을 구현합니다.
    /// </summary>
    protected virtual void ExecuteInUnsubscribe()
    {
        Debug.Log($"[{GetType().Name}] Unsubscribed");
    }

    #region IObserver
    /// <summary>
    /// 구독 중인 <see cref="EntityBroadcaster{T}"/> 객체가 일반적인 알림을 줬을 때, 해야할 내용을 구현합니다.
    /// </summary>
    /// <param name="entity"><see cref="NextAction"/>에서 사용할 데이터</param>
    public virtual void OnNext(T entity) => NextAction?.Invoke(entity);

    /// <summary>
    /// 구독 중인 <see cref="EntityBroadcaster{T}"/> 객체가 일을 완료했을 때, 해야할 내용을 구현합니다.
    /// </summary>
    public virtual void OnCompleted()
    {
        CompletedAction?.Invoke();
        Unsubscribe();
        Debug.Log($"[{GetType().Name}] Completed");
    }

    /// <summary>
    /// 구독 중인 <see cref="EntityBroadcaster{T}"/> 객체에서 에러를 발생 시켰을 때, 해야할 내용을 구현합니다.
    /// </summary>
    /// <param name="e">발생한 오류에 대한 인스턴스</param>
    public virtual void OnError(Exception e) => Debug.LogError($"[{GetType().Name}] Error: {e}");
    #endregion

    #region IDisposable
    private bool _disposed = false;

    /// <summary>
    /// 이 객체를 안전하게 정리합니다.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Unsubscribe();
                NextAction = null;
                CompletedAction = null;
            }
            _disposed = true;
        }
    }

    // 종료자
    ~EntityListener()
    {
        Dispose(false);
    }
    #endregion
}