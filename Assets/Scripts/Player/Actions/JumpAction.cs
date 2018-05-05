using UnityEngine;
using System.Collections;

public partial class Player
{
    public class JumpAction : ActionBase
    {
        float jumpYaw = 0;
        public override void Start(Player player)
        {
            base.Start(player);
            player.AnimateJump();  //设置动画
            player.SetActionDelay(Player.ActionDelayType.Whole);
            jumpYaw = player.orientation;
        }

        bool jumped = false;
        int upForceCount = 0;
        public override void Update()
        {
            base.Update();
            if (!jumped)
            {
                Vector3 moveDir = Quaternion.AngleAxis(jumpYaw, Vector3.up) * Vector3.forward;
                float speedOnDir = moveDir.x * player.rigidBody.velocity.x + moveDir.z * player.rigidBody.velocity.z;
                player.rigidBody.AddForce(moveDir * Player.moveForce);
                if (speedOnDir > Player.runSpeed)
                {
                    player.rigidBody.velocity = moveDir * Player.runSpeed;
                }
            }
            else
            {
                if (upForceCount > 0)
                {
                    player.rigidBody.AddForce(new Vector3(0, Player.jumpForce, 0));
                    upForceCount--;
                }
            }
        }

        public override void OnAnimationEvent(AnimationEvent aniEvent)
        {
            if (aniEvent.stringParameter.Equals("JumpUp"))
            {
                Vector3 moveDir = Quaternion.AngleAxis(player.orientation, Vector3.up) * Vector3.forward;
                player.rigidBody.velocity = new Vector3(moveDir.x * Player.runSpeed * 1.2f, player.rigidBody.velocity.y, moveDir.z * Player.runSpeed * 1.2f);
                player.rigidBody.AddForce(new Vector3(0, Player.jumpForce, 0));
                upForceCount = 3;
                jumped = true;
            }
        }
    }
}