using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;



public struct PlayerInput
{
    public bool hasDir;  //有输入方向
    public float yaw;

    public bool run;
    public bool jump;
    public bool roll;

    public bool attack;   //轻攻击
    public bool antiAttack;  //防反
    public bool strongAttack; //重攻击 
}

public enum AttackType
{
    NormalAttack,
    ChargeAttack,
    JumpAttack,
}

public partial class Player : MonoBehaviour
{
    //各种速度配置
    public const float rollSpeed = 12;
    public const float jumpSpeed = 20;
    public const float walkSpeed = 5;
    public const float runSpeed = 10;
    public const float moveForce = 300;
    public const float moveForceInAir = 10;
    public const float moveSpeedInAir = 2;

    public event UnityAction onPlayerDestroy; //角色销毁事件
    public PlayerInput input; //角色动作

    Transform groundCheck = null;

    [HideInInspector]
    public PlayerAnimation aniModule = null;
    [HideInInspector]
    public Rigidbody rigidBody = null;

    //************左右手*******************
    public Transform rightHand { get { return this._rightHand; } }
    Transform _rightHand = null;

    public Transform leftHand { get { return this._leftHand; } }
    Transform _leftHand = null;

    protected void Awake()
    {
        groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
        groundCheck = transform.FindChild("GroundCheck");
        aniModule = GetComponent<PlayerAnimation>();
        rigidBody = GetComponent<Rigidbody>();

        _rightHand = UnityHelper.FindChildRecursive(transform, "B_R_Hand");
        _leftHand = UnityHelper.FindChildRecursive(transform, "B_L_Hand");
    }

    // Use this for initialization
    protected void Start()
    {
        aniModule.onAnimationDone += this.OnAnimationDone; //动作事件回调
        aniModule.onStartAttack += this.OnStartAttack;
        aniModule.onStopAttack += this.OnStopAttack;
        ChangeWeapon(MainHandWeaponType.Sword, OffHandWeaponType.Empty);

        ActionInit();

        //角色朝向初始化
        this.orientation = transform.eulerAngles.y;
    }

    protected void OnDestroy()
    {
        aniModule.onAnimationDone -= this.OnAnimationDone;
        aniModule.onStartAttack -= this.OnStartAttack;
        aniModule.onStopAttack -= this.OnStopAttack;
        if (onPlayerDestroy != null)
        {
            onPlayerDestroy();
        }
    }

    //地面检测
    bool grounded = true;
    // Update is called once per frame
    protected void Update()
    {
        SmoothOrientation(); //角色朝向平滑过渡
    }

    LayerMask groundLayerMask;
    float groundCheckRadius = 0.4f;
    protected void FixedUpdate()
    {
        //落地检测
        Collider[] hitGround = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, groundLayerMask);

        if (hitGround == null || hitGround.Length == 0)
        {
            grounded = false;
            //rigidBody.useGravity = true; //在空中,受到重力影响
        }
        else
        {
            grounded = true;
            //rigidBody.useGravity = false; //在地面时,不用重力
        }
        Simulate(); //更新操作
    }

    //***************************角色朝向的代码******************************
    Vector3 playerDir = Vector3.forward; //角色朝向,初始朝向Z-Axis
    public float orientation //角色朝向,基于y轴旋转
    {
        set
        {
            Quaternion rotation = Quaternion.AngleAxis(this.orientation, Vector3.up);
            this.playerDir = rotation * Vector3.forward;
            float temp = _orientation;
            _orientation = value;
            _orientation = _orientation - Mathf.Floor(_orientation / 360f) * 360f; //范围在0~360之间
            StartSmoothOrientation();
        }
        get
        {
            return this._orientation;
        }
    }
    float _orientation = 0f;

    //角色朝向平滑过渡
    float smoothOrientation = 0; //实际显示的朝向
    const float smoothOriBaseStepLen = 300;
    float smoothOriStepLen = 0;
    void StartSmoothOrientation()
    {  //这里计算环形插值
        float diff = this.orientation - smoothOrientation;
        float absDiff = Mathf.Abs(diff);
        if (absDiff > 270)
        {
            smoothOriStepLen = smoothOriBaseStepLen * 4;
        }
        else if (absDiff > 180)
        {
            smoothOriStepLen = smoothOriBaseStepLen * 3;
        }
        else if (absDiff > 90)
        {
            smoothOriStepLen = smoothOriBaseStepLen * 2;
        }
        else
        {
            smoothOriStepLen = smoothOriBaseStepLen;
        }

    }
    void SmoothOrientation()
    {
        smoothOrientation = CommonHelper.AngleTowards(smoothOrientation, orientation, smoothOriStepLen * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, smoothOrientation, 0);
    }

    [HideInInspector]
    public Vector3 hitSrcPos;
    //受击动作
    public void GetHit(Vector3 hitPos, AttackType attack, int damage)
    {
        hitSrcPos = hitPos;
        IntoAction(ActionType.GetHit);
    }

    //动画开始攻击事件
    void OnStartAttack(string attack)
    {
        if (curActionType != ActionType.Empty)
        {
            actions[(int)curActionType].OnAnimationEvent(attack, PlayerAniEventType.StartAttack);
        }
    }
    //动画停止攻击事件
    void OnStopAttack()
    {
        if (curActionType != ActionType.Empty)
        {
            actions[(int)curActionType].OnAnimationEvent(null, PlayerAniEventType.StopAttack);
        }
    }

    //更换武器
    Weapon mainWeapon = null;
    void ChangeWeapon(MainHandWeaponType mainHand, OffHandWeaponType offHand)
    {
        if (mainHand == MainHandWeaponType.Sword)
        {
            //在右手上加上武器
            UnityEngine.Object res = Resources.Load("Weapons/Sword");
            GameObject go = GameObject.Instantiate(res, rightHand, false) as GameObject;

            //设置碰撞回调,并关掉武器碰撞
            mainWeapon = go.GetComponent<Weapon>();
            mainWeapon.onHit = this.OnHitOther;
            DisableMainWeapon();
        }

        aniModule.SetWeaponType(mainHand, offHand);
    }

    //开启武器碰撞
    public void EnableMainWeapon()
    {
        mainWeapon.colliderEanbled = true;
    }
    //关闭武器碰撞
    public void DisableMainWeapon()
    {
        mainWeapon.colliderEanbled = false;
    }

    //武器碰撞到其他player时的回调
    void OnHitOther(Collider other)
    {
        if (curActionType != ActionType.Empty)
        {
            actions[(int)curActionType].OnMainHandTrig(other);
        }
    }

    [HideInInspector]
    public Player target;
    //格挡
}
