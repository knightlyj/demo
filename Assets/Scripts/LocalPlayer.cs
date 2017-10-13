using UnityEngine;
using System.Collections;
using System;

public static class KeyboardInput
{
    public static KeyCode Forward = KeyCode.W;
    public static KeyCode Backward = KeyCode.S;
    public static KeyCode Left = KeyCode.A;
    public static KeyCode Right = KeyCode.D;


    public static KeyCode Run = KeyCode.LeftControl;
    public static KeyCode Jump = KeyCode.Space;
    public static KeyCode Roll = KeyCode.LeftAlt;

    public static KeyCode ResetCamera = KeyCode.Mouse2; //没平滑过渡时,效果很差,暂时不用
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
    [SerializeField]
    Transform groundCheck = null;

    //各种速度配置
    [SerializeField]
    float rollSpeed = 9;
    [SerializeField]
    float jumpSpeed = 15;
    [SerializeField]
    float WalkSpeed = 5;
    [SerializeField]
    float runSpeed = 10;
    [SerializeField]
    float moveForce = 100;

    [SerializeField]
    float moveForceInAir = 50;
    [SerializeField]
    float moveSpeedInAir = 2;

    PlayerAni playerAni = null;
    public Rigidbody rigidBody = null;
    void Awake()
    {
        playerAni = GetComponent<PlayerAni>();
        rigidBody = GetComponent<Rigidbody>();
    }

    //镜头
    CameraFollow cameraFollow = null;

    public Transform sight { get { return this._sight; } }
    Transform _sight;
    // Use this for initialization
    protected new void Start()
    {
        base.Start();
        Physics.gravity = Physics.gravity * 5; //设置重力
        playerAni.onActionDone += OnActionDone; //动作事件回调
        playerAni.ChangeWeaponModel("2Hand-Axe", MainHandWeaponType.TwoHandAxe, null, OffHandWeaponType.Empty);

        _sight = transform.FindChild("Sight");
        //镜头设置及初始化
        cameraFollow = GameObject.FindWithTag("MainCamera").GetComponent<CameraFollow>();
        //角色朝向初始化
        this.orientation = transform.eulerAngles.y;
        cameraFollow.cameraYaw = this.orientation;
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
        playerAni.onActionDone -= OnActionDone;
    }

    //地面检测
    bool grounded = true;
    //跑
    bool run = true;
    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
        SmoothOrientation(); //角色朝向平滑过渡

