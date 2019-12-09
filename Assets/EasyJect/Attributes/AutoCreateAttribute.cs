using System;

namespace EasyJect
{
    /// <summary>
    /// Can be placed unto commands and Clouds and then they will be created automatically
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoCreateAttribute : Attribute
    {
        /// <summary>
        /// Limit the auto creation to the presense of specific scene binders
        /// </summary>
        public Type BinderType;
    }
}
