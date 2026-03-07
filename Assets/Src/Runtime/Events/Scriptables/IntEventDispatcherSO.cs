using UnityEngine;

namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Events/Event Dispatcher Int")]
    public sealed class IntEventDispatcherSO : EventDispatcherSO<IntEvent, int> { }
}