using System;
using UnityEngine;

namespace Injector
{
    public sealed class ReferencePickerAttribute : PropertyAttribute
    {
        public bool DrawProperty;

        public ReferencePickerAttribute(bool drawProperty = true)
        {
            DrawProperty = drawProperty;
        }
    }

}
