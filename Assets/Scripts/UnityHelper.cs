using UnityEngine;
using System.Collections;

public static class UnityHelper{

    //找到本地玩家
    static LocalPlayer localPlayer = null;
    public static LocalPlayer FindLocalPlayer()
    {
        if (localPlayer != null)
            return localPlayer;

        GameObject goPlayer = GameObject.FindWithTag("LocalPlayer");
        if (goPlayer != null)
        {
            localPlayer = goPlayer.GetComponent<LocalPlayer>();
            if (localPlayer != null)
            {
                localPlayer.onPlayerDestroy += OnPlayerDestroy;
            }
        }
        return localPlayer;
    }

    private static void OnPlayerDestroy()
    {
        localPlayer.onPlayerDestroy -= OnPlayerDestroy;
        localPlayer = null;
    }


    //找到父节点中的对象
    public static T FindObjectUpward<T>(Transform origin)
    {
        while(origin != null)
        {
            T obj = origin.GetComponent<T>();
            if(obj != null)
            {
                return obj;
            }
            origin = origin.parent;
        }
        return default(T);
    }
}
