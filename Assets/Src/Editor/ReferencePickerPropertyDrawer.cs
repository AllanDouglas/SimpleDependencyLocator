using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Reflection;
using UnityEngine;
namespace Injector
{
    [CustomPropertyDrawer(typeof(ReferencePickerAttribute))]
    public class ReferencePickerDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, Type[]> _typeCache = new();

        public SerializedProperty _currentProperty;
        private SerializedObject _serializedObject;
        private string _propertyPath;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _currentProperty = property;
            _serializedObject = property.serializedObject;
            _propertyPath = property.propertyPath;


            var container = new VisualElement();

            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                container.Add(new Label("Use [ReferencePickerDrawer] only with SerializeReference fields."));
                return container;
            }

            Type baseType = GetBaseType(property);
            if (baseType == null)
            {
                container.Add(new Label("Could not resolve base type."));
                return container;
            }

            var allowStruct = fieldInfo.GetCustomAttribute<ReferencePickerAttribute>(true).AllowStruct;

            if (!_typeCache.TryGetValue(baseType, out var types))
            {
                types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => baseType.IsAssignableFrom(t)
                        && !t.IsAbstract
                        && !t.IsInterface
                        && (allowStruct || !t.IsValueType))
                    .OrderBy(t => t.Name)
                    .ToArray();

                _typeCache[baseType] = types;
            }

            var typeChoices = types.Select(t => t.FullName).Prepend($"(None) {property.displayName}").ToList();
            var currentType = property.managedReferenceValue?.GetType();
            var currentIndex = currentType == null ? 0 : typeChoices.IndexOf(currentType.FullName);


            var popup = new PopupField<string>("Select", typeChoices, currentIndex, FormatSelectionValue, FormatSelectionValue);

            container.Add(popup);
            var subContainer = new VisualElement();

            if (property.managedReferenceValue != null)
            {
                container.Add(subContainer);
            }

            void RefreshSubProperty()
            {
                var drawProperty = fieldInfo.GetCustomAttribute<ReferencePickerAttribute>(true).DrawProperty;
                if (!drawProperty)
                {
                    return;
                }

                subContainer.Clear();
                var property = _serializedObject.FindProperty(_propertyPath);
                if (property.managedReferenceValue != null)
                {
                    var iterator = property.Copy();
                    var box = new Box();
                    box.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 1f));
                    box.style.marginBottom = 4;
                    box.style.paddingLeft = 10;
                    box.style.paddingRight = 8;
                    box.style.paddingTop = 4;
                    box.style.paddingBottom = 4;
                    subContainer.Add(box);
                    PropertyField propertyField = new(property)
                    {
                        label = property.managedReferenceValue.GetType().Name
                    };
                    box.Add(propertyField);
                }
            }

            var serializedObject = property.serializedObject;
            var propertyPath = property.propertyPath;

            popup.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == evt.previousValue)
                {
                    return;
                }
                
                int selectedIndex = typeChoices.IndexOf(evt.newValue);

                var property = _serializedObject.FindProperty(_propertyPath);

                if (selectedIndex == 0)
                {
                    property.managedReferenceValue = null;
                }
                else
                {
                    Type selectedType = types[selectedIndex - 1];
                    property.managedReferenceValue = Activator.CreateInstance(selectedType);
                }

                _serializedObject.ApplyModifiedProperties();
                RefreshSubProperty();
                container.SendEvent(ChangeEvent<string>.GetPooled(evt.previousValue, evt.newValue));
            });
            if (property.managedReferenceValue != null)
            {
                RefreshSubProperty();
            }
            return container;

            string FormatSelectionValue(string typeName)
            {
                if (typeName.StartsWith("(None)"))
                {
                    return typeName;
                }

                var lastDotIndex = typeName.LastIndexOf('.');
                var afterLastDot = lastDotIndex >= 0 ? typeName[..lastDotIndex] : string.Empty;
                var beforeLastDot = lastDotIndex >= 0 ? typeName[(lastDotIndex + 1)..] : typeName;

                return $"{beforeLastDot} ({afterLastDot})";

            }

        }

        private static Type GetBaseType(SerializedProperty property)
        {
            string typeName = property.managedReferenceFieldTypename;
            if (string.IsNullOrEmpty(typeName))
                return null;

            var split = typeName.Split(' ');
            if (split.Length != 2)
                return null;

            string assemblyName = split[0];
            string className = split[1];
            return Type.GetType($"{className}, {assemblyName}");
        }
    }
}