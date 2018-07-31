using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using Protocol;

public static class Server
{
    public static event OnDateEvent onDataEvent = null;
    public static event OnConnectEvent onConnectEvent = null;
    public static event OnDisconnectEvent onDisconnectEvent = null;

    public static string localIp = null;
    public const int localPort = 27887;
    static int reliableSequencedChannel;
    static int stateUpdateChannel;
    static int allCostChannel;
    static int localHostId = -1;

    static bool initialized = false;
    // Use this for initialization
    public static void Init()
    {
        if (!initialized)
        {
            NetworkTransport.Init();
            ConnectionConfig config = new ConnectionConfig();

            reliableSequencedChannel = config.AddChannel(QosType.ReliableSequenced);
            stateUpdateChannel = config.AddChannel(QosType.StateUpdate);
            allCostChannel = config.AddChannel(QosType.AllCostDelivery);
            HostTopology topology = new HostTopology(config, 4);

            localIp = CommonHelper.GetIpAddress();
            localHostId = NetworkTransport.AddHost(topology, localPort, localIp);

            initialized = true;
        }
    }

    static List<int> connectionList = new List<int>(10);
    const int bufferSize = 4096;
    static byte[] recBuffer = new byte[bufferSize];
    // Update is called once per frame
    public static void Receive()
    {
        if (!initialized)
            return;

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
        if (initialized)
        {
            if (localHostId >= 0)
                NetworkTransport.RemoveHost(localHostId);
            localHostId = -1;
            connectionList.Clear();
            NetworkTransport.Shutdown();

            initialized = false;
        }
    }

    public static bool Disconnect(int connId)
    {
        byte error;
        bool res = NetworkTransport.Disconnect(localHostId, connId, out error);
        return res;
    }

    public static bool SendMessage(GameMsg msg, int connection, QosType qos)
    {
        if (localHostId < 0 || connection < 0)
            return false;
        int length;
        byte[] data = MsgPacker.Pack(msg, out length);
        byte error;
        switch (qos)
        {
            case QosType.Unreliable:
                break;
            case QosType.UnreliableFragmented:
                break;
            case QosType.UnreliableSequenced:
                break;
            case QosType.Reliable:
                break;
            case QosType.ReliableFragmented:
                break;
            case QosType.ReliableSequenced:
                return NetworkTransport.Send(localHostId, connection, reliableSequencedChannel, data, length, out error);
            case QosType.StateUpdate:
                return NetworkTransport.Send(localHostId, connection, stateUpdateChannel, data, length, out error);
            case QosType.ReliableStateUpdate:
                break;
            case QosType.AllCostDelivery:
                return NetworkTransport.Send(localHostId, connection, allCostChannel, data, length, out error);
            default:
                break;
        }
        return false;
    }

    //考虑到同样的信息可能发送多次，不要重复打包,直接发数组
    public static bool SendMessage(byte[] data, int length, int connection, QosType qos)
    {
        if (localHostId < 0 || connection < 0)
            return false;
        byte error;
        switch (qos)
        {
            case QosType.Unreliable:
                break;
            case QosType.UnreliableFragmented:
                break;
            case QosType.UnreliableSequenced:
                break;
            case QosType.Reliable:
                break;
            case QosType.ReliableFragmented:
                break;
            case QosType.ReliableSequenced:
                return NetworkTransport.Send(localHostId, connection, reliableSequencedChannel, data, length, out error);
            case QosType.StateUpdate:
                return NetworkTransport.Send(localHostId, connection, stateUpdateChannel, data, length, out error);
            case QosType.ReliableStateUpdate:
                break;
            case QosType.AllCostDelivery:
                return NetworkTransport.Send(localHostId, connection, allCostChannel, data, length, out error);
            default:
                break;
        }
        return false;
    }

}
