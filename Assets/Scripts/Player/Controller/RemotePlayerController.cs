using UnityEngine;
using System.Collections;
using System;

public class RemotePlayerController : MonoBehaviour
{
    Rigidbody rigidBody = null;
    Player player = null;
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = false;
        player = GetComponent<Player>();
        groundLayerMask = 1 << LayerMask.NameToLayer(StringAssets.groundLayerName);
    }

    // Use this for initialization
    void Start()
    {
        if (tag != StringAssets.remoteplayerTag)
        {
            //???
            Debug.LogError("RemotePlayerController.Start >> tag error");
        }
    }

    // Update is called once per frame
    void Update()
    {
        SmoothOrientation(); //角色朝向平滑过渡
    }
    
    void FixedUpdate()
    {
        GroundCheck();
      
        ChaseShadow();
        ChaseAniParam();
        
        int curLowerAniStateHash;
        float curLowerNormTime;
        player.GetLowerAniState(out curLowerAniStateHash, out curLowerNormTime);
        if (curLowerAniStateHash == Player.StateNameHash.idle
               || curLowerAniStateHash == Player.StateNameHash.blockIdle
               || curLowerAniStateHash == Player.StateNameHash.aim)
        {
            if (grounded)
            {
                player.footIk = true;
            }
        }
        else
        {
            player.footIk = false;
        }
        
    }

    bool grounded = false;
    LayerMask groundLayerMask;
    float groundCheckRadius = 0.5f;
    //落地检测
    void GroundCheck()
    {
        RaycastHit hitInfo;
        Vector3 origin = transform.position + Vector3.up * 0.6f;
        if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out hitInfo, 0.15f, groundLayerMask))
        {
            grounded = true;
            rigidBody.useGravity = false;
            rigidBody.drag = 3f;
        }
        else
        {
            grounded = false;
            rigidBody.useGravity = true;
            rigidBody.drag = 0f;
        }
    }

    const float maxChaseDistance = 3.5f;
    float chaseVelocity = 10f;
    Vector3 posDiff = Vector3.zero;
    void ChaseShadow()
    {
        float sqrMag = posDiff.sqrMagnitude;
        if(sqrMag > 0.05f)
        {
            float stepLen = chaseVelocity * Time.fixedDeltaTime;

            if (stepLen * stepLen > sqrMag)
            {
                transform.position = transform.position + posDiff;
                posDiff = Vector3.zero;
            }
            else
            {
                Vector3 step = posDiff;
                step.Normalize();
                step *= stepLen;
                transform.position = transform.position + step;
                posDiff -= step;
            }

        }
    }

    //***************************角色朝向的代码******************************
    public float faceYaw //角色朝向角度,基于y轴旋转
    {
        set
        {
            _yaw = value;
            _yaw = _yaw - Mathf.Floor(_yaw / 360f) * 360f; //范围在0~360之间
            StartSmoothYaw();
        }
        get
        {
            return this._yaw;
        }
    }
    float _yaw = 0f;

    public float immediateYaw
    {
        set
        {
            _yaw = value;
            smoothYaw = value;
        }
    }

    //角色朝向平滑过渡
    float smoothYaw = 0; //实际显示的朝向
    float smoothYawStepLen = 0;
    void StartSmoothYaw()
    {  //这里计算环形插值
        float diff = CommonHelper.AngleDiffClosest(this.faceYaw, smoothYaw);
        smoothYawStepLen = Mathf.Abs(diff) * 6f;
    }
    void SmoothOrientation()
    {
        smoothYaw = CommonHelper.AngleTowards(smoothYaw, faceYaw, smoothYawStepLen * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, smoothYaw, 0);
    }


    public float tarWalkRun;
    public float tarAimUp;
    public float tarStrafeForward;
    public float tarStrafeRight;
    void ChaseAniParam()
    {
        player.walkRun = CommonHelper.FloatTowards(player.walkRun, tarWalkRun, Time.fixedDeltaTime * 5f);
        player.aimUp = CommonHelper.FloatTowards(player.aimUp, tarAimUp, Time.fixedDeltaTime * 5f);
        player.strafeForward = CommonHelper.FloatTowards(player.strafeForward, tarStrafeForward, Time.fixedDeltaTime * 5f);
        player.strafeRight = CommonHelper.FloatTowards(player.strafeRight, tarStrafeRight, Time.fixedDeltaTime * 5f);
    }
    
    public void ApplyRemoteInfo(Protocol.PlayerInfo info)
    {
        if (info == null)
            return;
        Vector3 velocity = new Vector3(info.velocityX, info.velocityY, info.velocityZ);
        Vector3 position = new Vector3(info.positionX, info.positionY, info.positionZ);
        rigidBody.velocity = velocity;
        posDiff = position - transform.position;
        float magnitude = posDiff.magnitude;
        if (magnitude > maxChaseDistance)
        {
            transform.position = transform.position + posDiff;
            posDiff = Vector3.zero;
        }
        else
        {
            chaseVelocity = magnitude * 3;
        }
        faceYaw = info.yaw;

        if(player.weaponType != info.weapon)
        {
            player.ChangeWeapon(info.weapon);
        }

        tarWalkRun = info.walkRun;
        tarAimUp = info.aimUp;
        tarStrafeForward = info.strafeForward;
        tarStrafeRight = info.strafeRight;

        int curUpperAniStateHash;
        float curUpperNormTime;
        player.GetUpperAniState(out curUpperAniStateHash, out curUpperNormTime);

        int curLowerAniStateHash;
        float curLowerNormTime;
        player.GetLowerAniState(out curLowerAniStateHash, out curLowerNormTime);

        if (info.upperAniState != curUpperAniStateHash)
        {
            player.SetUpperAniState(info.upperAniState, true, 0.1f, 0);
        }
        else
        {
            float diffTime = Mathf.Abs(info.upperAniNormTime - curUpperNormTime);
            if (diffTime > 0.15f)
            {
                player.SetUpperAniState(info.upperAniState, true, 0.1f, info.upperAniNormTime);
            }
        }
        
        if (info.lowerAniState != curLowerAniStateHash)
        {
            player.SetLowerAniState(info.lowerAniState, true, 0.1f, 0);
        }
        else
        {
            float diffTime = Mathf.Abs(info.lowerAniNormTime - curLowerNormTime);
            if (diffTime > 0.15f)
            {
                player.SetLowerAniState(info.lowerAniState, true, 0.1f, info.lowerAniNormTime);
            }
        }
        
    }
}
