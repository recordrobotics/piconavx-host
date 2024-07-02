using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    public struct HealthUpdate
    {
        public int MemoryUsed;
        public int MemoryTotal;

        public override string ToString()
        {
            return string.Format("{{Memory Used: {0}, Memory Total: {1}}}", MemoryUsed, MemoryTotal);
        }
    }
}
