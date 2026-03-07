using System;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEngine.UIElements;

namespace Injector
{
    public static class EventLocator
    {
        private static readonly Dictionary<Type, IEvent> _events = new();
        private static readonly Dictionary<Type, object> _eventsWithData = new();


        public static void Subscribe<TEvent>(EventHandler eventHandler)
            where TEvent : IEvent, new()
        {
            if (_events.ContainsKey(typeof(TEvent)))
            {
                _events[typeof(TEvent)].Subscribe(eventHandler);
                return;
            }

            var newEvent = new TEvent();
            newEvent.Subscribe(eventHandler);
            _events.Add(typeof(TEvent), newEvent);
        }

        public static void Unsubscribe<TEvent>(EventHandler eventHandler)
            where TEvent : IEvent, new()
        {
            if (_events.ContainsKey(typeof(TEvent)))
            {
                _events[typeof(TEvent)].Unsubscribe(eventHandler);
                return;
            }
        }

        public static void Subscribe<TEvent, TData>(EventHandler<TData> eventHandler)
            where TEvent : IEvent<TData>, new()
        {
            if (_eventsWithData.ContainsKey(typeof(TEvent)))
            {
                ((IEvent<TData>)_eventsWithData[typeof(TEvent)]).Subscribe(eventHandler);
                return;
            }

            var newEvent = new TEvent();
            newEvent.Subscribe(eventHandler);
            _eventsWithData.Add(typeof(TEvent), newEvent);
        }

        public static void Unsubscribe<TEvent, TData>(EventHandler<TData> eventHandler)
            where TEvent : IEvent<TData>, new()
        {
            if (_eventsWithData.ContainsKey(typeof(TEvent)))
            {
                ((IEvent<TData>)_eventsWithData[typeof(TEvent)]).Unsubscribe(eventHandler);
                return;
            }
        }

        public static void Dispatch<TEvent>()
            where TEvent : IEvent, new()
        {
            if (_events.ContainsKey(typeof(TEvent)))
            {
                _events[typeof(TEvent)].Dispatch();
                return;
            }

            var newEvent = new TEvent();
            newEvent.Dispatch();
            _events.Add(typeof(TEvent), newEvent);
        }

        public static void Dispatch<TEvent, TData>(TData data)
            where TEvent : IEvent<TData>, new()
        {
            if (_eventsWithData.ContainsKey(typeof(TEvent)))
            {
                ((IEvent<TData>)_eventsWithData[typeof(TEvent)]).Dispatch(data);
                return;
            }

            var newEvent = new TEvent();
            newEvent.Dispatch(data);
            _eventsWithData.Add(typeof(TEvent), newEvent);
        }

        public static void Subscribe(Type eventType, EventHandler handler)
        {
            if (!typeof(IEvent).IsAssignableFrom(eventType))
                throw new ArgumentException("type must implement IEvent", nameof(eventType));

            if (_events.TryGetValue(eventType, out var existing))
            {
                existing.Subscribe(handler);
                return;
            }

            var newEvent = (IEvent)Activator.CreateInstance(eventType)!;
            newEvent.Subscribe(handler);
            _events.Add(eventType, newEvent);
        }

        public static void Subscribe(Type eventType, EventHandler<object> handler)
        {
            if (_eventsWithData.TryGetValue(eventType, out var existing))
            {
                ((IEvent<object>)existing).Subscribe(handler);
                return;
            }

            var newEvent = (IEvent<object>)Activator.CreateInstance(eventType)!;
            newEvent.Subscribe(handler);
            _eventsWithData.Add(eventType, newEvent);
        }

        public static void Unsubscribe(Type eventType, EventHandler<object> handler)
        {
            if (_eventsWithData.TryGetValue(eventType, out var existing))
            {
                ((IEvent<object>)existing).Unsubscribe(handler);
                return;
            }
        }

        public static void Unsubscribe(Type eventType, EventHandler handler)
        {
            if (_events.TryGetValue(eventType, out var existing))
            {
                existing.Unsubscribe(handler);
            }
        }

        public static void Dispatch(Type eventType)
        {
            if (_events.TryGetValue(eventType, out var existing))
            {
                existing.Dispatch();
                return;
            }

            var newEvent = (IEvent)Activator.CreateInstance(eventType)!;
            newEvent.Dispatch();
            _events.Add(eventType, newEvent);
        }

        public static void Dispatch(Type eventType, object data)
        {
            if (_eventsWithData.TryGetValue(eventType, out var existing))
            {
                ((IEvent<object>)existing).Dispatch(data);
                return;
            }

            var newEvent = Activator.CreateInstance(eventType)
                ?? throw new ArgumentException("could not create instance", nameof(eventType));

            ((IEvent<object>)newEvent).Dispatch(data);
            _eventsWithData.Add(eventType, newEvent);
        }
    }
}