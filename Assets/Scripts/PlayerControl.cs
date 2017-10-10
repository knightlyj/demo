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

    public static KeyCode ResetCamera = KeyCode.Mouse2;
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
        playerAni.onActionDone += OnActionDone;

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
        //落地检测
        Collider[] hitGround = Physics.OverlapBox(groundCheck.position, new Vector3(0.23f, 0, 0.23f), Quaternion.identity, LayerMask.NameToLayer("Ground"));
        if (hitGround == null || hitGround.Length == 0)
        {
            grounded = false;
        }
        else
        {
            grounded = true;
        }

        UpdateCamera(); //更新镜头
        UpdateInput(); //更新操作
        SmoothOrientation(); //角色朝向平滑过渡
    }


    void FixedUpdate()
    {

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
            _orientation = _orientation - Mathf.Floor(_orientation / 360f) * 360f;
            if (_orientation > 180f)
                _orientation -= 360f;//范围控制在-180~180之间
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



    //根据目标位置设置角色朝向
    void LookAt(Vector3 target)
    {

    }

    //角色旋转
    void AddOrientation(float angle)
    {

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
    {
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

        if (diff < 0)
            smoothOriStepLen *= -1;

        inSmoothOri = true;
    }
    void SmoothOrientation()
    {
        if (inSmoothOri)
        {
            showOrientation += smoothOriStepLen * Time.deltaTime;

            if (smoothOriStepLen > 0)
            {
                if (showOrientation >= this.orientation)
                {
                    showOrientation = this.orientation;
                    inSmoothOri = false;
                }
            }
            else
            {
                if (showOrientation <= this.orientation)
                {
                    showOrientation = this.orientation;
                    inSmoothOri = false;
                }
            }
            transform.eulerAngles = new Vector3(0, showOrientation, 0);
        }
    }

    //动作完成的回调
    bool inAction = false;
    void OnActionDone()
    {
        inAction = false;
        playerAni.SetAnimation(PlayerAniType.Idle);
    }

    void UpdateInput()
    {
        if (Input.GetKeyDown(KeyboardInput.ResetCamera))
        {
            cameraYaw = this.orientation;
            AdaptCamera();
        }

        if (inAction)
        {   //当前在做动作

        }
        else
        {   //没做动作,可以正常操作
            Vector3 dir = Vector3.zero; //向前为y轴方向,向右为x轴方向
            EightDir dirEnum = EightDir.Empty;
            //前后
            if (Input.GetKey(KeyboardInput.Forward))
            {
                dir.z = 1;
                dirEnum = EightDir.Front;
            }
            else if (Input.GetKey(KeyboardInput.Backward))
            {
                dir.z = -1;
                dirEnum = EightDir.Back;
            }

            //左右
            if (Input.GetKey(KeyboardInput.Left))
            {
                dir.x = -1;
                if (dirEnum == EightDir.Front)
                    dirEnum = EightDir.FrontLeft;
                else if (dirEnum == EightDir.Back)
                    dirEnum = EightDir.BackLeft;
                else
                    dirEnum = EightDir.Left;
            }
            else if (Input.GetKey(KeyboardInput.Right))
            {
                dir.x = 1;
                if (dirEnum == EightDir.Front)
                    dirEnum = EightDir.FrontRight;
                else if (dirEnum == EightDir.Back)
                    dirEnum = EightDir.BackRight;
                else
                    dirEnum = EightDir.Right;
            }

            if(dirEnum == EightDir.Empty) //如果没指定方向,则向前
            {
                dirEnum = EightDir.Front;
            }

            //得到方向向量和四元数
            if (dir.sqrMagnitude > 0.2f)
            {
                dir.Normalize();
            }
            Quaternion dirRotation = Quaternion.AngleAxis(cameraYaw, dir);

            //先特殊动作
            if (Input.GetKeyDown(KeyboardInput.Roll))
            {
                if (grounded) //在地面才可以滚
                {
                    switch (dirEnum)
                    {
                        case EightDir.Front:
                            playerAni.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
                            this.orientation = cameraYaw;
                            break;
                        case EightDir.FrontLeft:
                            playerAni.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
                            this.orientation = cameraYaw - 45f;
                            break;
                        case EightDir.Left:
                            playerAni.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
                            this.orientation = cameraYaw - 90f;
                            break;
                        case EightDir.BackLeft:
                            playerAni.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
                            this.orientation = cameraYaw - 135f;
                            break;
                        case EightDir.Back:
                            playerAni.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
                            this.orientation = cameraYaw + 180f;
                            break;
                        case EightDir.BackRight:
                            playerAni.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
                            this.orientation = cameraYaw + 135f;
                            break;
                        case EightDir.Right:
                            playerAni.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
                            this.orientation = cameraYaw + 90f;
                            break;
                        case EightDir.FrontRight:
                            playerAni.SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
                            this.orientation = cameraYaw + 45f;
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (Input.GetKeyDown(KeyboardInput.Jump))
            {

            }
            else
            { //没有做特殊动作,则处理移动
                if (grounded)
                {   //移动
                    if (Input.GetKey(KeyboardInput.Run))
                    { //跑

                    }

                    //rigidBody.velocity = dirRotation * dir * 3.0f;
                }
            }
        }
    }

}
