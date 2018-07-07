using UnityEngine;
using System.Collections;

public class JumpState : StateBase
{
    public override void Start(Player player, LocalPlayerController controller, System.Object param)
    {
        base.Start(player, controller, param);
        player.SetUpperAniState(Player.StateNameHash.jump, true);  //设置动画
        player.SetLowerAniState(Player.StateNameHash.jump, true);  //设置动画

        upForceCount = 5;

    }

    int upForceCount = 0;
    public override void FixedUpdate()
    {
        base.Update();
        if (upForceCount > 0)
        {
            controller.rigidBody.AddForce(new Vector3(0, Player.jumpForce, 0));
            upForceCount--;
        }
    }

    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (aniEvent.animatorStateInfo.IsName("Jump"))
        {
            if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.Done))
            {
                controller.IntoState(PlayerStateType.InAir);
            }
        }
    }

}
