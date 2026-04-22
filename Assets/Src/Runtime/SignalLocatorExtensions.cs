using System;
using System.Collections.Generic;

namespace Injector
{
    public static class SignalLocatorExtensions
    {
        private readonly static Dictionary<Type, List<ICommandPrototype>> _commandCache = new();

        public static void BindSignal<TSignal>(this SignalLocator locator, SignalHandler handler)

            where TSignal : ISignal, new()
        {
            locator.GetSignal<TSignal>().Subscribe(handler);
        }

        public static void BindSignal<TSignal, TData>(this SignalLocator locator, SignalHandler<TData> handler)
            where TSignal : ISignal<TData>, new()
        {
            locator.GetSignal<TSignal>().Subscribe(handler);
        }

        public static void UnbindSignal<TSignal>(this SignalLocator locator, SignalHandler handler)
            where TSignal : ISignal, new()
        {
            locator.GetSignal<TSignal>().Unsubscribe(handler);
        }

        public static void UnbindSignal<TSignal, TData>(this SignalLocator locator, SignalHandler<TData> handler)
            where TSignal : ISignal<TData>, new()
        {
            locator.GetSignal<TSignal>().Unsubscribe(handler);
        }

        public static void BindCommand<TSignal, TCommand>(this SignalLocator locator)
            where TSignal : ISignal, new()
            where TCommand : class, ICommand, new()
        {
            var command = new TCommand();

            locator.GetSignal<TSignal>().Subscribe(command.Execute);

            if (!_commandCache.ContainsKey(typeof(TSignal)))
            {
                _commandCache[typeof(TSignal)] = new List<ICommandPrototype>();
            }

            _commandCache[typeof(TSignal)].Add(command);
        }

        public static void BindCommand<TSignal, TCommand, TData>(this SignalLocator locator)
            where TSignal : ISignal<TData>, new()
            where TCommand : class, ICommand<TData>, new()
        {
            var command = new TCommand();

            locator.GetSignal<TSignal>().Subscribe(command.Execute);
            if (!_commandCache.ContainsKey(typeof(TSignal)))
            {
                _commandCache[typeof(TSignal)] = new List<ICommandPrototype>();
            }

            _commandCache[typeof(TSignal)].Add(command);
        }

        public static void UnbindCommand<TSignal, TCommand>(this SignalLocator locator)
            where TSignal : ISignal, new()
            where TCommand : class, ICommand, new()
        {
            if (_commandCache.TryGetValue(typeof(TSignal), out var command))
            {
                TCommand toRemove = null;
                foreach (var cmd in command)
                {
                    if (cmd.GetType() == typeof(TCommand))
                    {
                        toRemove = cmd as TCommand;
                        locator.GetSignal<TSignal>().Unsubscribe((cmd as TCommand).Execute);
                        break;
                    }
                }
                if (toRemove != null)
                    command.Remove(toRemove);
            }
        }
        public static void UnbindCommand<TSignal, TCommand, TData>(this SignalLocator locator)
            where TSignal : ISignal<TData>, new()
            where TCommand : class, ICommand<TData>, new()
        {
            if (_commandCache.TryGetValue(typeof(TSignal), out var command))
            {
                TCommand toRemove = null;
                foreach (var cmd in command)
                {
                    if (cmd.GetType() == typeof(TCommand))
                    {
                        toRemove = cmd as TCommand;
                        locator.GetSignal<TSignal>().Unsubscribe((cmd as ICommand<TData>).Execute);
                        break;
                    }
                }
                if (toRemove != null)
                    command.Remove(toRemove);
            }
        }
    }
}