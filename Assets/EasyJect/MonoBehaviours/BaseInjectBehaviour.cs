using UnityEngine;

namespace EasyJect.Internal
{
    public abstract class BaseInjectBehaviour : MonoBehaviour
    {
        private bool _isAwakeCalled;

        public virtual bool SupplyInject { get; protected set; }
        public virtual bool ConsumeInject { get; protected set; }

        protected virtual void Awake()
        {
            AwakeInner();
        }

        protected void AssureAwake()
        {
            AwakeInner();
        }

        private void AwakeInner()
        {
            InjectionSystem.AssureBindingsApplied();

            if (_isAwakeCalled)
            {
                return;
            }

            _isAwakeCalled = true;
            if( SupplyInject )
            {
                SupplyBehaviour();
            }

            if( ConsumeInject)
            {
                InjectionSystem.InjectBehaviour(this);
            }
        }

        protected void SupplyBehaviour()
        {
            InjectionSystem.RegisterBehaviour(this);
        }

        protected virtual void OnDestroy()
        {
            InjectionSystem.RemoveBehaviour(this);
        }

        protected SignalType GetSignal<SignalType>()
            where SignalType : class
        {
            var signalObject = InjectionSystem.GetSignal(typeof(SignalType));
            if( signalObject != null )
            {
                SignalType typedSignal = signalObject as SignalType;
                return typedSignal;
            }

            Debug.LogError(string.Format("Did not find signal {0} at {1} {2}", typeof(SignalType), this, name));
            return null;
        }
    }
}