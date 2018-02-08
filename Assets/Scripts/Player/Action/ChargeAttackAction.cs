using UnityEngine;
using System.Collections;

public class ChargeAttackAction : ActionBase {

    public override void Start(Player player)
    {
        base.Start(player);

        if (player.target != null) //朝向目标
        {
            Vector3 toTarget = player.target.transform.position - player.transform.position;
            toTarget.y = 0;

            float aCos = Mathf.Acos(toTarget.z / toTarget.magnitude);
            float yaw =  aCos / Mathf.PI * 180;
            if (toTarget.x < 0)
                yaw = -yaw;

            player.orientation = yaw;
        }

        player.aniModule.SetAnimation(PlayerAniType.Charge);
        chargeComplete = false;
        attack = false;
    }

    bool attack = false;
    public override void Update()
    {
        if (chargeComplete)
        {
            if (!player.input.strongAttack && !attack)
            {
                player.aniModule.SetAnimation(PlayerAniType.ChargeAttack);
                attack = true;
            }
        }
    }
    
    bool chargeComplete = false;
    public override void OnAnimationEvent(string aniName, PlayerAniEventType aniEvent)
    {
        if (aniEvent == PlayerAniEventType.Finish)
        {
            if (aniName == "Charge")
            {
                player.aniModule.SetAnimation(PlayerAniType.ChargeWait);
                chargeComplete = true;
            }
            else if(aniName == "ChargeAttack")
            {
                if (onActionDone != null)
                    onActionDone();
            }
        }
        else if (aniEvent == PlayerAniEventType.StartAttack)
        {
            player.EnableMainWeapon();  //开启武器碰撞
        }
        else if (aniEvent == PlayerAniEventType.StopAttack)
        {
            player.DisableMainWeapon(); //关掉武器碰撞
        }
    }

    public override void OnMainHandTrig(Collider other)
    {
        Player target = UnityHelper.FindObjectUpward<Player>(other.transform);
        if (target != null)
        {
            target.GetHit(player.transform.position, AttackType.ChargeAttack, 1);
        }
    }
}
