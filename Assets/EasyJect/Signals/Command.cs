namespace EasyJect
{
    public abstract class Command : Signal
    {
        public bool IsExecuting { get; private set; }

        public override void Invoke()
        {
            if( IsExecuting)
            {
                UnityEngine.Debug.LogWarning("This command is already executing, ignoring Invokation");
                return;
            }

            IsExecuting = true;

            EnsurePopulate();

            Execute();
        }

        protected abstract void Execute();

        protected void SetDone()
        {
            if( !IsExecuting)
            {
                return;
            }

            IsExecuting = false;
            base.Invoke();
        }

        protected void SetCancel()
        {
            IsExecuting = false;
        }

        internal void EnsurePopulate()
        {
            if(_isPopulated)
            {
                return;
            }

            InjectionSystem.InjectCommand(this);
            _isPopulated = true;
        }
    }

    public abstract class Command<T> : Signal<T>
    {
        public bool IsExecuting { get; private set; }

        private T _param;

        public override void Invoke(T param)
        {
            if (IsExecuting)
            {
                UnityEngine.Debug.LogWarning("This command is already executing, ignoring Invokation");
                return;
            }

            _param = param;

            IsExecuting = true;

            if (_isPopulated == false)
            {
                Populate();
            }

            Execute(param);
        }

        protected abstract void Execute(T param);

        protected void SetDone()
        {
            if (!IsExecuting)
            {
                return;
            }

            IsExecuting = false;
            base.Invoke(_param);
        }

        protected void SetCancel()
        {
            IsExecuting = false;
        }

        private void Populate()
        {
            InjectionSystem.InjectCommand(this);
            _isPopulated = true;
        }
    }

    public abstract class Command<TInvoke, TListen> : Signal<TInvoke, TListen>
    {
        public bool IsExecuting { get; private set; }

        protected override string Name { get; set; }

        public override void Invoke(TInvoke param)
        {
            if (IsExecuting)
            {
                UnityEngine.Debug.LogWarning("This command is already executing, ignoring Invokation");
                return;
            }

            IsExecuting = true;

            if (_isPopulated == false)
            {
                Populate();
            }

            Execute(param);
        }

        protected abstract void Execute(TInvoke param);

        protected void SetDone(TListen param)
        {
            if (!IsExecuting)
            {
                return;
            }

            IsExecuting = false;
            base.InternalInvoke(param);
        }

        protected void SetCancel()
        {
            IsExecuting = false;
        }

        private void Populate()
        {
            InjectionSystem.InjectCommand(this);
            _isPopulated = true;
        }
    }
}
