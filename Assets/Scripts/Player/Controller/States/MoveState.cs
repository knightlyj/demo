using UnityEngine;
using System.Collections;
using System;

public class MoveState : StateBase
{
    enum UpperAction
    {
        Empty,
        Block,
        SwapWeapon,
    }

    public override void Start(Player player, LocalPlayerController controller, System.Object param)
    {
        base.Start(player, controller, param);
        inAirTime = 0;
        targetStrafeForward = 0f;
        targetStrafeRight = 0f;
        upperAction = UpperAction.Empty;
    }

    float targetStrafeForward = 0f;
    float targetStrafeRight = 0f;
    float inAirTime = 0;

    UpperAction upperAction = UpperAction.Empty;
    WeaponType nextWeapon = WeaponType.Empty;
    public override void FixedUpdate()
    {
        if (!controller.grounded)
        {   //不在地面
            inAirTime += Time.fixedDeltaTime;
            if (inAirTime > 0.2f)
                controller.IntoState(PlayerStateType.InAir);
        }
        else
        {  //在地面
            inAirTime = 0;
            if (controller.input.jump)
            {
                if (controller.EnergyCost(Player.jumpEnergyCost) > 0)
                {
                    controller.IntoState(PlayerStateType.Jump);
                }
            }
            else if (controller.input.roll)
            {
                if (controller.EnergyCost(Player.rollEnergyCost) > 0)
                    controller.IntoState(PlayerStateType.Roll);
            }
            else if (controller.input.mainHand)
            {
                if (player.weaponType == WeaponType.Melee)
                {
                    if (controller.EnergyCost(Player.attackEnergyCost) > 0)
                    {
                        if (controller.comboCount == 1)
                        {
                            controller.IntoState(PlayerStateType.Attack, "Attack1");
                            controller.comboCount = 0;
                        }
                        else if (controller.comboCount == 0)
                        {
                            TimeSpan span = DateTime.Now - controller.lastAttackDoneTime;
                            if (span.TotalMilliseconds < 300)
                            {
                                controller.IntoState(PlayerStateType.Attack, "Attack2");
                                controller.comboCount = 1;
                            }
                            else
                            {
                                controller.IntoState(PlayerStateType.Attack, "Attack1");
                            }
                        }
                    }
                }
            }
            else if (controller.input.offHand && player.weaponType == WeaponType.Pistol)
            {
                controller.IntoState(PlayerStateType.Aim);
            }
            else
            {
                if (controller.input.swapWeapon) //切换武器
                {
                    if (upperAction != UpperAction.SwapWeapon)
                    {
                        if (player.weaponType == WeaponType.Melee)
                            nextWeapon = WeaponType.Pistol;
                        else
                            nextWeapon = WeaponType.Melee;

                        player.SetUpperAniState(Player.StateNameHash.swapWeapon);
                        upperAction = UpperAction.SwapWeapon;
                    }
                }
                if (controller.input.offHand
                                        && player.weaponType == WeaponType.Melee
                                        && upperAction == UpperAction.Empty)  //格挡
                {
                    player.blocking = true;
                }
                else
                {
                    player.blocking = false;
                }


                if (!controller.input.hasMove) //没输入方向,idle
                {
                    if (player.targetId > 0)
                    {
                        Player target = UnityHelper.GetLevelManager().GetPlayer(player.targetId);
                        controller.faceYaw = CommonHelper.YawOfVector3(target.transform.position - player.transform.position);
                    }

                    if (player.blocking)
                    {
                        if (upperAction == UpperAction.Empty)
                            player.SetLowerAniState(Player.StateNameHash.blockIdle);
                        player.SetUpperAniState(Player.StateNameHash.blockIdle);
                    }
                    else
                    {
                        if (upperAction == UpperAction.Empty)
                            player.SetUpperAniState(Player.StateNameHash.idle);
                        player.SetLowerAniState(Player.StateNameHash.idle);
                    }
                    controller.rigidBody.Sleep();
                    controller.moveSpeed = Player.walkSpeed;  //idle时，重置移动速度
                    player.footIk = true;
                }
                else  //输入了方向,则移动
                {
                    player.footIk = false;
                    if (player.targetId >= 0 && !controller.input.run)
                    {
                        StrafeMove();
                    }
                    else
                    {
                        NormalMove();  //普通移动
                    }

                }
            }
        }
    }

