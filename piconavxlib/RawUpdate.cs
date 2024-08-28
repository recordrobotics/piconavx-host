using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawUpdate
    {
        public short GyroX;
        public short GyroY;
        public short GyroZ;
        public short AccelX;
        public short AccelY;
        public short AccelZ;
        public short MagX;
        public short MagY;
        public short MagZ;
        public double TempC;

        public override string ToString()
        {
            return string.Format("{{Gyro:[{0},{1},{2}], Accel:[{3},{4},{5}], Mag:[{6},{7},{8}], Temp: {9}}}", GyroX, GyroY, GyroZ, AccelX, AccelY, AccelZ, MagX, MagY, MagZ, TempC);
        }
    }
}
