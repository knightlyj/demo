using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Player))]
public partial class LocalPlayerController : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody rigidBody = null;

    public GameInput input; //输入
    Player player = null;
    IPlayerInput inputProc = null;


    void Awake()
    {
        player = GetComponent<Player>();
        rigidBody = GetComponent<Rigidbody>();
        groundLayerMask = 1 << LayerMask.NameToLayer(StringAssets.groundLayerName);
    }

    // Use this for initialization
    void Start()
    {
        if (tag == StringAssets.localPlayerTag)
        {
            inputProc = new LocalPlayerInput();
            inputProc.Start(player, this);
        }
        else if (tag == StringAssets.AIPlayerTag)
        {
            inputProc = new LocalAIInput();
            inputProc.Start(player, this);
        }
        else
        {
            //????
        }
        //角色朝向初始化
        this.faceYaw = transform.eulerAngles.y;

        StateInit();
    }

    void OnDestroy()
    {
        inputProc.Stop();

        EventManager.RemoveListener(EventId.PlayerDamage, player.id, this.OnPlayerDamage);
        EventManager.RemoveListener(EventId.PlayerBlock, player.id, this.OnPlayerBlock);
        EventManager.RemoveListener(EventId.PlayerRevive, player.id, this.OnPlayerRevive);
        EventManager.RemoveListener(EventId.PlayerDie, player.id, this.OnPlayerDie);

        ClearTarget();
    }

    DateTime lastLockTime = DateTime.Now;
    // Update is called once per frame
    void Update()
    {
        input.Clear();
        inputProc.UpdateInput(ref input, this);

        UpdateState();
        SmoothOrientation(); //角色朝向平滑过渡

        if (input.lockTarget && player.playerType == PlayerType.Local)
        {
            TimeSpan span = DateTime.Now - lastLockTime;
            if (span.TotalMilliseconds > 500)
            {
                lastLockTime = DateTime.Now;
                if (player.targetId > 0)
                {  //有目标,则取消
                    ClearTarget();
                }
                else
                {  //锁定目标 
                    if (!LockTarget())
                    {  //没有目标可以锁定,就重置视角
                        CameraControl cameraControl = GameObject.FindWithTag("MainCamera").GetComponent<CameraControl>();
                        cameraControl.ResetCamera(faceYaw);
                    }
                }
            }
        }

    }

    string[] targetTags = { StringAssets.AIPlayerTag, StringAssets.remoteplayerTag };
    bool LockTarget()
    {
        if (player.weaponType == WeaponType.Pistol)
        {
            return false;
        }
        else
        {
            Player target = null;
            foreach (string tag in targetTags)
            {
                float nearestSqrDist = float.PositiveInfinity;
                GameObject[] goPlayers = GameObject.FindGameObjectsWithTag(tag);
                if (goPlayers != null && goPlayers.Length > 0)
                {
                    foreach (GameObject go in goPlayers)
                    {
                        Transform trPlayer = go.transform;
                        Vector3 diff = trPlayer.position - transform.position;
                        if (diff.sqrMagnitude < 100f && diff.sqrMagnitude < nearestSqrDist)  //这里偷懒了,就用距离判断是否锁定这个目标
                        {
                            nearestSqrDist = diff.sqrMagnitude;
                            target = trPlayer.GetComponent<Player>();
                        }
                    }
                }
            }

            if (target)
            {
                SetTarget(target);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    void OnTargetDestroy(System.Object sender, System.Object eventArg)
    {
        ClearTarget();
    }

    public void SetTarget(Player target)
    {
        if (player.playerType == PlayerType.Local)
        {
            EventManager.AddListener(EventId.RemovePlayer, target.id, this.OnTargetDestroy);
            UIManager um = UnityHelper.GetUIManager();
            if (um)
            {
                um.SetLockTarget(target.transform);
            }
            player.targetId = target.id;
        }
    }

    public void ClearTarget()
    {
        if (player.playerType == PlayerType.Local && player.targetId > 0)
        {
            UIManager um = UnityHelper.GetUIManager();
            if (um)
            {
                um.SetLockTarget(null);
            }
            EventManager.RemoveListener(EventId.RemovePlayer, player.targetId, this.OnTargetDestroy);
            player.targetId = -1;
        }
    }

    void FixedUpdate()
    {
        GroundCheck();

        FixedUpdateState();
        UpdateEnergy();

    }

    //*****************************************
    [HideInInspector]
    public bool grounded = false;
    LayerMask groundLayerMask;
    [HideInInspector]
    public Vector3 groundNormal;
    float groundCheckRadius = 0.5f;
    //落地检测
    void GroundCheck()
    {
        RaycastHit hitInfo;
        Vector3 origin = transform.position + Vector3.up * 0.6f;
        if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out hitInfo, 0.15f, groundLayerMask))
        {
            rigidBody.drag = 6f;
            grounded = true;
            groundNormal = hitInfo.normal;
        }
        else
        {
            rigidBody.drag = 0f;
            grounded = false;
            groundNormal = Vector3.up;
        }
    }

    //***************************角色朝向的代码******************************
    public float faceYaw //角色朝向角度,基于y轴旋转
    {
        set
        {
            _yaw = value;
            _yaw = _yaw - Mathf.Floor(_yaw / 360f) * 360f; //范围在0~360之间
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
    float smoothYawStepLen = 600f;
    void SmoothOrientation()
    {
        smoothYaw = CommonHelper.AngleTowards(smoothYaw, faceYaw, smoothYawStepLen * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, smoothYaw, 0);
    }

    //模拟地面
    [HideInInspector]
    public DateTime lastRunTime = DateTime.Now;
    [HideInInspector]
    public float moveSpeed = Player.walkSpeed;

    [HideInInspector]
    public int comboCount = 0;
    [HideInInspector]
    public DateTime lastAttackDoneTime = DateTime.Now;

    /// <summary>
    /// /////////////////////////////////////////////////
    /// </summary>
    [HideInInspector]
    public float cantRunTime = 0f;  //精力空了以后一段时间内不能跑步
    [HideInInspector]
    public float energyRecoverDelay = 0f;  //动作的时间,一定时间内不恢复精力
    //尝试消耗精力,如果不够则扣掉剩下的全部精力,精力不够时攻击动作的伤害会打折扣
    public float EnergyCost(float cost)
    {
        if (player.energyPoint <= 0)
            return 0;

        //这里有点trick,用精力消耗来判断跑步和其他动作.
        if (cost > 5f)
        {  //跑步消耗精力少,不会更新这个时间,跑步停下则立即恢复精力,1秒的精力恢复延迟,硬编码了
            energyRecoverDelay = 1f;
        }

        float realCost = cost;

        if (player.energyPoint <= cost)
        { //精力不够则消耗掉剩下的全部
            cantRunTime = (Player.maxEnergy / Player.energyRespawn);
            realCost = player.energyPoint;
            player.energyPoint = 0f;
        }
        else
        {
            player.energyPoint -= cost;
        }

        return realCost;  //返回消耗的精力
    }

    void UpdateEnergy()
    {
        energyRecoverDelay -= Time.fixedDeltaTime;
        if (energyRecoverDelay < 0f)  //
        {
            player.energyPoint += Player.energyRespawn * Time.fixedDeltaTime;
            if (player.energyPoint > Player.maxEnergy)
                player.energyPoint = Player.maxEnergy;
            energyRecoverDelay = 0f;
        }

        cantRunTime -= Time.fixedDeltaTime;
        if (cantRunTime < 0f)
            cantRunTime = 0f;
    }

    public float aimYaw
    {
        get
        {
            return input.aimAngle.y;
        }
    }

    public float aimPitch
    {
        get
        {
            return input.aimAngle.x;
        }
    }

    public void HitOtherPlayer(Player target, Vector3 hitPoint, float damage)
    {
        if (GlobalVariables.hostType == HostType.Server)
        {
            if (target.playerType == PlayerType.LocalAI || target.playerType == PlayerType.Local)
            {
                target.Damage(player, damage, hitPoint);
            }
            else if (target.playerType == PlayerType.Remote)
            {
                ServerAgent sm = UnityHelper.GetServerAgent();
                sm.SendDamage(player, target, hitPoint, damage);
            }
        }
        else if (GlobalVariables.hostType == HostType.Client)
        {
            if (target.playerType == PlayerType.Remote)
            {
                ClientAgent cm = UnityHelper.GetClientAgent();
                cm.SendDamage(player, target, hitPoint, damage);
            }
        }
    }

}
