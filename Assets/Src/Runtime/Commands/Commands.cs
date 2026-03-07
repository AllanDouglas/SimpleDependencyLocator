namespace Injector
{
    public interface ICommand : ICommandPrototype
    {
        void Execute();

        void Bind<TEvent>() where TEvent : IEvent, new()
            => EventLocator.Instance.Subscribe<TEvent>(Execute);

        void Unbind<TEvent>() where TEvent : IEvent, new()
            => EventLocator.Instance.Unsubscribe<TEvent>(Execute);
    }

    public interface ICommand<TData> : ICommandPrototype
    {
        void Execute(TData data);

        void Bind<TEvent>() where TEvent : IEvent<TData>, new()
            => EventLocator.Instance.Subscribe<TEvent, TData>(Execute);

        void Unbind<TEvent>() where TEvent : IEvent<TData>, new()
            => EventLocator.Instance.Unsubscribe<TEvent, TData>(Execute);
    }

    public interface ICommandPrototype
    {
        string Name => GetType().Name;
    }

}