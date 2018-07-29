using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Protocol;

public delegate void OnLoadingDone();

public enum LoadingTask
{
    Nothing,
    SwitchGameScene,
}

public class Loading : MonoBehaviour
{
    public static LoadingTask loadingTask = LoadingTask.Nothing;
    public static string targetSceneName = null;

    [SerializeField]
    Text txtTips = null;

    enum LoadingState
    {
        Nothing,
        Loading,
        Done,
    }

    float totalTime = 0f;
    void Start()
    {
        totalTime = 0f;
        if (targetSceneName == null)
            targetSceneName = StringAssets.mainMenuSceneName;

        if (loadingTask == LoadingTask.SwitchGameScene)
        {
            Client.onConnectEvent += this.OnConnectEvent;
            Client.onDataEvent += this.OnDateEvent;
            Client.onDisconnectEvent += this.OnDisconnectEvent;
        }

        Resources.UnloadUnusedAssets();
        StartLoading(targetSceneName);
    }

    int count = 0;
    // Update is called once per frame
    void Update()
    {
        totalTime += Time.deltaTime;
        if (state == LoadingState.Loading)
        {
            count++;
            if (count > 5)  //不要每一帧都处理  
            {
                count = 0;
                float progress = Mathf.Min(asyncOperation.progress, totalTime * 0.5f);
                if (progress < 0.9f)
                {
                    txtTips.text = string.Format("加载中: {0:0%}", progress);
                }
                else
                {
                    state = LoadingState.Done;
                    OnLoadingDone();
                }
            }
        }
    }

    void OnDestroy()
    {
        if (loadingTask == LoadingTask.SwitchGameScene)
        {
            Client.onConnectEvent -= this.OnConnectEvent;
            Client.onDataEvent -= this.OnDateEvent;
            Client.onDisconnectEvent -= this.OnDisconnectEvent;
        }
    }

    LoadingState state = LoadingState.Nothing;
    AsyncOperation asyncOperation = null;
    void StartLoading(string name)
    {
        if (state == LoadingState.Nothing)
        {
            asyncOperation = SceneManager.LoadSceneAsync(name);
            asyncOperation.allowSceneActivation = false;
            state = LoadingState.Loading;
            gameObject.SetActive(true);
        }
    }

    void OnLoadingDone()
    {
        if (loadingTask == LoadingTask.Nothing)
        {
            txtTips.text = "加载完成";
            asyncOperation.allowSceneActivation = true;
        }
        else if (loadingTask == LoadingTask.SwitchGameScene)
        {
            txtTips.text = "加载完成";
            ClientReady ready = new ClientReady();
            Client.SendMessage(new GameMsg(GameMsg.MsgType.ClientReady, ready), UnityEngine.Networking.QosType.ReliableSequenced);
        }
    }

    void OnDateEvent(GameMsg msg, int connection)
    {
        switch (msg.type)
        {
            case GameMsg.MsgType.InitServerGameInfo:
                {
                    InitServerGameInfo info = msg.content as InitServerGameInfo;
                    if (info != null)
                    {
                        GlobalVariables.clientInitInfo = info;
                        asyncOperation.allowSceneActivation = true;
                    }
                }
                break;
        }
    }

    void OnConnectEvent(int connection)
    {

    }

    void OnDisconnectEvent(int connection)
    {
        UIManager uiManager = UnityHelper.GetUIManager();
        uiManager.MessageBox("连接已断开", false, OnMsgBox);
    }

    void OnMsgBox(bool confirm)
    {
        SceneManager.LoadScene(StringAssets.mainMenuSceneName);
    }
}
