using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public struct LocalInput
{
    public bool hasDir;  //有输入方向
    public float yaw;

    public bool run;
    public bool roll;
    public bool jump;
    public bool rightHand;
    public bool leftHand;

    public void Clear()
    {
        hasDir = false;
        run = false;
        roll = false;
        jump = false;
        rightHand = false;
        leftHand = false;
    }
}


public enum WeaponType
{
    Melee,
    Pistol,
}

public partial class Player : MonoBehaviour
{
    //物理引擎各种配置
    public const float rollSpeed = 9;
    public const float jumpForce = 200;
    public const float walkSpeed = 4;
    public const float runSpeed = 7;
    public const float moveForce = 80;
    public const float moveForceInAir = 10;
    public const float moveSpeedInAir = 2;

    public event UnityAction onPlayerDestroy; //角色销毁事件
    public LocalInput input; //角色动作

    //角色属性
    public float healthPoint = maxHealth;
    public float energyPoint = maxEnergy;
    public const float maxHealth = 1000f; //最大血量
    public const float maxEnergy = 200f; //最大精力
    public const float energyRespawn = 25f; //精力恢复速度, per second
    public const float rollEnergyCost = 35f; //roll消耗的energy
    public const float runEnergyCost = 50f;  //run消耗energy, per second
    public const float jumpEnergyCost = 35f; //jump消耗的energy
    
    [HideInInspector]
    public Rigidbody rigidBody = null;
    Animator animator = null;

    //************左右手transform*******************
    public Transform rightHand { get { return this._rightHand; } }
    Transform _rightHand = null;

    public Transform leftHand { get { return this._leftHand; } }
    Transform _leftHand = null;

    protected void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();

        _rightHand = UnityHelper.FindChildRecursive(transform, "B_R_Hand");
        _leftHand = UnityHelper.FindChildRecursive(transform, "B_L_Hand");

        ActionInit();
    }

    // Use this for initialization
    protected void Start()
    {
        //ChangeRightWeapon(WeaponType.Sword);
        //ChangeLeftWeapon(WeaponType.Sword);
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


    //武器类型
    WeaponType weaponType = WeaponType.Melee;
    //右手武器脚本
    WeaponObj rightWeapon = null;
    //更换右手武器
    void ChangeWeapon(WeaponType weapon)
    {
        if (weapon != this.weaponType)
        {
            if (rightWeapon != null)
            {  //原来有武器的话,要销毁
                Destroy(rightWeapon.gameObject);
                rightWeapon = null;
            }

            this.weaponType = weapon;
            GameObject goWeapon = null;
            if (weapon == WeaponType.Melee)
            {
                //在右手上加上武器
                UnityEngine.Object res = Resources.Load("Weapons/Sword");
                goWeapon = GameObject.Instantiate(res, this.rightHand, false) as GameObject;
            }
            else if (weapon == WeaponType.Pistol)
            {
                //在右手上加上武器
                UnityEngine.Object res = Resources.Load("Weapons/Pistol");
                goWeapon = GameObject.Instantiate(res, this.rightHand, false) as GameObject;
            }

            if (goWeapon != null)
            {
                //设置碰撞回调,并关掉武器碰撞
                rightWeapon = goWeapon.GetComponent<WeaponObj>();
                rightWeapon.onHit = this.OnHitOther;
                rightWeapon.colliderEanbled = false;
            }
        }
    }

    //开启武器碰撞
    public void EnableMainWeapon()
    {
        rightWeapon.colliderEanbled = true;
    }
    //关闭武器碰撞
    public void DisableMainWeapon()
    {
        rightWeapon.colliderEanbled = false;
    }

    //武器碰撞到其他player时的回调
    void OnHitOther(Collider other)
    {

    }

    [HideInInspector]
    public Player target;
    //格挡
}
