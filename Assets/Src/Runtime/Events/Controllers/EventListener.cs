using System.Runtime.CompilerServices;
using UnityEngine;

namespace Injector
{
    public sealed class EventListener : MonoBehaviour
    {
        [SerializeReference, ReferencePicker] private IEvent _event;
        [SerializeField] private InspectorEvent _onPerformEvent;

        void OnEnable()
        {
            if (Application.IsPlaying(this))
            {
                EventLocator.Unsubscribe(_event.GetType(), _onPerformEvent.Invoke);
                EventLocator.Subscribe(_event.GetType(), _onPerformEvent.Invoke);
            }
        }
    }

    public abstract class EventListener<TEvent, TData> : MonoBehaviour
        where TEvent : IEvent<TData>
    {
        [SerializeReference, ReferencePicker] private TEvent _event;
        [SerializeField] private InspectorEvent<TEvent, TData> _onPerformEvent;

        void OnEnable()
        {
            if (Application.IsPlaying(this))
            {
                EventHandler<TData> handler = _onPerformEvent.Invoke;
                EventLocator.Unsubscribe(_event.GetType(), Unsafe.As<EventHandler<object>>(handler));
                EventLocator.Subscribe(_event.GetType(), Unsafe.As<EventHandler<object>>(handler));
            }
        }
    }


}