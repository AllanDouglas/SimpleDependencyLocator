using System;
using UnityEngine;

namespace Injector
{
    // [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ReferencePickerAttribute : PropertyAttribute
    {
        public bool DrawProperty;
        public bool AllowStruct;

        public ReferencePickerAttribute(bool drawProperty = true, bool allowStruct = true)
        {
            DrawProperty = drawProperty;

            AllowStruct = allowStruct;
        }
    }

}
