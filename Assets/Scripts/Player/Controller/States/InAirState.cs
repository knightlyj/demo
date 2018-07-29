using UnityEngine;
using System.Collections;
using System;

public class InAirState : StateBase
{
    public override void Start(Player player, LocalPlayerController controller, System.Object param)
    {
        base.Start(player, controller, param);
    }

    public override void FixedUpdate()
    {
        if (controller.grounded)
        {
            AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "foot2", typeof(AudioClip));
            player.audioSource.PlayOneShot(clip, 1f);
            controller.IntoState(PlayerStateType.Move);
        }
        else
        {
            if (controller.rigidBody.velocity.y < 0) //下落
            {
                player.SetUpperAniState(Player.StateNameHash.fall, false, 0.5f);
                player.SetLowerAniState(Player.StateNameHash.fall, false, 0.5f);
            }
        }
    }

    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {

    }
    
}
