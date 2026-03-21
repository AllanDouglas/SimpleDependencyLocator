using UnityEngine;

namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Signals/Signal Dispatcher Float")]
    public sealed class FloatSignalDispatcherSO : SignalDispatcherSO<FloatSignal, float> { }
}