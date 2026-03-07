using UnityEngine;

namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Events/Event Dispatcher Bool")]
    public sealed class BoolEventDispatcherSO : EventDispatcherSO<BoolEvent, bool> { }
}