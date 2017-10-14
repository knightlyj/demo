using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    WatchPoint watchPoint = null;

    new Camera camera;
    void Awake()
    {
        cameraDistance = minCameraDistance;
        camera = GetComponent<Camera>();
    }

	// Use this for initialization
	void Start () {
        watchPoint = GameObject.FindWithTag("WatchPoint").GetComponent<WatchPoint>();

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            cameraYaw += 180f;
        }
        //更新距离
        UpdateCameraDistance();

        if (Input.GetKeyDown(KeyboardInput.LockTarget))
        {
            if (target == null)
                LockTarget();
            else
                UnLockTarget();
        }

        if (target != null)
        { //锁定了目标
            UpdateLockedCamera();
        }
        else
        { //没有锁定目标,镜头自由转动
            UpdateFreeCamera();
        }
    }
    
    public float cameraPitch = 0;
    public float cameraYaw = 0;
    float smoothPitch = 0;
    float smoothYaw = 0;
    [SerializeField]
    float minPitchStep = 1;
    [SerializeField]
    float minYawStep = 1;
    [SerializeField]
    float smoohFactor = 2f;

    [SerializeField]
    LayerMask groundLayerMask = 0;
    //根据角色朝向和位置,以及镜头的yaw和pitch,调整镜头
    void SmoothCamera()
    {
        if (camera == null)
            return;
        //smooth pitch 
        float pitchDistance = Mathf.Abs(cameraPitch - smoothPitch);
        float pitchStep = pitchDistance * smoohFactor; 
        if (pitchStep < minPitchStep)
            pitchStep = minPitchStep;
        smoothPitch = CommonHelper.AngleTowards(smoothPitch, cameraPitch, pitchStep * Time.deltaTime);
        if (smoothPitch > 180) //角度限制在-180~180
            smoothPitch -= 360; 

        //smooth yaw
        float yawDist = Mathf.Abs(cameraYaw - smoothYaw);
        yawDist = Mathf.Min(yawDist, 360 - yawDist);  //角度要环形计算
        float yawStep = yawDist * smoohFactor;
        if (yawStep < minYawStep)
            yawStep = minYawStep;
        smoothYaw = CommonHelper.AngleTowards(smoothYaw, cameraYaw, yawStep * Time.deltaTime, true);
        if (this.smoothYaw > 360 || this.smoothYaw < 0)
            this.smoothYaw = smoothYaw - Mathf.Floor(smoothYaw / 360f) * 360f; //角度限制在0~360

        Quaternion rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);
        Vector3 watchDir = rotation * Vector3.forward;
        Vector3 realPoint = watchPoint.realWatchPoint;
        //这里用镜头尺寸的box检测碰撞,这样只要镜头没有插入ground,就不会穿帮
        RaycastHit hit;
        if (Physics.BoxCast(realPoint, new Vector3(0.5f,0.5f, 0.1f), watchDir * -1, out hit, rotation, cameraDistance, groundLayerMask))
        { //有碰撞,按碰撞点的距离调整位置
            camera.transform.position = realPoint - watchDir * (hit.distance - 0.2f);
        }
        else
        {
            //没有碰撞,按设置的镜头距离调整位置
            camera.transform.position = realPoint - watchDir * cameraDistance;
        }
        //调整镜头角度
        camera.transform.rotation = rotation;
    }

    void UpdateCameraDistance()
    {
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
    }

    [SerializeField]
    float minCameraDistance = 3.0f;
    [SerializeField]
    float maxCameraDistance = 7.0f;
    float cameraDistance = 7.0f; //镜头距离角色的距离

    float mouseRatio = 3f;
    float wheelRatio = 100f;
    void UpdateFreeCamera()  //更新自由镜头
    {
        //更新角度
        float deltaYaw = Input.GetAxis("Mouse X") * mouseRatio;
        this.cameraYaw += deltaYaw;
        if(this.cameraYaw > 360 || this.cameraYaw < 0)
            this.cameraYaw = cameraYaw - Mathf.Floor(cameraYaw / 360f) * 360f; //角度限制在0~360

        float deltaPith = Input.GetAxis("Mouse Y") * mouseRatio;
        this.cameraPitch -= deltaPith;
        this.cameraPitch = cameraPitch - Mathf.Floor(cameraPitch / 360f) * 360f; //角度限制在-180~180
        if (this.cameraPitch >= 180f)
            this.cameraPitch -= 360f;
        //镜头pitch范围限制
        if (this.cameraPitch > 89f)
            this.cameraPitch = 89f;
        else if (this.cameraPitch < -89f)
            this.cameraPitch = -89f;

        SmoothCamera();
    }

    
    Player target;
    void LockTarget()
    {
        GameObject[] allAI = GameObject.FindGameObjectsWithTag("AI");
        if(allAI != null)
        {
            foreach (GameObject go in allAI)
            {
                Vector3 dir = go.transform.position - this.transform.position;
                if (Vector3.Angle(this.transform.forward, dir) < 55)
                {

                    if (target != null)
                    { //原来有目标,退订事件
                        target.onPlayerDestroy -= this.OnTargetDestory;
                    }

                    //获取新目标,并订阅事件
                    target = go.GetComponent<Player>();
                    target.onPlayerDestroy += this.OnTargetDestory;

                    break;
                }
            }
        }
    }

    void UnLockTarget()
    {
        target.onPlayerDestroy -= this.OnTargetDestory;
        target = null;
    }

    void OnTargetDestory()
    {
        target.onPlayerDestroy -= this.OnTargetDestory;
        target = null;
    }

    void UpdateLockedCamera()
    {
        //镜头yaw朝向目标
        Vector3 toTarget = target.transform.position - watchPoint.realWatchPoint;
        toTarget.y = 0;

        float aCos = Mathf.Acos(toTarget.z / toTarget.magnitude);
        cameraYaw = aCos / Mathf.PI * 180;
        if (toTarget.x < 0)
            cameraYaw = -cameraYaw;

        //更新pitch
        float deltaPith = Input.GetAxis("Mouse Y") * mouseRatio;
        this.cameraPitch -= deltaPith;
        this.cameraPitch = cameraPitch - Mathf.Floor(cameraPitch / 360f) * 360f; //角度限制在-180~180
        if (this.cameraPitch >= 180f)
            this.cameraPitch -= 360f;
        //镜头pitch范围限制
        if (this.cameraPitch > 89f)
            this.cameraPitch = 89f;
        else if (this.cameraPitch < -89f)
            this.cameraPitch = -89f;

        SmoothCamera();
    }
}
