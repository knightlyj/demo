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

        AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "roll", typeof(AudioClip));
        player.audioSource.PlayOneShot(clip, 0.8f);


    }

    public override void FixedUpdate()
    {
        if (controller.grounded)
        {
            Vector3 moveDir = CommonHelper.DirOnPlane(controller.groundNormal, controller.faceYaw);
            float speedOnDir = Vector3.Dot(moveDir, controller.rigidBody.velocity);
            controller.rigidBody.AddForce(moveDir * Player.moveForce);
            if (speedOnDir > Player.rollSpeed)
            {
                controller.rigidBody.velocity = moveDir * Player.rollSpeed;
            }
        }
    }

    public override void OnStop()
    {
        player.invincible = false;
        controller.rigidBody.velocity = new Vector3(0, controller.rigidBody.velocity.y, 0);
    }
    
    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (aniEvent.animatorStateInfo.shortNameHash == Player.StateNameHash.roll)
        {
            if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.done))
            {
                controller.IntoState(PlayerStateType.Move);
            }
            else if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.invincible))
            {

                player.invincible = true;
            }
            else if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.notInvincible))
            {
                player.invincible = false;
            }
        }
    }

}


