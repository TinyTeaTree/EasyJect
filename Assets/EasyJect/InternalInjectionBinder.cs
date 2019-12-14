using System.Collections.Generic;
using System;
using EasyJect.Internal;

namespace EasyJect
{
    public class InternalInjectionBinder
    {
        private Dictionary<Type, object> _behaviours = new Dictionary<Type, object>();
        private Dictionary<Type, object> _signals = new Dictionary<Type, object>();
        private Dictionary<Type, object> _clouds = new Dictionary<Type, object>();
        private Dictionary<Type, object> _interfaces = new Dictionary<Type, object>();

        public void BindBehaviour<T>(T behaviour)
            where T : BaseInjectBehaviour
        {
            _behaviours.Add(typeof(T), behaviour);
        }

        public void BindSignal<T>(T signal)
            where T : BaseSignal
        {
            _signals.Add(typeof(T), signal);
        }

        public void BindCloud<T>(T cloud)
            where T : class
        {
            _clouds.Add(typeof(T), cloud);
        }

        public void BindInterface<T>(T implementation)
            where T : class
        {
            _interfaces.Add(typeof(T), implementation);
        }

        public void InitializeBindings()
        {
            foreach (var kvp in _behaviours)
            {
                InjectionSystem.AddBehaviourToSystem(kvp.Value, kvp.Key);
            }
            foreach (var kvp in _signals)
            {
                InjectionSystem.AddSignalToSystem(kvp.Value, kvp.Key);
            }
            foreach (var kvp in _clouds)
            {
                InjectionSystem.RegisterCloudToSystem(kvp.Value, kvp.Key);
            }
            foreach (var kvp in _interfaces)
            {
                InjectionSystem.RegisterInterfaceToSystem(kvp.Value, kvp.Key);
            }
        }

        public void InjectIntoBindings()
        {
            foreach (var kvp in _behaviours)
            {
                if((kvp.Value as BaseInjectBehaviour).ConsumeInject)
                {
                    InjectionSystem.InjectBehaviourInner(kvp.Value as BaseInjectBehaviour);
                }
            }

            foreach (var kvp in _clouds)
            {
                InjectionSystem.InjectBehaviourInner(kvp.Value);
            }

            foreach(var kvp in _signals)
            {
                InjectionSystem.InjectSignal(kvp.Value);
            }

            foreach(var kvp in _signals)
            {
                InjectionSystem.CallSignalStart(kvp.Value);
            }
        }

        public void CallCloudStart()
        {
            foreach (var kvp in _clouds)
            {
                InjectionSystem.CallStartMethod(kvp.Value);
            }
        }

        public void Reset()
        {
            _behaviours.Clear();
            _signals.Clear();
            _clouds.Clear();
        }
    }
}