using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using EasyJect.Internal;

namespace EasyJect
{
    public static class InjectionSystem
    {
        private static readonly Type SINGLETON_ATTR_TYPE = typeof(Singleton);
        private static readonly Type BASE_BEHAVIOUR_TYPE = typeof(BaseInjectBehaviour);
        private static readonly Type BASE_SIGNAL_TYPE = typeof(BaseSignal);
        private static readonly Type MONO_TYPE = typeof(Component);
        private static readonly Type START_ATTR_TYPE = typeof(StartAttribute);
        private static readonly BindingFlags INSTANCE_METHOD_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

#if UNITY_EDITOR
        public class InjectSystemDelegate
        {
            public Dictionary<Type, object> BehaviourDictionary
            {
                get { return InjectionSystem._behaviourDictionary; }
            }
            public int GetInjectionsCount
            {
                get { return _injectionsCount; }
            }

            public Dictionary<Type, object> TypesSignalDictionary
            {
                get { return _signalDictionary; }
            }

            public Dictionary<Type, object> CloudDictionary
            {
                get { return _cloudDictionary; }
            }

            public Dictionary<Type, object> SingletonDictionary
            {
                get { return _singletonDictionary; }
            }

            public int _injectionsCount = 0;

            public Dictionary<Type, object> _singletonDictionary = new Dictionary<Type, object>();
        }
        public static InjectSystemDelegate _systemDelegate = new InjectSystemDelegate();
#endif

        internal static Dictionary<Type, object> _behaviourDictionary = new Dictionary<Type, object>();
        internal static Dictionary<Type, object> _registerableDictionary = new Dictionary<Type, object>();
        internal static Dictionary<Type, List<Action<object>>> _postponedInjectionsDictionary = new Dictionary<Type, List<Action<object>>>();
        internal static Dictionary<Type, object> _signalDictionary = new Dictionary<Type, object>();

        internal static Dictionary<Type, object> _cloudDictionary = new Dictionary<Type, object>();
        internal static Dictionary<Type, object> _cloudSingletonsDictionary = new Dictionary<Type, object>();
        internal static Dictionary<Type, object> _interfaceDictionary = new Dictionary<Type, object>();

        internal static bool _bindingsApplyAttempted;
        internal static InternalInjectionBinder Binder = new InternalInjectionBinder();

        public static void Reset()
        {
#if UNITY_EDITOR
            _systemDelegate = new InjectSystemDelegate();
#endif
            _behaviourDictionary.Clear();
            _registerableDictionary.Clear();
            _interfaceDictionary.Clear();

            foreach (var list in _postponedInjectionsDictionary.Values)
            {
                list.Clear();
            }
            _postponedInjectionsDictionary.Clear();

            foreach(var kvp in _signalDictionary)
            {
                kvp.Key.GetMethod("RemoveAllListeners").Invoke(kvp.Value, null);
            }
            _signalDictionary.Clear();

            _cloudDictionary.Clear();

            foreach(var kvp in _cloudSingletonsDictionary)
            {
                RemoveBehaviourSingletonProperty(kvp.Value, kvp.Key);
            }
            _cloudSingletonsDictionary.Clear();

            Binder.Reset();

            _bindingsApplyAttempted = false;
        }

