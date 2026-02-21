using UnityEngine;
using UnityEditor;
namespace Injector
{
    [CustomPropertyDrawer(typeof(ReadyOnlyAttribute))]
    public class ReadonlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
}