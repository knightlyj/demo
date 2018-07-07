using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using Protocol;

public delegate void OnDateEvent(GameMsg msg, int connection);
public delegate void OnConnectEvent(int connection);
public delegate void OnDisconnectEvent(int connection);

public static class Client
{
    public static event OnDateEvent onDataEvent = null;
    public static event OnConnectEvent onConnectEvent = null;
    public static event OnDisconnectEvent onDisconnectEvent = null;

    public static int serverPort = -1;
    public static string serverIp = null;

    static int reliableChannelId;
    static int stateChannelId;
    static int localHostId = -1;
    static int connectionId = -1;

    public static bool initialized = false;
    // Use this for initialization
    public static void Init()
    {
        if (!initialized)
        {
            
            NetworkTransport.Init();
            ConnectionConfig config = new ConnectionConfig();

            reliableChannelId = config.AddChannel(QosType.ReliableSequenced);
            stateChannelId = config.AddChannel(QosType.StateUpdate);
            HostTopology topology = new HostTopology(config, 1);

            localHostId = NetworkTransport.AddHost(topology);

            initialized = true;
        }
        
    }

    public static void Connect()
    {
        byte error;
        connectionId = NetworkTransport.Connect(localHostId, serverIp, serverPort, 0, out error);
        Debug.Log("clent connect to " + serverIp + ": " + serverPort);
    }
    
    const int bufferSize = 4096;
    static byte[] recBuffer = new byte[bufferSize];
    // Update is called once per frame
    public static void Receive()
    {
        int dataSize;
        int channel;
        int connection;
        byte error;
        NetworkEventType recData = NetworkTransport.ReceiveFromHost(localHostId, out connection, out channel, recBuffer, bufferSize, out dataSize, out error);
        if (error != 0)
        {
            Debug.Log("Client recv error is " + error);
        }
        switch (recData)
        {
            case NetworkEventType.Nothing:
                //Debug.Log("nothing");
                break;
            case NetworkEventType.ConnectEvent:
                UnityEngine.Networking.Types.NetworkID network;
                UnityEngine.Networking.Types.NodeID node;
                string ip;
                int port;
                NetworkTransport.GetConnectionInfo(localHostId, connection, out ip, out port, out network, out node, out error);
                if (ip.EndsWith(serverIp) && port == serverPort)
                {
                    connectionId = connection;
                    if (onConnectEvent != null)
                        onConnectEvent(connectionId);
                }
                else
                {
                    NetworkTransport.Disconnect(localHostId, connection, out error);
                }
                break;
            case NetworkEventType.DisconnectEvent:
                if (connectionId == connection)
                {
                    if (onDisconnectEvent != null)
                        onDisconnectEvent(connectionId);
                    connectionId = -1;
                }
                break;
            case NetworkEventType.DataEvent:
                if (connectionId == connection)
                {
                    object o = MsgPacker.Unpack(recBuffer, dataSize);
                    GameMsg msg = o as GameMsg;
                    if (msg != null)
                    {
                        if (onDataEvent != null)
                            onDataEvent(msg, connectionId);
                    }
                }
                break;
        }

    }

    public static void Exit()
    {
        if (initialized)
        {
            if (localHostId >= 0)
                NetworkTransport.RemoveHost(localHostId);
            localHostId = -1;
            connectionId = -1;

            NetworkTransport.Shutdown();
            initialized = false;
        }
    }
    
    static public bool SendMessage(GameMsg msg, bool reliable = false)
    {
        if (localHostId < 0 || connectionId < 0)
            return false;
        int length;
        byte[] data = MsgPacker.Pack(msg, out length);
        byte error;
        if (!reliable)
        {
            return NetworkTransport.Send(localHostId, connectionId, stateChannelId, data, length, out error);
        }
        else
        {
            return NetworkTransport.Send(localHostId, connectionId, reliableChannelId, data, length, out error);
        }
    }
    
}
