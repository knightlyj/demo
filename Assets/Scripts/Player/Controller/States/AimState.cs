using UnityEngine;
using System.Collections;
using System;

public class AimState : StateBase
{
    CameraControl cameraControl;

    public override void Start(Player player, LocalPlayerController controller, System.Object param)
    {
        base.Start(player, controller, param);
        cameraControl = GameObject.FindWithTag("MainCamera").GetComponent<CameraControl>();
        cameraControl.SwitchCameraMode(CameraControl.ViewMode.Shoot);
        player.targetId = -1;

        player.SetUpperAniState(Player.StateNameHash.aim);
        player.shootIk = true;
        targetStrafeForward = 0f;
        targetStrafeRight = 0f;
        inAirTime = 0;

        if (player.playerType == PlayerType.Local)
        {
            UIManager um = UnityHelper.GetUIManager();
            if (um != null)
            {
                um.showFrontSight = true;
            }
        }
    }

    DateTime lastShootTime = DateTime.Now;
    //strafe的动画参数,做平滑插值用
    float targetStrafeForward = 0f;
    float targetStrafeRight = 0f;
    //空中时间,持续一定时间才切换状态
    float inAirTime = 0;
    public override void FixedUpdate()
    {
        if (!controller.grounded)
        {   //不在地面
            inAirTime += Time.fixedDeltaTime;
            if (inAirTime > 0.5f)
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
            else if (!controller.input.offHand)
            {  //退出瞄准状态
                controller.IntoState(PlayerStateType.Move);
            }
            else if (controller.input.mainHand)
            {
                TimeSpan span = DateTime.Now - lastShootTime;
                if (span.TotalMilliseconds > 300)
                {
                    Shoot();
                    lastShootTime = DateTime.Now;
                }
            }
            else
            {
                if (!controller.input.hasMove) //没输入方向
                {
                    player.SetLowerAniState(Player.StateNameHash.aim);
                    controller.rigidBody.Sleep();
                    targetStrafeForward = 0f;
                    targetStrafeRight = 0f;
                }
                else
                {
                    float moveYaw = controller.input.moveYaw;
                    float aimYaw = controller.aimYaw;
                    float diffYaw = CommonHelper.AngleDiffClosest(moveYaw, aimYaw);
                    targetStrafeForward = 1 - Mathf.Abs(diffYaw) / 180f * 2f;
                    diffYaw = CommonHelper.AngleDiffClosest(moveYaw, aimYaw + 90f);
                    targetStrafeRight = 1 - Mathf.Abs(diffYaw) / 180f * 2f;

                    player.SetLowerAniState(Player.StateNameHash.aimStrafe, false, 1.5f);
                    player.strafeForward = CommonHelper.FloatTowards(player.strafeForward, targetStrafeForward, Time.fixedDeltaTime * 5f);
                    player.strafeRight = CommonHelper.FloatTowards(player.strafeRight, targetStrafeRight, Time.fixedDeltaTime * 5f);

                    Vector3 moveDir = CommonHelper.DirOnPlane(controller.groundNormal, moveYaw);
                    float speedOnDir = Vector3.Dot(moveDir, controller.rigidBody.velocity);
                    controller.rigidBody.AddForce(moveDir * Player.moveForce);
                    if (speedOnDir > Player.strafeSpeed)
                    {
                        controller.rigidBody.velocity = moveDir * Player.strafeSpeed;
                    }
                }
            }
        }
    }

    public override void Update()
    {
        base.Update();
        controller.immediateYaw = controller.aimYaw;
        float aimUpRatio = 1f - ((controller.aimPitch + 90f) / 180f);
        player.aimUp = aimUpRatio;
    }

    public override void OnStop()
    {
        base.OnStop();
        cameraControl = GameObject.FindWithTag("MainCamera").GetComponent<CameraControl>();
        cameraControl.SwitchCameraMode(CameraControl.ViewMode.Normal);

        if (player.playerType == PlayerType.Local)
        {
            UIManager um = UnityHelper.GetUIManager();
            if (um != null)
            {
                um.showFrontSight = false;
            }
        }
    }

    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (aniEvent.animatorStateInfo.shortNameHash == Player.StateNameHash.shoot)
        {
            if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.done))
            {
                player.SetUpperAniState(Player.StateNameHash.aim);
            }
        }
    }

    void Shoot()
    {
        if (player.gun != null)
        {
            player.SetUpperAniState(Player.StateNameHash.shoot);
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 targetPoint;
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 100f, player.hitLayer))
            {
                targetPoint = hitInfo.point;

            }
            else
            {
                targetPoint = Camera.main.transform.position + ray.direction * 100f;
            }
            
            if (player.gun.Shoot(targetPoint, player.hitLayer, out hitInfo))
            {
                LevelManager lm = UnityHelper.GetLevelManager();
                Collider collider = hitInfo.collider;
                if (collider.gameObject.layer == LayerMask.NameToLayer(StringAssets.bodyLayerName))
                {
                    Player p = UnityHelper.FindObjectUpward<Player>(collider.transform);
                    if (p != null)
                    {
                        if (lm != null)
                            lm.CreateParticleEffect(LevelManager.ParticleEffectType.HitPlayer, hitInfo.point, hitInfo.normal);
                        controller.HitOtherPlayer(p, hitInfo.point, 20f);
                    }
                    else
                    {
                        Debug.LogError("AimState.Shoot >> hit body, but can't find Player component");
                    }
                }
                else if (collider.gameObject.layer == LayerMask.NameToLayer(StringAssets.groundLayerName))
                {
                    if (lm != null)
                        lm.CreateParticleEffect(LevelManager.ParticleEffectType.HitGround, hitInfo.point, hitInfo.normal);
                }
            }

            if (GlobalVariables.hostType == HostType.Server)
            {
                ServerAgent sa = UnityHelper.GetServerAgent();
                sa.SendShoot(player.id, targetPoint);
            }
            else if(GlobalVariables.hostType == HostType.Client)
            {
                ClientAgent ca = UnityHelper.GetClientAgent();
                ca.SendShoot(player.id, targetPoint);
            }
        }
    }

}
