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

        IntoState(PlayerStateType.Move, null);
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
        public static readonly string Done = "Done";
    }

}
