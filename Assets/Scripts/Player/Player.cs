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

public enum PlayerType
{
    Unknown,
    Remote,
    Local,
    LocalAI,
}

public partial class Player : MonoBehaviour
{
    [HideInInspector]
    public int id;
    [HideInInspector]
    public string nameInGame;

    [HideInInspector]
    public int targetId = -1;

    [HideInInspector]
    public int connectId = -1;

    [HideInInspector]
    public PlayerType playerType = PlayerType.Unknown;

    //物理引擎各种配置
    public const float rollSpeed = 9;
    public const float jumpForce = 200;
    public const float walkSpeed = 4;
    public const float runSpeed = 7;
    public const float strafeSpeed = 2f;
    public const float moveForce = 160;
    public const float moveForceInAir = 10;
    public const float moveSpeedInAir = 2;

    //角色属性
    public float _healthPoint = maxHealth;
    public float healthPoint
    {
        set
        {
            if(_healthPoint > 0 && value <= 0)
            {
                EventManager.RaiseEvent(EventId.PlayerDie, this.id, this, null);
            }
            else if(_healthPoint <= 0 && value > 0)
            {
                EventManager.RaiseEvent(EventId.PlayerRevive, this.id, this, null);
            }
            _healthPoint = value;
        }
        get
        {
            return _healthPoint;
        }
    }
    public float energyPoint = maxEnergy;
    public const float maxHealth = 1000f; //最大血量
    public const float maxEnergy = 200f; //最大精力
    public const float energyRespawn = 25f; //精力恢复速度, per second
    public const float rollEnergyCost = 35f; //roll消耗的energy
    public const float runEnergyCost = 50f;  //run消耗energy, per second
    public const float jumpEnergyCost = 35f; //jump消耗的energy
    public const float attackEnergyCost = 40f;
    public const float shootEnergyCost = 40f;

    [HideInInspector]
    public Animator animator = null;

    [HideInInspector]
    public AudioSource audioSource = null;

    //************左右手transform*******************
    public Transform rightHand { get { return this._rightHand; } }
    Transform _rightHand = null;

    public Transform leftArm { get { return this._leftArm; } }
    Transform _leftArm = null;

