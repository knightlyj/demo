using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(FadingAudioSource))]
public class BGMController : MonoBehaviour {

    [SerializeField]
    FadingAudioSource fadingAudioSource = null;

    AudioClip normalClip = null;
    AudioClip[] fightClip = null;

    float bgmVolum = 0.5f;
	// Use this for initialization
	void Start () {
        normalClip = (AudioClip)Resources.Load(StringAssets.bgmPath + "Normal", typeof(AudioClip));
        fightClip = new AudioClip[2];
        fightClip[0] = (AudioClip)Resources.Load(StringAssets.bgmPath + "Fight1", typeof(AudioClip));
        fightClip[1] = (AudioClip)Resources.Load(StringAssets.bgmPath + "Fight2", typeof(AudioClip));

        fadingAudioSource.Fade(normalClip, bgmVolum, true);
    }

    int playerId = -1;
    bool fighting = false;
    float switchTime = 10f;
	// Update is called once per frame
	void Update () {
	    if(playerId < 0)
        {
            Player player = GlobalVariables.localPlayer;
            if(player != null)
            {
                playerId = player.id;
                EventManager.AddListener(EventId.PlayerDamage, playerId, this.OnPlayerDamage);
                EventManager.AddListener(EventId.RemovePlayer, playerId, this.OnPlayerDestroy);
            }
        }
        if (fighting)
        {
            TimeSpan span = DateTime.Now - lastDamageTime;
            if (span.TotalSeconds > switchTime)
            {
                fadingAudioSource.Fade(normalClip, bgmVolum, true);
                fighting = false;
            }
        }
	}

    void OnDestroy()
    {
        EventManager.RemoveListener(EventId.PlayerDamage, playerId, this.OnPlayerDamage);
        EventManager.RemoveListener(EventId.RemovePlayer, playerId, this.OnPlayerDestroy);
    }

    DateTime lastDamageTime = DateTime.Now;
    void OnPlayerDamage(System.Object sender, System.Object eventArg)
    {
        lastDamageTime = DateTime.Now;
        if (!fighting)
        {
            fighting = true;
            int r = UnityEngine.Random.Range(0, fightClip.Length);
            fadingAudioSource.Fade(fightClip[r], bgmVolum, true);
        }
    }

    void OnPlayerDestroy(System.Object sender, System.Object eventArg)
    {
        EventManager.RemoveListener(EventId.PlayerDamage, playerId, this.OnPlayerDamage);
        EventManager.RemoveListener(EventId.RemovePlayer, playerId, this.OnPlayerDestroy);
    }
}
