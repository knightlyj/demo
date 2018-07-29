using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GamePlayMenu : MonoBehaviour {
    [SerializeField]
    Button btnAddAI = null;
    [SerializeField]
    Button btnCleraAI = null;
    [SerializeField]
    Button btnExit = null;
    [SerializeField]
    Button btnClose = null;
    void Awake()
    {
        btnAddAI.onClick.AddListener(this.OnAddAIClick);
        btnCleraAI.onClick.AddListener(this.OnClearAIClick);
        btnExit.onClick.AddListener(this.OnExitClick);
        btnClose.onClick.AddListener(this.OnCloseClick);
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDestroy()
    {
        btnAddAI.onClick.RemoveListener(this.OnAddAIClick);
        btnCleraAI.onClick.RemoveListener(this.OnClearAIClick);
        btnExit.onClick.RemoveListener(this.OnExitClick);
        btnClose.onClick.RemoveListener(this.OnCloseClick);
    }

    void OnAddAIClick()
    {
        ServerAgent sa = UnityHelper.GetServerAgent();
        if (sa)
        {
            sa.AddAI();
        }
    }

    void OnClearAIClick()
    {
        ServerAgent sa = UnityHelper.GetServerAgent();
        if (sa)
        {
            sa.ClearAI();
        }
    }

    void OnExitClick()
    {
        UIManager um = UnityHelper.GetUIManager();
        um.MessageBox("退出游戏?", true, this.MsgBoxCb);
    }


    void MsgBoxCb(bool confirm)
    {
        if (confirm)
        {
            UnityHelper.LoadSceneAsync(StringAssets.mainMenuSceneName);
        }
    }

    void OnCloseClick()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        GlobalVariables.menuOpened = true;
    }

    void OnDisable()
    {
        GlobalVariables.menuOpened = false;
    }
}