        internal static void AutoCreateEntities(BaseInjectionBinder binder)
        {
            var typeDic = InjectionUtils.GetTypesWithAttribute<AutoCreateAttribute>();
            List<object> cloudsCreated = new List<object>();
            List<object> signalsCreated = new List<object>();

            foreach(var typeKVP in typeDic)
            {
                var type = typeKVP.Key;
                var attribute = typeKVP.Value;
                var binderType = attribute.BinderType;

                if( binderType != null &&
                    binder.GetType().IsAssignableFrom(binderType) == false)
                {
                    continue; //This entity does not permit auto creation from this binder
                }

                if (BASE_SIGNAL_TYPE.IsAssignableFrom(type))
                {
                    bool isNew;
                    var newSignal = GetOrMakeSignal(type, out isNew, "[AutoCreate]-" + type.ToString());
                    if( isNew)
                    {
                        signalsCreated.Add(newSignal);
                    }
                }
                else if(InjectionUtils.GetAttribute<CloudAttribute>(type) != null)
                {
                    if(_cloudDictionary.ContainsKey(type))
                    {
                        continue;
                        /*
                         *  Cloud already exists, 
                         *  this may be normal behaviour if two Scenes 
                         *  have invoked the AutoCreate via their binders
                        */
                    }

                    cloudsCreated.Add(CreateCloud(type));
                }
                else
                {
                    Debug.LogWarning("Can't AutoCreate type " + type.ToString() + " Ignoring");
                }
            }

            foreach(var cloudCreated in cloudsCreated)
            {
                Binder._onStart.AddOnce(() => InjectBehaviourInner(cloudCreated));
                Binder._onStartFinished.AddOnce(() => CallStartMethod(cloudCreated));
            }

            foreach (var signalCreate in signalsCreated)
            {
                Binder._onStart.AddOnce(() => InjectSignal(signalCreate));
                Binder._onStartFinished.AddOnce(() => CallSignalStart(signalCreate));
            }
        }

        internal static void RegisterBehaviour(BaseInjectBehaviour behaviour)
        {
            var type = behaviour.GetType();

            if (_behaviourDictionary.ContainsKey(type))
            {
                Debug.LogWarning("InjectBehaviour of type " + type + " already exists");
            }
            else
            {
                _behaviourDictionary.Add(type, behaviour);

                InjectPostponedConsumers(behaviour, type);
                RegisterBehaviourSingletonProperty(behaviour, type);

                LookupRegisterableMembers(behaviour, type);
                LookupRegisterableInterfaces(behaviour, type);
            }
        }

        internal static void RemoveBehaviour(BaseInjectBehaviour behaviour)
        {
            RemoveBehaviourInner(behaviour);
        }

        internal static void InjectCommand(Command command)
        {
            InjectBehaviourInner(command);
        }
        internal static void InjectCommand<T>(Command<T> command)
        {
            InjectBehaviourInner(command);
        }
        internal static void InjectCommand<TInvoke, TListen>(Command<TInvoke, TListen> command)
        {
            InjectBehaviourInner(command);
        }

        private static void InjectPostponedConsumers(object behaviour, Type type)
        {
            List<Action<object>> postponedInjections;
            if (_postponedInjectionsDictionary.TryGetValue(type, out postponedInjections))
            {
                foreach (var injectionInvokation in postponedInjections)
                {
                    injectionInvokation(behaviour);
                }
                _postponedInjectionsDictionary.Remove(type);
            }
        }

        private static bool RegisterBehaviourSingletonProperty(object behaviour, Type type)
        {
            var prop = GetSingletonProperty(behaviour, type, null);

            if( prop == null )
            {
                return false;
            }

            if( prop.PropertyType != type)
            {
                Debug.LogError(string.Format("Can't assign singleton {0} into property {1}, at object {2}. Singleton must be its own type as a public instance property", type.ToString(), prop.Name, behaviour.ToString()));
                return false;
            }
            else
            {
                prop.SetValue(behaviour, behaviour, null);
#if UNITY_EDITOR
                _systemDelegate._singletonDictionary[behaviour.GetType()] = behaviour;
#endif
                return true;
            }
        }

        private static void RemoveBehaviourInner(object behaviour)
        {
            var type = behaviour.GetType();
            if (_behaviourDictionary.ContainsKey(type))
            {
                _behaviourDictionary.Remove(type);
            }
            /*Else{
             * Did not find behaviour of this type to remove, possible that it was removed by a Reset() command, which is OK
             }*/

            RemoveBehaviourSingletonProperty(behaviour, type);
        }

        private static void RemoveBehaviourSingletonProperty(object behaviour, Type type)
        {
            var singletonProp = GetSingletonProperty(behaviour, type, behaviour);
            if (singletonProp != null)
            {
                singletonProp.SetValue(behaviour, null, null);
#if UNITY_EDITOR
                _systemDelegate._singletonDictionary[behaviour.GetType()] = null;
                _systemDelegate._singletonDictionary.Remove(behaviour.GetType());
#endif
            }
        }

