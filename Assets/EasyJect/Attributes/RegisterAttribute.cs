using System;

namespace EasyJect
{
    /// <summary>
    /// Properties and Fields marked with this Attribute get registered to the injection system if they are
    /// present on any object that is registering into the injection system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface)]
    public class RegisterAttribute : Attribute
    {
    }
}
