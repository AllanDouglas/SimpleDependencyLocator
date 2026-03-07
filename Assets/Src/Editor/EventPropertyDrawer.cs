using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Injector
{
    [CustomPropertyDrawer(typeof(IEvent))]
    public sealed class EventPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var propertyField = new PropertyField(property);
            container.Add(propertyField);

            var button = new Button(() =>
            {

                IEvent eventValue = (IEvent)property.managedReferenceValue;

                if (Application.IsPlaying(property.serializedObject.targetObject))
                {
                    EventLocator.Dispatch(eventValue.GetType());
                    return;
                }

                Debug.Log($"Dispatching event {eventValue.GetType().Name} in editor mode.");
            })
            {
                text = "Dispatch"
            };

            container.Add(button);
            return container;
        }
    }

    [CustomPropertyDrawer(typeof(IEvent<>))]
    public sealed class EventPropertyDrawerWithGeneric : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var propertyField = new PropertyField(property);
            container.Add(propertyField);

            var button = new Button(() =>
            {

                IEvent<object> eventValue = Unsafe.As<IEvent<object>>(property.managedReferenceValue);

                SerializedProperty dataProperty = property.FindPropertyRelative("_data");


                var data = dataProperty switch
                {
                    { propertyType: { } type } when type == SerializedPropertyType.Integer => dataProperty.intValue as object,
                    { propertyType: { } type } when type == SerializedPropertyType.Float => dataProperty.floatValue,
                    { propertyType: { } type } when type == SerializedPropertyType.String => dataProperty.stringValue,
                    { propertyType: { } type } when type == SerializedPropertyType.Boolean => dataProperty.boolValue,
                    { propertyType: { } type } when type == SerializedPropertyType.Float => dataProperty.doubleValue,
                    { propertyType: { } type } when type == SerializedPropertyType.String => dataProperty.longValue,
                    { isArray: true } => Enumerable.Range(0, dataProperty.arraySize).Select(i => dataProperty.GetArrayElementAtIndex(i)).ToArray(),
                    { managedReferenceValue: true } => dataProperty.managedReferenceValue,
                    _ => null
                };


                if (Application.IsPlaying(property.serializedObject.targetObject))
                {
                    EventLocator.Dispatch(eventValue.GetType(), data);
                    return;
                }

                Debug.Log($"Dispatching event {eventValue.GetType().Name} and data {data} in editor mode.");
            })
            {
                text = "Dispatch"
            };

            container.Add(button);
            return container;
        }
    }
}