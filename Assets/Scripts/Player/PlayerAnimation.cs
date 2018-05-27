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


    static class AniEventName
    {
       public static readonly string Done = "Done";
    }

    delegate void OnAnimationEvent(AnimationEvent aniEvent);
    event OnAnimationEvent onAnimationEvent;
    //动作完成
    void AnimationEventHandler(AnimationEvent aniEvent)
    {
        if (onAnimationEvent != null)
            onAnimationEvent(aniEvent);
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

    //设置瞄准时的上下blend比例
    public float aimUp
    {
        set
        {
            animator.SetFloat("AimUp", value);
        }
        get
        {
            return animator.GetFloat("AimUp");
        }
    }

    enum UpperAniState
    {
        Empty,
        Idle,
        Move,
        Aim,
        Strafe,
        BlockMove,
        BlockIdle,
        BlockStrafe,
        SwtichWeapon,
        Shoot,
    }
    UpperAniState curUpperAniState = UpperAniState.Empty;
    //设置上半身动作
    void SetUpperAniState(UpperAniState state, bool reset = false)
    {
        if (curUpperAniState == state && !reset)
        {
            return;
        }
        else
        {
            curWholeAniState = WholeAniState.Empty;
            curUpperAniState = state;
            SwitchBodyMode(true);
            switch (state)
            {
                case UpperAniState.Idle:
                    animator.CrossFade(Animator.StringToHash("Idle"), 0.1f, upperAniLayer, 0);
                    break;
                case UpperAniState.Move:
                    animator.CrossFade(Animator.StringToHash("Move"), 0.1f, upperAniLayer, 0);
                    break;
                case UpperAniState.Aim:
                    animator.CrossFade(Animator.StringToHash("Aim"), 0.1f, upperAniLayer, 0);
                    break;
                case UpperAniState.BlockIdle:
                    break;
                case UpperAniState.BlockMove:
                    break;
                case UpperAniState.SwtichWeapon:
                    break;
                case UpperAniState.Shoot:
                    animator.CrossFade(Animator.StringToHash("Shoot"), 0.1f, upperAniLayer, 0);
                    break;
            }
        }
    }

    enum LowerAniState
    {
        Empty,
        Idle,
        Move,
        Aim,
        Strafe,
    }
    LowerAniState curLowerAniState = LowerAniState.Empty;
    //设置下半身动作
    void SetLowerAniState(LowerAniState state, bool reset = false)
    {
        if (curLowerAniState == state && !reset)
        {
            return;
        }
        else
        {
            curWholeAniState = WholeAniState.Empty;
            curLowerAniState = state;
            SwitchBodyMode(true);
            switch (state)
            {
                case LowerAniState.Idle:
                    animator.CrossFade(Animator.StringToHash("Idle"), 0.1f, lowerAniLayer, 0);
                    break;
                case LowerAniState.Move:
                    animator.CrossFade(Animator.StringToHash("Move"), 0.1f, lowerAniLayer, 0);
                    break;
                case LowerAniState.Aim:
                    animator.CrossFade(Animator.StringToHash("Aim"), 0.1f, lowerAniLayer, 0);
                    break;
                case LowerAniState.Strafe:
                    animator.CrossFade(Animator.StringToHash("Strafe"), 0.1f, lowerAniLayer, 0);
                    break;
            }
        }
    }

    enum WholeAniState
    {
        Empty,
        Roll,
        Jump,
        Fall,
        Attack,
    }
    WholeAniState curWholeAniState = WholeAniState.Empty;
    //设置全身动作
    void SetWholeAniState(WholeAniState clip, bool reset = false, string stateName = null)
    {
        if (curWholeAniState == clip && !reset)
        {
            return;
        }
        else
        {
            curUpperAniState = UpperAniState.Empty;
            curLowerAniState = LowerAniState.Empty;
            curWholeAniState = clip;
            SwitchBodyMode(false);
            switch (clip)
            {
                case WholeAniState.Roll:
                    animator.CrossFade(Animator.StringToHash("Roll"), 0.1f, wholeAniLayer, 0);
                    break;
                case WholeAniState.Jump:
                    animator.CrossFade(Animator.StringToHash("JumpUp"), 0.1f, wholeAniLayer, 0);
                    break;
                case WholeAniState.Fall:
                    animator.CrossFade(Animator.StringToHash("Fall"), 0.1f, wholeAniLayer, 0);
                    break;
                case WholeAniState.Attack:
                    if(stateName != null)
                    {
                        animator.CrossFade(Animator.StringToHash(stateName), 0.1f, wholeAniLayer, 0);
                    }
                    else
                    {
                        animator.CrossFade(Animator.StringToHash("Attack1"), 0.1f, wholeAniLayer, 0);
                    }
                    break;
            }
        }
    }

    void UpperSuitLowerAnimation()
    {
        UpperAniState clip = UpperAniState.Empty;
        switch (curLowerAniState)
        {
            case LowerAniState.Aim:
                clip = UpperAniState.Aim;
                break;
            case LowerAniState.Idle:
                clip = UpperAniState.Idle;
                break;
            case LowerAniState.Move:
                clip = UpperAniState.Move;
                break;
            case LowerAniState.Strafe:
                clip = UpperAniState.Strafe;
                break;
        }
        SetUpperAniState(clip);
    }

    bool shootIk = false;
    bool footIk = false;
    [SerializeField]
    Transform leftFoot = null;
    [SerializeField]
    Transform rightFoot = null;
    void OnAnimatorIK()
    {
        if (shootIk)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, rightHand.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, rightHand.rotation * Quaternion.Euler(0, -90, 0));
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }


        if (footIk)
        {   //stand

            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

            float bb = 1.2f;
            //left foot
            Vector3 leftUp = leftFoot.position + Vector3.up * 0.5f;
            RaycastHit hitInfo;
            if (Physics.Raycast(leftUp, Vector3.down, out hitInfo, bb, groundLayerMask))
            {
                Vector3 leftPosition = leftUp + Vector3.down * (hitInfo.distance - 0.15f);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftPosition);
                Vector3 footFoward = leftFoot.up;  //脚的上方,是世界坐标前方
                float dd = CommonHelper.DirDerivativeOnPlane(hitInfo.normal, footFoward);
                footFoward.y = dd;
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(footFoward));
            }
            else
            {

            }

            //right foot
            Vector3 rightUp = rightFoot.position + Vector3.up * 0.5f;
            if (Physics.Raycast(rightUp, Vector3.down, out hitInfo, bb, groundLayerMask))
            {
                Vector3 rightPosition = rightUp + Vector3.down * (hitInfo.distance - 0.15f);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, rightPosition);
                Vector3 footFoward = rightFoot.up;  //脚的上方,是世界坐标前方
                float dd = CommonHelper.DirDerivativeOnPlane(hitInfo.normal, footFoward);
                footFoward.y = dd;
                animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(footFoward));
            }
            else
            {

            }
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
        }
    }
}
