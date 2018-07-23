using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Protocol;

public class ClientAgent : MonoBehaviour
{
    LevelManager lm = null;
    // Use this for initialization
    void Start()
    {
        Client.onConnectEvent += this.OnConnectEvent;
        Client.onDisconnectEvent += this.OnDisconnectEvent;
        Client.onDataEvent += this.OnDateEvent;
        lm = GetComponent<LevelManager>();
        if (GlobalVariables.clientInitInfo != null)
        {
            OnInitClientGame(GlobalVariables.clientInitInfo);
        }
        else
        { //
            Debug.LogError("ClientAgent.Start >> no init game info");
        }
    }

    void OnDestroy()
    {
        Client.Exit();
        Client.onConnectEvent -= this.OnConnectEvent;
        Client.onDisconnectEvent -= this.OnDisconnectEvent;
        Client.onDataEvent -= this.OnDateEvent;
    }

    const float stateUpdateRate = 15f;
    float noUpdateTime = 0;
    // Update is called once per frame
    void Update()
    {
        Client.Receive();

        //更新状态消息
        noUpdateTime += Time.deltaTime;
        if (noUpdateTime > 1 / stateUpdateRate)
        {
            noUpdateTime = 0f;
            SendPlayerState();
        }
    }

    void OnDateEvent(Protocol.GameMsg msg, int connection)
    {
        if (msg.type == GameMsg.MsgType.PlayerJoin)
        {   //新增玩家
            PlayerJoin join = msg.content as PlayerJoin;
            if (join != null)
                OnRemotePlayerJoin(join);
        }
        else if (msg.type == GameMsg.MsgType.PlayerQuit)
        {//移出
            PlayerQuit quit = msg.content as PlayerQuit;
            if (quit != null)
                OnRemotePlayerQuit(quit);
        }
        else if (msg.type == GameMsg.MsgType.ServerGameState)
        {
            //更新
            ServerGameState state = msg.content as ServerGameState;
            if (state != null)
                OnUpdateGameInfo(state);
        }
        else if (msg.type == GameMsg.MsgType.InitServerGameInfo)
        { //开始游戏
            InitServerGameInfo state = msg.content as InitServerGameInfo;
            if (state != null)
                OnInitClientGame(state);
        }
        else if (msg.type == GameMsg.MsgType.Damage)
        {
            HitPlayer hit = msg.content as HitPlayer;
            if (hit != null)
                OnDamage(hit);
        }
        else if (msg.type == GameMsg.MsgType.Shoot)
        {
            PlayerShoot shoot = msg.content as PlayerShoot;
            if (shoot != null)
                OnShoot(shoot);
        }
    }
    void OnConnectEvent(int connection)
    {
        //??? 不应该发生
    }
    void OnDisconnectEvent(int connection)
    {
        //与服务器断开连接了,弹出窗口告诉玩家,点确定后,退回主菜单
        UIManager um = UnityHelper.GetUIManager();
        um.MessageBox("与主机连接断开", false, this.MsgBoxCb);
    }

    void MsgBoxCb(bool confirm)
    {
        SceneManager.LoadScene(StringAssets.mainMenuSceneName);
    }

    bool initRecieved = false;
    void OnInitClientGame(InitServerGameInfo info)
    {
        if (!initRecieved)
        {

            PlayerInfo localInfo = info.clientLocalPlayer;
            if (localInfo != null)
            {
                Player localPlayer = lm.AddPlayer(ControllerType.Local, localInfo.id, localInfo.name);
                localPlayer.transform.position = new Vector3(localInfo.positionX, localInfo.positionY, localInfo.positionZ);

                foreach (PlayerInfo remoteInfo in info.elsePlayers)
                {
                    if (remoteInfo.id != localInfo.id)
                    {
                        Player remotePlayer = lm.AddPlayer(ControllerType.Remote, remoteInfo.id, remoteInfo.name);
                        // 状态设置
                        RemotePlayerController rpc = remotePlayer.GetComponent<RemotePlayerController>();
                        rpc.ApplyRemoteInfo(remoteInfo);
                    }
                }

                initRecieved = true;
            }
        }
    }

