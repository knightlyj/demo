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

    // Use this for initialization
    void Start()
    {
        noColPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
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

    //camera 当前 yaw和pitch
    [HideInInspector]
    public float cameraPitch = 0;
    [HideInInspector]
    public float cameraYaw = 0;

    float cameraDistance = 7.0f;  //镜头距离
    const float maxPitch = 75f;  //最大pitch

    public Transform watchPoint;
    //***********************镜头角度****************************
    float mouseRatio = 3f;
    float joystickRatio = 2f;
    Vector3 noColPos;
    void UpdateFreeCamera()  //更新镜头角度
    {
        Vector3 toWatchPoint = watchPoint.position - noColPos;
        
        //计算从当前的无碰撞位置到player的yaw
        toWatchPoint.y = 0;
        this.cameraYaw = Mathf.Acos(toWatchPoint.z / toWatchPoint.magnitude) / Mathf.PI * 180;
        if (toWatchPoint.x < 0)
            this.cameraYaw = -this.cameraYaw;
        
        float deltaYaw = Input.GetAxis("Mouse X") * mouseRatio;
        float deltaPith = Input.GetAxis("Mouse Y") * mouseRatio;
        float gamePadYaw = Input.GetAxis(GamePadInput.CameraX);
        float gamepadPitch = Input.GetAxis(GamePadInput.CameraY);

        if (gamePadYaw > GamePadInput.joystickThreshold)
            gamePadYaw = -60f * Time.deltaTime;
        else if (gamePadYaw < -GamePadInput.joystickThreshold)
            gamePadYaw = 60f * Time.deltaTime;
        this.cameraYaw += deltaYaw + gamePadYaw * joystickRatio;

        if (gamepadPitch > GamePadInput.joystickThreshold)
            gamepadPitch = 60f * Time.deltaTime;
        else if (gamepadPitch < -GamePadInput.joystickThreshold)
            gamepadPitch = -60f * Time.deltaTime;
        this.cameraPitch -= deltaPith + gamepadPitch * joystickRatio;
        
        //镜头pitch范围限制
        if (this.cameraPitch > maxPitch)
            this.cameraPitch = maxPitch;
        else if (this.cameraPitch < -maxPitch)
            this.cameraPitch = -maxPitch;


        //碰撞检测
        Collision();
    }

    void UpdateLockedCamera()
    {
        LocalPlayer localPlayer = UnityHelper.FindLocalPlayer();
        Transform aim = localPlayer.target.GetComponent<CreatureCommon>().aim;
        Vector3 toTarget = aim.position - watchPoint.position;
        float targetPitch = -Mathf.Asin(toTarget.y / toTarget.magnitude) / Mathf.PI * 180;
        targetPitch += 15f;

        //镜头pitch范围限制
        if (targetPitch > maxPitch)
            targetPitch = maxPitch;
        else if (targetPitch < -maxPitch)
            targetPitch = -maxPitch;

        toTarget.y = 0;
        float targetYaw = Mathf.Acos(toTarget.z / toTarget.magnitude) / Mathf.PI * 180;
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
        if (Physics.BoxCast(watchPoint.position, new Vector3(0.5f, 0.5f, 0.1f), -watchDir, out hit, rotation, cameraDistance, groundLayerMask))
        { //有碰撞,按碰撞点的距离调整位置
            transform.position = watchPoint.position - watchDir * hit.distance;
        }
        else
        {
            //没有碰撞
            transform.position = watchPoint.position - watchDir * cameraDistance;
        }

        //调整镜头角度和位置
        transform.rotation = rotation;
        noColPos = watchPoint.position - watchDir * cameraDistance;
    }
}
