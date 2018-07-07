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
    public void MessageBox(string msg, bool hasCancel, MessageBox.MsgBoxCb cb)
    {
        if (msgBox != null)
        {
            msgBox.ShowMessage(msg, hasCancel, cb);
        }
        else
        {
            Debug.LogError("UIManager.MessageBox >> MessageBox is null");
        }
    }

    public bool ShowWnd(string name)
    {
        Transform trWnd = transform.FindChild(name);
        if (trWnd != null)
        {
            trWnd.gameObject.SetActive(true);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HideWnd(string name)
    {
        Transform trWnd = transform.FindChild(name);
        if (trWnd != null)
        {
            trWnd.gameObject.SetActive(false);
            return true;
        }
        else
        {
            return false;
        }
    }


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



}
