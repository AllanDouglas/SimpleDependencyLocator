using UnityEngine;

namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Signals/Signal Dispatcher Int")]
    public sealed class IntSignalDispatcherSO : SignalDispatcherSO<IntSignal, int> { }
}