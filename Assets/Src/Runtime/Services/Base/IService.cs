using System;
using UnityEngine;

namespace Injector
{
    public interface IService { }

    public interface IInitializable
    {
        void Initialize(Action<InitializationState> action = null);
        Awaitable<InitializationState> InitializeAsync();
        InitializationState GetInitializationState();

        public enum InitializationState
        {
            UNINITIALIZED,
            INITIALIZING,
            INITIALIZED
        }
    }
}
