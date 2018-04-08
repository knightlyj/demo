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
        actions[(int)ActionType.Attack] = new AttackAction();
        actions[(int)ActionType.JumpAttack] = new JumpAttackAction();
        actions[(int)ActionType.ChargeAttack] = new ChargeAttackAction();
        actions[(int)ActionType.GetHit] = new GetHitAction();

        foreach (ActionBase action in actions)
        {
            if (action != null)
                action.onActionDone = this.OnActionDone;
        }
    }

    ActionType curActionType = ActionType.Empty;
    void IntoAction(ActionType actionType)
    {
        //结束当前动作
        if(curActionType != ActionType.Empty)
            actions[(int)curActionType].Stop();
        //开始新动作
        curActionType = actionType;
        actions[(int)actionType].Start(this);
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
    }

    //动画完成的回调
    void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (curActionType != ActionType.Empty)
        {
            actions[(int)curActionType].OnAnimationEvent(aniEvent);
        }
    }


    bool inAction { get { return this.curActionType != ActionType.Empty; } }
}
