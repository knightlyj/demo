using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;



public struct ActionInput
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
    //物理引擎各种配置
    public const float rollSpeed = 9;
    public const float jumpForce = 200;
    public const float walkSpeed = 4;
    public const float runSpeed = 7;
    public const float moveForce = 150;
    public const float moveForceInAir = 10;
    public const float moveSpeedInAir = 2;

    public event UnityAction onPlayerDestroy; //角色销毁事件
    public ActionInput input; //角色动作

    //角色属性
    public float healthPoint = maxHealth;
    public float energyPoint = maxEnergy;
    public const float maxHealth = 1000f; //最大血量
    public const float maxEnergy = 200f; //最大精力
    public const float energyRespawn = 25f; //精力恢复速度, per second
    public const float rollEnergyCost = 35f; //roll消耗的energy
    public const float runEnergyCost = 50f;  //run消耗energy, per second
    public const float jumpEnergyCost = 35f; //jump消耗的energy

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
        aniModule.onAnimationEvent += this.OnAnimationEvent; //动作事件回调
        ChangeWeapon(MainHandWeaponType.Sword, OffHandWeaponType.Empty);

        ActionInit();

        //角色朝向初始化
        this.orientation = transform.eulerAngles.y;
    }

    protected void OnDestroy()
    {
        if (onPlayerDestroy != null)
        {
            onPlayerDestroy();
        }
    }

    
    // Update is called once per frame
    protected void Update()
    {
        SmoothOrientation(); //角色朝向平滑过渡
    }

    
    protected void FixedUpdate()
    {
        Simulate(); //根据输入,模拟角色运动
    }

    //***************************角色朝向的代码******************************
    public float orientation //角色朝向,基于y轴旋转
    {
        set
        {
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
        actions[(int)curActionType].OnMainHandTrig(other);
    }

    [HideInInspector]
    public Player target;
    //格挡
}
