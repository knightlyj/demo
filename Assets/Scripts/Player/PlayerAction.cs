using UnityEngine;
using System.Collections;
using System;

public partial class Player
{
    enum ActionType
    {
        Empty, Roll, Jump, Attack, JumpAttack, ChargeAttack, GetHit,
    }
    //各个动作优先级
    int[] ActionPriorities = {
        0,      1,    1,     1,    1,          1,            5
    };

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
        if (curActionType != ActionType.Empty)
        {  //当前有动作,且不是受击动作,则不打断
            if (actionType != ActionType.GetHit)
                return;
        }

        //如果之前有动作,则停止
        if (curActionType != ActionType.Empty)
        {
            actions[(int)curActionType].Stop();
        }

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
        curActionType = ActionType.Empty;
    }


    //动画完成的回调
    void OnAnimationDone(string aniName)
    {
        if (curActionType != ActionType.Empty)
        {
            actions[(int)curActionType].OnAnimationEvent(aniName, PlayerAniEventType.Finish);
        }
        //else if (action.Equals("Attack1"))
        //{
        //    attackComboTime = DateTime.Now;
        //}
        //else if (action.Equals("Attack2"))
        //{

        //}
        //else if (action.Equals("Charge"))
        //{
        //    if (input.strongAttack)
        //    {
        //        chargeComplete = true;
        //    }
        //    else
        //    {
        //        aniController.SetAnimation(PlayerAniType.ChargeAttack);
        //        StopCharge();
        //    }
        //}
    }
}
