using UnityEngine;
using System.Collections;
using System;

public partial class Player
{
    enum PlayerState
    {
        OnGround,
        InAir,
        Action,
    }

    PlayerState state = PlayerState.OnGround;

    void Simulate()
    {
        if (curActionType == ActionType.Empty) //不在特殊动作中
        {
            if (grounded)
            {   //在地上
                SimulateOnGround();
            }
            else
            {   //在空中
                SimulateInAir();
            }

        }
        else
        {  //在动作中,处理动作的逻辑
            SimulateAction();
        }
    }



    //模拟地面
    bool lastNoDirInput = true;
    void SimulateOnGround()
    {
        rigidBody.useGravity = true;

        if (input.roll)
        {
            IntoAction(ActionType.Roll);
        }
        else if (input.jump)
        {
            IntoAction(ActionType.Jump);
        }
        else if (input.attack)
        {
            IntoAction(ActionType.Attack);
        }
        else if (input.strongAttack)
        {
            if (input.run && input.hasDir)
            { //跳劈
                IntoAction(ActionType.JumpAttack);
            }
            else
            {
                IntoAction(ActionType.ChargeAttack);
            }
        }
        else
        { //没有做任何动作,则处理跑/走逻辑
            if (!input.hasDir) //没输入方向
            {
                if (!lastNoDirInput)
                {   //没有按方向键,且上一帧按了方向键,这一帧就停下来
                    //如果是落地第一帧,则会按照离地前最后一帧的方向来处理,这样跳跃的落地就直接停下了
                    rigidBody.velocity = new Vector3(0, 0, 0);
                    rigidBody.useGravity = false;
                }
                aniModule.SetAnimation(PlayerAniType.Idle);
            }
            else
            {
                this.orientation = input.yaw;
                float moveSpeed = walkSpeed;
                if (input.run)
                { //跑
                    moveSpeed = runSpeed;
                    aniModule.SetAnimation(PlayerAniType.Run);
                }
                else
                {
                    aniModule.SetAnimation(PlayerAniType.Walk);
                }
                Vector3 newVel = Quaternion.Euler(0, this.orientation, 0) * Vector3.forward * moveSpeed;

                rigidBody.velocity = new Vector3(newVel.x, rigidBody.velocity.y, newVel.z);
            }
        }
        lastNoDirInput = input.hasDir;
        lastFrameInFall = false;
    }

    //模拟空中
    DateTime startFallTime = DateTime.Now;
    bool lastFrameInFall = false;
    void SimulateInAir()
    {
        if (rigidBody.velocity.y < 0) //下落
        {
            if (lastFrameInFall == false)
            {
                startFallTime = DateTime.Now;
            }
            else
            {
                TimeSpan span = DateTime.Now - startFallTime;
                if (span.TotalMilliseconds > 200)
                {
                    aniModule.SetAnimation(PlayerAniType.Fall, PlayerAniDir.Front, 0.5f);
                }
                if (input.hasDir)
                {
                    //空中给一点点水平移动的力
                    Vector3 forceDir = Quaternion.Euler(0, 0, 0) * Vector3.forward;
                    float speedOnDir = Vector3.Dot(forceDir, rigidBody.velocity);
                    if (speedOnDir < moveSpeedInAir)
                    {
                        rigidBody.AddForce(forceDir * moveForceInAir);
                    }
                }
            }

            lastFrameInFall = true;
        }
        else
        {
            lastFrameInFall = false;
        }
    }
}
