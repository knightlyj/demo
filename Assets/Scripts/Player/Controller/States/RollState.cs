using UnityEngine;
using System.Collections;

public class RollState : StateBase
{
    Vector3 rollForceDir;
    public override void Start(Player player, LocalPlayerController controller, System.Object param)
    {
        base.Start(player, controller, param);
        controller.faceYaw = controller.input.moveYaw; //调整方向
        player.SetUpperAniState(Player.StateNameHash.roll, true);
        player.SetLowerAniState(Player.StateNameHash.roll, true);
        rollForceDir = Quaternion.Euler(10, controller.faceYaw, 0) * Vector3.forward;
    }

    public override void FixedUpdate()
    {
        float curSpeed = controller.rigidBody.velocity.magnitude;
        controller.rigidBody.AddForce(rollForceDir * Player.moveForce * 2);
        if (curSpeed > Player.rollSpeed)
        {
            controller.rigidBody.velocity = Player.rollSpeed / curSpeed * controller.rigidBody.velocity;
        }
    }

    public override void OnStop()
    {
        controller.rigidBody.velocity = new Vector3(0, controller.rigidBody.velocity.y, 0);
    }


    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (aniEvent.animatorStateInfo.IsName("Roll"))
        {
            if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.Done))
            {
                controller.IntoState(PlayerStateType.Move);
            }
        }
    }

}


