using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Protocol;

public class ClientManager : MonoBehaviour
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
            InitClientGame(GlobalVariables.clientInitInfo);
        }
        else
        { //
            Debug.LogError("ClientManager.Start >> no init game info");
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
                RemotePlayerJoin(join);
        }
        else if (msg.type == GameMsg.MsgType.PlayerQuit)
        {//移出
            PlayerQuit quit = msg.content as PlayerQuit;
            if (quit != null)
                RemotePlayerQuit(quit);
        }
        else if (msg.type == GameMsg.MsgType.ServerGameState)
        {
            //更新
            ServerGameState state = msg.content as ServerGameState;
            if (state != null)
                UpdateGameInfo(state);
        }
        else if (msg.type == GameMsg.MsgType.InitServerGameInfo)
        { //开始游戏
            InitServerGameInfo state = msg.content as InitServerGameInfo;
            if (state != null)
                InitClientGame(state);
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
    void InitClientGame(InitServerGameInfo info)
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

    void UpdateGameInfo(ServerGameState info)
    {
        if (initRecieved)
        {
            foreach (PlayerInfo remoteInfo in info.info)
            {
                Player player = lm.GetPlayer(remoteInfo.id);
                if (player != null && player.playerType == PlayerType.Remote)
                {
                    // 状态设置
                    RemotePlayerController rpc = player.GetComponent<RemotePlayerController>();
                    rpc.ApplyRemoteInfo(remoteInfo);
                }
            }
        }
    }

    void RemotePlayerJoin(PlayerJoin join)
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

    void RemotePlayerQuit(PlayerQuit quit)
    {
        if (initRecieved)
        {
            foreach (int id in quit.ids)
            {
                lm.RemovePlayer(id);
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
            Client.SendMessage(msg, false);
        }
        else
        {
            Debug.LogError("ClientManager.SendPlayerState >> local player is null");
        }
    }

    public void SendQuitGame()
    {
        Player p = GlobalVariables.localPlayer;
        if (p != null)
        {
            Protocol.QuitGameReq quit = new QuitGameReq();
            GameMsg msg = new GameMsg(GameMsg.MsgType.QuitGameReq, quit);
            Client.SendMessage(msg, true);
        }
        else
        {
            Debug.LogError("ClientManager.SendPlayerState >> local player is null");
        }
    }
}
