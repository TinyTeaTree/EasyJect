namespace EasyJect
{
    public class Invoker<SignalType>
        where SignalType : Signal
    {
        SignalType _signal;

        public Invoker(SignalType signal)
        {
            _signal = signal;
        }

        public virtual void Invoke()
        {
            _signal.Invoke();
        }
    }

    public class Invoker<SignalType, SignalArg>
        where SignalType : Signal<SignalArg>
    {
        SignalType _signal;

        public Invoker(SignalType signal)
        {
            _signal = signal;
        }

        public virtual void Invoke(SignalArg arg)
        {
            _signal.Invoke(arg);
        }
    }
}
