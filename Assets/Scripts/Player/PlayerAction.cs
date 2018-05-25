using UnityEngine;
using System.Collections;
using System;

//带硬直的动作,都在这里处理
public partial class Player
{
    enum ActionType
    {
        Empty, Roll, Jump, Attack, JumpAttack, ChargeAttack, GetHit,
    }

    ActionBase[] actions = null;
    void ActionInit()
    {
        Array a = Enum.GetValues(typeof(ActionType));
        actions = new ActionBase[a.Length];
        actions[(int)ActionType.Roll] = new RollAction();
        actions[(int)ActionType.Jump] = new JumpAction();
    }

    ActionType curActionType = ActionType.Empty;
    void IntoAction(ActionType actionType)
    {
        //结束当前动作
        if (curActionType != ActionType.Empty)
        {
            actions[(int)curActionType].OnStop();
            onAnimationEvent -= actions[(int)curActionType].OnAnimationEvent;
        }
        //开始新动作
        curActionType = actionType;
        actions[(int)actionType].Start(this);
        onAnimationEvent += actions[(int)actionType].OnAnimationEvent;
    }

    void SimulateAction()
    {
        if (curActionType != ActionType.Empty)
        {
            actions[(int)curActionType].Update();
        }
    }

    void StopAction()
    {
        if (curActionType != ActionType.Empty)
        {
            actions[(int)curActionType].OnStop();
            onAnimationEvent -= actions[(int)curActionType].OnAnimationEvent;
            curActionType = ActionType.Empty;
        }
    }


    protected bool inAction { get { return this.curActionType != ActionType.Empty; } }
}
