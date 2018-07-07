using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public enum PlayerStateType
{
    Empty, Move, Aim, Roll, Jump, InAir, Attack, GetHit,
}

public class StateBase
{
    protected Player player = null;
    protected LocalPlayerController controller = null;
    virtual public void Start(Player player, LocalPlayerController controller, System.Object param)
    {
        this.player = player;
        this.controller = controller;
        player.footIk = false;
        player.shootIk = false;
    }

    virtual public void Update()
    {

    }

    virtual public void FixedUpdate()
    {

    }

    virtual public void OnStop()
    {

    }

    virtual public void OnAnimationEvent(AnimationEvent aniEvent)
    {

    }
    
}

