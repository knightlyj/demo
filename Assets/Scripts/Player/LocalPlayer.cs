using UnityEngine;
using System.Collections;
using System;

public static class KeyboardInput
{
    public static readonly string Forward = "Forward";
    public static readonly string Right = "Right";

    public static readonly string LeftAttack = "LeftHandAttack";
    public static readonly string RightAttack = "RightHandAttack";
    public static readonly string StrongAttack = "StrongAttack";

    public static readonly string RunAndRoll = "Run/Roll";
    public static readonly string Jump = "Jump";

    public static readonly string LockTarget = "Lock/RestCamera";
}

public static class GamePadInput
{
    public const float joystickThreshold = 0.6f;
    public static readonly string Forward = "JoystickForward";
    public static readonly string Right = "JoystickRight";
    public static readonly string CameraX = "JoystickCameraX";
    public static readonly string CameraY = "JoystickCameraY";

    public static readonly KeyCode Run = KeyCode.Joystick1Button1;
    public static readonly KeyCode LockTarget = KeyCode.Joystick1Button9;
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
        input.run = Input.GetButton(KeyboardInput.RunAndRoll) || Input.GetKey(GamePadInput.Run);
        input.jump = Input.GetButton(KeyboardInput.Jump);
        input.roll = false;// Input.GetKey(KeyboardInput.Roll);

        input.attack = Input.GetButton(KeyboardInput.LeftAttack);
        input.strongAttack = false;// Input.GetButton(KeyboardInput.StrongAttack);

        UpdateInputYaw();

        if (Input.GetButtonDown(KeyboardInput.LockTarget) || Input.GetKeyDown(GamePadInput.LockTarget))
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
        float keyForward = Input.GetAxis(KeyboardInput.Forward);
        float gamepadForward = Input.GetAxis(GamePadInput.Forward);
        if (keyForward > 0 || gamepadForward > GamePadInput.joystickThreshold)
        {
            inputDir = EightDir.Front;
        }
        else if (keyForward < 0 || gamepadForward < -GamePadInput.joystickThreshold)
        {
            inputDir = EightDir.Back;
        }

        //左右
        float keyRight = Input.GetAxis(KeyboardInput.Right);
        float gamepadRight = Input.GetAxis(GamePadInput.Right);
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

        SetOrientation(inputDir);
    }

    //根据镜头方向,设置人物的八个方向
    void SetOrientation(EightDir inputDir)
    {
        if (inputDir == EightDir.Empty)
            input.hasDir = false;  //没输入方向
        else
            input.hasDir = true;  //输入方向

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
