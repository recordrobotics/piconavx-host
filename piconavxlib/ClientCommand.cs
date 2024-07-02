using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    public class ClientCommand
    {
        public HostCommandType CommandType { get; }
        public object? Data { get; }

        public ClientCommand(HostCommandType commandType, object? data)
        {
            CommandType = commandType;
            Data = data;
        }

        public ClientCommand(HostCommandType commandType) : this(commandType, null)
        {
        }
    }
}
