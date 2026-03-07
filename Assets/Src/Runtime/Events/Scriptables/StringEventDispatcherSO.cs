using UnityEngine;

namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Events/Event Dispatcher String")]
    public sealed class StringEventDispatcherSO : EventDispatcherSO<StringEvent, string> { }
}