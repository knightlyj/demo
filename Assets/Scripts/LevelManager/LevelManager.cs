using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Protocol;

public enum ControllerType
{
    Unknown,
    Local,
    LocalAI,
    Remote,
}

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    float gravityScale = 1.0f;
    public float timeScale = 1f;

    // Use this for initialization
    void Start()
    {
        Physics.gravity = new Vector3(0f, -9.8f, 0f) * gravityScale; //设置重力
        Time.timeScale = timeScale;

        if (GlobalVariables.hostType == HostType.Server)
        {
            ServerManager sm = GetComponent<ServerManager>();
            sm.enabled = true;
        }
        else if (GlobalVariables.hostType == HostType.Client)
        {
            ClientManager cm = GetComponent<ClientManager>();
            cm.enabled = true;
        }
    }

    void OnDestroy()
    {
        if (GlobalVariables.hostType == HostType.Server)
        {
            ServerManager sm = GetComponent<ServerManager>();
            sm.enabled = false;

        }
        else if (GlobalVariables.hostType == HostType.Client)
        {
            ClientManager cm = GetComponent<ClientManager>();
            cm.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }


    [SerializeField]
    Transform playerPrefab = null;


    Dictionary<int, Player> playerDict = new Dictionary<int, Player>();

    public Player AddPlayer(ControllerType type, int id, string name)
    {
        if (playerDict.ContainsKey(id))
        {
            return null;
        }
        Player player = null;
        switch (type)
        {
            case ControllerType.Local:
                if (GlobalVariables.localPlayer == null)
                {
                    player = Instantiate(playerPrefab).GetComponent<Player>();
                    player.transform.position = new Vector3(22, 0, 80);
                    GlobalVariables.localPlayer = player;
                    player.tag = StringAssets.localPlayerTag;
                    player.playerType = PlayerType.Local;
                    player.gameObject.AddComponent<LocalPlayerController>();
                }
                else
                {
                    Debug.LogError("LevelManager.AddPlayer >> local player already exist");
                }
                break;
            case ControllerType.LocalAI:
                if (GlobalVariables.hostType == HostType.Server)
                {
                    player = Instantiate(playerPrefab).GetComponent<Player>();
                    player.tag = StringAssets.AIPlayerTag;
                    player.playerType = PlayerType.LocalAI;
                    player.gameObject.AddComponent<LocalPlayerController>();
                }
                else
                {
                    Debug.LogError("LevelManager.AddPlayer >> client try to add AI");
                }
                break;
            case ControllerType.Remote:
                player = Instantiate(playerPrefab).GetComponent<Player>();
                player.tag = StringAssets.remoteplayerTag;
                player.playerType = PlayerType.Remote;
                player.gameObject.AddComponent<RemotePlayerController>();
                break;
            default:
                break;
        }

        if (player != null)
        {
            player.id = id;
            player.nameInGame = name;
            playerDict.Add(player.id, player);
        }
        return player;
    }

    public bool RemovePlayer(int id)
    {
        if (playerDict.ContainsKey(id))
        {
            if (GlobalVariables.localPlayer != null)
            {
                if (GlobalVariables.localPlayer.id == id)
                {
                    GlobalVariables.localPlayer = null;
                }
            }
            Destroy(playerDict[id].gameObject);
            playerDict.Remove(id);
            return true;
        }
        else
        {
            return false;
        }
    }

    public Player GetPlayer(int id)
    {
        if (playerDict.ContainsKey(id))
        {
            return playerDict[id];
        }
        else
        {
            return null;
        }
    }

    public int playerCount
    {
        get
        {
            return playerDict.Count;
        }
    }

    void ClearAllPlayer()
    {
        GlobalVariables.localPlayer = null;
        foreach (Player p in playerDict.Values)
        {
            Destroy(p.gameObject);
        }
        playerDict.Clear();
    }

    /// <summary>
    /// idNotFill 对应的player信息不会填充，因为id不会为负数，用负数时会填充所有玩家信息
    /// </summary>
    /// <param name="info"></param>
    /// <param name="idNotFill"></param>
    /// <returns></returns>
    public bool AchievePlayerInfo(PlayerInfo[] info, int idNotFill)
    {
        if (info.Length < playerDict.Count - 1)
            return false;
        int i = 0;
        foreach (Player p in playerDict.Values)
        {
            if (p.id != idNotFill)
            {
                info[i++] = p.AchievePlayerInfo();
            }
        }
        return true;
    }
}
