using UnityEngine;
using System.Collections;
using System;

public enum EventId
{
    LocalPlayerCreate, //本地玩家创建
    LocalPlayerDestroy, //本地玩家销毁

    LocalPlayerLoad, //读取玩家存档
}

public static class EventManager
{
    public delegate void Listener(System.Object sender, System.Object eventArg);
    static Listener[] EventListener;

    static EventManager()
    {
        Array a = Enum.GetValues(typeof(EventId));
        EventListener = new Listener[a.Length];
    }

    public static void AddListener(EventId id, Listener listener)
    {
        EventListener[(int)id] += listener;
    }

    public static void RemoveListener(EventId id, Listener listener)
    {
        EventListener[(int)id] -= listener;
    }

    public static void RaiseEvent(EventId id, System.Object sender, System.Object eventArg)
    {
        if(EventListener[(int)id] != null)
        {
            EventListener[(int)id](sender, eventArg);
        }
    }
}