        private static PropertyInfo GetSingletonProperty(object behaviour, Type type, object value)
        {
            var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            foreach (var prop in properties)
            {   
                var singletonAttributes = prop.GetCustomAttributes(SINGLETON_ATTR_TYPE, false);
                if (singletonAttributes != null && singletonAttributes.Length > 0)
                {
                    var singleton = singletonAttributes[0] as Singleton;
                    if (singleton != null)
                    {
                        if (prop.PropertyType != type)
                        {
                            Debug.LogError(string.Format("Can't assign singleton {0} into property {1}, at object {2}. Singleton must be its own type as a public instance property", type.ToString(), prop.Name, behaviour.ToString()));
                        }
                        else if (prop.GetValue(behaviour, null) == value)
                        {
                            return prop;
                        }
                    }
                }
            }
            return null;
        }

        internal static void InjectBehaviourInner(object obj)
        {
            List<System.Type> typeChain = InjectionUtils.GetObjectTypeChain(obj);

            foreach (var t in typeChain)
            {
                var properties = t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in properties)
                {
                    var injectAttribute = InjectionUtils.GetAttribute<Inject>(prop);
                    if (injectAttribute != null)
                    {
                        InjectPropertyIntoObjByAttribute(obj, prop, injectAttribute);
                    }
                }
            }
        }

        static void InjectPropertyIntoObjByAttribute(object obj, PropertyInfo prop, Inject injectAttribute)
        {
            var propType = prop.PropertyType;
            switch (injectAttribute.Type)
            {
                case InjectType.Auto:
                    if( propType.IsInterface)
                    {
                        InjectInterfaceProperty(prop, obj);
                    }
                    else if (BASE_BEHAVIOUR_TYPE.IsAssignableFrom(propType))
                    {
                        InjectBehaviourProperty(prop, injectAttribute, obj);
                    }
                    else if (BASE_SIGNAL_TYPE.IsAssignableFrom(propType))
                    {
                        InjectSignalProperty(prop, injectAttribute, obj);
                    }
                    else if (MONO_TYPE.IsAssignableFrom(propType))
                    {
                        InjectComponentProperty(prop, injectAttribute, obj);
                    }
                    else if (CanInjectListener(prop))
                    {
                        InjectListener(prop, obj);
                    }
                    else if (CanInjectInvoker(prop))
                    {
                        InjectInvoker(prop, obj);
                    }
                    else if (CanInjectCloud(prop, injectAttribute, obj))
                    {
                        InjectCloud(prop, obj);
                    }
                    else
                    {
                        InjectRegisterable(prop, obj);
                    }
                    break;
                case InjectType.EasyBehaviour:
                    InjectBehaviourProperty(prop, injectAttribute, obj);
                    break;
                case InjectType.Signal:
                    InjectSignalProperty(prop, injectAttribute, obj);
                    break;
                case InjectType.Component:
                case InjectType.ComponentInChildren:
                    InjectComponentProperty(prop, injectAttribute, obj);
                    break;
                case InjectType.Cloud:
                    InjectCloudProperty(prop, injectAttribute, obj);
                    break;
                case InjectType.Listener:
                    if (CanInjectListener(prop))
                    {
                        InjectListener(prop, obj);
                    }
                    break;
                case InjectType.Invoker:
                    if(CanInjectInvoker(prop))
                    {
                        InjectInvoker(prop, obj);
                    }
                    break;
                case InjectType.Registerable:
                    InjectRegisterable(prop, obj);
                    break;
                case InjectType.Interface:
                    InjectInterfaceProperty(prop, obj);
                    break;
            }

#if UNITY_EDITOR
            _systemDelegate._injectionsCount++;
#endif
        }
    
