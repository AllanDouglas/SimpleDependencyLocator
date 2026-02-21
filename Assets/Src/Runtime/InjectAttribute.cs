using System;

namespace Injector
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InjectAttribute : Attribute
    {
        public Type[] types;

        public InjectAttribute(params Type[] types)
        {
            this.types = types;
        }
    }
}
