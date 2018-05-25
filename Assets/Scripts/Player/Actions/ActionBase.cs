using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public partial class Player
{
    public class ActionBase
    {
        protected Player player = null;
        virtual public void Start(Player player)
        {
            this.player = player;
            player.footIk = false;
            player.shootIk = false;
        }

        virtual public void Update()
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
