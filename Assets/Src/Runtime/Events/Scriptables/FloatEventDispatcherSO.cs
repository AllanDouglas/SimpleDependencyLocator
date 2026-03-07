using UnityEngine;

namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Events/Event Dispatcher Float")]
    public sealed class FloatEventDispatcherSO : EventDispatcherSO<FloatEvent, float> { }
}