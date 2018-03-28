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

    LayerMask groundLayerMask;
    float groundCheckRadius = 0.5f;
    void Simulate()
    {
        //落地检测
        Collider[] hitGround = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, groundLayerMask);

        if (hitGround == null || hitGround.Length == 0)
        {
            grounded = false;
            //rigidBody.useGravity = true; //在空中,受到重力影响
        }
        else
        {
            grounded = true;
            //rigidBody.useGravity = false; //在地面时,不用重力
        }

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
    void SimulateOnGround()
    {
        //rigidBody.useGravity = true;

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
                rigidBody.velocity = new Vector3(0, 0, 0);
                //rigidBody.useGravity = false;
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

                //水平方向变化时,按速度到当前方向的投影继承速度,如果投影与当前反向,则不继承
                //按这种写法,分析时序后,速度是稳定的,如果不到最大速度才施加力的话,速度不会稳定
                Vector3 moveDir = Quaternion.AngleAxis(this.orientation, Vector3.up) * Vector3.forward;
                float speedOnDir = moveDir.x * rigidBody.velocity.x + moveDir.z * rigidBody.velocity.z;
                rigidBody.AddForce(moveDir * moveForce);
                if (speedOnDir > moveSpeed)
                {
                    rigidBody.velocity = moveDir * moveSpeed;
                }

            }
        }
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
