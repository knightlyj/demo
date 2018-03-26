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

    Attack1,
    Attack2,
    JumpAttack,
    Charge,  //蓄力 
    ChargeWait, //蓄力完成时等待的动作
    ChargeAttack, //蓄力攻击
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
    TwoHandSword,
}

public enum OffHandWeaponType
{
    Empty,
    Shield,
}



public class PlayerAnimation : MonoBehaviour
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

    public void SetWeaponType(MainHandWeaponType mainHandType, OffHandWeaponType offHandType)
    {
        //动作调整
        if (this.mainHandType != mainHandType || this.offHandType != offHandType)
        {
            this.mainHandType = mainHandType;
            this.offHandType = offHandType;
            ChangeWeaponAniamtion();
        }
    }

    //改变武器动作
    void ChangeWeaponAniamtion()
    {

    }

    const int upperLayer = 1;
    const int lowerLayer = 2;
    const int wholeLayer = 3;
    public void SetAnimation(PlayerAniType aniType, PlayerAniDir dir = PlayerAniDir.Front, float fadeInTime = 0.1f, float speed = 1.0f)
    {
        if (aniType == this.curAniType)
        {   //循环动作,不用重复设置
            if (IsLoopedAnimation(this.curAniType))
                return;
        }
        this.curAniType = aniType;
        this.curAniDir = dir;
        if (IsHalfBodyAnimation(aniType))
        {
            //根据武器类型设置上半身动作
            string upperAni = GetUpperAniName();

            //设置下半身动作
            string lowerAni = GetLowerAniName();

            if (upperAni != null && lowerAni != null)
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
            if (wholeAni != null)
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

    //是否循环动作
    bool IsLoopedAnimation(PlayerAniType aniType)
    {
        switch (aniType)
        { //循环
            case PlayerAniType.Idle:
            case PlayerAniType.Walk:
            case PlayerAniType.Run:
            case PlayerAniType.Strafe:
            case PlayerAniType.ChargeWait:
                return true;
        }

        return false;
    }

    //是否半身动作
    bool IsHalfBodyAnimation(PlayerAniType aniType)
    {
        switch (aniType)
        {
            //半身
            case PlayerAniType.Idle:
            case PlayerAniType.Walk:
            case PlayerAniType.Run:
            case PlayerAniType.Strafe:
                return true;
        }

        return false;
    }

    string GetWeaponTypeName()
    {
        switch (mainHandType)
        {
            case MainHandWeaponType.TwoHandSword:
                return "2HandSword";
            case MainHandWeaponType.TwoHandAxe:
                return "2HandAxe";
            case MainHandWeaponType.Empty:
                return "1Hand";
        }
        return "1Hand";
    }

    //获取上半身动作名
    string GetUpperAniName()
    {
        string weaponName = null;
        string animationName = null;
        switch (curAniType)
        {
            case PlayerAniType.Idle:
                animationName = "Idle";
                break;
            case PlayerAniType.Walk:
                animationName = "Walk";
                break;
            case PlayerAniType.Run:
                animationName = "Run";
                break;
            case PlayerAniType.Strafe:
                animationName = "Strafe";
                break;
        }

        weaponName = GetWeaponTypeName();

        if (animationName != null && weaponName != null)
            return animationName + "-" + weaponName;
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
        if (curAniType == PlayerAniType.GetHit || curAniType == PlayerAniType.Roll)
        { //这两个动作分4个方向
            string animationName = null;
            string dirName = null;
            if (curAniType == PlayerAniType.GetHit)
            {
                animationName = "GetHit";
            }
            else if (curAniType == PlayerAniType.Roll)
            {
                animationName = "Roll";
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

            if (animationName != null && dirName != null)
                return animationName + dirName;
            else
                return null;
        }
        else if (curAniType == PlayerAniType.JumpUp) //这个动作不分方向
        {
            return "JumpUp";
        }
        else if (curAniType == PlayerAniType.Ground)
        {
            return "Ground";
        }
        else if (curAniType == PlayerAniType.Fall)
        {
            return "Fall";
        }
        else if (curAniType == PlayerAniType.Attack1)
        {
            return "Attack1-" + GetWeaponTypeName();
        }
        else if (curAniType == PlayerAniType.Attack2)
        {
            return "Attack2-" + GetWeaponTypeName();
        }
        else if (curAniType == PlayerAniType.JumpAttack)
        {
            return "JumpAttack-" + GetWeaponTypeName();
        }
        else if (curAniType == PlayerAniType.Charge)
        {
            return "Charge-" + GetWeaponTypeName();
        }
        else if (curAniType == PlayerAniType.ChargeWait)
        {
            return "ChargeWait-" + GetWeaponTypeName();
        }
        else if (curAniType == PlayerAniType.ChargeAttack)
        {
            return "ChargeAttack-" + GetWeaponTypeName();
        }
        else if (curAniType == PlayerAniType.Charge)
        {
            return "Charge-" + GetWeaponTypeName();
        }
        else if (curAniType == PlayerAniType.ChargeWait)
        {
            return "ChargeWait-" + GetWeaponTypeName();
        }
        else if (curAniType == PlayerAniType.ChargeAttack)
        {
            return "ChargeAttack-" + GetWeaponTypeName();
        }
        return null;
    }

    //动作完成
    public delegate void AniamtionCallback(string aniName);
    public event AniamtionCallback onAnimationDone;
    void AnimationDone(string aniName)
    {
        if (onAnimationDone != null)
            onAnimationDone(aniName);
    }

    //攻击事件
    public event AniamtionCallback onStartAttack;
    void StartAttack(string attack)
    {
        if (onStartAttack != null)
        {
            onStartAttack(attack);
        }
    }

    public event UnityAction onStopAttack;
    void StopAttack()
    {
        if (onStopAttack != null)
        {
            onStopAttack();
        }
    }
    

}