    void StrafeMove()
    {
        controller.moveSpeed = Player.walkSpeed;

        Player target = UnityHelper.GetLevelManager().GetPlayer(player.targetId);
        controller.faceYaw = CommonHelper.YawOfVector3(target.transform.position - player.transform.position);
        float moveYaw = controller.input.moveYaw;
        //strafe blend
        float aimYaw = controller.aimYaw;
        float diffYaw = CommonHelper.AngleDiffClosest(moveYaw, aimYaw);
        targetStrafeForward = 1 - Mathf.Abs(diffYaw) / 180f * 2f;
        diffYaw = CommonHelper.AngleDiffClosest(moveYaw, aimYaw + 90f);
        targetStrafeRight = 1 - Mathf.Abs(diffYaw) / 180f * 2f;

        player.strafeForward = CommonHelper.FloatTowards(player.strafeForward, targetStrafeForward, Time.fixedDeltaTime * 5f);
        player.strafeRight = CommonHelper.FloatTowards(player.strafeRight, targetStrafeRight, Time.fixedDeltaTime * 5f);

        PhysicsMove(moveYaw);

        if (upperAction == UpperAction.Empty)
        {
            if (player.blocking)
            {
                player.SetUpperAniState(Player.StateNameHash.blockIdle);
            }
            else
            {
                player.SetUpperAniState(Player.StateNameHash.meleeStrafe);
            }
        }
        player.SetLowerAniState(Player.StateNameHash.meleeStrafe);

    }

    void NormalMove()
    {
        controller.faceYaw = controller.input.moveYaw;
        //在移动时按下run，移动速度会逐渐递增到跑步速度
        if (controller.input.run && controller.cantRunTime <= 0)
        { //跑
            controller.moveSpeed += (Player.runSpeed - Player.walkSpeed) * 0.05f;
            if (controller.moveSpeed >= Player.runSpeed)
                controller.moveSpeed = Player.runSpeed;
            controller.EnergyCost(Player.runEnergyCost * Time.fixedDeltaTime); //消耗精力
            controller.lastRunTime = DateTime.Now;
        }
        else
        { //走
            controller.moveSpeed -= (Player.runSpeed - Player.walkSpeed) * 0.05f;
            if (controller.moveSpeed <= Player.walkSpeed)
                controller.moveSpeed = Player.walkSpeed;
        }

        player.walkRun = (controller.moveSpeed - Player.walkSpeed) / (Player.runSpeed - Player.walkSpeed);

        if (upperAction == UpperAction.Empty)
        {
            if (player.blocking)
            {
                player.SetUpperAniState(Player.StateNameHash.blockIdle);
            }
            else
            {
                player.SetUpperAniState(Player.StateNameHash.move);
            }
        }
        player.SetLowerAniState(Player.StateNameHash.move);

        PhysicsMove(controller.faceYaw);
    }

    void PhysicsMove(float yaw)
    {
        Vector3 moveDir = CommonHelper.DirOnPlane(controller.groundNormal, yaw);
        float speedOnDir = Vector3.Dot(moveDir, controller.rigidBody.velocity);
        controller.rigidBody.AddForce(moveDir * Player.moveForce);
        if (speedOnDir > controller.moveSpeed)
        {
            controller.rigidBody.velocity = moveDir * controller.moveSpeed;
        }
    }

    public override void OnStop()
    {
        base.OnStop();
        player.blocking = false;
    }

    DateTime lastStepTime = DateTime.Now;
    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (aniEvent.animatorStateInfo.shortNameHash == Player.StateNameHash.swapWeapon)
        {
            if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.swap))
            {
                player.ChangeWeapon(nextWeapon);
                if (nextWeapon == WeaponType.Pistol)
                    player.targetId = -1;
            }
            else if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.done))
            {
                player.UpperSuitLowerAnimation();
                nextWeapon = WeaponType.Empty;
                upperAction = UpperAction.Empty;
            }
        }

        //else if(aniEvent.animatorStateInfo.shortNameHash == Player.StateNameHash.move)
        {
            if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.step))
            {
                TimeSpan span = DateTime.Now - lastStepTime;
                if (span.TotalMilliseconds > 150f)
                {
                    int r = UnityEngine.Random.Range(1, 4);
                    AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "foot" + r, typeof(AudioClip));
                    player.audioSource.PlayOneShot(clip, 0.5f);
                    lastStepTime = DateTime.Now;
                }
            }
        }
    }
    
}

