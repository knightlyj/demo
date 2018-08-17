using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocalPlayerPanel : MonoBehaviour {
    [SerializeField]
    Bar hpBar;
    [SerializeField]
    Bar energyBar;
    [SerializeField]
    Text txtName;
    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        Init();
        
        if (localPlayer != null)
        {
            float ratio = localPlayer.healthPoint / Player.maxHealth;
            if (ratio < 0)
                ratio = 0f;
            else if (ratio > 1)
                ratio = 1f;
            hpBar.SetRatio(ratio);

            ratio = localPlayer.energyPoint / Player.maxEnergy;
            if (ratio < 0)
                ratio = 0f;
            else if (ratio > 1)
                ratio = 1f;
            energyBar.SetRatio(ratio);
        }
    }

    Player localPlayer = null;
    int playerId = -1;
    bool initialized = false;
    void Init()
    {
        if (!initialized)
        {
            localPlayer = GlobalVariables.localPlayer;
            if (localPlayer != null)
            {
                if (GlobalVariables.hostType == HostType.Server)
                {
                    txtName.text = localPlayer.nameInGame + " 主机 " + Server.localIp + ": " + Server.localPort;
                }
                else if(GlobalVariables.hostType == HostType.Client)
                {
                    txtName.text = localPlayer.nameInGame + " 非主机";
                }

                playerId = localPlayer.id;
                EventManager.AddListener(EventId.RemovePlayer, playerId, this.OnPlayerDestroy);

                initialized = true;
            }
        }
    }

    void OnDestroy()
    {
        EventManager.RemoveListener(EventId.RemovePlayer, playerId, this.OnPlayerDestroy);
        localPlayer = null;
        playerId = -1;
    }


    void OnPlayerDestroy(object sender, object eventArg)
    {
        EventManager.RemoveListener(EventId.RemovePlayer, playerId, this.OnPlayerDestroy);
        localPlayer = null;
        playerId = -1;
    }
}
