using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sh4
{
    /// <summary>
    /// <see cref="EntityBroadcaster{T}"/>의 사용자 인터페이스입니다.
    /// </summary>
    /// <typeparam name="T"><see cref="EntityListener{T}"/>, <see cref="EntityBroadcaster{T}"/> 객체가 서로 주고 받을 데이터의 형식</typeparam>
    public interface IEntityBroadcaster<T> where T : Entity
    {
        /// <summary>
        /// 인터페이스의 기능을 사용하기 위해 싱글톤 인스턴스에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static IEntityBroadcaster<T> Access { get; }

        /// <summary>
        /// <see cref="EntityBroadcaster{T}.OnBroadcast(object, T)"/>에 대한 이벤트 핸들러입니다.
        /// </summary>
        public EventHandler<T> BroadcastHandler { get; }

        /// <summary>
        /// <see cref="EntityBroadcaster{T}.OnCompleted"/>에 대한 이벤트 핸들러입니다.
        /// </summary>
        public Action CompletedHandler { get; }

        /// <summary>
        /// <see cref="EntityBroadcaster{T}.OnError(Exception)"/>에 대한 이벤트 핸들러입니다.
        /// </summary>
        public Action<Exception> ErrorHandler { get; }
    }

    /// <summary>
    /// 구독 중인 <see cref="EntityListener{T}"/> 객체들에 <typeparamref name="T"/>형식 데이터에 대한 알림을 제공하는 싱글톤 인스턴스입니다.
    /// </summary>
    /// <typeparam name="T"><see cref="EntityListener{T}"/>, <see cref="EntityBroadcaster{T}"/> 객체가 서로 주고 받을 데이터의 형식</typeparam>
    public class EntityBroadcaster<T> : Singleton<EntityBroadcaster<T>>, IEntityBroadcaster<T>, IObservable<T> where T : Entity
    {
#nullable enable
        // 필드
        private readonly HashSet<IObserver<T>?> _listeners = new();

        // 생성자
        static EntityBroadcaster()
        {
            Access = Instance;
            Observable = Instance;
        }

        protected EntityBroadcaster()
        {
            BroadcastHandler = OnBroadcast;
            CompletedHandler = OnCompleted;
            ErrorHandler = OnError;
        }

        // 프로퍼티
        /// <summary>
        /// <see cref="IEntityBroadcaster{T}"/>의 인터페이스를 사용하기 위해 싱글톤 인스턴스에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static IEntityBroadcaster<T> Access { get; }
        /// <summary>
        /// <see cref="IObservable{T}"/>의 인터페이스를 사용하기 위해 싱글톤 인스턴스에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static IObservable<T> Observable { get; }
        public EventHandler<T> BroadcastHandler { get; }
        public Action CompletedHandler { get; }
        public Action<Exception> ErrorHandler { get; }

        // public 메서드
        /// <summary>
        /// 이 객체를 구독하는 <see cref="EntityListener{T}"/> 객체를 목록에 추가합니다.
        /// </summary>
        /// <param name="listener">구독을 요청한 객체</param>
        /// <returns>연결을 해제할 때 사용할 객체</returns>
        public IDisposable? Subscribe(IObserver<T> listener)
        {
            if (_listeners.Add(listener))
            {
                return new Unsubscriber(_listeners, listener);
            }

            return null;
        }

        // private 메서드
        /// <summary>
        /// 이 객체를 구독 중인 모든 <see cref="EntityListener{T}"/> 객체에 <typeparamref name="T"/>형식 데이터에 대한 알림을 제공합니다.
        /// </summary>
        private void OnBroadcast(object sender, T entity)
        {
            UnityEditorTools.Log($"[{GetType().Name}] Broadcasting \"{sender} class - {entity.GetType().Name} \" to {_listeners.Count} listeners.");

            foreach (var listener in _listeners.ToArray())
            {
                if (listener is null)
                {
                    _listeners.Remove(listener);
                    continue;
                }
                listener.OnNext(entity);
            }
        }

        /// <summary>
        /// 이 객체를 구독 중인 모든 <see cref="EntityListener{T}"/> 객체에 완료에 대한 알림을 제공합니다.
        /// </summary>
        private void OnCompleted()
        {
            UnityEditorTools.Log($"[{GetType().Name}] Broadcasting complete to {_listeners.Count} listeners.");

            foreach (var listener in _listeners.ToArray())
            {
                if (listener is null)
                {
                    _listeners.Remove(listener);
                    continue;
                }

                listener.OnCompleted();
            }
        }

        /// <summary>
        /// 이 객체를 구독 중인 모든 <see cref="EntityListener{T}"/> 객체에 에러에 대한 알림을 제공합니다.
        /// </summary>
        private void OnError(Exception e)
        {
            UnityEditorTools.Log($"[{GetType().Name}] Broadcasting error to {_listeners.Count} listeners.");

            foreach (var listener in _listeners.ToArray())
            {
                if (listener is null)
                {
                    _listeners.Remove(listener);
                    continue;
                }

                listener.OnError(e);
            }
        }

        // 내부 클래스
        /// <summary>
        /// 관찰 대상의 구독 취소시 사용하는 객체에 대한 클래스입니다.<br/>
        /// 이 클래스의 <see cref="Dispose"/>를 사용해 안전하게 <see cref="EntityListener{T}"/> 객체와 <see cref="EntityBroadcaster{T}"/> 객체의 연결을 끊을 수 있습니다.
        /// </summary>
        private class Unsubscriber : IDisposable
        {
            private readonly ISet<IObserver<T>?> _observers;
            private readonly IObserver<T> _observer;

            public Unsubscriber(ISet<IObserver<T>?> observers, IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            /// <summary>
            /// 안전하게 <see cref="EntityListener{T}"/> 객체와 <see cref="EntityBroadcaster{T}"/> 객체의 연결을 끊을 수 있습니다.
            /// </summary>
            public void Dispose() => _observers.Remove(_observer);
        }
    }
}