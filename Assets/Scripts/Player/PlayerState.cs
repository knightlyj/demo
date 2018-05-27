using UnityEngine;
using System.Collections;
using System;

//带硬直的动作,都在这里处理
public partial class Player
{
    enum StateType
    {
        Empty, Move, Aim, Roll, Jump, InAir, Attack, GetHit,
    }

    StateBase[] states = null;
    void StateInit()
    {
        Array a = Enum.GetValues(typeof(StateType));
        states = new StateBase[a.Length];
        states[(int)StateType.Move] = new MoveState();
        states[(int)StateType.InAir] = new InAirState();
        states[(int)StateType.Aim] = new AimState();
        states[(int)StateType.Roll] = new RollState();
        states[(int)StateType.Jump] = new JumpState();

        IntoState(StateType.Move);
    }

    StateType curState = StateType.Empty;
    void IntoState(StateType stateType)
    {
        //结束当前状态
        if (curState != StateType.Empty)
        {
            states[(int)curState].OnStop();
            onAnimationEvent -= states[(int)curState].OnAnimationEvent;
        }
        //开始新状态
        curState = stateType;
        states[(int)stateType].Start(this, null);
        onAnimationEvent += states[(int)stateType].OnAnimationEvent;
    }

    void FixedUpdateState()
    {
        if (curState != StateType.Empty)
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
        if (curState != StateType.Empty)
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
        if (curState != StateType.Empty)
        {
            states[(int)curState].OnStop();
            onAnimationEvent -= states[(int)curState].OnAnimationEvent;
            curState = StateType.Empty;
        }
    }
    
}
