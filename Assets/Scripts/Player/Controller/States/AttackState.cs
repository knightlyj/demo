using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AttackState : StateBase
{
    string actionName = null;
    WeaponCollision wc = null;
    public override void Start(Player player, LocalPlayerController controller, System.Object param)
    {
        base.Start(player, controller, param);

        actionName = param as string;
        if (actionName == null)
            actionName = "Attack1";
        player.animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        player.animator.applyRootMotion = true;
        if (actionName == "Attack2")
        {
            player.SetUpperAniState(Player.StateNameHash.attack2, true);
            player.SetLowerAniState(Player.StateNameHash.attack2, true);
        }
        else
        {
            player.SetUpperAniState(Player.StateNameHash.attack1, true);
            player.SetLowerAniState(Player.StateNameHash.attack1, true);
        }

        wc = player.weaponCollision;
        if (wc != null)
        {
            wc.onHit += this.OnWeaponHit;
        }

        hitList.Clear();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnStop()
    {
        base.OnStop();
        player.animator.updateMode = AnimatorUpdateMode.Normal;
        player.animator.applyRootMotion = false;

        if (wc != null)
        {
            wc.onHit -= this.OnWeaponHit;
        }
    }

    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (aniEvent.animatorStateInfo.IsName(actionName))
        {
            if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.done))
            {
                controller.IntoState(PlayerStateType.Move);
                controller.lastAttackDoneTime = DateTime.Now;
            }
            else if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.startAttack))
            {
                if (wc != null)
                    wc.colliderEanbled = true;
            }
            else if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.endAttack))
            {
                if (wc != null)
                    wc.colliderEanbled = false;
            }
        }
    }
    
    List<Player> hitList = new List<Player>();
    void OnWeaponHit(Collision collision)
    {
        
        Player player = UnityHelper.FindObjectUpward<Player>(collision.collider.transform); 
        if (player != null)
        {
            if (player.playerType != PlayerType.Local)
            {
                bool alreadyHit = false;
                foreach (Player p in hitList)
                {
                    if (player == p)
                    {
                        alreadyHit = true;
                        break;
                    }
                }
                if (!alreadyHit)
                {
                    hitList.Add(player);
                    controller.HitOtherPlayer(player, collision.contacts[0].point, 200f);
                }
            }
        }
    }

    
}
