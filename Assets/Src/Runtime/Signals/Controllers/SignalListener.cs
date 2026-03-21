using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Injector
{
    [MovedFrom(true,sourceClassName:"EventListener")]
    public sealed class SignalListener : MonoBehaviour
    {
        [SerializeReference, ReferencePicker] private ISignal _Signal;
        [SerializeField] private InspectorSignal _onPerformSignal;

        void OnEnable()
        {
            if (Application.IsPlaying(this) && _Signal is not null)
            {
                SignalLocator.Instance.Unsubscribe(_Signal.GetType(), _onPerformSignal.Invoke);
                SignalLocator.Instance.Subscribe(_Signal.GetType(), _onPerformSignal.Invoke);
            }
        }
    }

    public abstract class SignalListener<TSignal, TData> : MonoBehaviour
        where TSignal : ISignal<TData>
    {
        [SerializeReference, ReferencePicker] private TSignal _Signal;
        [SerializeField] private InspectorSignal<TSignal, TData> _onPerformSignal;

        void OnEnable()
        {
            if (Application.IsPlaying(this))
            {
                SignalHandler<TData> handler = _onPerformSignal.Invoke;
                SignalLocator.Instance.Unsubscribe(_Signal.GetType(), Unsafe.As<SignalHandler<object>>(handler));
                SignalLocator.Instance.Subscribe(_Signal.GetType(), Unsafe.As<SignalHandler<object>>(handler));
            }
        }
    }


}