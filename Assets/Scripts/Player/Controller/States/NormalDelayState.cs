using UnityEngine;
using System.Collections;

public class NormalDelayState : StateBase
{
    int aniStateHash = 0;
    public override void Start(Player player, LocalPlayerController controller, System.Object param)
    {
        base.Start(player, controller, param);

        string actionName = param as string;
        if (actionName == null)
        {
            controller.IntoState(PlayerStateType.Move);
            return;
        }
        else if(actionName == "GetHitFront")
        {
            aniStateHash = Player.StateNameHash.getHitFront;
        }
        else if (actionName == "GetHitBack")
        {
            aniStateHash = Player.StateNameHash.getHitBack;
        }
        else if(actionName == "BlockGetHit")
        {
            aniStateHash = Player.StateNameHash.blockGetHit;
        }


        player.animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        player.animator.applyRootMotion = true;

        player.SetUpperAniState(aniStateHash, true);
        player.SetLowerAniState(aniStateHash, true);

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
        if (aniEvent.animatorStateInfo.shortNameHash == aniStateHash)
        {
            if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.done))
            {
                controller.IntoState(PlayerStateType.Move);
            }
        }
    }
}
