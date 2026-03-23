namespace Injector
{
    public sealed class Locator
    {
        public ServiceLocator ServiceLocator { get; } = ServiceLocator.Instance;
        public SignalLocator SignalLocator { get; } = SignalLocator.Instance;

        private static Locator _instance;
        public static Locator Instance => _instance ??= new Locator();
        private Locator() { }
    }
}