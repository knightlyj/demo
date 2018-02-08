using UnityEngine;
using System.Collections;
using System;

public static class KeyboardInput
{
    public static KeyCode Forward = KeyCode.W;
    public static KeyCode Backward = KeyCode.S;
    public static KeyCode Left = KeyCode.A;
    public static KeyCode Right = KeyCode.D;


    public static KeyCode Run = KeyCode.LeftShift;
    public static KeyCode Jump = KeyCode.Space;
    public static KeyCode Roll = KeyCode.LeftAlt;

    public static KeyCode Attack = KeyCode.Mouse0;
    public static KeyCode AntiAttack = KeyCode.F;
    public static KeyCode StrongAttack = KeyCode.Mouse1;

    public static KeyCode ResetCamera = KeyCode.Mouse2; //暂时不用
    public static KeyCode LockTarget = KeyCode.Q;
}

public enum EightDir
{
    Empty,
    Front,
    FrontLeft,
    Left,
    BackLeft,
    Back,
    BackRight,
    Right,
    FrontRight,
}

public class LocalPlayer : Player
{
    protected new void Awake()
    {
        base.Awake();
    }

    //镜头
    CameraControl cameraControl = null;
    // Use this for initialization
    protected new void Start()
    {
        base.Start();

        //镜头设置及初始化
        cameraControl = GameObject.FindWithTag("MainCamera").GetComponent<CameraControl>();
        cameraControl.cameraYaw = this.orientation;
        UnLockTarget();
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
        UnLockTarget();
    }

    // Update is called once per frame
    protected new void Update()
    {
        input.run = Input.GetKey(KeyboardInput.Run);
        input.jump = Input.GetKey(KeyboardInput.Jump);
        input.roll = Input.GetKey(KeyboardInput.Roll);

        input.attack = Input.GetKey(KeyboardInput.Attack);
        input.antiAttack = Input.GetKey(KeyboardInput.AntiAttack);
        input.strongAttack = Input.GetKey(KeyboardInput.StrongAttack);

        UpdateInputYaw();

        if (Input.GetKeyDown(KeyboardInput.LockTarget))
        {
            if (target == null)
                LockTarget();
            else
                UnLockTarget();
        }

        base.Update();
    }

    protected new void FixedUpdate()
    {
        base.FixedUpdate();
    }

    void UpdateInputYaw()
    {
        //处理输入方向
        EightDir inputDir = EightDir.Empty;
        //前后
        if (Input.GetKey(KeyboardInput.Forward))
        {
            inputDir = EightDir.Front;
        }
        else if (Input.GetKey(KeyboardInput.Backward))
        {
            inputDir = EightDir.Back;
        }

        //左右
        if (Input.GetKey(KeyboardInput.Left))
        {
            if (inputDir == EightDir.Front)
                inputDir = EightDir.FrontLeft;
            else if (inputDir == EightDir.Back)
                inputDir = EightDir.BackLeft;
            else
                inputDir = EightDir.Left;
        }
        else if (Input.GetKey(KeyboardInput.Right))
        {
            if (inputDir == EightDir.Front)
                inputDir = EightDir.FrontRight;
            else if (inputDir == EightDir.Back)
                inputDir = EightDir.BackRight;
            else
                inputDir = EightDir.Right;
        }

        SetOrientation(inputDir);
    }

    //根据镜头方向,设置人物的八个方向
    void SetOrientation(EightDir inputDir)
    {
        if (inputDir == EightDir.Empty)
            input.hasDir = false;  //没输入方向
        else
            input.hasDir = true;  //输入方向

        float yaw = 0;
        if (target == null)
        {   //没有锁定个目标,按镜头角度设置方向
            yaw = cameraControl.cameraYaw;
        }
        else
        {   //有锁定目标,根据目标方向设置角色朝向
            Vector3 toTarget = target.transform.position - transform.position;
            toTarget.y = 0;
            yaw = Mathf.Acos(toTarget.z / toTarget.magnitude) / Mathf.PI * 180;
            if (toTarget.x < 0)
                yaw = -yaw;
        }
        switch (inputDir)
        {
            case EightDir.Front:
                input.yaw = yaw;
                break;
            case EightDir.FrontLeft:
                input.yaw = yaw - 45;
                break;
            case EightDir.Left:
                input.yaw = yaw - 90;
                break;
            case EightDir.BackLeft:
                input.yaw = yaw - 135;
                break;
            case EightDir.Back:
                input.yaw = yaw + 180;
                break;
            case EightDir.BackRight:
                input.yaw = yaw + 135;
                break;
            case EightDir.Right:
                input.yaw = yaw + 90;
                break;
            case EightDir.FrontRight:
                input.yaw = yaw + 45;
                break;
            default:
                break;
        }

    }

    void LockTarget()
    {
        GameObject[] allAI = GameObject.FindGameObjectsWithTag("AI");
        if (allAI != null)
        {
            foreach (GameObject go in allAI)
            {
                Vector3 dir = go.transform.position - cameraControl.transform.position;
                if (Vector3.Angle(cameraControl.transform.forward, dir) < 55)
                {
                    //原来有目标,退订事件
                    UnLockTarget();
                    //获取新目标,并订阅事件
                    target = go.GetComponent<Player>();
                    target.onPlayerDestroy += this.OnTargetDestory;
                    break;
                }
            }
        }
    }

    void UnLockTarget()
    {
        if (target != null)
        {
            target.onPlayerDestroy -= this.OnTargetDestory;
            target = null;
        }
    }

    void OnTargetDestory()
    {
        UnLockTarget();
    }

}
