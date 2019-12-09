using System;
namespace EasyJect
{
    /// <summary>
    /// The class marked with the Cloud attribute can participate in the
    /// Injection infrastructure of the game without having a dedicated
    /// MonoBehaviour. 
    /// 
    /// However only manual binding (see BaseInjectionBinder), or upon first
    /// injection request, will this class be created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CloudAttribute : Attribute { }
}