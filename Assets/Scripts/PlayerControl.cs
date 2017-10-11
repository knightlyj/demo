using UnityEngine;
using System.Collections;

public static class KeyboardInput
{
    public static KeyCode Forward = KeyCode.W;
    public static KeyCode Backward = KeyCode.S;
    public static KeyCode Left = KeyCode.A;
    public static KeyCode Right = KeyCode.D;


    public static KeyCode Run = KeyCode.LeftShift;
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

public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    Transform watchPoint = null;
    [SerializeField]
    Transform groundCheck = null;

    //各种速度配置
    [SerializeField]
    float rollSpeed = 3;
    [SerializeField]
    float jumpSpeed = 7;
    [SerializeField]
    float WalkSpeed = 2;
    [SerializeField]
    float runSpeed = 4;

    PlayerAni playerAni = null;
    Rigidbody rigidBody = null;
    void Awake()
    {
        playerAni = GetComponent<PlayerAni>();
        rigidBody = GetComponent<Rigidbody>();
    }

    //镜头
    Camera camera = null;
    // Use this for initialization
    void Start()
    {
        Physics.gravity = Physics.gravity * 3;
        playerAni.onActionDone += OnActionDone;
        playerAni.ChangeWeaponModel("2Hand-Axe", MainHandWeaponType.TwoHandAxe, null, OffHandWeaponType.Empty);
        //镜头设置及初始化
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        this.orientation = transform.eulerAngles.y;
        cameraYaw = this.orientation;
        AdaptCamera();
    }

    void OnDestroy()
    {
        playerAni.onActionDone -= OnActionDone;
    }

    //地面检测
    bool grounded = true;
    // Update is called once per frame
    void Update()
    {
        UpdateCamera(); //更新镜头
        Simulate(); //更新操作
        SmoothOrientation(); //角色朝向平滑过渡
    }

