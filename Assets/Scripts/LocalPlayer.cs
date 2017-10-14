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
        _sight = transform.FindChild("Sight");
    }

    public Transform sight { get { return this._sight; } }
    Transform _sight;


    //镜头
    CameraFollow cameraFollow = null;
    // Use this for initialization
    protected new void Start()
    {
        base.Start();

        //镜头设置及初始化
        cameraFollow = GameObject.FindWithTag("MainCamera").GetComponent<CameraFollow>();
        cameraFollow.cameraYaw = this.orientation;
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
    }

    // Update is called once per frame
    protected new void Update()
    {
        playerAction.run = Input.GetKey(KeyboardInput.Run);
        playerAction.jump = Input.GetKey(KeyboardInput.Jump);
        playerAction.roll = Input.GetKey(KeyboardInput.Roll);

        UpdateInputYaw();

        
        

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
            noDirInput = true;  //输入了反向
        else
            noDirInput = false;  //没输入方向

        switch (inputDir)
        {
            case EightDir.Front:
                this.inputYaw = cameraFollow.cameraYaw;
                break;
            case EightDir.FrontLeft:
                this.inputYaw = cameraFollow.cameraYaw - 45;
                break;
            case EightDir.Left:
                this.inputYaw = cameraFollow.cameraYaw - 90;
                break;
            case EightDir.BackLeft:
                this.inputYaw = cameraFollow.cameraYaw - 135;
                break;
            case EightDir.Back:
                this.inputYaw = cameraFollow.cameraYaw + 180;
                break;
            case EightDir.BackRight:
                this.inputYaw = cameraFollow.cameraYaw + 135;
                break;
            case EightDir.Right:
                this.inputYaw = cameraFollow.cameraYaw + 90;
                break;
            case EightDir.FrontRight:
                this.inputYaw = cameraFollow.cameraYaw + 45;
                break;
            default:
                break;
        }
    }
}
