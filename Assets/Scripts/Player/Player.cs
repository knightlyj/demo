using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public enum WeaponType
{
    Empty,
    Melee,
    Pistol,
}

public partial class Player : MonoBehaviour
{
    [HideInInspector]
    public int id;
    [HideInInspector]
    public int targetId = -1;

    //物理引擎各种配置
    public const float rollSpeed = 9;
    public const float jumpForce = 200;
    public const float walkSpeed = 4;
    public const float runSpeed = 7;
    public const float strafeSpeed = 2f;
    public const float moveForce = 80;
    public const float moveForceInAir = 10;
    public const float moveSpeedInAir = 2;
    
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
    public Animator animator = null;

    //************左右手transform*******************
    public Transform rightHand { get { return this._rightHand; } }
    Transform _rightHand = null;

    public Transform leftArm { get { return this._leftArm; } }
    Transform _leftArm = null;

    void Awake()
    {
        animator = GetComponent<Animator>();

        _rightHand = UnityHelper.FindChildRecursive(transform, "B_R_Hand");
        _leftArm = UnityHelper.FindChildRecursive(transform, "B_L_Forearm");

    }

    // Use this for initialization
    void Start()
    {
        ChangeWeapon(WeaponType.Melee);
    }

    void OnDestroy()
    {

    }
    
    // Update is called once per frame
    void Update()
    {

    }

    
    void FixedUpdate()
    {

    }

    //武器类型
    [HideInInspector]
    public WeaponType weaponType = WeaponType.Empty;
    //右手武器脚本
    GameObject rightWeapon = null;
    GameObject shield = null;
    [HideInInspector]
    public bool blocking = false;
    //更换右手武器
    public void ChangeWeapon(WeaponType weapon)
    {
        if (weapon != this.weaponType)
        {
            if (rightWeapon != null)
            {  //原来有武器的话,要销毁
                Destroy(rightWeapon.gameObject);
                rightWeapon = null;
            }
            if (shield != null)
            {
                Destroy(shield.gameObject);
                shield = null;
            }

            this.weaponType = weapon;
            //GameObject goWeapon = null;
            if (weapon == WeaponType.Melee)
            {
                //在右手上加上武器
                UnityEngine.Object res = Resources.Load("Weapons/Sword");
                rightWeapon = GameObject.Instantiate(res, this.rightHand, false) as GameObject;

                res = Resources.Load("Weapons/Shield");
                shield = GameObject.Instantiate(res, this.leftArm, false) as GameObject;
            }
            else if (weapon == WeaponType.Pistol)
            {
                //在右手上加上武器
                UnityEngine.Object res = Resources.Load("Weapons/Pistol");
                rightWeapon = GameObject.Instantiate(res, this.rightHand, false) as GameObject;
            }

            //if (goWeapon != null)
            //{
            //    //设置碰撞回调,并关掉武器碰撞
            //    rightWeapon = goWeapon.GetComponent<WeaponObj>();
            //    rightWeapon.onHit = this.OnHitOther;
            //    rightWeapon.colliderEanbled = false;
            //}
        }
    }

    //开启武器碰撞
    public void EnableMainWeapon()
    {
    }
    //关闭武器碰撞
    public void DisableMainWeapon()
    {
    }

    //武器碰撞到其他player时的回调
    void OnHitOther(Collider other)
    {

    }

    
}
