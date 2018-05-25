using UnityEngine;
using System.Collections;
using System;



public partial class Player
{
    protected bool grounded = false;
    [SerializeField]
    protected LayerMask groundLayerMask;
    protected Vector3 groundNormal;
    float groundCheckRadius = 0.5f;
    void Simulate()
    {
        GroundCheck();

        if (curActionType == ActionType.Empty) //不在硬直动作中
        {
            SimulateFree();
        }
        else
        {  //在硬直动作中,处理动作的逻辑
            SimulateAction();
        }

        UpdateEnergy();
    }

    //落地检测
    void GroundCheck()
    {
        RaycastHit hitInfo;
        Vector3 origin = transform.position + Vector3.up * 0.6f;
        if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out hitInfo, 0.15f, groundLayerMask))
        {
            rigidBody.drag = 6f;
            grounded = true;
            groundNormal = hitInfo.normal;
        }
        else
        {
            rigidBody.drag = 0f;
            grounded = false;
            groundNormal = Vector3.up;
        }
    }
    
    //自由状态，非硬直
    void SimulateFree()
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

    //模拟地面
    DateTime lastRunTime = DateTime.Now;
    float moveSpeed = walkSpeed;
    void SimulateOnGround()
    {
        if (input.jump)
        {
            if (EnergyCost(jumpEnergyCost) > 0)
            {
                IntoAction(ActionType.Jump);
            }
        }
        else if (input.roll)
        {
            if (EnergyCost(rollEnergyCost) > 0)
                IntoAction(ActionType.Roll);
        }
        else
        { //没有做任何动作,则处理跑/走逻辑
            if (!input.hasDir) //没输入方向
            {
                SetLowerAniClip(LowerAniClip.Idle);
                SetUpperAniClip(UpperAniClip.Idle);
                rigidBody.Sleep();
                moveSpeed = walkSpeed;  //idle时，重置移动速度
                footIk = true;
            }
            else
            {
                footIk = false;
                this.orientation = input.yaw;
                //在移动时按下run，移动速度会逐渐递增到跑步速度
                if (input.run)
                { //跑
                    moveSpeed += (runSpeed - walkSpeed) * 0.05f;
                    if (moveSpeed >= runSpeed)
                        moveSpeed = runSpeed;
                    EnergyCost(runEnergyCost * Time.fixedDeltaTime); //消耗精力
                    lastRunTime = DateTime.Now;
                }
                else
                { //走
                    moveSpeed -= (runSpeed - walkSpeed) * 0.05f;
                    if (moveSpeed <= walkSpeed)
                        moveSpeed = walkSpeed;
                }

                walkRun = (moveSpeed - walkSpeed) / (runSpeed - walkSpeed);

                SetUpperAniClip(UpperAniClip.Move);
                SetLowerAniClip(LowerAniClip.Move);

                Vector3 moveDir = CommonHelper.DirOnPlane(groundNormal, this.orientation);
                //Debug.Log(Vector3.Dot(moveDir, groundNormal));
                //按这种写法,分析时序后,速度是稳定的,如果不到最大速度才施加力的话,速度不会稳定
                float speedOnDir = Vector3.Dot(moveDir, rigidBody.velocity);
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
        footIk = false;
        shootIk = false;
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
                    SetWholeAniClip(WholeAniClip.Fall);
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

    float cantRunTime = 0f;  //精力空了以后一段时间内不能跑步
    float energyRecoverDelay = 0f;  //动作的时间,一定时间内不恢复精力
    //尝试消耗精力,如果不够则扣掉剩下的全部精力,精力不够时攻击动作的伤害会打折扣
    float EnergyCost(float cost)
    {
        if (energyPoint <= 0)
            return 0;

        //这里有点trick,用精力消耗来判断跑步和其他动作.
        if (cost > 5f)
        {  //跑步消耗精力少,不会更新这个时间,跑步停下则立即恢复精力,1秒的精力恢复延迟,硬编码了
            energyRecoverDelay = 1f;
        }

        float realCost = cost;

        if (energyPoint <= cost)
        { //精力不够则消耗掉剩下的全部
            cantRunTime = (maxEnergy / energyRespawn);
            realCost = energyPoint;
            energyPoint = 0f;
        }
        else
        {
            energyPoint -= cost;
        }

        return realCost;  //返回消耗的精力
    }

    void UpdateEnergy()
    {
        energyRecoverDelay -= Time.fixedDeltaTime;
        if (energyRecoverDelay < 0f)  //
        {
            energyPoint += energyRespawn * Time.fixedDeltaTime;
            if (energyPoint > maxEnergy)
                energyPoint = maxEnergy;
            energyRecoverDelay = 0f;
        }

        cantRunTime -= Time.fixedDeltaTime;
        if (cantRunTime < 0f)
            cantRunTime = 0f;
    }

}
