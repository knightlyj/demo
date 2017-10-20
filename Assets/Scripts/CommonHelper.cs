using UnityEngine;
using System.Collections;

public static class CommonHelper
{
    //浮点数相等判断
    static public bool FloatEqual(float a, float b)
    {
        return Mathf.Abs(a - b) < 0.001f;
    }

    //朝目标角度变化,按最近距离计算,simple为true时,不执行step的abs和两个角度重规划到0~360
    static public float AngleTowards(float now, float target, float step, bool simple = false)
    {
        float result = 0;
        if (!simple)
        {
            step = Mathf.Abs(step);
            now = now - Mathf.Floor(now / 360f) * 360f; //范围在0~360之间
            target = target - Mathf.Floor(target / 360f) * 360f; //范围在0~360之间
        }
        //计算顺时针到目标的距离
        float distClockwise = 0;
        if (target > now)
            distClockwise = target - now;
        else
            distClockwise = 360 - (now - target);

        if (Mathf.Min(distClockwise, 360 - distClockwise) <= step)
        { //足够近,直接相等即可
            result = target;
        }
        else
        {
            if (distClockwise < 180)
            {  //顺时针距离小于180,则顺时针移动
                result = now + step;
            }
            else
            {   //顺时针距离大于180,则逆时针移动
                result = now - step;
            }
        }


        return result;
    }

    //根据距离*比例,计算步长
    static public float AngleTowardsByDiff(float now, float target, float ratio, float minStep)
    {
        ratio = Mathf.Abs(ratio);
        minStep = Mathf.Abs(minStep);
        now = now - Mathf.Floor(now / 360f) * 360f; //范围在0~360之间
        target = target - Mathf.Floor(target / 360f) * 360f; //范围在0~360之间

        //计算顺时针到目标的距离
        float distClockwise = 0;
        if (target > now)
            distClockwise = target - now;
        else
            distClockwise = 360 - (now - target);

        if (distClockwise < 180)
        {  //顺时针距离小于180,则顺时针移动
            float step = distClockwise * ratio;
            if (step <= minStep)  //距离小于最小步长,直接等于
                step = minStep;

            if (step >= distClockwise)
                return target;
            else
                return now + step;
        }
        else
        {   //顺时针距离大于180,则逆时针移动
            float step = (360 - distClockwise) * ratio;
            if (step <= minStep)
                step = minStep;

            if (step >= (360 - distClockwise))
                return target;
            else
                return now - step;
        }
    }
}
