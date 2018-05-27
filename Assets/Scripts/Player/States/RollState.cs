using UnityEngine;
using System.Collections;

public partial class Player
{
    public class RollState : StateBase
    {
        Vector3 rollForceDir;
        public override void Start(Player player, System.Object param)
        {
            base.Start(player, param);
            player.yaw = player.input.yaw; //调整方向
            player.SetWholeAniState(WholeAniState.Roll, true);
            rollForceDir = Quaternion.Euler(10, player.yaw, 0) * Vector3.forward;
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

        public override void OnStop()
        {
            player.rigidBody.velocity = new Vector3(0, player.rigidBody.velocity.y, 0);
        }


        public override void OnAnimationEvent(AnimationEvent aniEvent)
        {
            if (aniEvent.animatorStateInfo.IsName("Roll"))
            {
                if (aniEvent.stringParameter.Equals(AniEventName.Done))
                {
                    player.IntoState(StateType.Move);
                }
            }
            else
            { //不应该运行到这里
                Debug.LogError("RollState.OnAnimationEvent >> unexpected animator state");
            }
        }
    }

}