        if (Input.GetKeyDown(KeyboardInput.Run)) //跑/走 切换
        {
            run = !run;
        }
    }

    [SerializeField]
    LayerMask groundLayerMask;
    [SerializeField]
    float groundCheckRadius = 0.1f;
    void FixedUpdate()
    {
        //落地检测
        Collider[] hitGround = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, groundLayerMask);

        if (hitGround == null || hitGround.Length == 0)
        {
            grounded = false;
            //rigidBody.useGravity = true; //在空中,受到重力影响
        }
        else
        {
            grounded = true;
            //rigidBody.useGravity = false; //在地面时,不用重力
        }
        Simulate(); //更新操作
    }

    //***************************角色和镜头朝向的代码******************************
    Vector3 playerDir = Vector3.forward; //角色朝向,初始朝向Z-Axis
    public float orientation //角色朝向,基于y轴旋转
    {
        set
        {
            Quaternion rotation = Quaternion.AngleAxis(this.orientation, Vector3.up);
            this.playerDir = rotation * Vector3.forward;
            float temp = _orientation;
            _orientation = value;
            _orientation = _orientation - Mathf.Floor(_orientation / 360f) * 360f; //范围在0~360之间
            StartSmoothOrientation();
        }
        get
        {
            return this._orientation;
        }
    }
    float _orientation = 0f;

    //角色朝向平滑过渡
    float showOrientation = 0; //实际显示的朝向
    const float smoothOriBaseStepLen = 300;
    float smoothOriStepLen = 0;
    void StartSmoothOrientation()
    {  //这里计算环形插值
        float diff = this.orientation - showOrientation;
        float absDiff = Mathf.Abs(diff);
        if (absDiff > 270)
        {
            smoothOriStepLen = smoothOriBaseStepLen * 4;
        }
        else if (absDiff > 180)
        {
            smoothOriStepLen = smoothOriBaseStepLen * 3;
        }
        else if (absDiff > 90)
        {
            smoothOriStepLen = smoothOriBaseStepLen * 2;
        }
        else
        {
            smoothOriStepLen = smoothOriBaseStepLen;
        }

    }
    void SmoothOrientation()
    {
        showOrientation = CommonHelper.AngleTowards(showOrientation, orientation, smoothOriStepLen * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, showOrientation, 0);
    }

    //动作完成的回调
    bool inAction = false;
    void OnActionDone()
    {
        inAction = false;
        //playerAni.SetAnimation(PlayerAniType.Idle);
        StopRollProc();
    }

    void TrunIntoAction()
    {
        inAction = true;

    }

    EightDir lastInputDir = EightDir.Empty;
    void Simulate()
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

        if (grounded)
        {   //在地上
            if (!inAction) //不在特殊动作中
            {
                if (Input.GetKey(KeyboardInput.Roll))
                {
                    SetEightOrientation(inputDir);

                    playerAni.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
                    StartRollProc(this.orientation);
                    TrunIntoAction();
                }
                else if (Input.GetKey(KeyboardInput.Jump))
                {
                    playerAni.SetAnimation(PlayerAniType.JumpUp, PlayerAniDir.Front); //设置动画
                    rigidBody.velocity = new Vector3(rigidBody.velocity.x, jumpSpeed, rigidBody.velocity.z); //设置垂直速度

                    //如果有按方向键,则设置方向和水平速度
                    float moveSpeed = WalkSpeed;
                    if (run)
                    { //跑
                        moveSpeed = runSpeed;
                    }
                    if (inputDir != EightDir.Empty)
                    {
                        SetEightOrientation(inputDir); //设置角色方向
                        //设置水平速度
                        Vector3 moveDir = Quaternion.AngleAxis(this.orientation, Vector3.up) * Vector3.forward;
                        rigidBody.velocity = new Vector3(moveDir.x * moveSpeed, rigidBody.velocity.y, moveDir.z * moveSpeed);
                    }
                    TrunIntoAction();
                }
                else
                { //没有做任何动作,则处理跑/走逻辑
                    if (inputDir == EightDir.Empty)
                    {
                        if (lastInputDir != EightDir.Empty)
                        {
                            //没有按方向键,且上一帧按了方向键,这一帧就停下来
                            //如果是落地第一帧,则会按照离地前最后一帧的方向来处理,这样跳跃的落地就直接停下了
                            rigidBody.velocity = new Vector3(0, 0, 0);
                        }
                        playerAni.SetAnimation(PlayerAniType.Idle);
                    }
                    else
                    {
                        SetEightOrientation(inputDir);
                        float moveSpeed = WalkSpeed;
                        if (run)
                        { //跑
                            moveSpeed = runSpeed;
                            playerAni.SetAnimation(PlayerAniType.Run);
                        }
                        else
                        {
                            playerAni.SetAnimation(PlayerAniType.Walk);
                        }
                        //力的角度向下一点点,
                        Vector3 forceDir = Quaternion.Euler(10, this.orientation, 0) * Vector3.forward;
                        //水平方向变化时,按速度到当前方向的投影继承速度,如果投影与当前反向,则不继承
                        Vector3 moveDir = Quaternion.AngleAxis(this.orientation, Vector3.up) * Vector3.forward;
                        float speedOnDir = moveDir.x * rigidBody.velocity.x + moveDir.z * rigidBody.velocity.z;
                        if (speedOnDir > 0)
                            rigidBody.velocity = new Vector3(moveDir.x * speedOnDir, rigidBody.velocity.y, moveDir.z * speedOnDir);
                        float curSpeed = rigidBody.velocity.magnitude;
                        if (curSpeed < moveSpeed)
                        {
                            rigidBody.AddForce(forceDir * moveForce);
                        }
                        else
                        {
                            rigidBody.velocity = moveSpeed / curSpeed * rigidBody.velocity;
                        }
                    }
                }
            }
            else
            {  //在动作中,处理动作的逻辑
                RollProc();
            }
            lastInputDir = inputDir;
            lastFrameInFall = false;
        }
        else
        {   //在空中
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
                        playerAni.SetAnimation(PlayerAniType.Fall, PlayerAniDir.Front, 0.5f);
                }
                OnActionDone();
                if (inputDir != EightDir.Empty)
                {
                    //空中给一点点水平移动的力
                    Vector3 forceDir = Quaternion.Euler(0, cameraFollow.cameraYaw, 0) * Vector3.forward;
                    float speedOnDir = Vector3.Dot(forceDir, rigidBody.velocity);
                    if (speedOnDir < moveSpeedInAir)
                    {
                        rigidBody.AddForce(forceDir * moveForceInAir);
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
    DateTime startFallTime = DateTime.Now;
    bool lastFrameInFall = false;


    //根据镜头方向,设置人物的八个方向
    void SetEightOrientation(EightDir dir)
    {
        switch (dir)
        {
            case EightDir.Front:
                this.orientation = cameraFollow.cameraYaw;
                break;
            case EightDir.FrontLeft:
                this.orientation = cameraFollow.cameraYaw - 45;
                break;
            case EightDir.Left:
                this.orientation = cameraFollow.cameraYaw - 90;
                break;
            case EightDir.BackLeft:
                this.orientation = cameraFollow.cameraYaw - 135;
                break;
            case EightDir.Back:
                this.orientation = cameraFollow.cameraYaw + 180;
                break;
            case EightDir.BackRight:
                this.orientation = cameraFollow.cameraYaw + 135;
                break;
            case EightDir.Right:
                this.orientation = cameraFollow.cameraYaw + 90;
                break;
            case EightDir.FrontRight:
                this.orientation = cameraFollow.cameraYaw + 45;
                break;
            default:
                break;
        }
    }

    bool inRoll = false;
    Vector3 rollForceDir;
    void StartRollProc(float yaw)
    {
        inRoll = true;
        rollForceDir = Quaternion.Euler(10, yaw, 0) * Vector3.forward;
    }

    void StopRollProc()
    {
        if (inRoll)
        {
            inRoll = false;
            rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, 0);
        }
    }

    void RollProc()
    {
        if (inRoll)
        {
            float curSpeed = rigidBody.velocity.magnitude;
            if (curSpeed < rollSpeed)
            {
                rigidBody.AddForce(rollForceDir * moveForce);
            }
            else
            {
                rigidBody.velocity = rollSpeed / curSpeed * rigidBody.velocity;
            }
        }
    }



}
