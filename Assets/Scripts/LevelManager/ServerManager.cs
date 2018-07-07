using UnityEngine;
using System.Collections;
using Protocol;
using System.Collections.Generic;
using System;

public class ServerManager : MonoBehaviour
{
    enum ConnState
    {
        Connected,
        WaitForReady,
        InGame,
    }
    class Connection
    {
        public int connId;
        public ConnState state;
        public DateTime lastRecvTime;
        public string playerName = null;
        public int playerId = -1;

    }

    Dictionary<int, Connection> connDict = new Dictionary<int, Connection>();

    LevelManager lm = null;
    // Use this for initialization
    void Start()
    {
        Server.onConnectEvent += this.OnConnectEvent;
        Server.onDisconnectEvent += this.OnDisconnectEvent;
        Server.onDataEvent += this.OnDateEvent;
        Server.Init();
        lm = GetComponent<LevelManager>();
        lm.AddPlayer(ControllerType.Local, idCount++, SystemInfo.deviceName);
    }

    void OnDestroy()
    {
        Server.Exit();
        Server.onConnectEvent -= this.OnConnectEvent;
        Server.onDisconnectEvent -= this.OnDisconnectEvent;
        Server.onDataEvent -= this.OnDateEvent;
    }

    const float stateUpdateRate = 15f;
    float noUpdateStateTime = 0;
    float noLostCheckTime = 0;
    // Update is called once per frame
    void Update()
    {
        Server.Receive();

        //更新状态消息
        noUpdateStateTime += Time.deltaTime;
        if (noUpdateStateTime > 1 / stateUpdateRate)
        {
            noUpdateStateTime = 0f;
            PlayerStateUpdate();
        }

        //超时检测
        noLostCheckTime += Time.deltaTime;
        if (noLostCheckTime > 1f)
        {
            noLostCheckTime = 0f;
            int inGameCount = 0;
            List<Connection> lostConnList = new List<Connection>();
            foreach (Connection conn in connDict.Values)
            {
                TimeSpan span = DateTime.Now - conn.lastRecvTime;
                if (span.TotalSeconds > 10f)
                {
                    lostConnList.Add(conn);
                    if (conn.state == ConnState.InGame)
                        inGameCount++;
                }
            }

            if (lostConnList.Count > 0)
            {

                PlayerQuit quit = new PlayerQuit();
                quit.ids = new int[inGameCount];
                int idCount = 0;

                foreach (Connection conn in lostConnList)
                {
                    if (conn.state == ConnState.InGame)
                    {
                        quit.ids[idCount++] = conn.playerId;
                        lm.RemovePlayer(conn.playerId);
                    }
                    Server.Disconnect(conn.connId);
                    connDict.Remove(conn.connId);
                }

                if (idCount > 0)
                {
                    GameMsg msg = new GameMsg(GameMsg.MsgType.PlayerQuit, quit);
                    int length;
                    byte[] data = MsgPacker.Pack(msg, out length);
                    SendToAllClientsInGame(data, length, true);
                }
            }

        }
    }


    void OnDateEvent(GameMsg msg, int connId)
    {
        if (connDict.ContainsKey(connId))
        {
            Connection conn = connDict[connId];
            if (conn == null)
            {  //不应该发生
                Debug.LogError("ServerManager.OnDateEvent>> connection is null");
                return;
            }
            // 设置conn的lastTime
            conn.lastRecvTime = DateTime.Now;

            if (msg.type == GameMsg.MsgType.JoinGameReq)
            {
                JoinGameReq req = msg.content as JoinGameReq;
                if (req != null)
                {
                    OnJoinGameReq(req, conn);
                }
            }
            else if (msg.type == GameMsg.MsgType.ClientReady)
            {
                ClientReady ready = msg.content as ClientReady;
                if (ready != null)
                {
                    OnClientReady(ready, conn);
                }
            }
            else if (msg.type == GameMsg.MsgType.QuitGameReq)
            {
                QuitGameReq quit = msg.content as QuitGameReq;
                if (quit != null)
                {
                    OnClientQuit(quit, conn);
                }
            }
            else if (msg.type == GameMsg.MsgType.ClientLocalPlayerInfo)
            {
                ClientLocalPlayerInfo state = msg.content as ClientLocalPlayerInfo;
                if (state != null)
                {
                    OnClientLocalState(state, conn);
                }
            }
        }
    }

    void OnConnectEvent(int connId)
    {   //有客户端连接进来
        if (connDict.ContainsKey(connId))
        {   //???已经存在?

        }
        else
        { //添加到dict
            Connection conn = new Connection();
            conn.connId = connId;
            conn.state = ConnState.Connected;
            conn.lastRecvTime = DateTime.Now;
            connDict.Add(connId, conn);
        }
    }

