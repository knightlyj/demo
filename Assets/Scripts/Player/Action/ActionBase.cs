using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public enum PlayerAniEventType
{
    Finish,
    StartAttack,
    StopAttack,

}

public class ActionBase
{
    protected Player player = null;
    virtual public void Start(Player player)
    {
        this.player = player;
    }

    virtual public void Update()
    {
        
    }

    virtual public void Stop()
    {

    }

    virtual public void OnAnimationEvent(string aniName, PlayerAniEventType aniEvent)
    {

    }

    virtual public void OnMainHandTrig(Collider other)
    {

    }

    virtual public void OnOffHandTrig(Collider other)
    {

    }

    public UnityAction onActionDone;
}
