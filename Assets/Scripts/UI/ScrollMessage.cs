using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

[RequireComponent(typeof(ScrollRect))]
public class ScrollMessage : MonoBehaviour
{
    [SerializeField]
    Text content = null;
    [SerializeField]
    Outline contentOutline = null;

    ScrollRect scrollRect = null;

    List<string> messages = new List<string>(maxMsgCount);
    const int maxMsgCount = 5;

    Image image = null;
    void Awake()
    {
        image = GetComponent<Image>();

        scrollRect = GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnScroll);

    }

    DateTime lastScrollTime = DateTime.Now;
    void OnScroll(Vector2 position)
    {
        lastScrollTime = DateTime.Now;
        hideTimer = 0f;
    }

    float alpha = 1f;
    float hideTimer = 5f;
    void Update()
    {
        hideTimer += Time.deltaTime;
        if (hideTimer > 3f)
        {
            alpha -= Time.deltaTime;
            if (alpha < 0f)
                alpha = 0f;
        }
        else
        {
            alpha += Time.deltaTime * 2f;
            if (alpha > 1f)
                alpha = 1f;
        }
        SetAlpha();
        
    }

    void SetAlpha()
    {
        Color c = image.color;
        c.a = 0f + alpha * 0.4f;
        image.color = c;

        c = content.color;
        c.a = 0.5f + 0.5f * alpha;
        content.color = c;

        c = contentOutline.effectColor;
        c.a = 0.5f + 0.5f * alpha;
        contentOutline.effectColor = c;

    }


    void OnDestroy()
    {
        scrollRect.onValueChanged.RemoveListener(OnScroll);
    }


    public void AddMessage(string str)
    {
        if (messages.Count >= maxMsgCount)
        {
            messages.RemoveAt(0);
        }
        messages.Add(str);

        StringBuilder sb = new StringBuilder();
        foreach (string s in messages)
        {
            sb.AppendLine(s);
        }
        content.text = sb.ToString();

        //禁用rancast target了
        //TimeSpan span = DateTime.Now - lastScrollTime;
        //if (span.TotalSeconds > 5)
        //{   //一段时间没拖动,会跳到最新消息
        scrollRect.normalizedPosition = Vector2.zero;
        //}

        hideTimer = 0f;
    }
}
