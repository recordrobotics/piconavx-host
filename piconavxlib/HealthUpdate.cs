using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HealthUpdate
    {
        public int MemoryUsed;
        public int MemoryTotal;
        public double CoreTemp;

        public override string ToString()
        {
            return string.Format("{{Memory Used: {0}, Memory Total: {1}, Core Temp: {2}}}", MemoryUsed, MemoryTotal, CoreTemp);
        }
    }
}
