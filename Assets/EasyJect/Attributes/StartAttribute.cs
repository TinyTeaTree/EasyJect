using System;

namespace EasyJect
{
    /// <summary>
    /// Function marked with Start will be called after all Clouds/Commands in 
    /// manual binding have been created and awoken and injected into.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class StartAttribute : Attribute { }
}
