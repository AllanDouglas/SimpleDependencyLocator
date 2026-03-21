namespace Injector
{
    public interface ICommand : ICommandPrototype
    {
        void Execute();

        void Bind<TEvent>() where TEvent : ISignal, new()
            => SignalLocator.Instance.Subscribe<TEvent>(Execute);

        void Unbind<TEvent>() where TEvent : ISignal, new()
            => SignalLocator.Instance.Unsubscribe<TEvent>(Execute);
    }

    public interface ICommand<TData> : ICommandPrototype
    {
        void Execute(TData data);

        void Bind<TEvent>() where TEvent : ISignal<TData>, new()
            => SignalLocator.Instance.Subscribe<TEvent, TData>(Execute);

        void Unbind<TEvent>() where TEvent : ISignal<TData>, new()
            => SignalLocator.Instance.Unsubscribe<TEvent, TData>(Execute);
    }

    public interface ICommandPrototype
    {
        string Name => GetType().Name;
    }

}