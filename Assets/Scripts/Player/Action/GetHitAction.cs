using UnityEngine;
using System.Collections;

public class GetHitAction : ActionBase {

    public override void Start(Player player)
    {
        base.Start(player);
        EightDir dirEnum = EightDir.Front; ;
        Vector3 dir = player.transform.position;
        dir.y = 0;
        float frontProj = Vector3.Dot(player.transform.forward, dir);
        if (frontProj < 0)
        {
            dirEnum = EightDir.Back;
        }
        switch (dirEnum)
        {
            case EightDir.Front:
                player.aniModule.SetAnimation(PlayerAniType.GetHit, PlayerAniDir.Front);
                break;
            case EightDir.Back:
                player.aniModule.SetAnimation(PlayerAniType.GetHit, PlayerAniDir.Back);
                break;
        }
    }

    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        //if (aniName.Equals("GetHit") && aniEvent == PlayerAniEventType.Finish)
        //{

        //}
    }
}

