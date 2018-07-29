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

    DateTime lastSwingTime = DateTime.Now;
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
                TimeSpan span = DateTime.Now - lastSwingTime;
                if (span.TotalMilliseconds > 250f)
                {
                    int r = UnityEngine.Random.Range(1, 3);
                    AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "sword" + r, typeof(AudioClip));
                    player.audioSource.PlayOneShot(clip, 0.8f);
                    lastSwingTime = DateTime.Now;
                }

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
        
        Player target = UnityHelper.FindObjectUpward<Player>(collision.collider.transform); 
        if (target != null)
        {
            if (target.id != this.player.id)
            {
                bool alreadyHit = false;
                foreach (Player p in hitList)
                {
                    if (target == p)
                    {
                        alreadyHit = true;
                        break;
                    }
                }
                if (!alreadyHit)
                {
                    hitList.Add(target);
                    controller.HitOtherPlayer(target, collision.contacts[0].point, 200f);
                }
            }
        }
    }

    
}
