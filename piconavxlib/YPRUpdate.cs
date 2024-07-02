using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    public struct YPRUpdate
    {
        public double Yaw;
        public double Pitch;
        public double Roll;

        public override string ToString()
        {
            return string.Format("{{Yaw: {0}, Pitch: {1}, Roll: {2}}}", Yaw, Pitch, Roll);
        }
    }
}
