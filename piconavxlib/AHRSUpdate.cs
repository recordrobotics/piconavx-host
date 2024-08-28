using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AHRSUpdate
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
        public double CalMagX;
        public double CalMagY;
        public double CalMagZ;
        public double MagFieldNormRatio;
        public double MagFieldNormScalar;
        public double RawMagX;
        public double RawMagY;
        public double RawMagZ;

        public override string ToString()
        {
            return string.Format("{{yaw:{0},pitch:{1},roll:{2},compass:{3},altitude:{4},heading:{5},accel:[{6},{7},{8}],mputemp:{9},quat:[{10},{11},{12},{13}],baro:{14},barotemp:{15},op:{16},sensor:{17},cal:{18},selftest:{19},calmag:[{20},{21},{22}],magfieldnormratio:{23},magfieldnormscalar:{24},rawmag:[{25},{26},{27}]}}",
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
                CalMagX,
                CalMagY,
                CalMagZ,
                MagFieldNormRatio,
                MagFieldNormScalar,
                RawMagX,
                RawMagY,
                RawMagZ
            );
        }
    }
}
