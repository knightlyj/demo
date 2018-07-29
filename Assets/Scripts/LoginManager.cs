using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Protocol;

public class LoginManager : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        Client.onConnectEvent += this.OnConnectEvent;
        Client.onDataEvent += this.OnDateEvent;
        Client.onDisconnectEvent += this.OnDisconnectEvent;
    }

    void OnDestroy()
    {
        Client.onConnectEvent -= this.OnConnectEvent;
        Client.onDataEvent -= this.OnDateEvent;
        Client.onDisconnectEvent -= this.OnDisconnectEvent;
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            Client.Receive();
        }
    }

    bool started = false;
    public void StartClient()
    {
        if (!started)
        {
            Client.Init();
            Client.Connect();
            started = true;
        }
    }

    public void StopClient()
    {
        started = false;
        Client.Exit();
    }
    

    void OnDateEvent(GameMsg msg, int connection)
    {
        switch (msg.type)
        {
            case GameMsg.MsgType.JoinGameRsp:
                {
                    JoinGameRsp rsp = msg.content as JoinGameRsp;
                    if (rsp != null && rsp.success)
                    {
                        //加载场景
                        UnityHelper.LoadSceneAsync(StringAssets.gamePlaySceneName, LoadingTask.SwitchGameScene);
                    }
                }
                break;
        }
    }

    void OnConnectEvent(int connection)
    {
        JoinGameReq req = new JoinGameReq(SystemInfo.deviceName);
        GameMsg msg = new GameMsg(GameMsg.MsgType.JoinGameReq, req);
        Client.SendMessage(msg, UnityEngine.Networking.QosType.ReliableSequenced);
    }

    void OnDisconnectEvent(int connection)
    {
        UIManager uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        uiManager.MessageBox("连接已断开", false, null);
        started = false;
    }

}