    [SerializeField]
    LayerMask groundLayerMask;
    void FixedUpdate()
    {
        //落地检测
        Collider[] hitGround = Physics.OverlapBox(groundCheck.position, new Vector3(0.23f, 0.1f, 0.23f), Quaternion.identity, groundLayerMask);

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

    float minCameraDistance = 3.0f;
    float maxCameraDistance = 7.0f;
    float cameraDistance = 3.0f; //镜头距离角色的距离
    float cameraYaw = 0; //镜头的全局yaw
    float cameraPitch = 0;
    //根据角色朝向和位置,以及镜头的yaw和pitch,调整镜头
    void AdaptCamera()
    {
        if (camera == null)
            return;

        //计算旋转,镜头相对角色的yaw+角色本身yaw,就是镜头的global yaw
        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        //调整镜头位置
        camera.transform.position = watchPoint.position - rotation * Vector3.forward * cameraDistance;

        //调整镜头角度
        camera.transform.rotation = rotation;
    }

    float mouseRatio = 3f;
    float wheelRatio = 100f;
    void UpdateCamera()
    {
        //更新角度
        float deltaYaw = Input.GetAxis("Mouse X") * mouseRatio;
        float deltaPith = Input.GetAxis("Mouse Y") * mouseRatio;
        this.cameraYaw += deltaYaw;
        this.cameraPitch -= deltaPith;
        this.cameraPitch = cameraPitch - Mathf.Floor(cameraPitch / 360f) * 360f; 
        if (cameraPitch > 180f)//范围在-180~180之间
            cameraPitch -= 360f;
        //镜头pitch范围限制
        if (this.cameraPitch > 89f)
            this.cameraPitch = 89f;
        else if (this.cameraPitch < -89)
            this.cameraPitch = -89;

        //更新距离
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (wheel != 0)
        {
            cameraDistance -= wheel * wheelRatio * Time.deltaTime;
            if (cameraDistance > maxCameraDistance)
                cameraDistance = maxCameraDistance;
            if (cameraDistance < minCameraDistance)
                cameraDistance = minCameraDistance;
        }

        AdaptCamera();
    }

    //角色朝向平滑过渡
    float showOrientation = 0; //实际显示的朝向
    const float smoothOriBaseStepLen = 300;
    float smoothOriStepLen = 0;
    bool inSmoothOri = false;
    void StartSmoothOrientation()
    {  //这里计算环形插值
        float diff = this.orientation - showOrientation;
        float absDiff = Mathf.Abs(diff);
        if (absDiff > 180) { }

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
    }

    void Simulate()
    {
        if (grounded)
        {   //在地上

            EightDir dirEnum = EightDir.Empty;
            //前后
            if (Input.GetKey(KeyboardInput.Forward))
            {
                dirEnum = EightDir.Front;
            }
            else if (Input.GetKey(KeyboardInput.Backward))
            {
                dirEnum = EightDir.Back;
            }

            //左右
            if (Input.GetKey(KeyboardInput.Left))
            {
                if (dirEnum == EightDir.Front)
                    dirEnum = EightDir.FrontLeft;
                else if (dirEnum == EightDir.Back)
                    dirEnum = EightDir.BackLeft;
                else
                    dirEnum = EightDir.Left;
            }
            else if (Input.GetKey(KeyboardInput.Right))
            {
                if (dirEnum == EightDir.Front)
                    dirEnum = EightDir.FrontRight;
                else if (dirEnum == EightDir.Back)
                    dirEnum = EightDir.BackRight;
                else
                    dirEnum = EightDir.Right;
            }

            if (!inAction) //不在特殊动作中
            {
                if (Input.GetKeyDown(KeyboardInput.Roll))
                {
                    SetEightOrientation(dirEnum);

                    playerAni.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
                    Quaternion rotation = Quaternion.AngleAxis(this.orientation, Vector3.up);
                    rigidBody.velocity = rotation * Vector3.forward * rollSpeed;
                    inAction = true;
                }
                else if (Input.GetKeyDown(KeyboardInput.Jump))
                {
                    playerAni.SetAnimation(PlayerAniType.JumpUp, PlayerAniDir.Front);
                    rigidBody.velocity = new Vector3(rigidBody.velocity.x, jumpSpeed, rigidBody.velocity.z);
                    inAction = true;
                }
                else
                { //没有做任何动作,则跑/走
                    if (dirEnum == EightDir.Empty)
                    {   //没有按方向键,则停下来
                        rigidBody.velocity = new Vector3(0, 0, 0);
                        if (playerAni.GetAnimation() != PlayerAniType.Idle)
                            playerAni.SetAnimation(PlayerAniType.Idle);
                    }
                    else
                    {
                        SetEightOrientation(dirEnum);
                        float moveSpeed = WalkSpeed;
                        if (Input.GetKey(KeyboardInput.Run))
                        { //跑
                            moveSpeed = runSpeed;
                            if (playerAni.GetAnimation() != PlayerAniType.Run)
                                playerAni.SetAnimation(PlayerAniType.Run);
                        }
                        else
                        {
                            if (playerAni.GetAnimation() != PlayerAniType.Walk)
                                playerAni.SetAnimation(PlayerAniType.Walk);
                        }
                        Quaternion rotation = Quaternion.AngleAxis(this.orientation, Vector3.up);
                        Vector3 moveDir = rotation * Vector3.forward;
                        rigidBody.velocity = new Vector3(moveDir.x * moveSpeed, rigidBody.velocity.y, moveDir.z * moveSpeed);
                    }
                }
            }
        }
        else
        {   //在空中
            if (rigidBody.velocity.y < 0) //下落
            {
                if (playerAni.GetAnimation() != PlayerAniType.Fall)
                    playerAni.SetAnimation(PlayerAniType.Fall, PlayerAniDir.Front, 0.5f);
            }
        }
    }

    void SetEightOrientation(EightDir dir)
    {
        switch (dir)
        {
            case EightDir.Front:
                this.orientation = cameraYaw;
                break;
            case EightDir.FrontLeft:
                this.orientation = cameraYaw - 45;
                break;
            case EightDir.Left:
                this.orientation = cameraYaw - 90;
                break;
            case EightDir.BackLeft:
                this.orientation = cameraYaw - 135;
                break;
            case EightDir.Back:
                this.orientation = cameraYaw + 180;
                break;
            case EightDir.BackRight:
                this.orientation = cameraYaw + 135;
                break;
            case EightDir.Right:
                this.orientation = cameraYaw + 90;
                break;
            case EightDir.FrontRight:
                this.orientation = cameraYaw + 45;
                break;
            default:
                break;
        }
    }
}
