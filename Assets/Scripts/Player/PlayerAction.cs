using UnityEngine;
using System.Collections;
using System;

//带硬直的动作,都在这里处理
public partial class Player
{
    //动作硬直类型
    public enum ActionDelayType
    {
        Empty,
        Upper,
        Whole,
    }
    ActionDelayType actionDelay = ActionDelayType.Empty;
    public void SetActionDelay(ActionDelayType delay)
    {
        actionDelay = delay;
    }

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

    protected bool canInputAction = true;
    ActionType curActionType = ActionType.Empty;
    void IntoAction(ActionType actionType)
    {
        //结束当前动作
        if(curActionType != ActionType.Empty)
            actions[(int)curActionType].Stop();
        //开始新动作
        curActionType = actionType;
        actions[(int)actionType].Start(this);
        canInputAction = false;
    }

    void SimulateAction()
    {
        if (curActionType != ActionType.Empty)
        {
            actions[(int)curActionType].Update();
        }
    }

    void OnActionDone()
    {
        actions[(int)curActionType].Stop();
        curActionType = ActionType.Empty;
        canInputAction = true;
    }
    

    protected bool inAction { get { return this.curActionType != ActionType.Empty; } }
}
