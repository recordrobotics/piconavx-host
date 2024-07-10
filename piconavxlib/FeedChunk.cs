using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    public class FeedChunk
    {
        public uint Timestamp { get; }
        public AHRSPosUpdate Data { get; }

        public FeedChunk(uint timestamp, AHRSPosUpdate data)
        {

            Timestamp = timestamp; Data = data;
        }
    }
}
