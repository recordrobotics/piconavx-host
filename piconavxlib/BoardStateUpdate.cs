using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    public struct BoardStateUpdate
    {
        public NavXOPStatus OpStatus;
        public NavXSensorStatus SensorStatus;
        public NavXCalStatus CalStatus;
        public NavXSelftestStatus SelftestStatus;
        public NavXCapabilityFlags CapabilityFlags;
        public byte UpdateRateHz;
        public byte AccelFsrG;
        public ushort GyroFsrDps;

        public override string ToString()
        {
            return string.Format("{{op:{0},sensor:{1},cal:{2},selftest:{3},cap:{4},update:{5},accel:{6},gyro:{7}}}", OpStatus, SensorStatus, CalStatus, SelftestStatus, CapabilityFlags, UpdateRateHz, AccelFsrG, GyroFsrDps);
        }
    }
}
