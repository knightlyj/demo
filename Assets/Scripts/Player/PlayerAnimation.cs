using UnityEngine;
using System.Collections;
using UnityEngine.Events;


public partial class Player
{
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

    float aniModeStep = 0.1f;
    IEnumerator ToHalfBodyMode()
    {
        float rate = 0;
        while (rate < 1.0f)
        {
            rate += aniModeStep;
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
            rate -= aniModeStep;
            if (rate < 0f)
                rate = 0f;

            animator.SetLayerWeight(1, rate);
            animator.SetLayerWeight(2, rate);
            animator.SetLayerWeight(3, 1 - rate);

            yield return null;
        }
    }

    const int upperAniLayer = 1;
    const int lowerAniLayer = 2;
    const int wholeAniLayer = 3;

    void SetUpperAnimation(string clip, float startTime = 0f, float fadeInTime = 0.1f, float speed = 1f)
    {
        SwitchBodyMode(true);
        animator.CrossFade(Animator.StringToHash(clip), fadeInTime, upperAniLayer, startTime);
        animator.speed = speed;
    }

    void SetLowerAnimation(string clip, float startTime = 0f, float fadeInTime = 0.1f, float speed = 1f)
    {
        SwitchBodyMode(true);
        animator.CrossFade(Animator.StringToHash(clip), fadeInTime, lowerAniLayer, startTime);
        animator.speed = speed;
    }

    void SetWholeAnimation(string clip, float startTime = 0f, float fadeInTime = 0.1f, float speed = 1f)
    {
        SwitchBodyMode(false);
        animator.CrossFade(Animator.StringToHash(clip), fadeInTime, wholeAniLayer, startTime);
        animator.speed = speed;
    }

    //动作完成
    void AnimationEventHandler(AnimationEvent aniEvent)
    {
        if (curActionType != ActionType.Empty)
        {
            if (aniEvent.stringParameter.Equals("Done"))
            {
                OnActionDone();
            }
            else if (aniEvent.stringParameter.Equals("CanInput"))
            {
                this.canInputAction = true;
            }
            else
            {
                actions[(int)curActionType].OnAnimationEvent(aniEvent);
            }
        }
    }

    enum UpperAniState
    {
        Empty,
        Idle,
        Move,
        Block,
    }
    UpperAniState upperAniState = UpperAniState.Empty;

    enum LowerAniState
    {
        Empty,
        Idle,
        Move,
    }
    LowerAniState lowerAniState = LowerAniState.Empty;

    void UpperSuitLowerAnimation()
    {
        if (lowerAniState == LowerAniState.Idle)
        {
            AnimatorStateInfo info = animator.GetNextAnimatorStateInfo(lowerAniLayer);
            //根据右手武器做不同动作
            WeaponData wd = WeaponConfig.GetWeaponData(rightWeaponType);
            if (twoHanded)
            {
                SetUpperAnimation(wd.upperIdleTwoHanded, info.normalizedTime);
            }
            else
            {
                SetUpperAnimation(wd.upperIdle, info.normalizedTime);

            }
            upperAniState = UpperAniState.Idle;
        }
        else if (lowerAniState == LowerAniState.Move)
        {
            AnimatorStateInfo info = animator.GetNextAnimatorStateInfo(lowerAniLayer);
            //根据右手武器做不同动作
            WeaponData wd = WeaponConfig.GetWeaponData(rightWeaponType);
            if (twoHanded)
            {
                SetUpperAnimation(wd.upperMoveTwoHanded, info.normalizedTime);
            }
            else
            {
                SetUpperAnimation(wd.upperMove, info.normalizedTime);
            }
            upperAniState = UpperAniState.Move;
        }
    }

    //播放idle动画
    void AnimateUpperIdle()
    {
        WeaponData wd = WeaponConfig.GetWeaponData(rightWeaponType);
        if (twoHanded)
        {
            SetUpperAnimation(wd.upperIdleTwoHanded);
        }
        else
        {
            SetUpperAnimation(wd.upperIdle);

        }
        upperAniState = UpperAniState.Idle;
    }
    void AnimateLowerIdle()
    {
        SetLowerAnimation("Idle");
        lowerAniState = LowerAniState.Idle;
    }

    //播放移动动画
    void AnimateUpperMove()
    {
        //根据右手武器做不同动作
        WeaponData wd = WeaponConfig.GetWeaponData(rightWeaponType);
        if (twoHanded)
        {
            SetUpperAnimation(wd.upperMoveTwoHanded);
        }
        else
        {
            SetUpperAnimation(wd.upperMove);
        }
        upperAniState = UpperAniState.Move;
    }
    void AnimateLowerMove()
    {
        SetLowerAnimation("Move");
        lowerAniState = LowerAniState.Move;
    }
    //设置walk和run的blend比例
    float walkRun
    {
        set
        {
            animator.SetFloat("WalkRun", value);
        }
        get
        {
            return animator.GetFloat("WalkRun");
        }
    }

    //播放jump动画
    void AnimateJump()
    {
        upperAniState = UpperAniState.Empty;
        lowerAniState = LowerAniState.Empty;
        SetWholeAnimation("JumpUp");
    }
    //播放roll动画
    void AnimateRoll()
    {
        upperAniState = UpperAniState.Empty;
        lowerAniState = LowerAniState.Empty;
        SetWholeAnimation("Roll");
    }
}
