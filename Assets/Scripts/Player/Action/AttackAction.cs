using UnityEngine;
using System.Collections;
using System;

public class AttackAction : ActionBase
{
    DateTime attackComboTime = DateTime.Now;
    public override void Start(Player player)
    {
        base.Start(player);

        if (player.target != null)  //朝向目标
        {
            Vector3 toTarget = player.target.transform.position - player.transform.position;
            toTarget.y = 0;

            float aCos = Mathf.Acos(toTarget.z / toTarget.magnitude);
            float yaw = aCos / Mathf.PI * 180;
            if (toTarget.x < 0)
                yaw = -yaw;

            player.orientation = yaw;
        }

        TimeSpan span = DateTime.Now - attackComboTime;
        if (span.TotalMilliseconds < 100)
        {
            player.aniModule.SetAnimation(PlayerAniType.Attack2);
        }
        else
        {
            player.aniModule.SetAnimation(PlayerAniType.Attack1);
        }
    }

    public override void Stop()
    {
        player.DisableMainWeapon(); //关掉武器碰撞
    }

    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        //if (aniEvent == PlayerAniEventType.StartAttack)
        //{
        //    player.EnableMainWeapon();  //开启武器碰撞
        //}
        //else if (aniEvent == PlayerAniEventType.StopAttack)
        //{
        //    player.DisableMainWeapon(); //关掉武器碰撞
        //}
        //else if (aniEvent == PlayerAniEventType.Finish)
        //{
        //    if (aniName == "Attack1")
        //        attackComboTime = DateTime.Now;

        //    Stop();
        //}
    }

    public override void OnMainHandTrig(Collider other)
    {

    }


}
