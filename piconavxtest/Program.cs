using piconavx;
using System.Diagnostics;

Stopwatch sw = Stopwatch.StartNew();

Server server = new Server(65432);
Task serverTask = server.Start();

server.ClientConnected += Server_ClientConnected;
server.ClientDisconnected += Server_ClientDisconnected;

void Server_ClientConnected(object? sender, ClientConnectedEventArgs e)
{
    Console.WriteLine("Client connected: " + e.Client.Id);
    e.Client.SetDataType(HostSetDataType.AHRSPos);
    e.Client.ZeroYaw();
    e.Client.UpdateReceieved += Client_UpdateReceieved;

    Task.Run(async () =>
    {
        while (e.Client.Connected)
        {
            await Task.Delay(2000);
            Console.WriteLine(await e.Client.GetHealth());
            await Task.Delay(2000);
            Console.WriteLine(await e.Client.GetBoardState());
            await Task.Delay(2000);
            Console.WriteLine(await e.Client.GetBoardId());
        }
    });
}

void Client_UpdateReceieved(object? sender, ClientUpdateReceivedEventArgs e)
{
    if (e.DataType == DataType.AHRSPosUpdate)
    {
        AHRSPosUpdate update = (AHRSPosUpdate)e.Data;
        if (sw.ElapsedMilliseconds >= 500)
        {
            sw.Restart();
            Console.WriteLine(update.ToString());
        }
    }
    else
    {
        Console.WriteLine("Unexpected data type received: " + e.DataType);
    }
}

void Server_ClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
{
    Console.WriteLine("Client disconnected: " + e.Client.Id);
}

await serverTask;