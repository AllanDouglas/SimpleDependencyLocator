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

    public interface ISignal<T> : ISignal
    {
        void Dispatch(T data);
        void Subscribe(SignalHandler<T> handler);
        void Unsubscribe(SignalHandler<T> handler);
    }

    public abstract class Signal<T> : ISignal<T>
    {
        [UnityEngine.SerializeField] private T _data;
        private SignalHandler<T> _signalHandler;
        private SignalHandler _signalHandlerNoData;
        public void Dispatch(T data) => _signalHandler?.Invoke(data);
        public void Dispatch() => _signalHandlerNoData?.Invoke();
        public void Subscribe(SignalHandler<T> handler) => _signalHandler += handler;
        public void Subscribe(SignalHandler SignalHandler) => _signalHandlerNoData += SignalHandler;
        public void Unsubscribe(SignalHandler<T> handler) => _signalHandler -= handler;
        public void Unsubscribe(SignalHandler SignalHandler) => _signalHandlerNoData -= SignalHandler;
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