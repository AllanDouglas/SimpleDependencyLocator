using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Injector
{
    public static class ServiceLocator
    {
        private static bool _loaded;
        readonly static Dictionary<Type, IService> _serviceMapping = new();


        static ServiceLocator()
        {
            Load();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Load()
        {
            if (_loaded)
            {
                return;
            }

            var config = Resources.Load<ServiceContainerConfig>("ServiceContainerConfig");
            foreach (var (entry, type) in from ServiceContainerConfig.ServiceEntry entry in config.ServicesEntry
                                          from typeName in entry.types
                                          from type in TypeCache.GetTypesDerivedFrom<IService>()
                                          where typeName == type.FullName
                                          select (entry, type))
            {
                _serviceMapping.Add(type, entry.service);
                break;
            }

            _loaded = true;
        }

        public static T Resolve<T>()
            where T : IService => Resolve<T>(typeof(T));

        public static T Resolve<T>(Type type)
            where T : IService
        {
#if UNITY_EDITOR
            if (!_loaded)
            {
                return default;
            }
#endif

            TryResolve(type, out T service);
            return service is not null
                ? service
                : throw new KeyNotFoundException($"Service for {typeof(T)} not found!");
        }

        public static bool ResolveToRef<T>(ref T service)
            where T : IService
        {
            TryResolve(typeof(T), out T _service);
            service = _service;
            return service is not null;
        }

        public static bool TryResolve<T>(out T service)
            where T : IService => TryResolve(typeof(T), out service);

        public static bool TryResolve<T>(Type type, out T service)
            where T : IService
        {
            if (_loaded)
            {
                if (_serviceMapping.TryGetValue(type, out var resolvedService))
                {
                    service = (T)resolvedService;
                    return true;
                }

                service = default;
                return false;
            }

            service = default;
            return false;

        }
    }

}
