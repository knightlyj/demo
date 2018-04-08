using UnityEngine;
using System.Collections;

public class RollAction : ActionBase {
    Vector3 rollForceDir;
    public override void Start(Player player)
    {
        base.Start(player);
        player.orientation = player.input.yaw; //调整方向
        player.aniModule.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
        rollForceDir = Quaternion.Euler(10, player.orientation, 0) * Vector3.forward;
    }

    public override void Update()
    {
        float curSpeed = player.rigidBody.velocity.magnitude;
        player.rigidBody.AddForce(rollForceDir * Player.moveForce * 2);
        if (curSpeed > Player.rollSpeed)
        {
            player.rigidBody.velocity = Player.rollSpeed / curSpeed * player.rigidBody.velocity;
        }
    }

    public override void Stop()
    {
        player.rigidBody.velocity = new Vector3(0, player.rigidBody.velocity.y, 0);
    }

    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (aniEvent.stringParameter.Equals("Roll") && aniEvent.intParameter == 0)
        {
            if (this.onActionDone != null)
                onActionDone();
        }
    }
}
