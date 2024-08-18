using piconavx.ui.graphics;
using piconavx.ui.graphics.ui;
using System.Diagnostics;
using System.Net;

namespace piconavx.ui.controllers
{
    public class ClientCardUpdateController : Controller
    {
        private AHRSPosUpdate? lastUpdate = null;
        private Client? client;
        public Client? Client { get; set; }

        public ClientCard Target { get; }

        public static Action<Client>? RemoveCard { get; set; }

        public ClientCardUpdateController(ClientCard target)
        {
            Target = target;
        }

        public override void Subscribe()
        {
            UIServer.ClientUpdate += new PrioritizedAction<GenericPriority, Client, AHRSPosUpdate>(GenericPriority.Medium, Server_ClientUpdate);
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            UIServer.ClientUpdate -= Server_ClientUpdate;
            Scene.Update -= Scene_Update;
        }

        private void Server_ClientUpdate(Client client, AHRSPosUpdate update)
        {
            if (client == this.client)
            {
                lastUpdate = update;
            }
        }

        public static ClientCard UpdateClient(ClientCard card, Client? client, AHRSPosUpdate? lastUpdate)
        {
            if (client != null)
            {
                if (!client.Connected)
                {
                    RemoveCard?.Invoke(client);
                }

                try
                {
                    card.Id = client.Id ?? "<UNKNOWN>";
                    card.HighBandwidth = client.HighBandwidthMode;
                    card.Address = (((IPEndPoint?)client.Tcp?.Client.RemoteEndPoint)?.Address)?.ToString() ?? "<UNKNOWN>";
                    card.Port = (((IPEndPoint?)client.Tcp?.Client.RemoteEndPoint)?.Port)?.ToString() ?? "<UNKNOWN>";
                    card.Version = client.BoardId.FwVerMajor + "." + client.BoardId.FwVerMinor + "." + client.BoardId.FwRevision;
                    card.Status =
                        client.BoardState.OpStatus == NavXOPStatus.Initializing ? "Initializing" :
                        (client.BoardState.CalStatus == NavXCalStatus.InProgress || client.BoardState.CalStatus == NavXCalStatus.Accumulate) ? "Calibrating" :
                        client.BoardState.OpStatus == NavXOPStatus.Normal ? "Calibrated" :
                        client.BoardState.OpStatus == NavXOPStatus.Error ? "Error" :
                        client.BoardState.OpStatus == NavXOPStatus.SelftestInProgress ? "Testing" :
                        "Unsupported";
                    card.Memory = $"{client.Health.MemoryUsed / 1024}kB / {client.Health.MemoryTotal / 1024}kB ({(client.Health.MemoryTotal == 0 ? "0" : ((long)client.Health.MemoryUsed * 10000L / (long)client.Health.MemoryTotal).ToString().InsertFromEnd(2, "."))}%)";
                    card.Temperature = $"{client.Health.CoreTemp:N2} °C | {(lastUpdate == null ? "----" : lastUpdate.Value.MpuTemp.ToString("N2"))} °C";
                }
                catch
                {
                    UpdateClient(card, null, null);
                }
            }
            else
            {
                card.Id = "<UNKNOWN>";
                card.HighBandwidth = false;
                card.Address = "<UNKNOWN>";
                card.Port = "<UNKNOWN>";
                card.Version = "0.0.0";
                card.Status = "Unsupported";
                card.Memory = "---kB / ---kB (----%)";
                card.Temperature = "---- °C | ---- °C";
            }
            return card;
        }

        private Stopwatch sw_hifreq = Stopwatch.StartNew();
        private Stopwatch sw_lowfreq = Stopwatch.StartNew();
        private Stopwatch sw_tick = Stopwatch.StartNew();

        private void Scene_Update(double deltaTime)
        {
            if (Client != client)
            {
                client = Client;
            }

            if (client != null && client.BoardState.UpdateRateHz > 0)
            {
                int updateTime = 1000 / client.BoardState.UpdateRateHz;

                if (sw_hifreq.ElapsedMilliseconds > 4 * updateTime) // request every 4th update
                {
                    sw_hifreq.Restart();
                    client?.RequestBoardState();
                }

                if (sw_lowfreq.ElapsedMilliseconds > 25 * updateTime) // request every 25th update
                {
                    sw_lowfreq.Restart();
                    client?.RequestBoardId();
                }

                if (sw_tick.ElapsedMilliseconds > 50 * updateTime) // request every 50th update
                {
                    sw_tick.Restart();
                    client?.RequestHealth();
                }
            }

            UpdateClient(Target, client, lastUpdate);
        }
    }
}
