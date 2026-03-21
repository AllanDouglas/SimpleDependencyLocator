using System;
using UnityEngine.Events;

namespace Injector
{
    public delegate void SignalHandler();
    public delegate void SignalHandler<T>(T data);

    public interface ISignal
    {
        void Dispatch();
        void Subscribe(SignalHandler SignalHandler);
        void Unsubscribe(SignalHandler SignalHandler);
    }

    public interface ISignal<T>
    {
        void Dispatch(T data);
        void Subscribe(SignalHandler<T> handler);
        void Unsubscribe(SignalHandler<T> handler);
    }

    public abstract class Signal<T> : ISignal<T>
    {
        [UnityEngine.SerializeField] private T _data;
        private SignalHandler<T> SignalHandler;
        public void Dispatch(T data) => SignalHandler?.Invoke(data);
        public void Subscribe(SignalHandler<T> handler) => SignalHandler += handler;
        public void Unsubscribe(SignalHandler<T> handler) => SignalHandler -= handler;

    }

    public abstract class Signal : ISignal
    {
        private SignalHandler SignalHandler;
        public void Dispatch() => SignalHandler?.Invoke();
        public void Subscribe(SignalHandler handler) => SignalHandler += handler;
        public void Unsubscribe(SignalHandler handler) => SignalHandler -= handler;
    }

    [Serializable]
    public abstract class IntSignal : Signal<int> { }
    [Serializable]
    public abstract class FloatSignal : Signal<float> { }
    [Serializable]
    public abstract class StringSignal : Signal<string> { }
    [Serializable]
    public abstract class BoolSignal : Signal<bool> { }

    [Serializable]
    public sealed class InspectorSignal : UnityEvent { }

    [Serializable]
    public sealed class InspectorSignal<TSignal, TData> : UnityEvent<TData>
        where TSignal : ISignal<TData>
    { }


}