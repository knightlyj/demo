using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum EventId
{
    Empty = 0,
    PlayerBlock, 
    PlayerDamage,
    PlayerDie,
    PlayerRevive,
    PlayerDestory, //玩家销毁
}

public static class EventManager
{
    struct EventKey
    {
        public EventId id;
        public int session;
        
    }

    public delegate void Listener(System.Object sender, System.Object eventArg);
    static Dictionary<EventKey, Listener> eventListenerDict = new Dictionary<EventKey, Listener>();
    
    public static void AddListener(EventId id, int session, Listener listener)
    {
        EventKey key = new EventKey();
        key.id = id;
        key.session = session;
        if (eventListenerDict.ContainsKey(key))
        {
            eventListenerDict[key] += listener;
        }
        else
        {
            eventListenerDict.Add(key, listener);
        }
    }

    public static void RemoveListener(EventId id, int session, Listener listener)
    {
        EventKey key = new EventKey();
        key.id = id;
        key.session = session;
        if (eventListenerDict.ContainsKey(key))
        {
            eventListenerDict[key] -= listener;
            if(eventListenerDict[key] == null)
            {
                eventListenerDict.Remove(key);
            }
        }
    }

    public static void RaiseEvent(EventId id, int session, System.Object sender, System.Object eventArg)
    {
        EventKey key = new EventKey();
        key.id = id;
        key.session = session;
        if (eventListenerDict.ContainsKey(key))
        {
            Listener temp = eventListenerDict[key];
            if (temp != null)
            {
                temp(sender, eventArg);
            }
        }
    }
}
