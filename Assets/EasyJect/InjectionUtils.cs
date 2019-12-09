using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace EasyJect.Internal
{
    public static class InjectionUtils
    {
        public static T GetAttribute<T>(ICustomAttributeProvider fromAttributeProvider)
            where T : Attribute
        {
            var foundAttributes = fromAttributeProvider.GetCustomAttributes(typeof(T), false);
            if (foundAttributes != null && foundAttributes.Length > 0)
            {
                var attribute = foundAttributes[0] as T;
                if (attribute != null)
                {
                    return attribute;
                }
            }

            return null;
        }

        public static void CallAttributedMethod(object obj, Type attributeType, BindingFlags methodFlags, params object[] args)
        {
            var methods = obj.GetType().GetMethods(methodFlags);
            foreach (var method in methods)
            {
                if (method.GetParameters().Length != args.Length)
                {
                    continue;
                }

                var attributes = method.GetCustomAttributes(attributeType, false);
                if (attributes != null && attributes.Length > 0)
                {
                    method.Invoke(obj, args);
                }
            }
        }

        public static List<System.Type> GetObjectTypeChain(object obj)
        {
            var fromType = obj.GetType();
            var assembly = fromType.Assembly;

            List<System.Type> typeChain = new List<System.Type>();
            while (fromType != null && fromType.Assembly == assembly)
            {
                typeChain.Add(fromType);
                fromType = fromType.BaseType;
            }

            return typeChain;
        }

        public static Dictionary<Type, AttributeType> GetTypesWithAttribute<AttributeType>()
            where AttributeType : Attribute
        {
            Dictionary<Type, AttributeType> typeDic = new Dictionary<Type, AttributeType>();

            Type attributeType = typeof(AttributeType);

            foreach (var ass in GetAllAssembliesThatRefernceType(attributeType, false))
            {
                foreach (Type type in ass.GetTypes())
                {
                    var foundAttribute = GetAttribute<AttributeType>(type);
                    if( foundAttribute != null)
                    {
                        typeDic.Add(type, foundAttribute);
                    }
                }
            }

            return typeDic;
        }

        public static IEnumerable<Assembly> GetAllAssembliesThatRefernceType(Type type, bool includeTypeHomeAssembly = true)
        {
            if( includeTypeHomeAssembly)
            {
                yield return type.Assembly;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetReferencedAssemblies().FirstOrDefault(referencedAssembly => referencedAssembly.FullName == type.Assembly.FullName) != null)
                {
                    yield return assembly;
                }
            }
        }
    }
}