using System.Numerics;

namespace piconavx.ui
{
    public class Transition<T> where T : INumber<T>
    {
        private T currentValue;
        private T targetValue;
        private T threshold;
        private double duration;

        public T Value { get => currentValue; set => targetValue = value; }
        public T Threshold { get => threshold; set => threshold = value; }
        public double Duration
        {
            get => duration; set
            {
                if (value == 0)
                    throw new ArgumentException("Duration can't be equal to zero.", nameof(value));
                duration = value;
            }
        }

        public bool Reached => T.Abs(currentValue - targetValue) <= Threshold;

        public static N GetSuggestedThreshold<N>() where N : INumber<N>
        {
            var n3 = N.CreateChecked(3);
            var n2 = N.CreateChecked(2);
            bool isInteger = N.IsInteger(n3 / n2); // 3/2 is not integer unless the number type is integer

            if (isInteger)
                return N.CreateChecked(1); // threshold of '1' for integer types

            return N.CreateChecked(0.01); // threshold of '0.01' for decimal types
        }

        public Transition(T initalValue, double duration) : this(initalValue, duration, GetSuggestedThreshold<T>())
        {
        }

        public Transition(T initalValue, double duration, T threshold)
        {
            if (duration == 0)
                throw new ArgumentException("Duration can't be equal to zero.", nameof(duration));

            currentValue = initalValue;
            targetValue = initalValue;
            this.duration = duration;
            this.threshold = threshold;
        }

        public void Step(double delta)
        {
            T deltaGeneric = T.CreateTruncating(delta / duration);
            currentValue += (targetValue - currentValue) * deltaGeneric;
        }
    }
}
