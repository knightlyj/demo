using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public enum PlayerAniType
{
    //半身分离动作
    Idle = 0,
    Walk,
    Run,
    Strafe,

    //全身动作
    Roll,
    GetHit,
    JumpUp,
    Fall,
    Ground,
}

public enum PlayerAniDir
{
    Front,
    Back,
    Left,
    Right,
}

public enum MainHandWeaponType
{
    //单手部分
    Empty,
    Sword,
    Pistol,

    //双手部分
    TwoHandSpear,
    TwoHandAxe,
}

public enum OffHandWeaponType
{
    Empty,
    Shield,
}



public class PlayerAni : MonoBehaviour
{
    PlayerAniType curAniType = PlayerAniType.Idle;
    PlayerAniDir curAniDir = PlayerAniDir.Front;
    Animator animator = null;
    MainHandWeaponType mainHandType = MainHandWeaponType.Empty;
    OffHandWeaponType offHandType = OffHandWeaponType.Empty;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //AnimationTest();
    }

    bool curHalfBodyMode = true; //当前是否半身模式?
    void SwitchBodyMode(bool halfBodyMode) //半身或全身模式
    {
        if (curHalfBodyMode != halfBodyMode)
        {
            curHalfBodyMode = halfBodyMode;

            if (halfBodyMode)
            {   //半身模式
                StopCoroutine("ToWholeBodyMode"); //关闭全身模式
                StartCoroutine("ToHalfBodyMode"); //切换到半身模式
            }
            else
            {   //全身模式
                StopCoroutine("ToHalfBodyMode"); //关闭半身模式
                StartCoroutine("ToWholeBodyMode"); //切换到全身模式
            }
        }
    }

    float bodyModeStep = 0.1f;
    IEnumerator ToHalfBodyMode()
    {
        float rate = 0;
        while (rate < 1.0f)
        {
            rate += bodyModeStep;
            if (rate > 1.0f)
                rate = 1;

            animator.SetLayerWeight(1, rate);
            animator.SetLayerWeight(2, rate);
            animator.SetLayerWeight(3, 1 - rate);

            yield return null;
        }
    }

    IEnumerator ToWholeBodyMode()
    {
        float rate = 1.0f;
        while (rate > 0f)
        {
            rate -= bodyModeStep;
            if (rate < 0f)
                rate = 0f;

            animator.SetLayerWeight(1, rate);
            animator.SetLayerWeight(2, rate);
            animator.SetLayerWeight(3, 1 - rate);

            yield return null;
        }
    }


    [SerializeField]
    MeshFilter mainWeaponMeshFilter = null;
    [SerializeField]
    MeshFilter offWeaponMeshFilter = null;

    Vector3 mainWeaponLocalPos = new Vector3(-0.044f, 0.062f, -0.06f);
    Vector3 mainWeaponLocalRot = new Vector3(1.479f, -0.349f, 8.041f);
    public void ChangeWeaponModel(string mainWeaponName, MainHandWeaponType mainHandType, string offWeaponName, OffHandWeaponType offHandType)
    {
        //主手武器处理
        //加载需要的武器资源
        Mesh newWeapon = null;
        if(mainWeaponName != null)
            newWeapon = Resources.Load<Mesh>("Weapons/" + mainWeaponName);
        mainWeaponMeshFilter.mesh = newWeapon;

        //副手武器处理

        //动作调整
        if (this.mainHandType != mainHandType || this.offHandType != offHandType)
        {
            this.mainHandType = mainHandType;
            this.offHandType = offHandType;
            ChangeWeaponAction();
        }
    }

    //改变武器动作
    void ChangeWeaponAction()
    {

    }

    const int upperLayer = 1;
    const int lowerLayer = 2;
    const int wholeLayer = 3;
    public void SetAnimation(PlayerAniType aniType, PlayerAniDir dir = PlayerAniDir.Front, float fadeInTime = 0.1f, float speed = 1.0f)
    {
        this.curAniType = aniType;
        this.curAniDir = dir;
        if (IsHalfBodyAnimation(aniType))
        {
            //根据武器类型设置上半身动作
            string upperAni = GetUpperAniName();

            //设置下半身动作
            string lowerAni = GetLowerAniName();

            if(upperAni != null && lowerAni != null)
            {
                SwitchBodyMode(true);
                animator.CrossFade(Animator.StringToHash(upperAni), fadeInTime, upperLayer, 0);
                animator.CrossFade(Animator.StringToHash(lowerAni), fadeInTime, lowerLayer, 0);
                animator.speed = speed;
            }
        }
        else
        {
            string wholeAni = GetWholeAniName();
            if(wholeAni != null)
            {
                SwitchBodyMode(false);
                animator.CrossFade(Animator.StringToHash(wholeAni), fadeInTime, wholeLayer, 0);
                animator.speed = speed;
            }
        }
    }

    public PlayerAniType GetAnimation()
    {
        return this.curAniType;
    }

    bool IsHalfBodyAnimation(PlayerAniType aniType)
    {
        switch (aniType)
        {
            //半身
            case PlayerAniType.Idle:
                return true;
            case PlayerAniType.Walk:
                return true;
            case PlayerAniType.Run:
                return true;
            case PlayerAniType.Strafe:
                return true;

            //全身
            case PlayerAniType.JumpUp:
                return false;
            case PlayerAniType.Fall:
                return false;
            case PlayerAniType.Ground:
                return false;
            case PlayerAniType.Roll:
                return false;
            case PlayerAniType.GetHit:
                return false;
        }

        return false;
    }
    
    //获取上半身动作名
    string GetUpperAniName()
    {
        string weaponName = null;
        string actionName = null;
        switch (curAniType)
        {
            case PlayerAniType.Idle:
                actionName = "Idle";
                break;
            case PlayerAniType.Walk:
                actionName = "Walk";
                break;
            case PlayerAniType.Run:
                actionName = "Run";
                break;
            case PlayerAniType.Strafe:
                actionName = "Strafe";
                break;
        }

        switch (mainHandType)
        {
            case MainHandWeaponType.TwoHandAxe:
                weaponName = "2HandAxe";
                break;
            case MainHandWeaponType.Empty:
                weaponName = "1Hand";
                break;
        }

        if (actionName != null && weaponName != null)
            return actionName + "-" + weaponName;
        else
            return null;
    }

    //下半身动作名
    string GetLowerAniName()
    {
        switch (curAniType)
        {
            case PlayerAniType.Idle:
                return "Idle";
            case PlayerAniType.Walk:
                return "Walk";
            case PlayerAniType.Run:
                return "Run";
            case PlayerAniType.Strafe:
                switch (curAniDir)
                {
                    case PlayerAniDir.Front:
                        return "StrafeFront";
                    case PlayerAniDir.Back:
                        return "StrafeBack";
                    case PlayerAniDir.Left:
                        return "StrafeLeft";
                    case PlayerAniDir.Right:
                        return "StrafeRight";
                }
                break;
        }
        return null;
    }

    //获取全身动作名
    string GetWholeAniName()
    {
        if(curAniType == PlayerAniType.GetHit || curAniType == PlayerAniType.Roll)
        { //这两个动作分4个方向
            string actionName = null;
            string dirName = null;
            if(curAniType == PlayerAniType.GetHit)
            {
                actionName = "GetHit";
            }
            else if(curAniType == PlayerAniType.Roll)
            {
                actionName = "Roll";
            }

            switch (curAniDir)
            {
                case PlayerAniDir.Front:
                    dirName = "Front";
                    break;
                case PlayerAniDir.Back:
                    dirName = "Back";
                    break;
                case PlayerAniDir.Left:
                    dirName = "Left";
                    break;
                case PlayerAniDir.Right:
                    dirName = "Right";
                    break;
            }

            if (actionName != null && dirName != null)
                return actionName + dirName;
            else
                return null;
        }
        else if (curAniType == PlayerAniType.JumpUp) //这个动作不分方向
        {
            return  "JumpUp";
        }
        else if(curAniType == PlayerAniType.Ground)
        {
            return "Ground";
        }
        else if(curAniType == PlayerAniType.Fall)
        {
            return "Fall";
        }
        return null;
    }

    //动作完成
    public event UnityAction onActionDone;
    void ActionDone()
    {
        if (onActionDone != null)
            onActionDone();
    }

    void AnimationTest()
    {
        //strafe
        if (Input.GetKeyDown(KeyCode.W))
        {
            SetAnimation(PlayerAniType.Strafe, PlayerAniDir.Front);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            SetAnimation(PlayerAniType.Strafe, PlayerAniDir.Left);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SetAnimation(PlayerAniType.Strafe, PlayerAniDir.Back);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            SetAnimation(PlayerAniType.Strafe, PlayerAniDir.Right);
        }

        //Roll
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SetAnimation(PlayerAniType.Roll, PlayerAniDir.Front);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SetAnimation(PlayerAniType.Roll, PlayerAniDir.Left);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SetAnimation(PlayerAniType.Roll, PlayerAniDir.Back);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SetAnimation(PlayerAniType.Roll, PlayerAniDir.Right);
        }

        //idle
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetAnimation(PlayerAniType.Idle);
        }
        //walk
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetAnimation(PlayerAniType.Walk);
        }
        //run
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetAnimation(PlayerAniType.Run);
        }


        //weapon
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetAnimation(PlayerAniType.Run);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetAnimation(PlayerAniType.Run, PlayerAniDir.Front, 1f);
        }
    }

}
