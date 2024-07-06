using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class PrioritizedAction<P, T>(P? priority, Action<T> action) : IComparable<PrioritizedAction<P, T>>, IEquatable<PrioritizedAction<P, T>> where P : Enum
    {
        public P? Priority { get; } = priority;
        public Action<T> Action { get; } = action;

        public int CompareTo(PrioritizedAction<P, T>? other)
        {
            if (other == null)
                return -1;
            return Priority?.CompareTo(other.Priority) ??( other.Priority == null ? 0 : 1);
        }

        public bool Equals(PrioritizedAction<P, T>? other)
        {
            if(other == null) return false;
            return Action.Equals(other.Action);
        }

        public override bool Equals(object? obj)
        {
            if (obj is PrioritizedAction<P, T> p)
                return Equals(p);
            else if (obj is Action<T> a)
                return Action.Equals(a);
            else return false;
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode();
        }

        public static implicit operator Action<T>(PrioritizedAction<P,T> obj)
        {
            return obj.Action;
        }

        public static implicit operator PrioritizedAction<P, T>(Action<T> obj)
        {
            return new PrioritizedAction<P, T>(default, obj);
        }
    }

    public class PrioritizedAction<P, T1, T2>(P? priority, Action<T1, T2> action) : IComparable<PrioritizedAction<P, T1, T2>>, IEquatable<PrioritizedAction<P, T1, T2>> where P : Enum
    {
        public P? Priority { get; } = priority;
        public Action<T1, T2> Action { get; } = action;

        public int CompareTo(PrioritizedAction<P, T1, T2>? other)
        {
            if (other == null)
                return -1;
            return Priority?.CompareTo(other.Priority) ?? (other.Priority == null ? 0 : 1);
        }

        public bool Equals(PrioritizedAction<P, T1, T2>? other)
        {
            if (other == null) return false;
            return Action.Equals(other.Action);
        }

        public override bool Equals(object? obj)
        {
            if (obj is PrioritizedAction<P, T1, T2> p)
                return Equals(p);
            else if (obj is Action<T1, T2> a)
                return Action.Equals(a);
            else return false;
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode();
        }

        public static implicit operator Action<T1, T2>(PrioritizedAction<P, T1, T2> obj)
        {
            return obj.Action;
        }

        public static implicit operator PrioritizedAction<P, T1, T2>(Action<T1, T2> obj)
        {
            return new PrioritizedAction<P, T1, T2>(default, obj);
        }
    }

    public class PrioritizedAction<P, T1, T2, T3>(P? priority, Action<T1, T2, T3> action) : IComparable<PrioritizedAction<P, T1, T2, T3>>, IEquatable<PrioritizedAction<P, T1, T2, T3>> where P : Enum
    {
        public P? Priority { get; } = priority;
        public Action<T1, T2, T3> Action { get; } = action;

        public int CompareTo(PrioritizedAction<P, T1, T2, T3>? other)
        {
            if (other == null)
                return -1;
            return Priority?.CompareTo(other.Priority) ?? (other.Priority == null ? 0 : 1);
        }

        public bool Equals(PrioritizedAction<P, T1, T2, T3>? other)
        {
            if (other == null) return false;
            return Action.Equals(other.Action);
        }

        public override bool Equals(object? obj)
        {
            if (obj is PrioritizedAction<P, T1, T2, T3> p)
                return Equals(p);
            else if (obj is Action<T1, T2, T3> a)
                return Action.Equals(a);
            else return false;
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode();
        }

        public static implicit operator Action<T1, T2, T3>(PrioritizedAction<P, T1, T2, T3> obj)
        {
            return obj.Action;
        }

        public static implicit operator PrioritizedAction<P, T1, T2, T3>(Action<T1, T2, T3> obj)
        {
            return new PrioritizedAction<P, T1, T2, T3>(default, obj);
        }
    }

    public class PrioritizedAction<P, T1, T2, T3, T4>(P? priority, Action<T1, T2, T3, T4> action) : IComparable<PrioritizedAction<P, T1, T2, T3, T4>>, IEquatable<PrioritizedAction<P, T1, T2, T3, T4>> where P : Enum
    {
        public P? Priority { get; } = priority;
        public Action<T1, T2, T3, T4> Action { get; } = action;

        public int CompareTo(PrioritizedAction<P, T1, T2, T3, T4>? other)
        {
            if (other == null)
                return -1;
            return Priority?.CompareTo(other.Priority) ?? (other.Priority == null ? 0 : 1);
        }

        public bool Equals(PrioritizedAction<P, T1, T2, T3, T4>? other)
        {
            if (other == null) return false;
            return Action.Equals(other.Action);
        }

        public override bool Equals(object? obj)
        {
            if (obj is PrioritizedAction<P, T1, T2, T3, T4> p)
                return Equals(p);
            else if (obj is Action<T1, T2, T3, T4> a)
                return Action.Equals(a);
            else return false;
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode();
        }

        public static implicit operator Action<T1, T2, T3, T4>(PrioritizedAction<P, T1, T2, T3, T4> obj)
        {
            return obj.Action;
        }

        public static implicit operator PrioritizedAction<P, T1, T2, T3, T4>(Action<T1, T2, T3, T4> obj)
        {
            return new PrioritizedAction<P, T1, T2, T3, T4>(default, obj);
        }
    }

    public enum GenericPriority : int
    {
        Highest = 0,
        High = 1,
        Medium = 2,
        Low = 3,
        Lowest = 4
    }
}
