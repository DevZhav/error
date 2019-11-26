using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Console;
using MLAPI;

[ConsoleCommand("connect", "Connects to a server via the port")]
class Connect : Command
{
    [CommandParameter("ip")]
    public string ip = "127.0.0.1";
    [CommandParameter("port")]
    public int port = 15937;

    public override ConsoleOutput Logic()
    {
        NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Address = ip;
        NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Port = (ushort)port;

        //NetworkingManager.Singleton.GetComponent<LiteNetLibTransport.LiteNetLibTransport>().Address = ip;
        //NetworkingManager.Singleton.GetComponent<LiteNetLibTransport.LiteNetLibTransport>().Port = (ushort)port;

        NetworkingManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(DB_API.ConnectionData);
        NetworkingManager.Singleton.StartClient();

        return new ConsoleOutput(string.Format("Connecting to {0} via {1}", ip, port), ConsoleOutput.OutputType.Log);
    }
}

[ConsoleCommand("localconnect", "Connects to localhost")]
class LocalConnect : Command
{
    public override ConsoleOutput Logic()
    {
        NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Address = "127.0.0.1";
        NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Port = 15937;

        //NetworkingManager.Singleton.GetComponent<LiteNetLibTransport.LiteNetLibTransport>().Address = "127.0.0.1";
        //NetworkingManager.Singleton.GetComponent<LiteNetLibTransport.LiteNetLibTransport>().Port = 15937;

        NetworkingManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(DB_API.ConnectionData);
        NetworkingManager.Singleton.StartClient();

        return new ConsoleOutput("Connecting to a local server", ConsoleOutput.OutputType.Log);
    }
}
