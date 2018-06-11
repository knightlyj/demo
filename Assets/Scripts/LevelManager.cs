using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerType
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
        AddPlayer(PlayerType.Local);
        AddPlayer(PlayerType.LocalAI);
    }

    // Update is called once per frame
    void Update()
    {
        Physics.gravity = new Vector3(0f, -9.8f, 0f) * gravityScale; //设置重力
        Time.timeScale = timeScale;
    }

    [SerializeField]
    Transform localPlayerPrefab = null;
    [SerializeField]
    Transform localAIPrefab = null;
    [SerializeField]
    Transform remotePlayerPrefab = null;


    int playerIdCount = 0;
    Dictionary<int, Player> playerDict = new Dictionary<int, Player>();

    public Player AddPlayer(PlayerType type)
    {
        Player player = null;
        switch (type)
        {
            case PlayerType.Local:
                if (GlobalVariables.localPlayer == null)
                {
                    player = Instantiate(localPlayerPrefab).GetComponent<Player>();
                    player.transform.position = new Vector3(22, 0, 80);
                    GlobalVariables.localPlayer = player;
                    player.tag = StringAssets.localPlayerTag;
                }
                break;
            case PlayerType.LocalAI:
                if (GlobalVariables.hostType == HostType.Master)
                {
                    player = Instantiate(localAIPrefab).GetComponent<Player>();
                    player.tag = StringAssets.AIPlayerTag;
                }
                break;
            case PlayerType.Remote:
                player = Instantiate(remotePlayerPrefab).GetComponent<Player>();
                //player.tag = StringAssets.remoteplayerTag;
                break;
            default:
                break;
        }

        if (player != null)
        {
            //player.transform.position = Vector3.zero;
            player.id = playerIdCount++;
            playerDict.Add(player.id, player);
        }
        return player;
    }

    public bool RemovePlayer(Player player)
    {
        if (playerDict.ContainsKey(player.id))
        {
            if(player == GlobalVariables.localPlayer)
            {
                GlobalVariables.localPlayer = null;
            }
            Destroy(playerDict[player.id]);
            playerDict.Remove(player.id);
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

    void ClearAllPlayer()
    {
        foreach (Player p in playerDict.Values)
        {
            Destroy(p);
        }
        playerDict.Clear();
    }
}
