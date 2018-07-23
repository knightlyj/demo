using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour {
    [SerializeField]
    Button btnConfirm = null;
    [SerializeField]
    Button btnCancel = null;
    // Use this for initialization
    void Awake()
    {
        btnConfirm.onClick.AddListener(this.OnConfirmClick);
        btnCancel.onClick.AddListener(this.OnCancelClick);
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
    public void ShowMessage(string msg, bool hasCancel, MsgBoxCb cb, string[] buttonText = null)
    {
        MsgCb = cb;
        if (hasCancel)
        {
            //设置确定和取消两个按钮
            RectTransform rect = btnConfirm.transform  as RectTransform;
            rect.anchorMin = new Vector2(btnToSide, btnBaseY);
            rect.anchorMax = new Vector2(btnToSide + btnWidth, btnBaseY + btnHeight);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            btnConfirm.gameObject.SetActive(true);
            
            rect = btnCancel.transform as RectTransform;
            rect.anchorMin = new Vector2(1 - btnToSide - btnWidth, btnBaseY);
            rect.anchorMax = new Vector2(1 - btnToSide, btnBaseY + btnHeight);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            btnCancel.gameObject.SetActive(true);
            
            if(buttonText != null)
            {
                Text t = btnConfirm.transform.FindChild("Text").GetComponent<Text>();
                t.text = buttonText[0];

                t = btnCancel.transform.FindChild("Text").GetComponent<Text>();
                t.text = buttonText[1];
            }
            else
            {
                Text t = btnConfirm.transform.FindChild("Text").GetComponent<Text>();
                t.text = "确定";

                t = btnCancel.transform.FindChild("Text").GetComponent<Text>();
                t.text = "取消";
            }
        }
        else
        {
            //把确定按钮放到中间就行
            RectTransform rect = btnConfirm.transform as RectTransform;
            rect.anchorMin = new Vector2((1 - btnWidth) / 2, btnBaseY);
            rect.anchorMax = new Vector2((1 - btnWidth) / 2 + btnWidth, btnBaseY + btnHeight);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            btnConfirm.gameObject.SetActive(true);
            
            btnCancel.gameObject.SetActive(false);

            if (buttonText != null)
            {
                Text t = btnConfirm.transform.FindChild("Text").GetComponent<Text>();
                t.text = buttonText[0];
            }
            else
            {
                Text t = btnConfirm.transform.FindChild("Text").GetComponent<Text>();
                t.text = "确定";
            }
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
