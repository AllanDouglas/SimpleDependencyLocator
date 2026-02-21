using System;
using UnityEngine;


namespace Injector
{
    [CreateAssetMenu(menuName = "SimpleDependencyLocator/Config/ServiceContainerConfig")]
    public sealed class ServiceContainerConfig : ScriptableObject
    {
        [SerializeField] private ServiceEntry[] _servicesEntry;

        public ServiceEntry[] ServicesEntry { get => _servicesEntry; set => _servicesEntry = value; }

        [Serializable]
        public struct ServiceEntry : ISerializationCallbackReceiver
        {
            [SerializeField, ReadyOnly] public string[] types;
            [SerializeReference, ReferencePicker] public IService service;

            public void OnAfterDeserialize() { }

            public void OnBeforeSerialize()
            {
                var serviceType = service?.GetType();
                if (serviceType == null)
                    return;

                var interfaces = serviceType.GetInterfaces();
                var implementedTypes = new System.Collections.Generic.List<string>();

                foreach (var interfaceType in interfaces)
                {
                    if (interfaceType != typeof(IService))
                    {
                        implementedTypes.Add(interfaceType.FullName);
                    }
                }

                types = implementedTypes.ToArray();
            }
        }
    }
}
