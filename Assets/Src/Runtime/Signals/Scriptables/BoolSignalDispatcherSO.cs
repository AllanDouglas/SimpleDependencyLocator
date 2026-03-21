using UnityEngine;

namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Signals/Signal Dispatcher Bool")]
    public sealed class BoolSignalDispatcherSO : SignalDispatcherSO<BoolSignal, bool> { }
}