using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui
{
    public class HighLevelServer
    {
        Server server;
        Task? serverTask = null;

        public HighLevelServer(int port)
        {
            server = new Server(port);
        }

        public void Start()
        {
            serverTask = server.Start();

            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
        }

        public void Shutdown()
        {
            server.Stop();
            serverTask?.Wait(500);
        }

        void Server_ClientConnected(object? sender, ClientConnectedEventArgs e)
        {
            Debug.WriteLine("Client connected: " + e.Client.Id);
            e.Client.SetDataType(HostSetDataType.AHRSPos);
            e.Client.ZeroYaw();
            e.Client.ZeroDisplacement();
            e.Client.UpdateReceieved += Client_UpdateReceieved;
        }

        void Client_UpdateReceieved(object? sender, ClientUpdateReceivedEventArgs e)
        {
            if (e.DataType == DataType.AHRSPosUpdate)
            {
                
            }
            else
            {
                Debug.WriteLine("Unexpected data type received: " + e.DataType);
            }
        }

        void Server_ClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
        {
            Debug.WriteLine("Client disconnected: " + e.Client.Id);
        }
    }
}
