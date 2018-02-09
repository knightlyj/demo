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

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            cameraYaw += 180f;
        }
        //更新角度

        UpdateFreeCamera();
        
        //镜头碰撞
        CameraCollision();
    }

    int fixedCount = 0;
    void FixedUpdate()
    {
        fixedCount++;
        UpdateLockedCamera();
    }

    [HideInInspector]
    public float cameraPitch = 0;
    [HideInInspector]
    public float cameraYaw = 0;
    
    float cameraDistance = 7.0f;

    float maxPitch = 75f;
    //***********************镜头角度****************************
    float mouseRatio = 3f;
    void UpdateFreeCamera()  //更新镜头角度
    {
        LocalPlayer localPlayer = UnityHelper.FindLocalPlayer();
        if (localPlayer != null)
        {
            //非锁定状态,根据输入更新角度
            if (localPlayer.target == null)
            {   //没有锁定时,就根据鼠标输入改变yaw
                float deltaYaw = Input.GetAxis("Mouse X") * mouseRatio;
                this.cameraYaw += deltaYaw;

                //根据输入改变pitch
                float deltaPith = Input.GetAxis("Mouse Y") * mouseRatio;
                this.cameraPitch -= deltaPith;
                this.cameraPitch = cameraPitch - Mathf.Floor(cameraPitch / 360f) * 360f; //角度限制在-180~180
                if (this.cameraPitch >= 180f)
                    this.cameraPitch -= 360f;
                //镜头pitch范围限制
                if (this.cameraPitch > maxPitch)
                    this.cameraPitch = maxPitch;
                else if (this.cameraPitch < -maxPitch)
                    this.cameraPitch = -maxPitch;
                
            }
        }
    }
    
    void UpdateLockedCamera()
    {
        LocalPlayer localPlayer = UnityHelper.FindLocalPlayer();
        if (localPlayer != null)
        {
            if (localPlayer.target != null)
            {   //锁定状态,平滑追踪
                //计算锁定目标的yaw
                Transform aim = localPlayer.target.GetComponent<CreatureCommon>().aim;
                Transform sight = localPlayer.GetComponent<CreatureCommon>().sight;
                Vector3 toTarget = aim.position - sight.position;
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

                SmoothCamera(targetPitch, targetYaw);
                
            }
        }
    }
    
    //平滑旋转
    void SmoothCamera(float targetPitch, float targetYaw)
    { //根据fixedupdate与物理引擎step次数相同,step引入fixed计数作为系数,结果很好,没有抖动了
        //做抖动测试的结果是,0.2度以内的抖动,肉眼几乎不可见,所以主要是update间隔的物理引擎step次数不同造成的.
        float ratio = 0.1f * fixedCount;
        cameraYaw = CommonHelper.AngleTowardsByDiff(cameraYaw, targetYaw, ratio, 0.01f);
        cameraPitch = CommonHelper.AngleTowardsByDiff(cameraPitch, targetPitch, ratio, 0.01f);

        fixedCount = 0;
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
