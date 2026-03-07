using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Events/Event Dispatcher")]
    public sealed class EventDispatcherSO : ScriptableObject
    {
        [SerializeReference, ReferencePicker] private IEvent _event;
        [SerializeField] private InspectorEvent _onPerformEvent;
        public IEvent Event { get => _event; set => _event = value; }
        public void Dispatch() => EventLocator.Dispatch(_event.GetType());
        public void Subscribe(EventHandler handler) => EventLocator.Subscribe(_event.GetType(), handler);
        public void Unsubscribe(EventHandler handler) => EventLocator.Unsubscribe(_event.GetType(), handler);

        void OnEnable()
        {
            if (Application.IsPlaying(this))
            {
                _event.Unsubscribe(_onPerformEvent.Invoke);
                _event.Subscribe(_onPerformEvent.Invoke);
            }
        }

    }

    public interface IEventDispatcherSO { }
    public abstract class EventDispatcherSO<TEvent, TData> : ScriptableObject, IEventDispatcherSO
        where TEvent : IEvent<TData>
    {
        [SerializeField] TData _data;
        [SerializeReference, ReferencePicker] private TEvent _event;
        [SerializeField] private InspectorEvent<TEvent, TData> _onPerformEvent;
        public TEvent Event { get => _event; set => _event = value; }
        public virtual void Dispatch(TData data) => EventLocator.Dispatch(Event.GetType(), data);
        public virtual void Subscribe(EventHandler<TData> handler)
            => EventLocator.Subscribe(Event.GetType(), UnsafeUtility.As<EventHandler<TData>, EventHandler<object>>(ref handler));
        public virtual void Unsubscribe(EventHandler<TData> handler) 
            => EventLocator.Unsubscribe(Event.GetType(), UnsafeUtility.As<EventHandler<TData>, EventHandler<object>>(ref handler));

        void OnEnable()
        {
            if (Application.IsPlaying(this))
            {
                Event.Unsubscribe(_onPerformEvent.Invoke);
                Event.Subscribe(_onPerformEvent.Invoke);
            }
        }

        //         [ContextMenu("Dispatch")]
        //         public void Dispatch()
        //         {
        //             if (Application.IsPlaying(this))
        //             {
        //                 Dispatch(_data);
        //                 return;
        //             }
        //             Debug.Log($"Dispatching event {_event.GetType().Name} with data {_data} in editor mode.");
        //         }
        // #if UNITY_EDITOR
        //         [UnityEditor.CustomEditor(typeof(IEventDispatcherSO), false)]
        //         private sealed class EventDispatcherSOEditor : UnityEditor.Editor
        //         {
        //             public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        //             {
        //                 var root = new VisualElement();
        //                 UnityEditor.UIElements.InspectorElement.FillDefaultInspector(root, serializedObject, this);
        //                 var button = new Button(() => (target as EventDispatcherSO<TEvent, TData>).Dispatch()) { text = "Dispatch" };
        //                 root.Add(button);

        //                 return root;
        //             }
        //         }
        // #endif

    }
}