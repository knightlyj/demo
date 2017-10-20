using UnityEngine;
using System.Collections;

public class RollAction : ActionBase {
    Vector3 rollForceDir;
    public override void Start(Player player)
    {
        base.Start(player);
        player.orientation = player.input.yaw; //调整方向
        player.aniController.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
        rollForceDir = Quaternion.Euler(10, player.orientation, 0) * Vector3.forward;
    }

    public override void Update()
    {
        float curSpeed = player.rigidBody.velocity.magnitude;
        if (curSpeed < Player.rollSpeed)
        {
            player.rigidBody.AddForce(rollForceDir * Player.moveForce);
        }
        else
        {
            player.rigidBody.velocity = Player.rollSpeed / curSpeed * player.rigidBody.velocity;
        }
    }

    public override void Stop()
    {
        player.rigidBody.velocity = new Vector3(0, player.rigidBody.velocity.y, 0);
    }

    public override void OnAnimationEvent(string aniName, PlayerAniEventType aniEvent)
    {
        if (aniName.Equals("Roll") && aniEvent == PlayerAniEventType.Finish)
        {
            Stop();
            if (this.onActionDone != null)
                onActionDone();
        }
    }
}