    Rigidbody rigidBody = null;
    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        _rightHand = UnityHelper.FindChildRecursive(transform, "B_R_Hand");
        _leftArm = UnityHelper.FindChildRecursive(transform, "B_L_Forearm");

    }

    // Use this for initialization
    void Start()
    {
        SetupMaterial();
        ChangeWeapon(WeaponType.Melee);
    }

    void OnDestroy()
    {
        EventManager.RaiseEvent(EventId.PlayerDestory, id, this, null);
    }

    // Update is called once per frame
    void Update()
    {

    }


    void FixedUpdate()
    {

    }
    public LayerMask hitLayer;
    //武器类型
    [HideInInspector]
    public WeaponType weaponType = WeaponType.Empty;
    //右手武器脚本
    GameObject rightWeapon = null;
    GameObject shield = null;
    [HideInInspector]
    public WeaponCollision weaponCollision = null;
    [HideInInspector]
    public Gun gun = null;
    [HideInInspector]
    public bool blocking = false;
    [HideInInspector]
    public bool invincible = false;
    //更换右手武器
    public void ChangeWeapon(WeaponType weapon)
    {
        if (weapon != this.weaponType)
        {
            if (rightWeapon != null)
            {  //原来有武器的话,要销毁
                Destroy(rightWeapon.gameObject);
                rightWeapon = null;
                weaponCollision = null;
                gun = null;
            }
            if (shield != null)
            {
                Destroy(shield.gameObject);
                shield = null;
            }

            this.weaponType = weapon;
            if (weapon == WeaponType.Melee)
            {
                //在右手上加上武器
                UnityEngine.Object res = Resources.Load("Weapons/Sword");
                rightWeapon = GameObject.Instantiate(res, this.rightHand, false) as GameObject;
                weaponCollision = rightWeapon.GetComponent<WeaponCollision>();

                res = Resources.Load("Weapons/Shield");
                shield = GameObject.Instantiate(res, this.leftArm, false) as GameObject;
                
            }
            else if (weapon == WeaponType.Pistol)
            {
                //在右手上加上武器
                UnityEngine.Object res = Resources.Load("Weapons/Pistol");
                rightWeapon = GameObject.Instantiate(res, this.rightHand, false) as GameObject;

                gun = rightWeapon.GetComponent<Gun>();
            }

            if(rightWeapon != null)
            {
                MeshRenderer mr = rightWeapon.GetComponent<MeshRenderer>();
                if (playerType == PlayerType.Remote || playerType == PlayerType.LocalAI)
                {
                    mr.material = enemyMat;
                }
            }

            if (shield != null)
            {
                MeshRenderer mr = shield.GetComponent<MeshRenderer>();
                if (playerType == PlayerType.Remote || playerType == PlayerType.LocalAI)
                {
                    mr.material = enemyMat;
                }
            }

            if (weaponCollision != null)
            {
                weaponCollision.colliderEanbled = false;
            }
        }
    }
    
    [SerializeField]
    Material enemyMat = null;

    public void SetupMaterial()
    {
        if (playerType == PlayerType.Remote || playerType == PlayerType.LocalAI)
        {
            SkinnedMeshRenderer sm = transform.FindChild("RPG-Character-Mesh").GetComponent<SkinnedMeshRenderer>();
            sm.material = enemyMat;
        }
    }

    public void Damage(Player src, float d, Vector3 hitPoint)
    {//直接用伤害来源者的位置判断格挡了
        if (invincible)
        {

        }
        else
        {
            bool damage = false;
            if (src != null)
            {
                if (this.blocking)
                {
                    Vector3 dir = src.transform.position - transform.position;
                    float angle = Vector3.Angle(dir, transform.forward);
                    if (angle > 90f)
                    {
                        damage = true;
                    }
                    else
                    {
                        EventManager.RaiseEvent(EventId.PlayerBlock, this.id, this, null);

                        PlayBlockSound();
                    }
                }
                else
                {
                    damage = true;
                }
            }
            if (damage)
            {
                EventManager.RaiseEvent(EventId.PlayerDamage, this.id, this, hitPoint);
                this.healthPoint -= d;

                PlayHitBodySound();
            }
        }
    }
    
    public Protocol.PlayerInfo AchievePlayerInfo()
    {
        Protocol.PlayerInfo info = new Protocol.PlayerInfo();
        info.id = id;
        info.name = nameInGame;
        info.hp = this.healthPoint;

        info.yaw = transform.eulerAngles.y;

        Vector3 velocity = rigidBody.velocity;
        Vector3 position = transform.position;
        info.velocityX = velocity.x;
        info.velocityY = velocity.y;
        info.velocityZ = velocity.z;
        info.positionX = position.x;
        info.positionY = position.y;
        info.positionZ = position.z;

        info.weapon = this.weaponType;
        info.invincible = this.invincible;

        GetUpperAniState(out info.upperAniState, out info.upperAniNormTime);
        GetLowerAniState(out info.lowerAniState, out info.lowerAniNormTime);

        info.walkRun = walkRun;
        info.aimUp = aimUp;
        info.strafeForward = strafeForward;
        info.strafeRight = strafeRight;
        return info;
    }

    public void PlayBlockSound()
    {
        AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "hitBlock", typeof(AudioClip));
        audioSource.PlayOneShot(clip, 0.2f);
    }

    public void PlayHitBodySound()
    {
        int s = UnityEngine.Random.Range(1, 3);
        AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "hitBody" + s, typeof(AudioClip));
        audioSource.PlayOneShot(clip, 0.2f);
    }
}