        private static void InjectInterfaceProperty(PropertyInfo prop, object obj)
        {
            var interfaceType = prop.PropertyType;
            var registerAttribute = interfaceType.GetCustomAttribute<RegisterAttribute>();
            if( registerAttribute == null)
            {
                Debug.LogWarning($"Can't inject an interface that isn't marked by [Register] in {obj.ToString()} prop {prop.Name}");
                return;
            }

            object registeredInterface;
            if (_interfaceDictionary.TryGetValue(interfaceType, out registeredInterface))
            {
                prop.SetValue(obj, registeredInterface, null);
            }
            else
            {
                //Means that the injectable type has not registered yet, will retain injection application to a later time.
                System.Action<object> postponedAction = (lateImplementation) => prop.SetValue(obj, lateImplementation, null);

                if (_postponedInjectionsDictionary.ContainsKey(interfaceType) == false)
                {
                    _postponedInjectionsDictionary[interfaceType] = new List<System.Action<object>>();
                }

                _postponedInjectionsDictionary[interfaceType].Add(postponedAction);
            }
        }

        private static void InjectBehaviourProperty(PropertyInfo prop, Inject inject, object obj)
        {
            if (CanInjectBehaviour(prop, inject))
            {
                InjectBehaviour(prop, obj);
            }
            else
            {
                Debug.LogWarning("Can't inject property type " + prop.PropertyType.ToString() + " with inject attr of type " + inject.Type.ToString() + " on an object of type " + obj.GetType());
            }
        }

        private static void InjectSignalProperty(PropertyInfo prop, Inject inject, object obj)
        {
            if (CanInjectSignalByType(prop, inject))
            {
                InjectSignal(prop, obj);
            }
            else
            {
                Debug.LogWarning("Can't inject property type " + prop.PropertyType.ToString() + " with inject attr of type " + inject.Type.ToString() + " on an object of type " + obj.GetType());
            }
        }

        private static void InjectComponentProperty(PropertyInfo prop, Inject inject, object obj)
        {
            if (CanInjectComponent(prop, inject, obj))
            {
                InjectComponent(prop, obj);
            }
            else if (CanInjectComponentInChildren(prop, inject, obj))
            {
                InjectComponentChildren(prop, obj);
            }
            else
            {
                Debug.LogWarning("Can't inject property type " + prop.PropertyType.ToString() + " with inject attr of type " + inject.Type.ToString() + " on an object of type " + obj.GetType());
            }
        }

        private static void InjectCloudProperty(PropertyInfo prop, Inject inject, object obj)
        {
            if (CanInjectCloud(prop, inject, obj))
            {
                InjectCloud(prop, obj);
            }
            else
            {
                Debug.LogWarning("Can't inject property type " + prop.PropertyType.ToString() + " with inject attr of type " + inject.Type.ToString() + " on an object of type " + obj.GetType());
            }
        }

        private static void InjectListener(PropertyInfo prop, object obj)
        {
            Type propType = prop.PropertyType;

            var signalType = propType.GetGenericArguments()[0];
            object signal = GetOrCreateSignal(signalType, propType.Name);
            object listener = Activator.CreateInstance(propType, signal);
            prop.SetValue(obj, listener, null);
        }

        private static void InjectInvoker(PropertyInfo prop, object obj)
        {
            Type propType = prop.PropertyType;

            var signalType = propType.GetGenericArguments()[0];
            object signal = GetOrCreateSignal(signalType, propType.Name);
            object invoker = Activator.CreateInstance(propType, signal);
            prop.SetValue(obj, invoker, null);
        }

        private static void InjectRegisterable(PropertyInfo prop, object obj)
        {
            object registeredBehaviour;
            if (_registerableDictionary.TryGetValue(prop.PropertyType, out registeredBehaviour))
            {
                prop.SetValue(obj, registeredBehaviour, null);
            }
            else
            {
                //Means that the injectable type has not registered yet, will retain injection application to a later time.
                System.Action<object> postponedAction = (lateBehaviour) => prop.SetValue(obj, lateBehaviour, null);

                if (_postponedInjectionsDictionary.ContainsKey(prop.PropertyType) == false)
                {
                    _postponedInjectionsDictionary[prop.PropertyType] = new List<System.Action<object>>();
                }

                _postponedInjectionsDictionary[prop.PropertyType].Add(postponedAction);
            }
        }

