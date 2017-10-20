using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public WatchPoint watchPoint = null;

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
        
        UpdateCameraAngle();
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

    bool useSmoothCamera = true;
    //根据角色朝向和位置,以及镜头的yaw和pitch,调整镜头
    void SmoothCamera()
    {
        if (camera == null)
            return;
        Quaternion rotation;
        if (useSmoothCamera)  
        {
            //smooth pitch 
            smoothPitch = CommonHelper.AngleTowardsByDiff(smoothPitch, cameraPitch, smoohFactor * Time.deltaTime, minPitchStep * Time.deltaTime);
            //if (smoothPitch > 180) //角度限制在-180~180
            //    smoothPitch -= 360;

            //smooth yaw
            smoothYaw = CommonHelper.AngleTowardsByDiff(smoothYaw, cameraYaw, smoohFactor * Time.deltaTime, minYawStep * Time.deltaTime);

            rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);
        }
        else
        {
            smoothPitch = cameraPitch;
            smoothYaw = cameraYaw;
            rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        }
        
        Vector3 watchDir = rotation * Vector3.forward;
        //这里用镜头尺寸的box检测碰撞,这样只要镜头没有插入ground,就不会穿帮
        RaycastHit hit;
        if (Physics.BoxCast(watchPoint.transform.position, new Vector3(0.5f,0.5f, 0.1f), watchDir * -1, out hit, rotation, cameraDistance, groundLayerMask))
        { //有碰撞,按碰撞点的距离调整位置
            camera.transform.position = watchPoint.transform.position - watchDir * (hit.distance - 0.2f);
        }
        else
        {
            //没有碰撞,按设置的镜头距离调整位置
            camera.transform.position = watchPoint.transform.position - watchDir * cameraDistance;
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

    public bool locked = false;
    void UpdateCameraAngle()  //更新自由镜头
    {
        //更新角度
        if (!locked)
        {   //没有锁定时,就根据鼠标输入改变yaw
            float deltaYaw = Input.GetAxis("Mouse X") * mouseRatio;
            this.cameraYaw += deltaYaw;
        }

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
