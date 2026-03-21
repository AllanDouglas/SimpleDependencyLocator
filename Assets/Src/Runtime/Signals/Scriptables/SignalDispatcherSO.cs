using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Signals/Signal Dispatcher")]
    public sealed class SignalDispatcherSO : ScriptableObject
    {
        [SerializeReference, ReferencePicker] private ISignal _Signal;
        [SerializeField] private InspectorSignal _onPerformSignal;
        public ISignal Signal { get => _Signal; set => _Signal = value; }
        public void Dispatch() => SignalLocator.Instance.Dispatch(_Signal.GetType());
        public void Subscribe(SignalHandler handler) => SignalLocator.Instance.Subscribe(_Signal.GetType(), handler);
        public void Unsubscribe(SignalHandler handler) => SignalLocator.Instance.Unsubscribe(_Signal.GetType(), handler);

        void OnEnable()
        {
            if (Application.IsPlaying(this))
            {
                _Signal.Unsubscribe(_onPerformSignal.Invoke);
                _Signal.Subscribe(_onPerformSignal.Invoke);
            }
        }

    }

    public interface ISignalDispatcherSO { }
    public abstract class SignalDispatcherSO<TSignal, TData> : ScriptableObject, ISignalDispatcherSO
        where TSignal : ISignal<TData>
    {
        [SerializeField] TData _data;
        [SerializeReference, ReferencePicker] private TSignal _Signal;
        [SerializeField] private InspectorSignal<TSignal, TData> _onPerformSignal;
        public TSignal Signal { get => _Signal; set => _Signal = value; }
        public virtual void Dispatch(TData data) => SignalLocator.Instance.Dispatch(Signal.GetType(), data);
        public virtual void Subscribe(SignalHandler<TData> handler)
            => SignalLocator.Instance.Subscribe(Signal.GetType(), UnsafeUtility.As<SignalHandler<TData>, SignalHandler<object>>(ref handler));
        public virtual void Unsubscribe(SignalHandler<TData> handler) 
            => SignalLocator.Instance.Unsubscribe(Signal.GetType(), UnsafeUtility.As<SignalHandler<TData>, SignalHandler<object>>(ref handler));

        void OnEnable()
        {
            if (Application.IsPlaying(this))
            {
                Signal.Unsubscribe(_onPerformSignal.Invoke);
                Signal.Subscribe(_onPerformSignal.Invoke);
            }
        }

    }
}