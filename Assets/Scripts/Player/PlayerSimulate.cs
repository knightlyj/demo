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

    bool grounded = false;
    LayerMask groundLayerMask;
    float groundCheckRadius = 0.5f;
    void Simulate()
    {
        //落地检测
        Collider[] hitGround = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, groundLayerMask);

        if (hitGround == null || hitGround.Length == 0)
        {
            grounded = false;
        }
        else
        {
            grounded = true;
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

    float moveSpeed = walkSpeed;
    //模拟地面
    void SimulateOnGround()
    {
        if (input.attack)
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
        else if (input.jump)
        {
            IntoAction(ActionType.Jump);
            input.jump = false;
        }
        else if (input.roll)
        {
            IntoAction(ActionType.Roll);
            input.roll = false;
        }
        else
        { //没有做任何动作,则处理跑/走逻辑
            if (!input.hasDir) //没输入方向
            {
                rigidBody.velocity = new Vector3(0, 0, 0);
                aniModule.SetAnimation(PlayerAniType.Idle);
                moveSpeed = walkSpeed;  //idle时，重置移动速度
            }
            else
            { 
                this.orientation = input.yaw;
                //在移动时按下run，移动速度会逐渐递增到跑步速度
                if (input.run)
                { //跑
                    moveSpeed += (runSpeed - walkSpeed) * 0.08f;
                    if (moveSpeed >= runSpeed)
                        moveSpeed = runSpeed;
                }
                else
                {
                    moveSpeed -= (runSpeed - walkSpeed) * 0.08f;
                    if (moveSpeed <= walkSpeed)
                        moveSpeed = walkSpeed;
                }
                float walkRun = (moveSpeed - walkSpeed) / (runSpeed - walkSpeed);
                aniModule.SetWalkRun(walkRun);
                aniModule.SetAnimation(PlayerAniType.WalkRun);
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
