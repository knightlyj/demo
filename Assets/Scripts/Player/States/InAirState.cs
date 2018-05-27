using UnityEngine;
using System.Collections;
using System;

public partial class Player
{
    public class InAirState : StateBase
    {
        DateTime inAirTime = DateTime.Now;
        public override void Start(Player player, System.Object param)
        {
            base.Start(player, param);
            inAirTime = DateTime.Now;
        }

        public override void Update()
        {
            if (player.grounded)
            {
                player.IntoState(StateType.Move);
            }
            else
            {
                TimeSpan span = DateTime.Now - inAirTime;
                if (span.TotalMilliseconds > 200)
                {
                    if (player.rigidBody.velocity.y < 0) //下落
                    {
                        player.SetWholeAniState(WholeAniState.Fall);
                    }
                }
            }
            //if (player.input.hasDir)
            //{
            //    //空中给一点点水平移动的力
            //    Vector3 forceDir = Quaternion.Euler(0, 0, 0) * Vector3.forward;
            //    float speedOnDir = Vector3.Dot(forceDir, player.rigidBody.velocity);
            //    if (speedOnDir < moveSpeedInAir)
            //    {
            //        player.rigidBody.AddForce(forceDir * moveForceInAir);
            //    }
            //}
        }

        public override void OnAnimationEvent(AnimationEvent aniEvent)
        {
            //if (aniEvent.animatorStateInfo.IsName(""))
            //{
            //    if (aniEvent.stringParameter.Equals(""))
            //    {
            //    }
            //}
            //else
            //{ //不应该运行到这里
            //    Debug.LogError("InAirState.OnAnimationEvent >> unexpected animator state");
            //}
        }
    }
}