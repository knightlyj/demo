using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    RectTransform rtBorder;
    //RectTransform rtEmpty;
    RectTransform rtLost;
    RectTransform rtPoint;

    bool init = false;
    //RectTransform rtBorder
    void Init()
    {
        if (!init)
        {
            rtBorder = transform.FindChild("Border") as RectTransform;
            //rtEmpty = transform.FindChild("Empty") as RectTransform;
            rtLost = transform.FindChild("Lost") as RectTransform;
            rtPoint = transform.FindChild("XPoint") as RectTransform;
            init = true;
        }
    }

    void Awake()
    {
        Init(); 
    }

    // Use this for initialization
    void Start()
    {
        //这种UI尺寸直接硬编码了,计算出point bar的最大尺寸
        Rect rectBorder = rtBorder.rect;
        pointSize.x = rectBorder.width - 12;
        pointSize.y = rectBorder.height - 7;
    }
    
    [SerializeField]
    float delay = 0.5f;
    [SerializeField]
    float lostSpeed = 0.8f;
    
    Vector2 pointSize;
    
    bool inLost = false;
    float delayTimer = 0;
    // Update is called once per frame
    void Update()
    {
        if (inLost)
        { //带延迟的lost bar
            delayTimer -= Time.deltaTime;
            if (delayTimer < 0)
            {
                delayTimer = 0;
                if (lostRatio > pointRatio)
                {
                    lostRatio -= lostSpeed * Time.deltaTime;
                    if (lostRatio < pointRatio)
                    {
                        lostRatio = pointRatio;
                        inLost = false;
                    }
                    SetLostRatioView();
                }
            }
        }
    }

    float pointRatio = 1.0f;
    void SetPointRatioView()
    {
        float toRight = pointSize.x * (1.0f - pointRatio) + 6;
        rtPoint.offsetMax = new Vector2(-toRight, -4);
    }

    float lostRatio = 1.0f;
    void SetLostRatioView()
    {
        float toRight = pointSize.x * (1.0f - lostRatio) + 6;
        rtLost.offsetMax = new Vector2(-toRight, -4);
    }

    public void SetRatio(float ratio)
    {
        if (ratio > 1.0f)
            ratio = 1.0f;
        else if (ratio < 0)
            ratio = 0;

        pointRatio = ratio;
        if(pointRatio > lostRatio)
        {
            lostRatio = pointRatio;
            SetPointRatioView();
            SetLostRatioView();
        }
        else
        {
            SetPointRatioView();
            delayTimer = delay;
            inLost = true;
        }
    }
    
}
