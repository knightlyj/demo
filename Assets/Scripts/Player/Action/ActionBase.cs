using UnityEngine;
using System.Collections;
using UnityEngine.Events;

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

    virtual public void OnAnimationEvent(AnimationEvent aniEvent)
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
