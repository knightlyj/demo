using UnityEngine;
using System.Collections;
using System;

public static class KeyboardInput
{
    public static readonly string forward = "Forward";
    public static readonly string right = "Right";

    public static readonly KeyCode leftHand = KeyCode.Mouse0;
    public static readonly KeyCode rightHand = KeyCode.Mouse1;
    public static readonly KeyCode strongAttack = KeyCode.LeftShift;

    public static readonly KeyCode jump = KeyCode.Space;
    public static readonly KeyCode roll = KeyCode.LeftAlt;
    public static readonly KeyCode run = KeyCode.LeftShift;

    public static readonly KeyCode lockTarget = KeyCode.Mouse2;
}

public static class GamePadInput
{
    public const float joystickThreshold = 0.6f;
    public static readonly string forward = "JoystickForward";
    public static readonly string right = "JoystickRight";
    public static readonly string cameraX = "JoystickCameraX";
    public static readonly string cameraY = "JoystickCameraY";

    public static readonly KeyCode jump = KeyCode.JoystickButton1;
    public static readonly KeyCode roll = KeyCode.JoystickButton1;
    public static readonly KeyCode run = KeyCode.JoystickButton1;
    public static readonly KeyCode lockTarget = KeyCode.JoystickButton9;

    public static readonly KeyCode leftHand = KeyCode.JoystickButton4;
    public static readonly KeyCode rightHand = KeyCode.JoystickButton5;

    public static bool leftHand2 { get { return Input.GetAxis("LTRT") > 0.7f; } }
    public static bool rightHand2 { get { return Input.GetAxis("LTRT") < -0.7f; } }
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
        cameraControl.cameraYaw = this.yaw;
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

        base.Update();

        //更新UI的精力条
        UnityHelper.GetUIManager().SetPlayerEnergy(energyPoint / maxEnergy);
    }

    protected new void FixedUpdate()
    {
        UpdateInput();
        base.FixedUpdate();
    }

    DateTime lastLockTime = DateTime.Now;
    void UpdateInput()
    {
        input.Clear();

        UpdateInputYaw();

        //跑
        if (Input.GetKey(KeyboardInput.run) || Input.GetKey(GamePadInput.run))
        {
            input.run = true;
        }
        //翻滚
        if (Input.GetKeyDown(KeyboardInput.roll) || Input.GetKeyDown(GamePadInput.roll))
        {
            input.roll = true;
        }
        //跳跃
        if (Input.GetKeyDown(KeyboardInput.jump) || Input.GetKeyDown(GamePadInput.jump))
        {
            input.jump = true;
        }
        //左手攻击
        if (Input.GetKey(KeyboardInput.leftHand))
        {
            input.leftHand = true;
        }
        if (Input.GetKey(GamePadInput.leftHand))
        {
            input.leftHand = true;
        }

        //右手攻击
        if (Input.GetKey(KeyboardInput.rightHand))
        {
            input.rightHand = true;
        }
        if (Input.GetKey(GamePadInput.rightHand))
        {
            input.rightHand = true;
        }

        //锁定目标
        if (Input.GetKeyDown(KeyboardInput.lockTarget) || Input.GetKeyDown(GamePadInput.lockTarget))
        {
            TimeSpan span = DateTime.Now - lastLockTime;
            if (span.TotalMilliseconds > 500)
            {
                if (target == null)
                    LockTarget();
                else
                    UnLockTarget();
            }
        }
    }

    void UpdateInputYaw()
    {
        //处理输入方向
        EightDir inputDir = EightDir.Empty;
        //前后
        float keyForward = Input.GetAxis(KeyboardInput.forward);
        float gamepadForward = Input.GetAxis(GamePadInput.forward);
        if (keyForward > 0 || gamepadForward > GamePadInput.joystickThreshold)
        {
            inputDir = EightDir.Front;
        }
        else if (keyForward < 0 || gamepadForward < -GamePadInput.joystickThreshold)
        {
            inputDir = EightDir.Back;
        }

        //左右
        float keyRight = Input.GetAxis(KeyboardInput.right);
        float gamepadRight = Input.GetAxis(GamePadInput.right);
        if (keyRight > 0 || gamepadRight > GamePadInput.joystickThreshold)
        {
            if (inputDir == EightDir.Front)
                inputDir = EightDir.FrontRight;
            else if (inputDir == EightDir.Back)
                inputDir = EightDir.BackRight;
            else
                inputDir = EightDir.Right;
        }
        else if (keyRight < 0 || gamepadRight < -GamePadInput.joystickThreshold)
        {
            if (inputDir == EightDir.Front)
                inputDir = EightDir.FrontLeft;
            else if (inputDir == EightDir.Back)
                inputDir = EightDir.BackLeft;
            else
                inputDir = EightDir.Left;
        }

        if (inputDir == EightDir.Empty)
            input.hasDir = false;  //没输入方向
        else
            input.hasDir = true;  //输入方向

        //从八方向转到yaw
        float yaw = cameraControl.cameraYaw;
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

        if (target == null)
        {
            cameraControl.ResetCamera(this.yaw);
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
