using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Injector
{
    [MovedFrom(true, sourceClassName: "EventListener")]
    public sealed class SignalDispatcher : MonoBehaviour
    {
        [SerializeReference, ReferencePicker] private ISignal _Signal;
        [SerializeField] private bool _dispatchOnEnable;


        void OnEnable()
        {
            if (Application.IsPlaying(this) && _Signal is not null && _dispatchOnEnable)
            {

                Dispatch();
            }
        }

        public void Dispatch()
        {
            Locator.Instance.SignalLocator.GetSignal(_Signal.GetType()).Dispatch();
        }
    }


}