using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;


namespace Protocol
{
    [Serializable]
    public class GameMsg
    {
        public enum MsgType
        {
            None,
            Test,
            //client
            JoinGameReq,
            ClientReady,
            QuitGameReq,
            ClientLocalPlayerInfo,

            //server
            JoinGameRsp,
            PlayerJoin,
            PlayerQuit,
            InitServerGameInfo,  //在client加入后,第一次发送此消息
            ServerGameState, //后续状态更新
            
        }

        public MsgType type = MsgType.None;
        public object content = null;

        public GameMsg(MsgType t, object c)
        {
            type = t;
            content = c;
        }
    }

    [Serializable]
    public class JoinGameReq
    {
        public JoinGameReq(string name)
        {
            this.name = name;
        }
        public string name = null;
    }

    [Serializable]
    public class JoinGameRsp
    {
        public bool success;
    }

    [Serializable]
    public class ClientReady
    {

    }

    [Serializable]
    public class QuitGameReq
    {

    }

    [Serializable]
    public class PlayerInfo
    {
        public int id;
        public string name;
        public float yaw;
        public float velocityX;
        public float velocityY;
        public float velocityZ;
        public float positionX;
        public float positionY;
        public float positionZ;
        public WeaponType weapon;

        //动画状态信息
        public int upperAniState;
        public float upperAniNormTime;
        public int lowerAniState;
        public float lowerAniNormTime;

        public float walkRun;
        public float aimUp;
        public float strafeForward;
        public float strafeRight;
    }
    
    [Serializable]
    public class PlayerJoin
    {
        public PlayerInfo info;
    }

    [Serializable]
    public class PlayerQuit
    {
        public int[] ids;
    }

    [Serializable]
    public class ClientLocalPlayerInfo
    {
        public PlayerInfo info;
    }

    [Serializable]
    public class ServerGameState
    {
        public PlayerInfo[] info;
    }

    [Serializable]
    public class InitServerGameInfo
    {
        public PlayerInfo clientLocalPlayer;
        public PlayerInfo[] elsePlayers;
    }
}