    void OnDisconnectEvent(int connId)
    { //有客户端断开了连接
        if (connDict.ContainsKey(connId))
        {
            Connection conn = connDict[connId];
            OnClientQuit(null, conn);
        }

    }


    //*********************消息处理部分********************************
    int idCount = 1;
    int inGamePlayerCount = 0;
    void OnJoinGameReq(JoinGameReq req, Connection conn)
    {
        if (conn.state == ConnState.Connected)
        {
            JoinGameRsp rsp = new JoinGameRsp();
            if (inGamePlayerCount >= 4)
            {
                rsp.success = false;
            }
            else
            {
                if (req.name != null)
                {  //名字有效
                    rsp.success = true;
                    conn.state = ConnState.WaitForReady;
                    conn.playerName = req.name;
                    inGamePlayerCount++;
                }
            }
            Server.SendMessage(new GameMsg(GameMsg.MsgType.JoinGameRsp, rsp), conn.connId, true);
        }
    }

    void OnClientReady(ClientReady ready, Connection conn)
    {
        if (conn.state == ConnState.WaitForReady)
        {
            conn.state = ConnState.InGame;
            Player p = lm.AddPlayer(ControllerType.Remote, idCount++, conn.playerName);
            conn.playerId = p.id;
            //发送游戏状态
            InitServerGameInfo info = new InitServerGameInfo();
            //此客户端的玩家信息
            info.clientLocalPlayer = p.AchievePlayerInfo();
            //其他玩家的信息 
            info.elsePlayers = new PlayerInfo[lm.playerCount - 1];
            lm.AchievePlayerInfo(info.elsePlayers, p.id);
            Server.SendMessage(new GameMsg(GameMsg.MsgType.InitServerGameInfo, info), conn.connId, true);


            //通知其他玩家 
            PlayerJoin join = new PlayerJoin();
            join.info = info.clientLocalPlayer;
            GameMsg msg = new GameMsg(GameMsg.MsgType.PlayerJoin, join);
            //直接打包信息,避免多次打包
            int length;
            byte[] data = MsgPacker.Pack(msg, out length);

            SendToAllClientsInGame(data, length, conn.connId, true);
        }
    }

    void OnClientQuit(QuitGameReq req, Connection conn)
    {
        Server.Disconnect(conn.connId);
        //从dict中移出这个连接
        connDict.Remove(conn.connId);

        if (conn.state == ConnState.InGame)
        {   //游戏玩家-1,并通知其他玩家
            inGamePlayerCount--;

            Protocol.PlayerQuit quit = new PlayerQuit();
            quit.ids = new int[1];
            quit.ids[0] = conn.playerId;
            GameMsg msg = new GameMsg(GameMsg.MsgType.PlayerQuit, quit);

            int length;
            byte[] data = MsgPacker.Pack(msg, out length);

            SendToAllClientsInGame(data, length, true);
        }
        else if (conn.state == ConnState.WaitForReady)
        {   //游戏玩家-1,
            inGamePlayerCount--;
        }
        else if (conn.state == ConnState.Connected)
        {   //不用做什么

        }


    }

    void OnClientLocalState(ClientLocalPlayerInfo state, Connection conn)
    {
        if (conn.state == ConnState.InGame)
        {
            Player p = lm.GetPlayer(conn.playerId);
            if (p != null)
            {
                RemotePlayerController rp = p.GetComponent<RemotePlayerController>();
                if (rp != null)
                {
                    rp.ApplyRemoteInfo(state.info);
                }
                else
                {//???不应该发生
                    Debug.LogError("ServerManager.OnClientLocalState >> player has no remote controller component");
                }
            }
            else
            {   //???不应该发生
                Debug.LogError("ServerManager.OnClientLocalState >> player doesn't exist");
            }
        }
    }

    void SendToAllClientsInGame(byte[] data, int length, bool reliable = false)
    {
        foreach (Connection c in connDict.Values)
        {
            if (c.state == ConnState.InGame)
                Server.SendMessage(data, length, c.connId, reliable);
        }
    }

    void SendToAllClientsInGame(byte[] data, int length, int ignoreId, bool reliable = false)
    {
        foreach (Connection c in connDict.Values)
        {
            if (c.state == ConnState.InGame && c.connId != ignoreId)
                Server.SendMessage(data, length, c.connId, reliable);
        }
    }

    //向所有client更新玩家状态,收到的消息甚至包括客户端本地玩家状态，除了hp等关键信息可以不处理
    void PlayerStateUpdate()
    {
        Protocol.ServerGameState state = new ServerGameState();
        state.info = new PlayerInfo[lm.playerCount];
        lm.AchievePlayerInfo(state.info, -1);
        GameMsg msg = new GameMsg(GameMsg.MsgType.ServerGameState, state);

        //直接打包信息,避免多次打包
        int length;
        byte[] data = MsgPacker.Pack(msg, out length);

        SendToAllClientsInGame(data, length, false);
    }
}
