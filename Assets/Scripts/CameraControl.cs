using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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
        colisionBox.y = camera.nearClipPlane * Mathf.Tan(halfFov);
        colisionBox.x = colisionBox.y * camera.aspect;
        colisionBox.z = 0.1f;

    }

    // Update is called once per frame
    void Update()
    {
        //更新watch point位置
        UpdateWatchPoint();

        //更新镜头位置和角度
        LocalPlayer localPlayer = UnityHelper.FindLocalPlayer();
        if (localPlayer != null)
        {
            if (localPlayer.target == null)
            {   //非锁定状态,根据输入更新角度
                UpdateFreeCamera();
            }
            else
            {   //锁定状态,追踪目标
                UpdateLockedCamera();
            }
        }

        fixedCount = 0;

    }

    int fixedCount = 0;
    void FixedUpdate()
    {
        fixedCount++;
    }

    //***********************watch point*****************************
    Vector3 watchPoint;
    bool wpInit = false;
    Transform sight = null;
    void UpdateWatchPoint()
    {
        if (!wpInit)
        {
            LocalPlayer localPlayer = UnityHelper.FindLocalPlayer();
            if (localPlayer != null)
            {
                sight = localPlayer.GetComponent<CreatureCommon>().sight;
                watchPoint = sight.position;
                wpInit = true;
            }
        }
        else
        {
            Vector3 toPlayer = sight.position - watchPoint;
            float step = Mathf.Max(toPlayer.magnitude * fixedCount * 0.07f, 0.001f);
            watchPoint = Vector3.MoveTowards(watchPoint, sight.position, step);
        }
    }

    //camera 当前 yaw和pitch
    [HideInInspector]
    public float cameraPitch = 0;
    [HideInInspector]
    public float cameraYaw = 0;

    float cameraDistance = 5.0f;  //镜头距离
    const float maxPitch = 75f;  //最大pitch

    //***********************镜头角度****************************
    float mouseRatio = 3f;
    float joystickRatio = 4f;
    Vector3 noColPos;
    
    void UpdateFreeCamera()  //更新镜头角度
    {
        Vector3 toWatchPoint = watchPoint - noColPos;

        //计算鼠标输入
        float mouseYaw = Input.GetAxis("Mouse X") * mouseRatio;
        float mousePith = Input.GetAxis("Mouse Y") * mouseRatio;
        //计算手柄输入
        float gamePadYaw = Input.GetAxis(GamePadInput.cameraX);
        float gamepadPitch = Input.GetAxis(GamePadInput.cameraY);

        if (gamePadYaw > GamePadInput.joystickThreshold)
            gamePadYaw = -joystickRatio * fixedCount;
        else if (gamePadYaw < -GamePadInput.joystickThreshold)
            gamePadYaw = joystickRatio * fixedCount;
        else
            gamePadYaw = 0;

        if (gamepadPitch > GamePadInput.joystickThreshold)
            gamepadPitch = joystickRatio * fixedCount;
        else if (gamepadPitch < -GamePadInput.joystickThreshold)
            gamepadPitch = -joystickRatio * fixedCount;
        else
            gamepadPitch = 0;
        
        float deltaYaw = mouseYaw + gamePadYaw;
        float deltaPitch = -(mousePith + gamepadPitch);
        
        if (resetCamera)
        {
            if (deltaYaw != 0 || deltaPitch != 0)
            {
                resetCamera = false;
            }
            else
            {
                cameraYaw = CommonHelper.AngleTowardsByDiff(cameraYaw, resetYaw, Time.deltaTime * 5, 0.5f);
                if(cameraYaw == resetYaw)
                {
                    resetCamera = false;
                }
            }
        }
        else
        {

            //计算从当前的无碰撞位置到player的yaw
            toWatchPoint.y = 0;
            this.cameraYaw = Mathf.Acos(toWatchPoint.z / toWatchPoint.magnitude) * Mathf.Rad2Deg;
            if (toWatchPoint.x < 0)
                this.cameraYaw = -this.cameraYaw;


            this.cameraYaw += deltaYaw;
            this.cameraPitch += deltaPitch;

            //镜头pitch范围限制
            if (this.cameraPitch > maxPitch)
                this.cameraPitch = maxPitch;
            else if (this.cameraPitch < -maxPitch)
                this.cameraPitch = -maxPitch;
        }

        //碰撞检测
        Collision();
    }

    void UpdateLockedCamera()
    {
        LocalPlayer localPlayer = UnityHelper.FindLocalPlayer();
        Transform aim = localPlayer.target.GetComponent<CreatureCommon>().aim;
        Vector3 toTarget = aim.position - watchPoint;
        float targetPitch = -Mathf.Asin(toTarget.y / toTarget.magnitude) * Mathf.Rad2Deg;
        targetPitch += 15f;

        //镜头pitch范围限制
        if (targetPitch > maxPitch)
            targetPitch = maxPitch;
        else if (targetPitch < -maxPitch)
            targetPitch = -maxPitch;

        toTarget.y = 0;
        float targetYaw = Mathf.Acos(toTarget.z / toTarget.magnitude) * Mathf.Rad2Deg;
        if (toTarget.x < 0)
            targetYaw = -targetYaw;

        float ratio = 0.08f * fixedCount;
        cameraYaw = CommonHelper.AngleTowardsByDiff(cameraYaw, targetYaw, ratio, 0.01f);
        cameraPitch = CommonHelper.AngleTowardsByDiff(cameraPitch, targetPitch, ratio, 0.01f);


        //碰撞检测
        Collision();
    }

    //****************根据镜头距离和角度,与场景的碰撞,计算镜头的参数******************
    [SerializeField]
    LayerMask groundLayerMask = 0;
    void Collision()
    {
        //碰撞检测
        Quaternion rotation = Quaternion.Euler(this.cameraPitch, this.cameraYaw, 0);
        Vector3 watchDir = rotation * Vector3.forward;
        //这里用镜头尺寸的box检测碰撞,这样只要镜头没有插入ground,就不会穿帮
        RaycastHit hit;
        if (Physics.BoxCast(watchPoint, colisionBox, -watchDir, out hit, rotation, cameraDistance, groundLayerMask))
        { //有碰撞,按碰撞点的距离调整位置
            float distance = Mathf.Max(camera.nearClipPlane + 0.5f, hit.distance);
            transform.position = watchPoint - watchDir * distance;
        }
        else
        {
            //没有碰撞
            transform.position = watchPoint - watchDir * cameraDistance;
        }

        //调整镜头角度和位置
        transform.rotation = rotation;
        noColPos = watchPoint - watchDir * cameraDistance;
    }

    bool resetCamera = false;
    float resetYaw = 0;
    public void ResetCamera(float yaw)
    {
        this.resetCamera = true;
        this.resetYaw = yaw;
    }
}
