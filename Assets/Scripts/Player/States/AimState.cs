using UnityEngine;
using System.Collections;
using System;

public partial class Player
{
    public class AimState : StateBase
    {
        CameraControl cameraControl;
        public override void Start(Player player, System.Object param)
        {
            base.Start(player, param);
            cameraControl = GameObject.FindWithTag("MainCamera").GetComponent<CameraControl>();
            cameraControl.SwitchCameraMode(CameraControl.ViewMode.Shoot);
            player.SetUpperAniState(UpperAniState.Aim);
            player.shootIk = true;
        }

        DateTime lastShootTime = DateTime.Now;
        public override void FixedUpdate()
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
                else if (!player.input.rightHand)
                {  //退出瞄准状态
                    player.IntoState(StateType.Move);
                }
                else if (player.input.leftHand)
                {
                    TimeSpan span = DateTime.Now - lastShootTime;
                    if (span.TotalMilliseconds > 500)
                    {
                        player.SetUpperAniState(UpperAniState.Shoot);
                        lastShootTime = DateTime.Now;
                    }
                }
                else
                {
                    if (!player.input.hasDir) //没输入方向
                    {
                        player.SetLowerAniState(LowerAniState.Aim);
                        player.rigidBody.Sleep();
                    }
                    else
                    {
                        player.SetLowerAniState(LowerAniState.Strafe);

                        Vector3 moveDir = CommonHelper.DirOnPlane(player.groundNormal, player.yaw);
                        //按这种写法,分析时序后,速度是稳定的,如果不到最大速度才施加力的话,速度不会稳定
                        float speedOnDir = Vector3.Dot(moveDir, player.rigidBody.velocity);
                        player.rigidBody.AddForce(moveDir * moveForce);
                        if (speedOnDir > strafeSpeed)
                        {
                            player.rigidBody.velocity = moveDir * strafeSpeed;
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            player.immediateYaw = cameraControl.cameraYaw;
            float aimUpRatio = 1f - ((cameraControl.cameraPitch + 90f) / 180f);
            player.aimUp = aimUpRatio;
        }

        public override void OnStop()
        {
            base.OnStop();
            cameraControl = GameObject.FindWithTag("MainCamera").GetComponent<CameraControl>();
            cameraControl.SwitchCameraMode(CameraControl.ViewMode.Normal);
        }

        bool shooting = false;
        public override void OnAnimationEvent(AnimationEvent aniEvent)
        {
            if (aniEvent.animatorStateInfo.IsName("Shoot"))
            {
                if (aniEvent.stringParameter.Equals(AniEventName.Done))
                {
                    player.SetUpperAniState(UpperAniState.Aim);
                }
            }
            else
            { //不应该运行到这里
                Debug.LogError("MoveState.OnAnimationEvent >> unexpected animator state");
            }
        }
    }
}