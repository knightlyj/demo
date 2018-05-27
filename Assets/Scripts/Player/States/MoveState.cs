using UnityEngine;
using System.Collections;
using System;

public partial class Player
{
    public class MoveState : StateBase
    {
        public override void Start(Player player, System.Object param)
        {
            base.Start(player, param);
        }

        public override void Update()
        {
            if (!player.grounded)
            {   //不在地面
                player.IntoState(StateType.InAir);
            }
            else
            {  //在地面
                if (player.input.jump)
                {
                    if (player.EnergyCost(jumpEnergyCost) > 0)
                    {
                        player.IntoState(StateType.Jump);
                    }
                }
                else if (player.input.roll)
                {
                    if (player.EnergyCost(rollEnergyCost) > 0)
                        player.IntoState(StateType.Roll);
                }
                else if (player.input.rightHand)
                {
                    if(player.weaponType == WeaponType.Pistol)
                    {
                        player.IntoState(StateType.Aim);
                    }
                }
                else
                {
                    if (!player.input.hasDir) //没输入方向
                    {
                        player.SetLowerAniState(LowerAniState.Idle);
                        player.SetUpperAniState(UpperAniState.Idle);
                        player.rigidBody.Sleep();
                        player.moveSpeed = walkSpeed;  //idle时，重置移动速度
                        player.footIk = true;
                    }
                    else
                    {
                        player.footIk = false;
                        player.yaw = player.input.yaw;
                        //在移动时按下run，移动速度会逐渐递增到跑步速度
                        if (player.input.run && player.cantRunTime <= 0)
                        { //跑
                            player.moveSpeed += (runSpeed - walkSpeed) * 0.05f;
                            if (player.moveSpeed >= runSpeed)
                                player.moveSpeed = runSpeed;
                            player.EnergyCost(runEnergyCost * Time.fixedDeltaTime); //消耗精力
                            player.lastRunTime = DateTime.Now;
                        }
                        else
                        { //走
                            player.moveSpeed -= (runSpeed - walkSpeed) * 0.05f;
                            if (player.moveSpeed <= walkSpeed)
                                player.moveSpeed = walkSpeed;
                        }

                        player.walkRun = (player.moveSpeed - walkSpeed) / (runSpeed - walkSpeed);

                        player.SetUpperAniState(UpperAniState.Move);
                        player.SetLowerAniState(LowerAniState.Move);

                        Vector3 moveDir = CommonHelper.DirOnPlane(player.groundNormal, player.yaw);
                        //Debug.Log(Vector3.Dot(moveDir, groundNormal));
                        //按这种写法,分析时序后,速度是稳定的,如果不到最大速度才施加力的话,速度不会稳定
                        float speedOnDir = Vector3.Dot(moveDir, player.rigidBody.velocity);
                        player.rigidBody.AddForce(moveDir * moveForce);
                        if (speedOnDir > player.moveSpeed)
                        {
                            player.rigidBody.velocity = moveDir * player.moveSpeed;
                        }
                    }
                }
            }
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
            //    Debug.LogError("MoveState.OnAnimationEvent >> unexpected animator state");
            //}
        }
    }

}