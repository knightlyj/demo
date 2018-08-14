using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;

public class CameraControl : MonoBehaviour
{
    new Camera camera;
    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    Vector3 colisionBox;
    // Use this for initialization
    void Start()
    {
        noColPos = transform.position;

        float halfFov = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        colisionBox.y = camera.nearClipPlane * Mathf.Tan(halfFov) + 0.05f;
        colisionBox.x = colisionBox.y * camera.aspect + 0.05f;
        colisionBox.z = 0.02f;
    }

    public enum ViewMode
    {
        Normal,
        Shoot,
    }

    Vector3 watchPoint;
    ViewMode curViewMode = ViewMode.Normal;
    float viewWeight = 1f;
    // Update is called once per frame
    void Update()
    {
        //更新watch point位置
        Player localPlayer = GlobalVariables.localPlayer;
        if (localPlayer != null)
        {
            Transform sight = localPlayer.GetComponent<CreatureCommon>().sight;
            watchPoint = sight.position;

            if (viewWeight != 1)
            {
                viewWeight = CommonHelper.FloatTowards(viewWeight, 1f, Time.deltaTime * 5);
                if (curViewMode == ViewMode.Shoot)
                {   //从普通视角切换到射击视角
                    UpdateShootView();
                    UpdateNormalView();
                    transform.position = shootViewPos * viewWeight + normalViewPos * (1 - viewWeight);
                }
                else
                {
                    //从设计视角切换到普通视角
                    UpdateShootView();
                    UpdateNormalView();
                    transform.position = normalViewPos * viewWeight + shootViewPos * (1 - viewWeight);
                }
            }
            else
            {
                if (curViewMode == ViewMode.Shoot)
                {   //更新射击视角
                    UpdateShootView();
                    transform.position = shootViewPos;
                }
                else
                {
                    //更新普通视角
                    UpdateNormalView();
                    transform.position = normalViewPos;
                }
            }
        }
        fixedCount = 0;
    }

    int fixedCount = 0;
    void FixedUpdate()
    {
        fixedCount++;
    }

    //camera 当前 yaw和pitch
    [HideInInspector]
    public float cameraPitch = 0;
    [HideInInspector]
    public float cameraYaw = 0;
    public const float maxPitch = 75f;  //最大pitch

    [SerializeField]
    LayerMask cameraCollideLayer = 0;  //镜头碰撞的layer

    /// <summary>
    /// 输入处理
    /// </summary>
    float mouseRatio = 3f;
    float joystickRatio = 200f;
    void UpdateInput()
    {
        if (Application.platform == RuntimePlatform.Android || GlobalVariables.mobileUIOnPC)
        {
            UpdateInputOnMobile();
        }
        else
        {
            UpdateInputOnPC();
        }
    }

    const float maxPadYawSpeed = 1.5f;
    float _padYawSpeed = 0f;
    float gamePadYawSpeed
    {
        set
        {
            _padYawSpeed = value;
            if (_padYawSpeed < -maxPadYawSpeed)
                _padYawSpeed = -maxPadYawSpeed;
            else if (_padYawSpeed > maxPadYawSpeed)
                _padYawSpeed = maxPadYawSpeed;
        }
        get
        {
            return _padYawSpeed;
        }
    }

