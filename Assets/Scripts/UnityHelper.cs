using UnityEngine;
using System.Collections;

public static class UnityHelper
{

    public static Transform FindChildRecursive(Transform t, string name)
    {
        if (t.name.Equals(name))
            return t;
        int childCount = t.childCount;
        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                Transform c = t.GetChild(i);
                Transform r = FindChildRecursive(c, name);
                if (r != null)
                    return r;
            }
        }
        return null;
    }

    ///找到本地玩家
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
        while (origin != null)
        {
            T obj = origin.GetComponent<T>();
            if (obj != null)
            {
                return obj;
            }
            origin = origin.parent;
        }
        return default(T);
    }

    //找到main camera
    public static CameraControl GetCameraControl()
    {
        GameObject go = GameObject.FindWithTag("MainCamera");
        return go.GetComponent<CameraControl>();
    }

}
