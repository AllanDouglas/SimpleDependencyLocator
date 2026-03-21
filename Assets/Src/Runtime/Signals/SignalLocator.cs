using System;
using System.Collections.Generic;

namespace Injector
{
    public class SignalLocator
    {
        private static readonly Lazy<SignalLocator> _instance = new(() => new SignalLocator());
        public static SignalLocator Instance => _instance.Value;

        private readonly Dictionary<Type, ISignal> _Signals = new();
        private readonly Dictionary<Type, object> _SignalsWithData = new();

        private SignalLocator()
        {
        }

        public void Subscribe<TSignal>(SignalHandler SignalHandler)
            where TSignal : ISignal, new()
        {
            if (_Signals.ContainsKey(typeof(TSignal)))
            {
                _Signals[typeof(TSignal)].Subscribe(SignalHandler);
                return;
            }

            var newSignal = new TSignal();
            newSignal.Subscribe(SignalHandler);
            _Signals.Add(typeof(TSignal), newSignal);
        }

        public void Unsubscribe<TSignal>(SignalHandler SignalHandler)
            where TSignal : ISignal, new()
        {
            if (_Signals.ContainsKey(typeof(TSignal)))
            {
                _Signals[typeof(TSignal)].Unsubscribe(SignalHandler);
                return;
            }
        }

        public void Subscribe<TSignal, TData>(SignalHandler<TData> SignalHandler)
            where TSignal : ISignal<TData>, new()
        {
            if (_SignalsWithData.ContainsKey(typeof(TSignal)))
            {
                ((ISignal<TData>)_SignalsWithData[typeof(TSignal)]).Subscribe(SignalHandler);
                return;
            }

            var newSignal = new TSignal();
            newSignal.Subscribe(SignalHandler);
            _SignalsWithData.Add(typeof(TSignal), newSignal);
        }

        public void Unsubscribe<TSignal, TData>(SignalHandler<TData> SignalHandler)
            where TSignal : ISignal<TData>, new()
        {
            if (_SignalsWithData.ContainsKey(typeof(TSignal)))
            {
                ((ISignal<TData>)_SignalsWithData[typeof(TSignal)]).Unsubscribe(SignalHandler);
                return;
            }
        }

        public void Dispatch<TSignal>()
            where TSignal : ISignal, new()
        {
            if (_Signals.ContainsKey(typeof(TSignal)))
            {
                _Signals[typeof(TSignal)].Dispatch();
                return;
            }

            var newSignal = new TSignal();
            newSignal.Dispatch();
            _Signals.Add(typeof(TSignal), newSignal);
        }

        public void Dispatch<TSignal, TData>(TData data)
            where TSignal : ISignal<TData>, new()
        {
            if (_SignalsWithData.ContainsKey(typeof(TSignal)))
            {
                ((ISignal<TData>)_SignalsWithData[typeof(TSignal)]).Dispatch(data);
                return;
            }

            var newSignal = new TSignal();
            newSignal.Dispatch(data);
            _SignalsWithData.Add(typeof(TSignal), newSignal);
        }

        public void Subscribe(Type SignalType, SignalHandler handler)
        {
            if (!typeof(ISignal).IsAssignableFrom(SignalType))
                throw new ArgumentException("type must implement ISignal", nameof(SignalType));

            if (_Signals.TryGetValue(SignalType, out var existing))
            {
                existing.Subscribe(handler);
                return;
            }

            var newSignal = (ISignal)Activator.CreateInstance(SignalType)!;
            newSignal.Subscribe(handler);
            _Signals.Add(SignalType, newSignal);
        }

        public void Subscribe(Type SignalType, SignalHandler<object> handler)
        {
            if (_SignalsWithData.TryGetValue(SignalType, out var existing))
            {
                ((ISignal<object>)existing).Subscribe(handler);
                return;
            }

            var newSignal = (ISignal<object>)Activator.CreateInstance(SignalType)!;
            newSignal.Subscribe(handler);
            _SignalsWithData.Add(SignalType, newSignal);
        }

        public void Unsubscribe(Type SignalType, SignalHandler<object> handler)
        {
            if (_SignalsWithData.TryGetValue(SignalType, out var existing))
            {
                ((ISignal<object>)existing).Unsubscribe(handler);
                return;
            }
        }

        public void Unsubscribe(Type SignalType, SignalHandler handler)
        {
            if (_Signals.TryGetValue(SignalType, out var existing))
            {
                existing.Unsubscribe(handler);
            }
        }

        public void Dispatch(Type SignalType)
        {
            if (_Signals.TryGetValue(SignalType, out var existing))
            {
                existing.Dispatch();
                return;
            }

            var newSignal = (ISignal)Activator.CreateInstance(SignalType)!;
            newSignal.Dispatch();
            _Signals.Add(SignalType, newSignal);
        }

        public void Dispatch(Type SignalType, object data)
        {
            if (_SignalsWithData.TryGetValue(SignalType, out var existing))
            {
                ((ISignal<object>)existing).Dispatch(data);
                return;
            }

            var newSignal = Activator.CreateInstance(SignalType)
                ?? throw new ArgumentException("could not create instance", nameof(SignalType));

            ((ISignal<object>)newSignal).Dispatch(data);
            _SignalsWithData.Add(SignalType, newSignal);
        }
    }
}