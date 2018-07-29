using UnityEngine;
using System.Collections;
using Protocol;
using System.Collections.Generic;
using System;

public class ServerAgent : MonoBehaviour
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
        lm.AddPlayer(ControllerType.Local, NewPlayerId(), SystemInfo.deviceName);
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
                int i = 0;

                foreach (Connection conn in lostConnList)
                {
                    if (conn.state == ConnState.InGame)
                    {
                        quit.ids[i++] = conn.playerId;
                        lm.RemovePlayer(conn.playerId);
                    }
                    Server.Disconnect(conn.connId);
                    connDict.Remove(conn.connId);
                }

                if (i > 0)
                {
                    GameMsg msg = new GameMsg(GameMsg.MsgType.PlayerQuit, quit);
                    SendToAllClientsInGame(msg, UnityEngine.Networking.QosType.ReliableSequenced);
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
                Debug.LogError("ServerAgent.OnDateEvent>> connection is null");
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
            else if (msg.type == GameMsg.MsgType.Damage)
            {
                HitPlayer hit = msg.content as HitPlayer;
                if (hit != null)
                {
                    OnDamage(hit, conn);
                }
            }
            else if(msg.type == GameMsg.MsgType.Shoot)
            {
                PlayerShoot shoot = msg.content as PlayerShoot;
                if(shoot != null)
                {
                    OnShoot(shoot, conn);
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
    int NewPlayerId()
    {
        int id = idCount;
        idCount++;
        return id;
    }

    
    int inGameClientPlayerCount = 0;
    void OnJoinGameReq(JoinGameReq req, Connection conn)
    {
        if (conn.state == ConnState.Connected)
        {
            JoinGameRsp rsp = new JoinGameRsp();
            if (inGameClientPlayerCount >= 4)
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
                    inGameClientPlayerCount++;
                }
            }
            Server.SendMessage(new GameMsg(GameMsg.MsgType.JoinGameRsp, rsp), conn.connId, UnityEngine.Networking.QosType.ReliableSequenced);
        }
    }

    void OnClientReady(ClientReady ready, Connection conn)
    {
        if (conn.state == ConnState.WaitForReady)
        {
            conn.state = ConnState.InGame;
            Player p = lm.AddPlayer(ControllerType.Remote, NewPlayerId(), conn.playerName);
            conn.playerId = p.id;
            //发送游戏状态
            InitServerGameInfo info = new InitServerGameInfo();
            //此客户端的玩家信息
            info.clientLocalPlayer = p.AchievePlayerInfo();
            //其他玩家的信息 
            info.elsePlayers = new PlayerInfo[lm.playerCount - 1];
            lm.AchievePlayerInfo(info.elsePlayers, p.id);
            Server.SendMessage(new GameMsg(GameMsg.MsgType.InitServerGameInfo, info), conn.connId, UnityEngine.Networking.QosType.ReliableSequenced);


            //通知其他玩家 
            PlayerJoin join = new PlayerJoin();
            join.info = info.clientLocalPlayer;
            GameMsg msg = new GameMsg(GameMsg.MsgType.PlayerJoin, join);

            SendToAllClientsInGame(msg, conn.connId, UnityEngine.Networking.QosType.ReliableSequenced);
        }
    }

    void OnClientQuit(QuitGameReq req, Connection conn)
    {
        Server.Disconnect(conn.connId);
        //从dict中移出这个连接
        connDict.Remove(conn.connId);

        if (conn.state == ConnState.InGame)
        {   //游戏玩家-1,并通知其他玩家
            inGameClientPlayerCount--;

            LevelManager lm = UnityHelper.GetLevelManager();
            lm.RemovePlayer(conn.playerId);

            UIManager um = UnityHelper.GetUIManager();
            um.AddScrollMessage(string.Format("{0}离开游戏", conn.playerName));

            Protocol.PlayerQuit quit = new PlayerQuit();
            quit.ids = new int[1];
            quit.ids[0] = conn.playerId;
            GameMsg msg = new GameMsg(GameMsg.MsgType.PlayerQuit, quit);
            
            SendToAllClientsInGame(msg, UnityEngine.Networking.QosType.ReliableSequenced);
        }
        else if (conn.state == ConnState.WaitForReady)
        {   //游戏玩家-1,
            inGameClientPlayerCount--;
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
                    Debug.LogError("ServerAgent.OnClientLocalState >> player has no remote controller component");
                }
            }
            else
            {   //???不应该发生
                Debug.LogError("ServerAgent.OnClientLocalState >> player doesn't exist");
            }
        }
    }

    void OnDamage(HitPlayer hit, Connection conn)
    {
        if (conn.state == ConnState.InGame)
        {
            Player src = lm.GetPlayer(hit.sourceId);
            Player p = lm.GetPlayer(hit.targetId);
            if (p != null)
            {
                if (p.playerType == PlayerType.Local || p.playerType == PlayerType.LocalAI)
                {   //这里偷懒了,没有做任何判定
                    Vector3 point = new Vector3(hit.hitPosX, hit.hitPosY, hit.hitPosZ);
                    p.Damage(src, hit.damage, point);
                }
                else if (p.playerType == PlayerType.Remote)
                {
                    SendDamage(hit);
                }
            }
        }
    }

    void OnShoot(PlayerShoot shoot, Connection conn)
    {
        if (conn.state == ConnState.InGame)
        {
            Player p = lm.GetPlayer(shoot.id);
            if (p != null)
            {
                if (p.playerType == PlayerType.Remote)
                {
                    RemotePlayerController rpc = p.GetComponent<RemotePlayerController>();
                    Vector3 targePoint = new Vector3(shoot.targetPointX, shoot.targetPointY, shoot.targetPointZ);
                    rpc.Shoot(targePoint);
                    
                    SendShoot(shoot);
                }
            }
        }
    }
    

    void SendToAllClientsInGame(GameMsg msg, UnityEngine.Networking.QosType qos)
    {
        //直接打包信息,避免多次打包
        int length;
        byte[] data = MsgPacker.Pack(msg, out length);

        foreach (Connection c in connDict.Values)
        {
            if (c.state == ConnState.InGame)
                Server.SendMessage(data, length, c.connId, qos);
        }
    }

    void SendToAllClientsInGame(GameMsg msg, int ignoreId, UnityEngine.Networking.QosType qos)
    {
        //直接打包信息,避免多次打包
        int length;
        byte[] data = MsgPacker.Pack(msg, out length);

        foreach (Connection c in connDict.Values)
        {
            if (c.state == ConnState.InGame && c.connId != ignoreId)
                Server.SendMessage(data, length, c.connId, qos);
        }
    }

    //向所有client更新玩家状态,收到的消息甚至包括客户端本地玩家状态，除了hp等关键信息可以不处理
    void PlayerStateUpdate()
    {
        Protocol.ServerGameState state = new ServerGameState();
        state.info = new PlayerInfo[lm.playerCount];
        lm.AchievePlayerInfo(state.info, -1);
        GameMsg msg = new GameMsg(GameMsg.MsgType.ServerGameState, state);
        
        SendToAllClientsInGame(msg, UnityEngine.Networking.QosType.StateUpdate);
    }

    public void SendDamage(Player src, Player target, Vector3 pos, float damage)
    {
        foreach (Connection conn in connDict.Values)
        {
            if (conn.state == ConnState.InGame)
            {
                if (conn.playerId == target.id)
                {
                    Protocol.HitPlayer hit = new Protocol.HitPlayer();
                    hit.sourceId = src.id;
                    hit.targetId = target.id;
                    hit.damage = damage;
                    hit.hitPosX = pos.x;
                    hit.hitPosY = pos.y;
                    hit.hitPosZ = pos.z;
                    GameMsg msg = new GameMsg(GameMsg.MsgType.Damage, hit);
                    Server.SendMessage(msg, conn.connId, UnityEngine.Networking.QosType.ReliableSequenced);
                }
            }
        }
    }

    public void SendDamage(HitPlayer hit)
    {
        foreach (Connection conn in connDict.Values)
        {
            if (conn.state == ConnState.InGame)
            {
                if (conn.playerId == hit.targetId)
                {
                    GameMsg msg = new GameMsg(GameMsg.MsgType.Damage, hit);
                    Server.SendMessage(msg, conn.connId, UnityEngine.Networking.QosType.ReliableSequenced);
                }
            }
        }
    }

    public void SendShoot(int id, Vector3 pos)
    {
        Protocol.PlayerShoot shoot = new PlayerShoot();
        shoot.id = id;
        shoot.targetPointX = pos.x;
        shoot.targetPointY = pos.y;
        shoot.targetPointZ = pos.z;
        GameMsg msg = new GameMsg(GameMsg.MsgType.Shoot, shoot);
        
        SendToAllClientsInGame(msg, UnityEngine.Networking.QosType.AllCostDelivery);
    }

    public void SendShoot(Protocol.PlayerShoot shoot)
    {
        GameMsg msg = new GameMsg(GameMsg.MsgType.Shoot, shoot);
        
        SendToAllClientsInGame(msg, UnityEngine.Networking.QosType.AllCostDelivery);
    }

    List<int> AIIdList = new List<int>();
    public void AddAI()
    {
        if (AIIdList.Count < 3)
        {
            int id = NewPlayerId();
            Player player = lm.AddPlayer(ControllerType.LocalAI, id, "AI id:" + id);
            AIIdList.Add(id);

            if (inGameClientPlayerCount > 0)
            {
                //通知其他玩家
                PlayerJoin join = new PlayerJoin();
                join.info = player.AchievePlayerInfo();
                GameMsg msg = new GameMsg(GameMsg.MsgType.PlayerJoin, join);
                
                SendToAllClientsInGame(msg, UnityEngine.Networking.QosType.ReliableSequenced);
            }
        }
        else
        {
            UIManager um = UnityHelper.GetUIManager();
            um.AddScrollMessage("AI数量达到上限");
        }
    }

    public void ClearAI()
    {
        PlayerQuit quit = new PlayerQuit();
        quit.ids = new int[AIIdList.Count];

        int i = 0;
        foreach (int id in AIIdList)
        {
            lm.RemovePlayer(id);
            quit.ids[i++] = id;
        }

        if (inGameClientPlayerCount > 0)
        {
            GameMsg msg = new GameMsg(GameMsg.MsgType.PlayerQuit, quit);
            SendToAllClientsInGame(msg, UnityEngine.Networking.QosType.ReliableSequenced);
        }

        AIIdList.Clear();
    }
}
