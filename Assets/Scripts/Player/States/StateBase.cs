using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public partial class Player
{
    public class StateBase
    {
        protected Player player = null;
        virtual public void Start(Player player, System.Object param)
        {
            this.player = player;
            player.footIk = false;
            player.shootIk = false;
        }

        virtual public void Update()
        {

        }

        virtual public void FixedUpdate()
        {

        }

        virtual public void OnStop()
        {

        }

        virtual public void OnAnimationEvent(AnimationEvent aniEvent)
        {

        }

    }
}
