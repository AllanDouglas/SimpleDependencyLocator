using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Injector
{
    [CreateAssetMenu(menuName = "Simple Dependency Locator/Config/Service Container Config")]
    public sealed class ServiceContainerConfig : ScriptableObject
    {
        private static ServiceContainerConfig _instance;
        public static ServiceContainerConfig Instance => _instance = _instance != null ? _instance : Resources.Load<ServiceContainerConfig>("ServiceContainerConfig");

        [SerializeField] private List<ServiceEntry> _servicesEntry;

        public List<ServiceEntry> ServicesEntry { get => _servicesEntry; set => _servicesEntry = value; }

        public void Bind<T>(T service)
            where T : IService
        {
#if UNITY_EDITOR
            var serviceType = typeof(T);
            if (ServicesEntry.Any(entry => entry.types.Contains(serviceType.FullName)))
            {
                throw new InvalidOperationException($"Service for {serviceType} is already registered in the config!");
            }
#endif

            ServicesEntry.Add(new()
            {
                service = service
            });
        }

        [Serializable]
        public struct ServiceEntry : ISerializationCallbackReceiver
        {
            [SerializeField, ReadyOnly] public string[] types;
            [SerializeReference, ReferencePicker(allowStruct: false)] public IService service;

            public void OnAfterDeserialize() { }

            public void OnBeforeSerialize()
            {
                var serviceType = service?.GetType();
                if (serviceType == null)
                    return;

                var interfaces = serviceType.GetInterfaces();
                var implementedTypes = new List<string>();

                foreach (var interfaceType in interfaces)
                {
                    if (interfaceType != typeof(IService))
                    {
                        implementedTypes.Add(interfaceType.FullName);
                    }
                }

                implementedTypes.Add(service.GetType().FullName);

                types = implementedTypes.ToArray();
            }
        }
    }
}
