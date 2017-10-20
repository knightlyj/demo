using UnityEngine;
using System.Collections;

public class JumpAction : ActionBase {

	public override void Start(Player player)
    {
        base.Start(player);
        player.aniController.SetAnimation(PlayerAniType.JumpUp, PlayerAniDir.Front); //设置动画
        player.rigidBody.velocity = new Vector3(player.rigidBody.velocity.x, Player.jumpSpeed, player.rigidBody.velocity.z); //设置垂直速度

        //如果有按方向键,则设置方向和水平速度
        float moveSpeed = Player.walkSpeed;
        if (player.input.run)
        { //跑
            moveSpeed = Player.runSpeed;
        }
        if (player.input.hasDir)
        {
            player.orientation = player.input.yaw; //设置角色方向
            //设置水平速度
            Vector3 moveDir = Quaternion.AngleAxis(player.orientation, Vector3.up) * Vector3.forward;
            player.rigidBody.velocity = new Vector3(moveDir.x * moveSpeed, player.rigidBody.velocity.y, moveDir.z * moveSpeed);
        }
    }

    public override void OnAnimationEvent(string aniName, PlayerAniEventType aniEvent)
    {
        if (aniName.Equals("JumpUp") && aniEvent == PlayerAniEventType.Finish)
        {
            if (this.onActionDone != null)
                onActionDone();
        }
    }
}
