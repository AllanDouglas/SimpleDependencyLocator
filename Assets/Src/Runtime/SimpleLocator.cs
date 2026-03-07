namespace Injector
{
    public sealed class SimpleLocator
    {
        public ServiceLocator ServiceLocator { get; } = ServiceLocator.Instance;
        // public EventLocator EventLocator { get; } = EventLocator.Instance;
    }
}