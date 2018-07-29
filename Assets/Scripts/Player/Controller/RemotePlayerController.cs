using UnityEngine;
using System.Collections;
using System;

public class RemotePlayerController : MonoBehaviour
{
    Rigidbody rigidBody = null;
    Player player = null;
    Collider collider = null;
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        rigidBody.useGravity = false;
        player = GetComponent<Player>();
        groundLayerMask = 1 << LayerMask.NameToLayer(StringAssets.groundLayerName);

        player.onAnimationEvent += this.OnAnimationEvent;
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

    void OnDestroy()
    {
        player.onAnimationEvent -= this.OnAnimationEvent;
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


        if (curLowerAniStateHash == Player.StateNameHash.die)
        {
            collider.isTrigger = true;
            rigidBody.isKinematic = true;
        }
        else
        {
            collider.isTrigger = false;
            rigidBody.isKinematic = false;
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
        if (sqrMag > 0.05f)
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

    public void Shoot(Vector3 targetPoint)
    {
        if (player.gun != null)
        {
            RaycastHit hitInfo;
            if (player.gun.Shoot(targetPoint, player.hitLayer, out hitInfo))
            {
                LevelManager lm = UnityHelper.GetLevelManager();
                Collider collider = hitInfo.collider;
                if (collider.gameObject.layer == LayerMask.NameToLayer(StringAssets.bodyLayerName))
                {
                    if (lm != null)
                        lm.CreateParticleEffect(LevelManager.ParticleEffectType.HitPlayer, hitInfo.point, hitInfo.normal);
                }
                else if (collider.gameObject.layer == LayerMask.NameToLayer(StringAssets.groundLayerName))
                {
                    if (lm != null)
                        lm.CreateParticleEffect(LevelManager.ParticleEffectType.HitGround, hitInfo.point, hitInfo.normal);
                    
                }
            }
        }
    }

    public void ApplyRemoteInfo(Protocol.PlayerInfo info)
    {
        if (info == null)
            return;
        player.healthPoint = info.hp;
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

        if (player.weaponType != info.weapon)
        {
            player.ChangeWeapon(info.weapon);
        }
        player.invincible = info.invincible;

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
            if(info.upperAniState == Player.StateNameHash.roll)
            {
                AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "roll", typeof(AudioClip));
                player.audioSource.PlayOneShot(clip, 1f);
            }
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
            if (curLowerAniStateHash == Player.StateNameHash.fall)
            {
                AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "foot2", typeof(AudioClip));
                player.audioSource.PlayOneShot(clip, 1f);
            }

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

    DateTime lastStepTime = DateTime.Now;
    DateTime lastSwingTime = DateTime.Now;
    void OnAnimationEvent(AnimationEvent aniEvent)
    {
        if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.step))
        {
            TimeSpan span = DateTime.Now - lastStepTime;
            if (span.TotalMilliseconds > 150f)
            {
                int s = UnityEngine.Random.Range(1, 4);
                AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "foot" + s, typeof(AudioClip));
                player.audioSource.PlayOneShot(clip, 0.5f);
                lastStepTime = DateTime.Now;
            }
        }
        else if (aniEvent.stringParameter.Equals(LocalPlayerController.AniEventName.startAttack))
        {
            TimeSpan span = DateTime.Now - lastSwingTime;
            if (span.TotalMilliseconds > 250f)
            {
                int r = UnityEngine.Random.Range(1, 3);
                AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "sword" + r, typeof(AudioClip));
                player.audioSource.PlayOneShot(clip, 0.5f);
                lastSwingTime = DateTime.Now;
            }
        }
    }
}
