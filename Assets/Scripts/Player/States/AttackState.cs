using UnityEngine;
using System.Collections;


public partial class Player
{
    public class AttackState : StateBase
    {
        string stateName = null;
        public override void Start(Player player, System.Object param)
        {
            base.Start(player, param);
            stateName = param as string;
            if (stateName == null)
                stateName = "Attack1";
            player.animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            player.animator.applyRootMotion = true;
            player.SetWholeAniState(WholeAniState.Attack, true, stateName);
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
        }
        
        public override void OnAnimationEvent(AnimationEvent aniEvent)
        {
            if (aniEvent.animatorStateInfo.IsName(stateName))
            {
                if (aniEvent.stringParameter.Equals(AniEventName.Done))
                {
                    player.IntoState(StateType.Move);
                }
                else if (aniEvent.stringParameter.Equals(""))
                {

                }
                else if(aniEvent.stringParameter.Equals(""))
                {

                }
            }
            else
            { //不应该运行到这里
                Debug.LogError("AttackState.OnAnimationEvent >> unexpected animator state");
            }
        }
    }
}