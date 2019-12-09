using System;
using UnityEngine;

namespace EasyJect
{
    public abstract class BaseSignal
    {
        protected virtual string Name { get; set; }
        protected bool _isPopulated = false; //ReflectionBound

        public BaseSignal()
        {

        }
    }

    public class Signal : BaseSignal
    {
        protected event System.Action _invokable;

        public void AddOnce(System.Action listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to register to DISignal {0}", Name));
                return;
            }

            System.Action onceListener = null;
            onceListener = () =>
            {
                listener.Invoke();
                RemoveListener(onceListener);
            };

            AddListener(onceListener);
        }

        public void AddListener(System.Action listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to register to DISignal {0}", Name));
                return;
            }

            _invokable += listener;
        }

        public void RemoveListener(System.Action listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to remove from DISignal {0}", Name));
                return;
            }

            _invokable -= listener;
        }

        public void RemoveAllListeners()
        {
            _invokable = null;
        }

        public virtual void Invoke()
        {
            if (_invokable == null)
            {
                return;
            }

            try
            {
                _invokable.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Exception caught at invokation of {0} : {1}", Name, e.ToString()));
            }
        }
    }

    public class Signal<T> : BaseSignal
    {
        private event System.Action<T> _invokableWithArgs;
        private event System.Action _invokableNoArgs;

        protected override string Name { get; set; }

        public void AddOnce(System.Action listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to register to DISignal {0}", Name));
                return;
            }

            System.Action onceListener = null;
            onceListener = () =>
            {
                listener.Invoke();
                RemoveListener(onceListener);
            };

            AddListener(onceListener);
        }

        public void AddOnce(System.Action<T> listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to register to DISignal {0}", Name));
                return;
            }

            System.Action<T> onceListener = null;
            onceListener = (param) =>
            {
                listener.Invoke(param);
                RemoveListener(onceListener);
            };

            AddListener(onceListener);
        }

        public void AddListener(System.Action listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to register to DISignal {0}", Name));
                return;
            }

            _invokableNoArgs += listener;
        }

        public void AddListener(System.Action<T> listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to register to DISignal {0}", Name));
                return;
            }

            _invokableWithArgs += listener;
        }

        public void RemoveListener(System.Action listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to remove from DISignal {0}", Name));
                return;
            }

            _invokableNoArgs -= listener;
        }

        public void RemoveListener(System.Action<T> listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to remove from DISignal {0}", Name));
                return;
            }

            _invokableWithArgs -= listener;
        }

        public void RemoveAllListeners()
        {
            _invokableWithArgs = null;
            _invokableNoArgs = null;
        }

        public virtual void Invoke(T arg)
        {
            if (_invokableWithArgs != null)
            {
                try
                {
                    _invokableWithArgs.Invoke(arg);
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Exception caught at invokation of {0} : {1}", Name, e.ToString()));
                }
            }

            if (_invokableNoArgs != null)
            {
                try
                {
                    _invokableNoArgs.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Exception caught at invokation of {0} : {1}", Name, e.ToString()));
                }
            }
        }
    }

    /// <summary>
    /// Do not inherit from this class, This is used internally for the Command class. 
    /// </summary>
    public abstract class Signal<TInvoke, TListen> : BaseSignal
    {
        private event System.Action<TListen> _invokableWithArgs;
        private event System.Action _invokableNoArgs;

        public void AddOnce(System.Action listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to register to DISignal {0}", Name));
                return;
            }

            System.Action onceListener = null;
            onceListener = () =>
            {
                listener.Invoke();
                RemoveListener(onceListener);
            };

            AddListener(onceListener);
        }

        public void AddOnce(System.Action<TListen> listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to register to DISignal {0}", Name));
                return;
            }

            System.Action<TListen> onceListener = null;
            onceListener = (param) =>
            {
                listener.Invoke(param);
                RemoveListener(onceListener);
            };

            AddListener(onceListener);
        }

        public void AddListener(System.Action listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to register to DISignal {0}", Name));
                return;
            }

            _invokableNoArgs += listener;
        }

        public void AddListener(System.Action<TListen> listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to register to DISignal {0}", Name));
                return;
            }

            _invokableWithArgs += listener;
        }

        public void RemoveListener(System.Action listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to remove from DISignal {0}", Name));
                return;
            }

            _invokableNoArgs -= listener;
        }

        public void RemoveListener(System.Action<TListen> listener)
        {
            if (listener == null)
            {
                Debug.LogError(string.Format("Null listener was attempted to remove from DISignal {0}", Name));
                return;
            }

            _invokableWithArgs -= listener;
        }

        public void RemoveAllListeners()
        {
            _invokableWithArgs = null;
            _invokableNoArgs = null;
        }

        protected void InternalInvoke(TListen param)
        {
            if (_invokableWithArgs != null)
            {
                try
                {
                    _invokableWithArgs.Invoke(param);
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Exception caught at invokation of {0} : {1}", Name, e.ToString()));
                }
            }

            if (_invokableNoArgs != null)
            {
                try
                {
                    _invokableNoArgs.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Exception caught at invokation of {0} : {1}", Name, e.ToString()));
                }
            }
        }

        public abstract void Invoke(TInvoke param);
    }
}