    void OnUpdateGameInfo(ServerGameState state)
    {
        if (initRecieved)
        {
            foreach (PlayerInfo info in state.info)
            {
                Player player = lm.GetPlayer(info.id);
                if (player != null)
                {
                    if (player.playerType == PlayerType.Remote)
                    {
                        // 状态设置
                        RemotePlayerController rpc = player.GetComponent<RemotePlayerController>();
                        rpc.ApplyRemoteInfo(info);
                    }
                }
            }
        }
    }

    void OnRemotePlayerJoin(PlayerJoin join)
    {
        if (initRecieved)
        {
            PlayerInfo remoteInfo = join.info;
            if (remoteInfo != null)
            {
                Player remotePlayer = lm.AddPlayer(ControllerType.Remote, remoteInfo.id, remoteInfo.name);
                RemotePlayerController rpc = remotePlayer.GetComponent<RemotePlayerController>();
                rpc.ApplyRemoteInfo(remoteInfo);
            }
        }
    }

    void OnRemotePlayerQuit(PlayerQuit quit)
    {
        if (initRecieved)
        {
            foreach (int id in quit.ids)
            {
                lm.RemovePlayer(id);
            }
        }
    }

    void OnDamage(HitPlayer hit)
    {
        Player src = lm.GetPlayer(hit.sourceId);
        Player p = lm.GetPlayer(hit.targetId);
        if (p != null)
        {
            if (p.playerType == PlayerType.Local)
            {
                Vector3 point = new Vector3(hit.hitPosX, hit.hitPosY, hit.hitPosZ);
                p.Damage(src, hit.damage, point);
            }
        }
    }

    void OnShoot(PlayerShoot shoot)
    {
        Player p = lm.GetPlayer(shoot.id);
        if (p != null)
        {
            if (p.playerType == PlayerType.Remote)
            {
                RemotePlayerController rpc = p.GetComponent<RemotePlayerController>();
                Vector3 targePoint = new Vector3(shoot.targetPointX, shoot.targetPointY, shoot.targetPointZ);
                rpc.Shoot(targePoint);
            }
        }
    }

    //发送当前玩家状态
    void SendPlayerState()
    {
        Player p = GlobalVariables.localPlayer;
        if (p != null)
        {
            Protocol.ClientLocalPlayerInfo info = new ClientLocalPlayerInfo();
            info.info = p.AchievePlayerInfo();
            GameMsg msg = new GameMsg(GameMsg.MsgType.ClientLocalPlayerInfo, info);
            Client.SendMessage(msg, UnityEngine.Networking.QosType.StateUpdate);
        }
        else
        {
            Debug.LogError("ClientAgent.SendPlayerState >> local player is null");
        }
    }

    public void SendQuitGame()
    {
        Player p = GlobalVariables.localPlayer;
        if (p != null)
        {
            Protocol.QuitGameReq quit = new QuitGameReq();
            GameMsg msg = new GameMsg(GameMsg.MsgType.QuitGameReq, quit);
            Client.SendMessage(msg, UnityEngine.Networking.QosType.ReliableSequenced);
        }
        else
        {
            Debug.LogError("ClientAgent.SendPlayerState >> local player is null");
        }
    }

    public void SendDamage(Player src, Player target, Vector3 pos, float damage)
    {
        Protocol.HitPlayer hit = new Protocol.HitPlayer();
        hit.sourceId = src.id;
        hit.targetId = target.id;
        hit.damage = damage;
        hit.hitPosX = pos.x;
        hit.hitPosY = pos.y;
        hit.hitPosZ = pos.z;
        GameMsg msg = new GameMsg(GameMsg.MsgType.Damage, hit);
        Client.SendMessage(msg, UnityEngine.Networking.QosType.ReliableSequenced);
    }

    public void SendShoot(int id, Vector3 pos)
    {
        Protocol.PlayerShoot shoot = new PlayerShoot();
        shoot.id = id;
        shoot.targetPointX = pos.x;
        shoot.targetPointY = pos.y;
        shoot.targetPointZ = pos.z;
        GameMsg msg = new GameMsg(GameMsg.MsgType.Shoot, shoot);
        Client.SendMessage(msg, UnityEngine.Networking.QosType.AllCostDelivery);
    }
}
