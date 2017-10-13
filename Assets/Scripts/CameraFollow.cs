using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    WatchPoint watchPoint = null;

    new Camera camera;
    void Awake()
    {
        camera = GetComponent<Camera>();
    }

	// Use this for initialization
	void Start () {
        watchPoint = GameObject.FindWithTag("WatchPoint").GetComponent<WatchPoint>();

    }
	
	// Update is called once per frame
	void Update () {
        UpdateCamera();

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
    float cameraMoveStep = 10f;
    [SerializeField]
    LayerMask groundLayerMask;
    //根据角色朝向和位置,以及镜头的yaw和pitch,调整镜头
    void SmoothCamera()
    {
        if (camera == null)
            return;
        //smooth pitch
        float pitchDistance = Mathf.Abs(cameraPitch - smoothPitch);
        float pitchStep = pitchDistance * 1;
        if (pitchStep < minPitchStep)
            pitchStep = minPitchStep;
        smoothPitch = CommonHelper.AngleTowards(smoothPitch, cameraPitch, pitchStep);

        //smooth yaw
        float yawDist = Mathf.Abs(cameraYaw - smoothYaw);
        float yawStep = yawDist * 1;
        if (yawStep < minYawStep)
            yawStep = minYawStep;
        smoothYaw = CommonHelper.AngleTowards(smoothYaw, cameraYaw, yawStep);

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

    float minCameraDistance = 7.0f;
    float maxCameraDistance = 7.0f;
    float cameraDistance = 7.0f; //镜头距离角色的距离

    float mouseRatio = 3f;
    float wheelRatio = 100f;
    void UpdateCamera()
    {
        //更新角度
        float deltaYaw = Input.GetAxis("Mouse X") * mouseRatio;
        float deltaPith = Input.GetAxis("Mouse Y") * mouseRatio;
        this.cameraYaw += deltaYaw;
        this.cameraPitch -= deltaPith;
        this.cameraPitch = cameraPitch - Mathf.Floor(cameraPitch / 360f) * 360f; //角度限制在-180~180
        if (this.cameraPitch >= 180f)
            this.cameraPitch -= 360f;
        //镜头pitch范围限制
        if (this.cameraPitch > 89f)
            this.cameraPitch = 89f;
        else if (this.cameraPitch < -89f)
            this.cameraPitch = -89f;

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

        SmoothCamera();
    }
}
