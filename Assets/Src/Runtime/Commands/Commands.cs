namespace Injector
{
    public interface ICommand : ICommandPrototype
    {
        void Execute();
    }

    public interface ICommand<T> : ICommandPrototype
    {
        void Execute(T data);
    }

    public interface ICommandPrototype
    {
        string Name => GetType().Name;
    }
}