using UnityEngine;
using System.Collections;
using System;



public partial class Player
{
    //模拟地面
    DateTime lastRunTime = DateTime.Now;
    float moveSpeed = walkSpeed;
    
    /// <summary>
    /// /////////////////////////////////////////////////
    /// </summary>
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
