using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LocalAIInput : IPlayerInput
{
    enum Strategy
    {
        Attack,
        Defense,
    }

    Player player = null;
    public void Start(Player player, LocalPlayerController controller)
    {
        this.player = player;
        if (player)
            EventManager.AddListener(EventId.PlayerDamage, player.id, this.OnDamage);
    }

    public void Stop()
    {
        if (player)
            EventManager.AddListener(EventId.PlayerDamage, player.id, this.OnDamage);
    }

    Strategy stratgy = Strategy.Defense;

    public void UpdateInput(ref GameInput input, LocalPlayerController controller)
    {
        stratgyTimer -= Time.deltaTime;
        if (stratgyTimer < 0f)
        {
            stratgyTimer = UnityEngine.Random.Range(2f, 5f);
            if (stratgy == Strategy.Defense)
                stratgy = Strategy.Attack;
            else
                stratgy = Strategy.Defense;
        }

        Player target = FindTarge();
        if (target != null)
        {
            switch (stratgy)
            {
                case Strategy.Attack:
                    AttackStrategy(ref input, controller, target);
                    break;
                case Strategy.Defense:
                    DefenseStrategy(ref input, controller, target);
                    break;
            }
        }
    }

    Player FindTarge()
    {
        GameObject go = GameObject.FindGameObjectWithTag(StringAssets.localPlayerTag);
        if (go == null)
        {
            go = GameObject.FindGameObjectWithTag(StringAssets.remoteplayerTag);
        }

        if (go)
            return go.GetComponent<Player>();

        return null;
    }

    void AttackStrategy(ref GameInput input, LocalPlayerController controller, Player target)
    {

        Vector3 diffPos = target.transform.position - controller.transform.position;
        bool close = false;
        if (diffPos.magnitude < 2f)
            close = true;
        float yaw = CommonHelper.YawOfVector3(diffPos);
        if (close)
        {
            input.mainHand = true;
            input.moveYaw = yaw;
        }
        else
        {
            input.run = true;
            input.hasMove = true;
            input.moveYaw = yaw;
            player.targetId = -1;
        }
    }

    bool strafeRight = false;
    float strafeDirTimer = 0f;
    void DefenseStrategy(ref GameInput input, LocalPlayerController controller, Player target)
    {
        player.targetId = target.id;
        Vector3 diffPos = target.transform.position - controller.transform.position;
        float yaw = CommonHelper.YawOfVector3(diffPos);
        if (diffPos.magnitude < 4f)
        {
            input.moveYaw = yaw + 180f;
            input.roll = true;
        }
        else
        {
            strafeDirTimer -= Time.deltaTime;
            if (strafeDirTimer < 0)
            {
                strafeDirTimer = UnityEngine.Random.Range(1f, 2f);
                strafeRight = !strafeRight;
            }
            input.offHand = true;
            input.hasMove = true;
            if (strafeRight)
                input.moveYaw = yaw - 80f;
            else
                input.moveYaw = yaw + 80f;
        }
    }

    float stratgyTimer = 0f;
    void OnDamage(System.Object sender, System.Object eventArg)
    {
        stratgy = Strategy.Defense;
        stratgyTimer = UnityEngine.Random.Range(2f, 5f);
    }
}
