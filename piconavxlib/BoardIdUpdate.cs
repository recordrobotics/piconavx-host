using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BoardIdUpdate
    {
        public byte Type;
        public byte HwRev;
        public byte FwVerMajor;
        public byte FwVerMinor;
        public byte FwRevision;

        public override string ToString()
        {
            return string.Format("{{type:{0},hwrev:{1},fwvermajor:{2},fwvarminor:{3},fwrevision:{4}}}", Type, HwRev, FwVerMajor, FwVerMinor, FwRevision);
        }
    }
}
