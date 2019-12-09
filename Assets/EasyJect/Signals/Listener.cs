namespace EasyJect
{

    public class Listener<SignalType>
        where SignalType : Signal
    {
        SignalType _signal;

        public Listener(SignalType signal)
        {
            _signal = signal;
        }

        public void AddOnce(System.Action listener)
        {
            _signal.AddOnce(listener);
        }

        public void AddListener(System.Action listener)
        {
            _signal.AddListener(listener);
        }

        public void RemoveListener(System.Action listener)
        {
            _signal.RemoveListener(listener);
        }

        public void RemoveAllListeners()
        {
            _signal.RemoveAllListeners();
        }
    }

    public class Listener<SignalType, SignalArgType>
        where SignalType : Signal<SignalArgType>
    {
        SignalType _signal;

        public Listener(SignalType signal)
        {
            _signal = signal;
        }

        public void AddOnce(System.Action listener)
        {
            _signal.AddOnce(listener);
        }

        public void AddOnce(System.Action<SignalArgType> listener)
        {
            _signal.AddOnce(listener);
        }

        public void AddListener(System.Action listener)
        {
            _signal.AddListener(listener);
        }

        public void AddListener(System.Action<SignalArgType> listener)
        {
            _signal.AddListener(listener);
        }

        public void RemoveListener(System.Action listener)
        {
            _signal.RemoveListener(listener);
        }

        public void RemoveListener(System.Action<SignalArgType> listener)
        {
            _signal.RemoveListener(listener);
        }

        public void RemoveAllListeners()
        {
            _signal.RemoveAllListeners();
        }
    }
}