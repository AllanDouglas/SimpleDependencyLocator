using UnityEngine;

namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Signals/Signal Dispatcher String")]
    public sealed class StringSignalDispatcherSO : SignalDispatcherSO<StringSignal, string> { }
}