        private static bool CanInjectBehaviour(PropertyInfo prop, Inject inject)
        {
            return (inject.Type == InjectType.EasyBehaviour || inject.Type == InjectType.Auto) &&
                    (typeof(EasyBehaviour).IsAssignableFrom(prop.PropertyType) ||
                    typeof(EasySupplier).IsAssignableFrom(prop.PropertyType));
        }

        private static bool CanInjectSignalByType(PropertyInfo prop, Inject inject)
        {
            if (typeof(Signal).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(Signal))
            {
                return inject.Type == InjectType.Auto || inject.Type == InjectType.Signal;
            }
            else
            {
                var baseProperty = prop.PropertyType.GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance);
                if (baseProperty != null)
                {
                    var genericInstanceType = baseProperty.DeclaringType;

                    if (genericInstanceType.IsGenericType &&
                        (genericInstanceType.GetGenericTypeDefinition() == typeof(Signal<>) ||
                         genericInstanceType.GetGenericTypeDefinition() == typeof(Command<,>)) &&
                        prop.PropertyType != genericInstanceType)
                    {
                        return inject.Type == InjectType.Auto || inject.Type == InjectType.Signal;
                    }
                }
            }

            return false;
        }

        private static bool CanInjectComponent(PropertyInfo prop, Inject inject, object obj)
        {
            return (inject.Type == InjectType.Component || inject.Type == InjectType.Auto) &&
            (typeof(UnityEngine.Component).IsAssignableFrom(prop.PropertyType)) &&
            obj as MonoBehaviour != null;
        }

        private static bool CanInjectComponentInChildren(PropertyInfo prop, Inject inject, object obj)
        {
            return inject.Type == InjectType.ComponentInChildren &&
            (typeof(UnityEngine.Component).IsAssignableFrom(prop.PropertyType)) &&
            obj as MonoBehaviour != null;
        }

        private static bool CanInjectCloud(PropertyInfo prop, Inject inject, object obj)
        {
            bool isValidInjectType = (inject.Type == InjectType.Cloud || inject.Type == InjectType.Auto);
            if(isValidInjectType == false)
            {
                return false;
            }

            var cloudAttribute = InjectionUtils.GetAttribute<CloudAttribute>(prop.PropertyType);
            bool hasCloudAttribute = cloudAttribute != null;
            return hasCloudAttribute;
        }

        private static bool CanInjectListener(PropertyInfo prop)
        {
            var propType = prop.PropertyType;

            bool IsProperListener = propType.IsGenericType &&
                        ((propType.GetGenericArguments().Length == 1 && propType.GetGenericTypeDefinition() == typeof(Listener<>)) ||
                        (propType.GetGenericArguments().Length == 2 && propType.GetGenericTypeDefinition() == typeof(Listener<,>)));

            return IsProperListener;
        }

        private static bool CanInjectInvoker(PropertyInfo prop)
        {
            var propType = prop.PropertyType;

            bool IsProperInvoker = propType.IsGenericType &&
                        ((propType.GetGenericArguments().Length == 1 && propType.GetGenericTypeDefinition() == typeof(Invoker<>)) ||
                        (propType.GetGenericArguments().Length == 2 && propType.GetGenericTypeDefinition() == typeof(Invoker<,>)));

            return IsProperInvoker;
        }

        private static void InjectBehaviour(PropertyInfo prop, object obj)
        {
            object registeredBehaviour;
            if (_behaviourDictionary.TryGetValue(prop.PropertyType, out registeredBehaviour))
            {
                prop.SetValue(obj, registeredBehaviour, null);
            }
            else
            {
                //Means that the injectable type has not registered yet, will retain injection application to a later time.
                System.Action<object> postponedAction = (lateBehaviour) => prop.SetValue(obj, lateBehaviour, null);

                if (_postponedInjectionsDictionary.ContainsKey(prop.PropertyType) == false)
                {
                    _postponedInjectionsDictionary[prop.PropertyType] = new List<System.Action<object>>();
                }

                _postponedInjectionsDictionary[prop.PropertyType].Add(postponedAction);
            }
        }

        private static void InjectSignal(PropertyInfo prop, object obj)
        {
            object signal = GetOrCreateSignal(prop.PropertyType, prop.Name);

            prop.SetValue(obj, signal, null);
        }

        internal static object GetOrMakeSignal(Type signalType, out bool isNew, string name = null)
        {
            object signal;
            if (_signalDictionary.TryGetValue(signalType, out signal) == false)
            {
                signal = System.Activator.CreateInstance(signalType);
                signal.GetType().GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(signal, name ?? signalType.ToString(), null);

                _signalDictionary.Add(signalType, signal);

                isNew = true;
            }
            else
            {
                isNew = false;
            }

            return signal;
        }

        internal static object GetOrCreateSignal(Type signalType, string name = null)
        {
            object signal;
            if (_signalDictionary.TryGetValue(signalType, out signal) == false)
            {
                signal = System.Activator.CreateInstance(signalType);
                signal.GetType().GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(signal, name ?? signalType.ToString(), null);

                _signalDictionary.Add(signalType, signal);


                if( Binder._state == InternalInjectionBinder.State.Registered || 
                    Binder._state == InternalInjectionBinder.State.Injected)
                {
                    Binder._onStart.AddOnce(() => InjectSignal(signal));
                    Binder._onStartFinished.AddOnce(() => CallSignalStart(signal));
                }
                else
                {
                    InjectSignal(signal);
                    CallSignalStart(signal);
                }
            }

            return signal;
        }

        internal static void InjectSignal(object signal)
        {
            InjectBehaviourInner(signal);

            var field = signal.GetType().GetField("_isPopulated", BindingFlags.Instance | BindingFlags.NonPublic);
            if(field != null)
            {
                field.SetValue(signal, true);
            }
            else
            {
                Debug.LogWarning("Did not find population field to apply populated state");
            }
        }

        internal static void CallSignalStart(object signal)
        {
            InjectionUtils.CallAttributedMethod(signal, START_ATTR_TYPE, INSTANCE_METHOD_FLAGS);
        }

        private static void InjectComponent(PropertyInfo prop, object obj)
        {
            var monoBehaviour = obj as MonoBehaviour;
            var componentToInject = monoBehaviour.GetComponent(prop.PropertyType);
            if (componentToInject == null)
            {
                Debug.LogWarning("Did not find a component of type " + prop.PropertyType + " To inject into " + monoBehaviour.GetType() + " Name: " + monoBehaviour.name);
            }
            else
            {
                prop.SetValue(monoBehaviour, componentToInject, null);
            }
        }

        private static object CreateCloud(Type type)
        {
            var newCloud = Activator.CreateInstance(type);
            RegisterCloudToSystem(newCloud, type);
            return newCloud;
        }

        private static void InjectCloud(PropertyInfo prop, object obj)
        {
            var propType = prop.PropertyType;
            object newCloud;
            if (_cloudDictionary.TryGetValue(propType, out newCloud) == false)
            {
                Debug.LogWarning("Cloud is not injected, because system does not have it " + prop.PropertyType + " in " + obj.GetType());
                return;
            }

            prop.SetValue(obj, newCloud, null);
        }

        internal static void AddBehaviourToSystem(object behaviour, Type type)
        {
            _behaviourDictionary.Add(type, behaviour);
            RegisterBehaviourSingletonProperty(behaviour, type);
        }

        internal static void AddSignalToSystem(object signal, Type type)
        {
            if (_signalDictionary.ContainsKey(type))
            {
                Debug.LogWarning("Signal binded already exists, ignoring new signal");
                return;
            }

            _signalDictionary[type] = signal;
        }

        internal static void RegisterCloudToSystem(object cloud, Type type)
        {
            if(_cloudDictionary.ContainsKey(type))
            {
                Debug.LogWarning($"There is already a cloud of this type {type} Ignoring");
                return;
            }

            _cloudDictionary.Add(type, cloud);
            bool hasSingleton = RegisterBehaviourSingletonProperty(cloud, type);
            if (hasSingleton)
            {
                _cloudSingletonsDictionary.Add(type, cloud);
            }

            LookupRegisterableMembers(cloud, type);
            LookupRegisterableInterfaces(cloud, type);
        }

        internal static void RegisterInterfaceToSystem(object implementation, Type interfaceType)
        {
            if(_interfaceDictionary.ContainsKey(interfaceType))
            {
                Debug.LogWarning($"There is already an interface of this type {interfaceType} Ignoring");
                return;
            }

            _interfaceDictionary.Add(interfaceType, implementation);
            InjectPostponedConsumers(implementation, interfaceType);
        }

        internal static void LookupRegisterableMembers(object obj, Type type)
        {
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach(var p in props)
            {
                var registerAttribute = p.GetCustomAttribute<RegisterAttribute>();
                if( registerAttribute != null)
                {
                    var propType = p.PropertyType;
                    if ( _registerableDictionary.ContainsKey(propType))
                    {
                        Debug.LogWarning("Duplicate registerable detected for " + propType + " in " + type.ToString());
                        continue;
                    }

                    var val = p.GetValue(obj);
                    if( val == null)
                    {
                        Debug.LogWarning("Unable to register registerable property because it is null " + propType + " in " + type.ToString());
                        continue;
                    }

                    _registerableDictionary.Add(propType, val);
                    InjectPostponedConsumers(val, propType);
                    LookupRegisterableMembers(val, propType);
                }
            }

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var f in fields)
            {
                var registerAttribute = f.GetCustomAttribute<RegisterAttribute>();
                if (registerAttribute != null)
                {
                    var fieldType = f.FieldType;
                    if (_registerableDictionary.ContainsKey(fieldType))
                    {
                        Debug.LogWarning("Duplicate registerable detected for " + fieldType + " in " + type.ToString());
                        continue;
                    }

                    var val = f.GetValue(obj);
                    if (val == null)
                    {
                        Debug.LogWarning("Unable to register registerable property because it is null " + fieldType + " in " + type.ToString());
                        continue;
                    }

                    _registerableDictionary.Add(fieldType, val);
                    InjectPostponedConsumers(val, fieldType);
                    LookupRegisterableMembers(val, fieldType);
                }
            }
        }

        internal static void LookupRegisterableInterfaces(object obj, Type type)
        {
            var interfaces = type.GetInterfaces();

            foreach(var i in interfaces)
            {
                var registerAttribute = i.GetCustomAttribute<RegisterAttribute>();
                if( registerAttribute != null)
                {
                    RegisterInterfaceToSystem(obj, i);
                }
            }
        }

        public static void PopulateCloudToSystem(object cloud)
        {
            InjectBehaviourInner(cloud);
        }

        internal static void CallStartMethod(object cloud)
        {
            InjectionUtils.CallAttributedMethod(cloud, START_ATTR_TYPE, INSTANCE_METHOD_FLAGS);
        }

        private static void InjectComponentChildren(PropertyInfo prop, object obj)
        {
            var monoBehaviour = obj as MonoBehaviour;
            var componentToInject = monoBehaviour.GetComponentInChildren(prop.PropertyType);
            if (componentToInject == null)
            {
                Debug.LogWarning("Did not find a component of type " + prop.PropertyType + " To inject into " + monoBehaviour.GetType() + " Name: " + monoBehaviour.name);
            }
            else
            {
                prop.SetValue(monoBehaviour, componentToInject, null);
            }
        }

        internal static void AssureBindingsApplied(BaseInjectionBinder binderToUse = null)
        {
            if(_bindingsApplyAttempted)
            {
                return;
            }

            var binder = binderToUse ?? UnityEngine.Object.FindObjectOfType<BaseInjectionBinder>();
            if(binder != null)
            {
                binder.Initialize();
            }
            else
            {
                var defaultBinderGameObject = new GameObject("EasyJect Default Binder", typeof(DefaultInjectionBinder));
            }

            _bindingsApplyAttempted = true;
        }
    }
}