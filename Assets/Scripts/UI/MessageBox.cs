using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour {

    // Use this for initialization
    void Awake()
    {
        Transform btnConfirm = this.transform.FindChild("Confirm");
        Button btn = btnConfirm.GetComponent<Button>();
        btn.onClick.AddListener(this.OnConfirmClick);

        Transform btnCancel = this.transform.FindChild("Cancel");
        btn = btnCancel.GetComponent<Button>();
        btn.onClick.AddListener(this.OnCancelClick);
        
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    const float btnHeight = 0.15f;
    const float btnWidth = 0.3f;
    const float btnBaseY = 0.05f;
    const float btnToSide = 0.1f;

    public MsgBoxCb MsgCb = null;
    public delegate void MsgBoxCb(bool confirm);
    // 设置显示内容
    public void ShowMessage(string msg, bool hasCancel, MsgBoxCb cb)
    {
        MsgCb = cb;
        if (hasCancel)
        {
            //设置确定和取消两个按钮
            Transform btnConfirm = this.transform.FindChild("Confirm");
            RectTransform rect = btnConfirm as RectTransform;
            rect.anchorMin = new Vector2(btnToSide, btnBaseY);
            rect.anchorMax = new Vector2(btnToSide + btnWidth, btnBaseY + btnHeight);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            btnConfirm.gameObject.SetActive(true);

            Transform btnCancel = this.transform.FindChild("Cancel");
            rect = btnCancel as RectTransform;
            rect.anchorMin = new Vector2(1 - btnToSide - btnWidth, btnBaseY);
            rect.anchorMax = new Vector2(1 - btnToSide, btnBaseY + btnHeight);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            btnCancel.gameObject.SetActive(true);
        }
        else
        {
            //把确定按钮放到中间就行
            Transform btnConfirm = this.transform.FindChild("Confirm");
            RectTransform rect = btnConfirm as RectTransform;
            rect.anchorMin = new Vector2((1 - btnWidth) / 2, btnBaseY);
            rect.anchorMax = new Vector2((1 - btnWidth) / 2 + btnWidth, btnBaseY + btnHeight);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            btnConfirm.gameObject.SetActive(true);

            Transform btnCancel = this.transform.FindChild("Cancel");
            btnCancel.gameObject.SetActive(false);
        }

        //设置文字内容
        Transform msgWnd = transform.FindChild("Message");
        Text txt = msgWnd.GetComponent<Text>();
        txt.text = msg;
        
        this.gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    private void OnConfirmClick()
    {
        if (MsgCb != null)
        {
            MsgCb(true);
        }
        MsgCb = null;
        this.gameObject.SetActive(false);
    }

    private void OnCancelClick()
    {
        if (MsgCb != null)
        {
            MsgCb(false);
        }
        MsgCb = null;
        this.gameObject.SetActive(false);
    }
}