    const float maxPadPitchSpeed = 1f;
    float _padPitchSpeed = 0f;
    float gamePadPitchSpeed
    {
        set
        {
            _padPitchSpeed = value;
            if (_padPitchSpeed < -maxPadPitchSpeed)
                _padPitchSpeed = -maxPadPitchSpeed;
            else if (_padPitchSpeed > maxPadPitchSpeed)
                _padPitchSpeed = maxPadPitchSpeed;
        }
        get
        {
            return _padPitchSpeed;
        }
    }
    void UpdateInputOnPC()
    {
        bool menuOpened = GlobalVariables.menuOpened;
        //计算鼠标输入
        float mouseYaw = menuOpened ? 0 : Input.GetAxis("Mouse X") * mouseRatio;
        float mousePith = menuOpened ? 0 : Input.GetAxis("Mouse Y") * mouseRatio;
        //计算手柄输入
        float gamePadX = Input.GetAxis(GamePadInput.cameraX);
        float gamepadY = Input.GetAxis(GamePadInput.cameraY);

        float padTurnAcc = Time.deltaTime * 5;
        if (gamePadX > GamePadInput.joystickThreshold)
            gamePadYawSpeed -= padTurnAcc;
        else if (gamePadX < -GamePadInput.joystickThreshold)
            gamePadYawSpeed += padTurnAcc;
        else
            gamePadYawSpeed = 0;

        if (gamepadY > GamePadInput.joystickThreshold)
            gamePadPitchSpeed += padTurnAcc;
        else if (gamepadY < -GamePadInput.joystickThreshold)
            gamePadPitchSpeed -= padTurnAcc;
        else
            gamePadPitchSpeed = 0;


        float deltaPadYaw = gamePadYawSpeed * joystickRatio * Time.deltaTime;
        float deltaPadPitch = gamePadPitchSpeed * joystickRatio * Time.deltaTime;

        float deltaYaw = mouseYaw + deltaPadYaw;
        float deltaPitch = -(mousePith + deltaPadPitch);

        if (deltaYaw != 0 || deltaPitch != 0)
        {
            resetNormalCamera = false;
        }

        this.cameraYaw += deltaYaw;
        this.cameraPitch += deltaPitch;

        //镜头pitch范围限制
        if (this.cameraPitch > maxPitch)
            this.cameraPitch = maxPitch;
        else if (this.cameraPitch < -maxPitch)
            this.cameraPitch = -maxPitch;
    }

    void UpdateInputOnMobile()
    {
        const float turnRatio = 200f;
        Vector2 turn;
        turn.x = CrossPlatformInputManager.GetAxis("CameraX");
        turn.y = -CrossPlatformInputManager.GetAxis("CameraY");

        float deltaYaw = turn.x * Time.deltaTime * turnRatio;
        float deltaPitch = turn.y * Time.deltaTime * turnRatio;

        if (deltaYaw != 0 || deltaPitch != 0)
        {
            resetNormalCamera = false;
        }

        this.cameraYaw += deltaYaw;
        this.cameraPitch += deltaPitch;

        //镜头pitch范围限制
        if (this.cameraPitch > maxPitch)
            this.cameraPitch = maxPitch;
        else if (this.cameraPitch < -maxPitch)
            this.cameraPitch = -maxPitch;
    }

    //****************普通视角,近战使用*************************
    void UpdateNormalView()
    {
        Player localPlayer = GlobalVariables.localPlayer;
        if (localPlayer.targetId > 0)
        {   //锁定状态,追踪目标
            UpdateLockedCamera();
        }
        else
        {   //非锁定状态,根据输入更新角度
            UpdateFreeCamera();
        }
    }


    Vector3 noColPos;  //普通镜头,无碰撞时的位置
    void UpdateFreeCamera()  //更新镜头角度
    {
        //按照无碰撞时的位置,计算镜头到玩家的角度,这样横向移动就会带动镜头旋转
        Vector3 toWatchPoint = watchPoint - noColPos;
        if (resetNormalCamera)
        {
            cameraYaw = CommonHelper.AngleTowardsByDiff(cameraYaw, resetYaw, Time.deltaTime * 5, 0.5f);
            if (cameraYaw == resetYaw)
            {
                resetNormalCamera = false;
            }
        }
        else
        {
            //计算从当前的无碰撞位置到player的yaw
            this.cameraYaw = CommonHelper.YawOfVector3(toWatchPoint);

            UpdateInput();
        }
        //碰撞检测
        NormalViewCollide();
    }

