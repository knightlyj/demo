using UnityEngine;
using System.Collections;
using System;
using System.IO;

public static class UnityHelper
{
    private static StreamWriter _writer;
    static UnityHelper()
    {
        
    }

    public static void StartWriteLogFile()
    {
        if (_writer == null)
        {
            try
            {
                _writer = File.AppendText("log.txt");
            }
            catch
            {
                _writer = null;
            }
            if (_writer != null)
            {
                _writer.Write("\r\n\r\n=============== Game started ================\r\n\r\n");
                Application.logMessageReceived += LogCallback;
            }
        }
    }
    
    public static void StopWriteLogFile()
    {
        if(_writer != null)
        {
            Application.logMessageReceived -= LogCallback;
            _writer.Close();
            _writer = null;
        }
    }

    public static bool writeStack = false;
    public static void LogCallback(string condition, string stackTrace, LogType type)
    {
        if (_writer != null)
        {
            if (writeStack)
            {
                string logEntry = string.Format("\r\n {0} {1} \r\n {2}\r\n {3}"
                     , DateTime.Now, type, condition, stackTrace);
                _writer.Write(logEntry);
            }
            else
            {
                string logEntry = string.Format("\r\n {0} {1} \r\n {2}"
                     , DateTime.Now, type, condition);
                _writer.Write(logEntry);
            }
        }
    }

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


    public static LevelManager GetLevelManager()
    {
        GameObject go = GameObject.FindWithTag("LevelManager");
        return go.GetComponent<LevelManager>();
    }

    public static ClientAgent GetClientAgent()
    {
        GameObject go = GameObject.FindWithTag("LevelManager");
        return go.GetComponent<ClientAgent>();
    }

    public static ServerAgent GetServerAgent()
    {
        GameObject go = GameObject.FindWithTag("LevelManager");
        return go.GetComponent<ServerAgent>();
    }

    public static UIManager GetUIManager()
    {
        GameObject go = GameObject.FindWithTag("UIManager");
        return go.GetComponent<UIManager>();
    }
}
