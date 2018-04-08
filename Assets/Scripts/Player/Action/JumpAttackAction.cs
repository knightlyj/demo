using UnityEngine;
using System.Collections;

public class JumpAttackAction : ActionBase {
    public override void Start(Player player)
    {
        base.Start(player);
        player.aniModule.SetAnimation(PlayerAniType.JumpAttack); //设置动画
        //player.rigidBody.velocity = new Vector3(player.rigidBody.velocity.x, Player.jumpSpeed * 0.6f, player.rigidBody.velocity.z); //设置垂直速度
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
        //    if (aniName == "JumpAttack")
        //    {
        //        Stop();
        //    }
        //}
    }

    public override void OnMainHandTrig(Collider other)
    {
        Player target = UnityHelper.FindObjectUpward<Player>(other.transform);
        if (target != null)
        {

        }
    }
}
