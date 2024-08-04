using System.Numerics;

namespace piconavx.ui
{
    public class Transition<T> where T : INumber<T>
    {
        private T currentValue;
        private T targetValue;
        private double duration;

        public T Value { get => currentValue; set => targetValue = value; }
        public double Duration
        {
            get => duration; set
            {
                if (value == 0)
                    throw new ArgumentException("Duration can't be equal to zero.", nameof(value));
                duration = value;
            }
        }

        public Transition(T initalValue, double duration)
        {
            if (duration == 0)
                throw new ArgumentException("Duration can't be equal to zero.", nameof(duration));

            currentValue = initalValue;
            targetValue = initalValue;
            this.duration = duration;
        }

        public void Step(double delta)
        {
            T deltaGeneric = T.CreateTruncating(delta / duration);
            currentValue += (targetValue - currentValue) * deltaGeneric;
        }
    }
}
