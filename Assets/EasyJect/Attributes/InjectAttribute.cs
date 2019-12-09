using System;

namespace EasyJect
{
    public enum InjectType
    {
        Auto,
        EasyBehaviour,
        Signal,
        Component,
        ComponentInChildren,
        Cloud,
        Listener,
        Invoker,
        Registerable, 
        Interface
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Inject : Attribute
    {
        public InjectType Type { get; set; }
    }
}