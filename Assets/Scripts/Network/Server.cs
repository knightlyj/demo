using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using Protocol;

public static class Server {
    public static event OnDateEvent onDataEvent = null;
    public static event OnConnectEvent onConnectEvent = null;
    public static event OnDisconnectEvent onDisconnectEvent = null;
    
    static int reliableChannelId;
    static int stateChannelId;
    static int localHostId = -1;
    // Use this for initialization
    public static void Init()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();

        reliableChannelId = config.AddChannel(QosType.ReliableSequenced);
        stateChannelId = config.AddChannel(QosType.StateUpdate);
        HostTopology topology = new HostTopology(config, 4);

        string localIp = CommonHelper.GetIpAddress();
        localHostId = NetworkTransport.AddHost(topology, 8888, localIp);
    }

    static List<int> connectionList = new List<int>(10);
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
        switch (recData)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                if (onConnectEvent != null)
                    onConnectEvent(connection);
                connectionList.Add(connection);
                break;
            case NetworkEventType.DisconnectEvent:
                if (onDisconnectEvent != null)
                    onDisconnectEvent(connection);
                connectionList.Remove(connection);
                break;
            case NetworkEventType.DataEvent:
                object o = MsgPacker.Unpack(recBuffer, dataSize);
                GameMsg msg = o as GameMsg;
                if (msg != null)
                {
                    if (onDataEvent != null)
                        onDataEvent(msg, connection);
                }
                break;
        }

    }


    public static void Exit()
    {
        if (localHostId >= 0)
            NetworkTransport.RemoveHost(localHostId);
        localHostId = -1;
        connectionList.Clear();
        NetworkTransport.Shutdown();
    }
    
    public static bool Disconnect(int connId)
    {
        byte error;
        bool res = NetworkTransport.Disconnect(localHostId, connId, out error);
        return res;
    }

    public static bool SendMessage(GameMsg msg, int connection, bool reliable = false)
    {
        if (localHostId < 0 || connection < 0)
            return false;
        int length;
        byte[] data = MsgPacker.Pack(msg, out length);
        byte error;
        if (!reliable)
        {
            return NetworkTransport.Send(localHostId, connection, stateChannelId, data, length, out error);
        }
        else
        {
            return NetworkTransport.Send(localHostId, connection, reliableChannelId, data, length, out error);
        }
    }

    //考虑到同样的信息可能发送多次，不要重复打包,直接发数组
    public static bool SendMessage(byte[] data, int length, int connection, bool reliable = false)
    {
        if (localHostId < 0 || connection < 0)
            return false;
        byte error;
        if (!reliable)
        {
            return NetworkTransport.Send(localHostId, connection, stateChannelId, data, length, out error);
        }
        else
        {
            return NetworkTransport.Send(localHostId, connection, reliableChannelId, data, length, out error);
        }
    }
    
}
