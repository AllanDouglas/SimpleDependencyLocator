using System;
using UnityEngine.Events;

namespace Injector
{
    public delegate void EventHandler();
    public delegate void EventHandler<T>(T data);

    public interface IEvent
    {
        void Dispatch();
        void Subscribe(EventHandler eventHandler);
        void Unsubscribe(EventHandler eventHandler);
    }

    public interface IEvent<T>
    {
        void Dispatch(T data);
        void Subscribe(EventHandler<T> handler);
        void Unsubscribe(EventHandler<T> handler);
    }

    public abstract class Event<T> : IEvent<T>
    {
        [UnityEngine.SerializeField] private T _data;
        private EventHandler<T> EventHandler;
        public void Dispatch(T data) => EventHandler?.Invoke(data);
        public void Subscribe(EventHandler<T> handler) => EventHandler += handler;
        public void Unsubscribe(EventHandler<T> handler) => EventHandler -= handler;

    }

    public abstract class Event : IEvent
    {
        private EventHandler EventHandler;
        public void Dispatch() => EventHandler?.Invoke();
        public void Subscribe(EventHandler handler) => EventHandler += handler;
        public void Unsubscribe(EventHandler handler) => EventHandler -= handler;
    }

    [Serializable]
    public abstract class IntEvent : Event<int> { }
    [Serializable]
    public abstract class FloatEvent : Event<float> { }
    [Serializable]
    public abstract class StringEvent : Event<string> { }
    [Serializable]
    public abstract class BoolEvent : Event<bool> { }

    [Serializable]
    public sealed class InspectorEvent : UnityEvent { }

    [Serializable]
    public sealed class InspectorEvent<TEvent, TData> : UnityEvent<TData>
        where TEvent : IEvent<TData>
    { }


}