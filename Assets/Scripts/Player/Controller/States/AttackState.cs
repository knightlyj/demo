using UnityEngine;
using System.Collections;
using System;

public class AttackState : StateBase
{
    string actionName = null;
    public override void Start(Player player, LocalPlayerController controller, System.Object param)
    {
        base.Start(player, controller, param);
        actionName = param as string;
        if (actionName == null)
            actionName = "Attack1";
        player.animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        player.animator.applyRootMotion = true;
        if (actionName == "Attack2")
        {
            player.SetUpperAniState(Player.StateNameHash.attack2, true);
            player.SetLowerAniState(Player.StateNameHash.attack2, true);
        }
        else
        {
            player.SetUpperAniState(Player.StateNameHash.attack1, true);
            player.SetLowerAniState(Player.StateNameHash.attack1, true);
        }

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnStop()
    {
        base.OnStop();
        player.animator.updateMode = AnimatorUpdateMode.Normal;
        player.animator.applyRootMotion = false;
    }

    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (aniEvent.animatorStateInfo.IsName(actionName))
        {
            if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.Done))
            {
                controller.IntoState(PlayerStateType.Move);
                controller.lastAttackDoneTime = DateTime.Now;
            }
            else if (aniEvent.stringParameter.Equals(""))
            {

            }
            else if (aniEvent.stringParameter.Equals(""))
            {

            }
        }
    }

}
