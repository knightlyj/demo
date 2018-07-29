using UnityEngine;
using System.Collections;
using System;
using UnityStandardAssets.CrossPlatformInput;

public static class KeyboardInput
{
    public static readonly string forward = "Forward";
    public static readonly string right = "Right";

    public static readonly KeyCode mainHand = KeyCode.Mouse0;
    public static readonly KeyCode offHand = KeyCode.Mouse1;
    public static readonly KeyCode swapWeapon = KeyCode.R;

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
    public static readonly KeyCode roll = KeyCode.JoystickButton0;
    public static readonly KeyCode lockTarget = KeyCode.JoystickButton9;

    public static readonly KeyCode offHand = KeyCode.JoystickButton4;
    public static readonly KeyCode mainHand = KeyCode.JoystickButton5;
    public static readonly KeyCode swapWeapon = KeyCode.JoystickButton3;

    //public static bool LT { get { return Input.GetAxis("LTRT") > 0.7f; } }
    public static bool run { get { return Input.GetAxis("LTRT") < -0.7f; } }
}

public struct GameInput
{
    public bool hasMove;  //有输入方向
    public float moveYaw;
    public Vector2 aimAngle;  //x为yaw,y为pitch

    public bool run;
    public bool roll;
    public bool jump;
    public bool mainHand;
    public bool offHand;
    public bool swapWeapon;
    public bool lockTarget;

    public void Clear()
    {
        hasMove = false;
        run = false;
        roll = false;
        jump = false;
        mainHand = false;
        offHand = false;
        swapWeapon = false;
        lockTarget = false;
    }
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

interface IPlayerInput
{
    void Start(Player player, LocalPlayerController controller);
    void UpdateInput(ref GameInput input, LocalPlayerController controller);
    void Stop();
}

public class LocalPlayerInput : IPlayerInput
{
    public void Start(Player player, LocalPlayerController controller)
    {

    }

    public void Stop()
    {

    }

    public void UpdateInput(ref GameInput input, LocalPlayerController controller)
    {
        input.Clear();

        CameraControl cameraControl = GameObject.FindWithTag("MainCamera").GetComponent<CameraControl>();
        input.aimAngle.x = cameraControl.cameraPitch;
        input.aimAngle.y = cameraControl.cameraYaw;

        if (Application.platform == RuntimePlatform.Android || GlobalVariables.mobileUIOnPC)
        {
            UpdateInputOnMobile(ref input);
        }
        else
        {
            UpdateInputOnPC(ref input);
        }
    }

    void UpdateMoveYawOnPC(ref GameInput input)
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
            input.hasMove = false;  //没输入方向
        else
            input.hasMove = true;  //输入方向

        //从八方向转到yaw
        float yaw = input.aimAngle.y;
        switch (inputDir)
        {
            case EightDir.Front:
                input.moveYaw = yaw;
                break;
            case EightDir.FrontLeft:
                input.moveYaw = yaw - 45;
                break;
            case EightDir.Left:
                input.moveYaw = yaw - 90;
                break;
            case EightDir.BackLeft:
                input.moveYaw = yaw - 135;
                break;
            case EightDir.Back:
                input.moveYaw = yaw + 180;
                break;
            case EightDir.BackRight:
                input.moveYaw = yaw + 135;
                break;
            case EightDir.Right:
                input.moveYaw = yaw + 90;
                break;
            case EightDir.FrontRight:
                input.moveYaw = yaw + 45;
                break;
            default:
                break;
        }
    }

    void UpdateInputOnPC(ref GameInput input)
    {
        UpdateMoveYawOnPC(ref input);

        //跑
        if (Input.GetKey(KeyboardInput.run) || GamePadInput.run)
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
        //副手
        if (Input.GetKey(KeyboardInput.offHand) && !GlobalVariables.menuOpened)
        {
            input.offHand = true;
        }
        if (Input.GetKey(GamePadInput.offHand))
        {
            input.offHand = true;
        }

        //主手
        if (Input.GetKey(KeyboardInput.mainHand) && !GlobalVariables.menuOpened)
        {
            input.mainHand = true;
        }
        if (Input.GetKey(GamePadInput.mainHand))
        {
            input.mainHand = true;
        }

        //换武器
        if (Input.GetKey(KeyboardInput.swapWeapon))
        {
            input.swapWeapon = true;
        }
        if (Input.GetKey(GamePadInput.swapWeapon))
        {
            input.swapWeapon = true;
        }


        //锁定目标
        if (Input.GetKeyDown(KeyboardInput.lockTarget) || Input.GetKeyDown(GamePadInput.lockTarget))
        {
            input.lockTarget = true;
        }
    }

    void UpdateInputOnMobile(ref GameInput input)
    {
        //***********move******************
        const float runThreshold = 0.75f;
        const float walkThreshold = 0.15f;
        Vector2 move;
        move.x = CrossPlatformInputManager.GetAxis("Right");
        move.y = CrossPlatformInputManager.GetAxis("Forward");
        if (move.sqrMagnitude > runThreshold * runThreshold)
        {   //run
            float yaw = CommonHelper.YawOfVector2(move);
            yaw += input.aimAngle.y;
            input.moveYaw = yaw;
            input.hasMove = true;
            input.run = true;
        }
        else if (move.sqrMagnitude > walkThreshold * walkThreshold)
        { //walk
            float yaw = CommonHelper.YawOfVector2(move);
            yaw += input.aimAngle.y;
            input.moveYaw = yaw;
            input.hasMove = true;
            input.run = false;
        }
        else
        {  //idle
            input.hasMove = false;
            input.run = false;
        }

        //翻滚
        if (CrossPlatformInputManager.GetButton("Roll"))
        {
            input.roll = true;
        }
        //跳跃
        if (CrossPlatformInputManager.GetButton("Jump"))
        {
            input.jump = true;
        }
        //主手攻击
        if (CrossPlatformInputManager.GetButton("MainHand"))
        {
            input.mainHand = true;
        }
        //副手攻击
        if (CrossPlatformInputManager.GetButton("OffHand"))
        {
            input.offHand = true;
        }
        //换武器
        if (CrossPlatformInputManager.GetButton("SwapWeapon"))
        {
            input.swapWeapon = true;
        }

        //锁定目标
        if (CrossPlatformInputManager.GetButton("LockTarget"))
        {
            input.lockTarget = true;
        }
    }
}
