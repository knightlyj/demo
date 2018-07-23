using UnityEngine;
using System.Collections;
using System;


public partial class LocalPlayerController
{
    StateBase[] states = null;
    void StateInit()
    {
        Array a = Enum.GetValues(typeof(PlayerStateType));
        states = new StateBase[a.Length];
        states[(int)PlayerStateType.Move] = new MoveState();
        states[(int)PlayerStateType.InAir] = new InAirState();
        states[(int)PlayerStateType.Attack] = new AttackState();
        states[(int)PlayerStateType.Aim] = new AimState();
        states[(int)PlayerStateType.Roll] = new RollState();
        states[(int)PlayerStateType.Jump] = new JumpState();
        states[(int)PlayerStateType.Die] = new DieState();
        states[(int)PlayerStateType.GetHit] = new NormalDelayState();

        IntoState(PlayerStateType.Move, null);

        EventManager.AddListener(EventId.PlayerDamage, player.id, this.OnPlayerDamage);
        EventManager.AddListener(EventId.PlayerDie, player.id, this.OnPlayerDie);
        EventManager.AddListener(EventId.PlayerRevive, player.id, this.OnPlayerRevive);
    }

    PlayerStateType curState = PlayerStateType.Empty;
    public void IntoState(PlayerStateType stateType, System.Object param = null)
    {
        //结束当前状态
        if (curState != PlayerStateType.Empty)
        {
            states[(int)curState].OnStop();
            player.onAnimationEvent -= states[(int)curState].OnAnimationEvent;
        }
        //开始新状态
        curState = stateType;
        states[(int)stateType].Start(player, this, param);
        player.onAnimationEvent += states[(int)stateType].OnAnimationEvent;
    }

    void FixedUpdateState()
    {
        if (curState != PlayerStateType.Empty)
        {
            states[(int)curState].FixedUpdate();
        }
        else
        {
            Debug.LogError("player state is empty");
        }
    }

    void UpdateState()
    {
        if (curState != PlayerStateType.Empty)
        {
            states[(int)curState].Update();
        }
        else
        {
            Debug.LogError("player state is empty");
        }
    }

    void StopState()
    {
        if (curState != PlayerStateType.Empty)
        {
            states[(int)curState].OnStop();
            player.onAnimationEvent -= states[(int)curState].OnAnimationEvent;
            curState = PlayerStateType.Empty;
        }
    }


    public static class AniEventName
    {
        public static readonly string done = "Done";
        public static readonly string startAttack = "StartAttack";
        public static readonly string endAttack = "EndAttack";
        public static readonly string swap = "Swap";
    }

    void OnPlayerDie(System.Object sender, System.Object eventArg)
    {
        IntoState(PlayerStateType.Die);
    }

    void OnPlayerRevive(System.Object sender, System.Object eventArg)
    {
        IntoState(PlayerStateType.Move);
    }

    void OnPlayerDamage(System.Object sender, System.Object eventArg)
    {
        Vector3 point = (Vector3)eventArg;
        Vector3 dir = point - transform.position;
        dir.y = 0f;
        float dot = Vector3.Dot(dir, transform.forward);
        if (dot > 0)
        {
            IntoState(PlayerStateType.GetHit, "GetHitFront");
        }
        else
        {
            IntoState(PlayerStateType.GetHit, "GetHitBack");
        }
    }
}
