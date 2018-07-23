using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    void Awake()
    {
        UnityHelper.StartWriteLogFile();
    }

    void OnApplicationQuit()
    {
        UnityHelper.StopWriteLogFile();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Loading loading = null;

    [SerializeField]
    MessageBox msgBox = null;
    public void MessageBox(string msg, bool hasCancel, MessageBox.MsgBoxCb cb, string[] buttonText = null)
    {
        if (msgBox != null)
        {
            msgBox.ShowMessage(msg, hasCancel, cb, buttonText);
        }
        else
        {
            Debug.LogError("UIManager.MessageBox >> MessageBox is null");
        }
    }

    [SerializeField]
    Transform playerInfoPanelPrefab = null;
    public void AddPlayerInfoPanel(Player p)
    {
        if (playerInfoPanelPrefab != null)
        {
            Transform trPanel = Instantiate(playerInfoPanelPrefab, transform) as Transform;
            PlayerInfoPanel panel = trPanel.GetComponent<PlayerInfoPanel>();

            panel.player = p;
        }
    }

    [SerializeField]
    ScrollMessage scrollMessage = null;
    public void AddScrollMessage(string str)
    {
        if (scrollMessage != null)
        {
            scrollMessage.AddMessage(str);
        }
    }

    //public bool ShowWnd(string name)
    //{
    //    Transform trWnd = transform.FindChild(name);
    //    if (trWnd != null)
    //    {
    //        trWnd.gameObject.SetActive(true);
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    //public bool HideWnd(string name)
    //{
    //    Transform trWnd = transform.FindChild(name);
    //    if (trWnd != null)
    //    {
    //        trWnd.gameObject.SetActive(false);
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}


    [SerializeField]
    Transform mobileUI = null;
    void OnEnable()
    {
        if (mobileUI != null)
        {
            if (Application.platform == RuntimePlatform.Android || GlobalVariables.mobileUIOnPC)
            {
                mobileUI.gameObject.SetActive(true);
            }
            else
            {
                mobileUI.gameObject.SetActive(false);
            }
        }
    }

    [SerializeField]
    Transform frontSight = null;
    public bool showFrontSight
    {
        set
        {
            if (frontSight != null)
            {
                frontSight.gameObject.SetActive(value);
            }
        }
        get
        {
            if (frontSight != null)
                return frontSight.gameObject.activeInHierarchy;
            else
                return false;
        }
    }



}
