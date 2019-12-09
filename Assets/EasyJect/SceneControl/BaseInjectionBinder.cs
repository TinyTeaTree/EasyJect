using UnityEngine;

namespace EasyJect
{
    /// <summary>
    /// This class provides mechanisms (via inheritance) for manually 
    /// assigning instances to the Injection System. Mainly used for Clouds.
    /// 
    /// Usage : Inherit from this class and place as a component in your scene.
    /// use the "_injectionBinder" in the "InitializeBindings()" to manually 
    /// set your Clouds and stuff.
    /// </summary>
    public abstract class BaseInjectionBinder : MonoBehaviour
    {
        private bool _isInitialized;

        protected InjectionSystem.InjectionBinder _injectionBinder = InjectionSystem.Binder;

        protected virtual bool ShouldAutoCreateAttributedEntities { get { return true; } }

        protected void Awake()
        {
            Initialize();
        }

        protected void Start()
        {
            _injectionBinder.CallCloudStart();
            _injectionBinder.Reset();
        }

        internal void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;

            InitializeBindings();

            _injectionBinder.InitializeBindings();
            _injectionBinder.InjectIntoBindings();

            AutoCreateAttributedEntities();
        }

        void AutoCreateAttributedEntities()
        {
            if (ShouldAutoCreateAttributedEntities == false)
            {
                return;
            }

            InjectionSystem.AutoCreateEntities(this);
        }

        protected abstract void InitializeBindings();
    }
}