    void UpdateLockedCamera()
    {
        //这里在距离过近,接近0时,会导致float为nan,不过正常情况下不会出现位置重叠,暂不处理
        Player localPlayer = GlobalVariables.localPlayer;
        Player target = UnityHelper.GetLevelManager().GetPlayer(localPlayer.targetId);
        Transform aim = target.GetComponent<CreatureCommon>().aim;
        Vector3 toTarget = aim.position - watchPoint;
        float targetPitch = -Mathf.Asin(toTarget.y / toTarget.magnitude) * Mathf.Rad2Deg;
        targetPitch += 15f;

        //镜头pitch范围限制
        if (targetPitch > maxPitch)
            targetPitch = maxPitch;
        else if (targetPitch < -maxPitch)
            targetPitch = -maxPitch;

        float targetYaw = CommonHelper.YawOfVector3(toTarget);

        float ratio = 0.08f * fixedCount;
        cameraYaw = CommonHelper.AngleTowardsByDiff(cameraYaw, targetYaw, ratio, 0.01f);
        cameraPitch = CommonHelper.AngleTowardsByDiff(cameraPitch, targetPitch, ratio, 0.01f);

        //碰撞检测
        NormalViewCollide();
    }

    const float maxNormalViewDist = 5.0f; //最大镜头距离
    //普通视角,镜头碰撞处理
    void NormalViewCollide()
    {
        //碰撞检测
        Quaternion rotation = Quaternion.Euler(this.cameraPitch, this.cameraYaw, 0);
        Vector3 watchDir = rotation * Vector3.forward;
        //这里用镜头尺寸的box检测碰撞,这样只要镜头没有插入ground,就不会穿帮
        RaycastHit hit;
        float distance = maxNormalViewDist;
        if (Physics.BoxCast(watchPoint, colisionBox, -watchDir, out hit, rotation, maxNormalViewDist, cameraCollideLayer))
        { //有碰撞,按碰撞点的距离调整位置
            distance = hit.distance;
            normalViewPos = watchPoint - watchDir * distance;
        }
        normalViewPos = watchPoint - watchDir * distance - rotation * Vector3.forward * camera.nearClipPlane;

        //调整镜头角度和位置
        transform.rotation = rotation;
        noColPos = watchPoint - watchDir * maxNormalViewDist;
    }
    Vector3 normalViewPos;  //普通视角计算出的位置

    //普通视角,自由镜头时,可以reset视角
    bool resetNormalCamera = false;
    float resetYaw = 0;
    public void ResetCamera(float yaw)
    {
        this.resetNormalCamera = true;
        this.resetYaw = yaw;
    }


    //更新射击镜头位置
    const float shootViewDistance = 1.0f;
    void UpdateShootView()
    {
        UpdateInput();
        RaycastHit hit;
        Quaternion cameraRot = Quaternion.Euler(this.cameraPitch, this.cameraYaw, 0);
        Vector3 backColDir = Quaternion.Euler(0, this.cameraYaw, 0) * -Vector3.forward;

        float a = 1 - Mathf.Abs(cameraPitch) / maxPitch;
        float backDistance = a * 0.25f + 0.25f;

        Vector3 backPos = watchPoint + backColDir * backDistance;


        Vector3 rightColDir = Quaternion.Euler(0, this.cameraYaw + 90f, 0) * Vector3.forward;
        float rightDistance = shootViewDistance;
        if (Physics.BoxCast(backPos, colisionBox, rightColDir, out hit, cameraRot, rightDistance, cameraCollideLayer))
        {
            rightDistance = hit.distance;
        }

        float b = rightDistance / shootViewDistance;
        if (b < 0.5f)
            backPos += Vector3.up * (1 - b * 2) * 1.1f;

        Vector3 rightPos = rightColDir * rightDistance - cameraRot * Vector3.forward * camera.nearClipPlane;
        shootViewPos = backPos + rightPos;
        transform.rotation = cameraRot;
    }
    Vector3 shootViewPos;  //射击视角计算出的位置


    public void SwitchCameraMode(ViewMode mode)
    {
        if (mode == curViewMode)
            return;

        curViewMode = mode;

        if (curViewMode == ViewMode.Normal)
        {
            Quaternion cameraRot = Quaternion.Euler(this.cameraPitch, this.cameraYaw, 0);
            noColPos = watchPoint - cameraRot * Vector3.forward;
        }

        //权重置0
        if (viewWeight == 1)
            viewWeight = 0f;
        else  //如果上一次还没切换完成,就反转权重
            viewWeight = 1 - viewWeight;
    }
}
