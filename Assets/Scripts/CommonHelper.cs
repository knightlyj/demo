using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

public static class CommonHelper
{
    //浮点数相等判断
    static public bool FloatEqual(float a, float b)
    {
        return Mathf.Abs(a - b) < 0.001f;
    }
    
    //浮点数towards
    static public float FloatTowards(float a, float b, float step)
    {
        float sign = Mathf.Sign(b - a);
        step = Mathf.Abs(step);
        if (sign > 0)
        {
            if (a + step > b)
                return b;
            else
                return a + step;
        }
        else
        {
            if (a - step < b)
                return b;
            else
                return a - step;
        }
    }

    //角度差值,按最近的方向,返回值为-180 ~ 180,顺时针为正值
    static public float AngleDiffClosest(float from, float to)
    {
        from = from - Mathf.Floor(from / 360f) * 360f; //范围在0~360之间
        to = to - Mathf.Floor(to / 360f) * 360f; //范围在0~360之间

        //计算顺时针到目标的距离
        float distClockwise = 0;
        if (to > from)
            distClockwise = to - from;
        else
            distClockwise = 360 - (from - to);

        if (distClockwise < 180)
        {
            return distClockwise;
        }
        else
        {
            return distClockwise - 360;
        }
    }

    //朝目标角度变化,按最近距离计算,simple为true时,不执行step的abs和两个角度重规划到0~360
    static public float AngleTowards(float from, float to, float step)
    {
        float result = 0;

        step = Mathf.Abs(step);
        from = from - Mathf.Floor(from / 360f) * 360f; //范围在0~360之间
        to = to - Mathf.Floor(to / 360f) * 360f; //范围在0~360之间

        //计算顺时针到目标的距离
        float distClockwise = 0;
        if (to > from)
            distClockwise = to - from;
        else
            distClockwise = 360 - (from - to);

        if (Mathf.Min(distClockwise, 360 - distClockwise) <= step)
        { //足够近,直接相等即可
            result = to;
        }
        else
        {
            if (distClockwise < 180)
            {  //顺时针距离小于180,则顺时针移动
                result = from + step;
            }
            else
            {   //顺时针距离大于180,则逆时针移动
                result = from - step;
            }
        }


        return result;
    }

    //根据距离*比例,计算步长
    static public float AngleTowardsByDiff(float from, float to, float ratio, float minStep)
    {
        ratio = Mathf.Abs(ratio);
        minStep = Mathf.Abs(minStep);
        from = from - Mathf.Floor(from / 360f) * 360f; //范围在0~360之间
        to = to - Mathf.Floor(to / 360f) * 360f; //范围在0~360之间

        //计算顺时针到目标的距离
        float distClockwise = 0;
        if (to > from)
            distClockwise = to - from;
        else
            distClockwise = 360 - (from - to);

        if (distClockwise < 180)
        {  //顺时针距离小于180,则顺时针移动
            float step = distClockwise * ratio;
            if (step <= minStep)  //距离小于最小步长,直接等于
                step = minStep;

            if (step >= distClockwise)
                return to;
            else
                return from + step;
        }
        else
        {   //顺时针距离大于180,则逆时针移动
            float step = (360 - distClockwise) * ratio;
            if (step <= minStep)
                step = minStep;

            if (step >= (360 - distClockwise))
                return to;
            else
                return from - step;
        }
    }

    public static float YawOfVector3(Vector3 vec)
    {
        vec.y = 0;
        float yaw = Mathf.Acos(vec.z / vec.magnitude) * Mathf.Rad2Deg;
        if (vec.x < 0)
            yaw = -yaw;

        return yaw;
    }

    public static float YawOfVector2(Vector2 vec)
    {
        float yaw = Mathf.Acos(vec.y / vec.magnitude) * Mathf.Rad2Deg;
        if (vec.x < 0)
            yaw = -yaw;

        return yaw;
    }

    //用degree
    public static float DirDerivativeOnPlane(Vector3 normal, float yaw)
    {
        yaw = yaw * Mathf.Deg2Rad;  //三角函数需要用radian
        float cosYaw = Mathf.Cos(yaw);
        float sinYaw = Mathf.Sin(yaw);
        float Yx = -(normal.x / normal.y);
        float Yz = -(normal.z / normal.y);
        return sinYaw * Yx + cosYaw * Yz;
    }


    public static float DirDerivativeOnPlane(Vector3 normal, Vector3 dir)
    {
        dir.y = 0;
        float magnitude = dir.magnitude;
        if (FloatEqual(magnitude, 0)) //防止除0,normal水平时,返回0
            return 0;

        float cosYaw = dir.z / magnitude;
        float sinYaw = dir.x / magnitude;
        float Yx = -(normal.x / normal.y);
        float Yz = -(normal.z / normal.y);
        return sinYaw * Yx + cosYaw * Yz;
    }

    //用degree
    public static Vector3 DirOnPlane(Vector3 normal, float yaw)
    {
        Vector3 dir = Quaternion.AngleAxis(yaw, Vector3.up) * Vector3.forward;
        
        float dd = 0;//在这个yaw上的方向导数
        if (!FloatEqual(normal.y, 0))  //法线接近水平,结果为infinity,这里设置为0吧 
        {
            dd = DirDerivativeOnPlane(normal, yaw);
        }
        dir.y = dd;
        dir.Normalize();
        return dir;
    }


    public static bool IpLegal(string ip)
    {
        try
        {
            System.Net.IPAddress.Parse(ip);
        }
        catch
        {
            return false;
        }
        return true;
    }
    
    public static string GetIpAddress()
    {
        IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
        foreach(IPAddress ipAddr in ipHost.AddressList)
        {

            if (ipAddr.AddressFamily == AddressFamily.InterNetwork)
                return ipAddr.ToString();
        }
        return null;
    }

}
