using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AHRSPosUpdate
    {
        public double Yaw;
        public double Pitch;
        public double Roll;
        public double CompassHeading;
        public double Altitude;
        public double FusedHeading;
        public double LinearAccelX;
        public double LinearAccelY;
        public double LinearAccelZ;
        public double MpuTemp;
        public double QuatW;
        public double QuatX;
        public double QuatY;
        public double QuatZ;
        public double BarometricPressure;
        public double BaroTemp;
        public NavXOPStatus OpStatus;
        public NavXSensorStatus SensorStatus;
        public NavXCalStatus CalStatus;
        public NavXSelftestStatus SelfTestStatus;
        public double VelX;
        public double VelY;
        public double VelZ;
        public double DispX;
        public double DispY;
        public double DispZ;

        public override string ToString()
        {
            return string.Format("{{yaw:{0},pitch:{1},roll:{2},compass:{3},altitude:{4},heading:{5},accel:[{6},{7},{8}],mputemp:{9},quat:[{10},{11},{12},{13}],baro:{14},barotemp:{15},op:{16},sensor:{17},cal:{18},selftest:{19},vel:[{20},{21},{22}],disp:[{23},{24},{25}]}}",
                Yaw,
                Pitch,
                Roll,
                CompassHeading,
                Altitude,
                FusedHeading,
                LinearAccelX,
                LinearAccelY,
                LinearAccelZ,
                MpuTemp,
                QuatW,
                QuatX,
                QuatY,
                QuatZ,
                BarometricPressure,
                BaroTemp,
                OpStatus,
                SensorStatus,
                CalStatus,
                SelfTestStatus,
                VelX,
                VelY,
                VelZ,
                DispX,
                DispY,
                DispZ
            );
        }
    }
}
