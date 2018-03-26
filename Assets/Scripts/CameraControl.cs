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
        curPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            cameraYaw += 180f;
        }
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
    
    float targetYaw; //目标yaw
    float targetPitch; //目标pitch
    Vector3 targetPos; //目标位置
    Vector3 curPos;
    public Transform watchPoint;
    //***********************镜头角度****************************
    float mouseRatio = 3f;
    void UpdateFreeCamera()  //更新镜头角度
    {
        LocalPlayer localPlayer = UnityHelper.FindLocalPlayer();
        Transform sight = localPlayer.GetComponent<CreatureCommon>().sight;

        Vector3 toPlayer = sight.position - curPos;

        //计算从当前位置到player的yaw
        toPlayer.y = 0;
        targetYaw = Mathf.Acos(toPlayer.z / toPlayer.magnitude) / Mathf.PI * 180;
        if (toPlayer.x < 0)
            targetYaw = -targetYaw;
        //计算平滑角度
        float ratio = 0.1f * fixedCount;
        //Debug.Log(cameraYaw + "," + targetYaw);
        this.cameraYaw = CommonHelper.AngleTowardsByDiff(cameraYaw, targetYaw, ratio, 0.1f);
        
        Vector3 targetDir = Quaternion.Euler(this.cameraPitch, this.targetYaw, 0) * Vector3.forward;
        //计算平滑位置
        targetPos = sight.position - targetDir * cameraDistance;
        Vector3 toTarget = targetPos - curPos;
        Vector3 smoothPos = Vector3.MoveTowards(curPos, targetPos, toTarget.magnitude * 0.06f * fixedCount);
        fixedCount = 0;

        Quaternion rotation = Quaternion.Euler(this.cameraPitch, this.cameraYaw, 0);
        //计算镜头碰撞
        Vector3 SmoothPosToPlayer = sight.position - smoothPos;
        Vector3 colDir = rotation * Vector3.forward;
        float colLength = Vector3.Dot(SmoothPosToPlayer, colDir);
        Vector3 colVec = colLength * colDir;
        Vector3 colOrigin = smoothPos + colVec;

        float deltaYaw = Input.GetAxis("Mouse X") * mouseRatio;
        this.cameraYaw += deltaYaw;
        
        float deltaPith = Input.GetAxis("Mouse Y") * mouseRatio;
        this.cameraPitch -= deltaPith;
        //镜头pitch范围限制
        if (this.cameraPitch > maxPitch)
            this.cameraPitch = maxPitch;
        else if (this.cameraPitch < -maxPitch)
            this.cameraPitch = -maxPitch;
        Quaternion inputRot = Quaternion.Euler(-deltaPith, deltaYaw, 0);
        smoothPos = colOrigin - inputRot * colVec;
        colVec.Normalize();

        //这里用镜头尺寸的box检测碰撞,这样只要镜头没有插入ground,就不会穿帮
        RaycastHit hit;
        if (Physics.BoxCast(colOrigin, new Vector3(0.5f, 0.5f, 0.1f), -colVec, out hit, rotation, colLength, groundLayerMask))
        { //有碰撞,按碰撞点的距离调整位置
            transform.position = colOrigin - colDir * hit.distance;
        }
        else
        {
            //没有碰撞,设置到平滑的位置
            transform.position = smoothPos;
        }

        //调整镜头角度和位置
        transform.rotation = rotation;
        curPos = smoothPos;
        
    }

    void UpdateLockedCamera()
    {
        LocalPlayer localPlayer = UnityHelper.FindLocalPlayer();

    }

    //****************根据镜头距离和角度,与场景的碰撞,计算镜头的参数******************
    [SerializeField]
    LayerMask groundLayerMask = 0;
    void CameraCollision()
    {
        LocalPlayer localPlayer = UnityHelper.FindLocalPlayer();
        if (localPlayer != null)
        {
            Transform sight = localPlayer.GetComponent<CreatureCommon>().sight;
            Quaternion rotation = Quaternion.Euler(this.cameraPitch, this.cameraYaw, 0);
            Vector3 watchDir = rotation * Vector3.forward;
            //这里用镜头尺寸的box检测碰撞,这样只要镜头没有插入ground,就不会穿帮
            RaycastHit hit;
            if (Physics.BoxCast(sight.position, new Vector3(0.5f, 0.5f, 0.1f), watchDir * -1, out hit, rotation, cameraDistance, groundLayerMask))
            { //有碰撞,按碰撞点的距离调整位置
                transform.position = sight.position - watchDir * (hit.distance - 0.2f);
            }
            else
            {
                //没有碰撞,按设置的镜头距离调整位置
                transform.position = sight.position - watchDir * cameraDistance;
            }

            //调整镜头角度
            transform.rotation = rotation;
        }
    }
}
