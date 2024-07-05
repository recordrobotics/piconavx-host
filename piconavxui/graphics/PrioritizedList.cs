using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class PrioritizedList<T> : IEnumerable<T>, IEnumerable where T : IComparable<T>, IEquatable<T>
    {
        private List<T> list;

        public PrioritizedList()
        {
            list = new List<T>();
        }

        public void Add(T item)
        {
            list.Add(item);
            list.Sort();
        }

        public bool Remove(T item)
        {
            bool ok = list.Remove(item);
            if (ok)
                list.Sort();
            return ok;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public static PrioritizedList<T> operator +(PrioritizedList<T> self, T item)
        {
            self.Add(item);
            return self;
        }

        public static PrioritizedList<T> operator -(PrioritizedList<T> self, T item)
        {
            self.Remove(item);
            return self;
        }

        public static PrioritizedList<T> operator -(PrioritizedList<T> self, object action)
        {
            int ind = self.list.FindIndex((p) => p.Equals(action));
            if (ind != -1)
            {
                self.list.RemoveAt(ind);
                self.list.Sort();
            }
            return self;
        }
    }
}
