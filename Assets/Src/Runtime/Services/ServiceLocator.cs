using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Injector
{
    public class ServiceLocator
    {
        private static bool _loaded;
        private readonly Dictionary<Type, IService> _serviceMapping = new();

        private static ServiceLocator _instance;
        public static ServiceLocator Instance => _instance ??= new ServiceLocator();

        public ServiceContainerConfig ServiceInstaller => ServiceContainerConfig.Instance;

        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private void Load()
        {
            if (_loaded)
            {
                return;
            }

            var config = ServiceContainerConfig.Instance;

            if (config == null)
            {
                Debug.LogError("ServiceContainerConfig not found! Please create a ServiceContainerConfig asset and configure your services.");
                return;
            }

            var allServiceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName.StartsWith("Unity") && !a.FullName.StartsWith("UnityEditor"))
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IService).IsAssignableFrom(p));

            foreach (var (entry, type) in from ServiceContainerConfig.ServiceEntry entry in config.ServicesEntry
                                          from typeName in entry.types
                                          from type in allServiceTypes
                                          where typeName == type.FullName
                                          select (entry, type))
            {
                Instance._serviceMapping.Add(type, entry.service);
            }

            _loaded = true;
        }

        public void Bind<T>(T service)
            where T : IService
        {
#if UNITY_EDITOR
            if (_serviceMapping.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException($"Service for {typeof(T)} is already registered!");
            }

#endif
            _serviceMapping[typeof(T)] = service;
        }

        public void Unbind<T>()
            where T : IService
        {
#if UNITY_EDITOR
            if (!_serviceMapping.ContainsKey(typeof(T)))
            {
                throw new KeyNotFoundException($"Service for {typeof(T)} not found!");
            }
#endif

            _serviceMapping.Remove(typeof(T));
        }

        public T Resolve<T>()
            where T : IService => Resolve<T>(typeof(T));

        public T Resolve<T>(Type type)
            where T : IService
        {
            TryResolve(type, out T service);
            return service is not null
                ? service
                : throw new KeyNotFoundException($"Service for {typeof(T)} not found!");
        }

        public bool ResolveToRef<T>(ref T service)
            where T : IService
        {
            TryResolve(typeof(T), out T _service);
            service = _service;
            return service is not null;
        }

        public bool TryResolve<T>(out T service)
            where T : IService => TryResolve(typeof(T), out service);

        public bool TryResolve<T>(Type type, out T service)
            where T : IService
        {
            Load();
            
            if (_serviceMapping.TryGetValue(type, out var resolvedService))
            {
                service = (T)resolvedService;
                return true;
            }

            service = default;
            return false;
        }
    }

}
