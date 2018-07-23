using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DieState : StateBase
{
    public override void Start(Player player, LocalPlayerController controller, System.Object param)
    {
        base.Start(player, controller, param);
        player.SetUpperAniState(Player.StateNameHash.die, true);  //设置动画
        player.SetLowerAniState(Player.StateNameHash.die, true);  //设置动画
        Collider col = player.GetComponent<Collider>();
        col.isTrigger = true;
        controller.rigidBody.isKinematic = true;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnStop()
    {
        base.OnStop();
        Collider col = player.GetComponent<Collider>();
        col.isTrigger = false;
        controller.rigidBody.isKinematic = false;
    }

    void OnMsgBox(bool confirm)
    {
        if (confirm)
        {
            player.healthPoint = Player.maxHealth;
        }
        else
        {
            SceneManager.LoadScene(StringAssets.mainMenuSceneName);
        }
    }


    public override void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (aniEvent.animatorStateInfo.shortNameHash == Player.StateNameHash.die)
        {
            if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.done))
            {
                if (player.playerType == PlayerType.Local)
                {
                    UIManager um = UnityHelper.GetUIManager();
                    um.MessageBox("You Died", true, this.OnMsgBox, new string[] { "重生", "退出" });
                }
                else if (player.playerType == PlayerType.LocalAI)
                {
                    player.healthPoint = Player.maxHealth;
                }
            }
        }
    }
}
