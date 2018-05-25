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

    enum UpperAniClip
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
    }
    UpperAniClip curUpperAniClip = UpperAniClip.Empty;
    //设置上半身动作
    void SetUpperAniClip(UpperAniClip clip, bool reset = false)
    {
        if (curUpperAniClip == clip && !reset)
        {
            return;
        }
        else
        {
            curWholeAniClip = WholeAniClip.Empty;
            curUpperAniClip = clip;
            SwitchBodyMode(true);
            switch (clip)
            {
                case UpperAniClip.Idle:
                    animator.CrossFade(Animator.StringToHash("Idle"), 0.1f, upperAniLayer, 0);
                    break;
                case UpperAniClip.Move:
                    animator.CrossFade(Animator.StringToHash("Move"), 0.1f, upperAniLayer, 0);
                    break;
                case UpperAniClip.Aim:
                    animator.CrossFade(Animator.StringToHash("Aim"), 0.1f, upperAniLayer, 0);
                    break;
                case UpperAniClip.BlockIdle:
                    break;
                case UpperAniClip.BlockMove:
                    break;
                case UpperAniClip.SwtichWeapon:
                    break;
            }
        }
    }

    enum LowerAniClip
    {
        Empty,
        Idle,
        Move,
        Aim,
        Strafe,
    }
    LowerAniClip curLowerAniClip = LowerAniClip.Empty;
    //设置下半身动作
    void SetLowerAniClip(LowerAniClip clip, bool reset = false)
    {
        if (curLowerAniClip == clip && !reset)
        {
            return;
        }
        else
        {
            curWholeAniClip = WholeAniClip.Empty;
            curLowerAniClip = clip;
            SwitchBodyMode(true);
            switch (clip)
            {
                case LowerAniClip.Idle:
                    animator.CrossFade(Animator.StringToHash("Idle"), 0.1f, lowerAniLayer, 0);
                    break;
                case LowerAniClip.Move:
                    animator.CrossFade(Animator.StringToHash("Move"), 0.1f, lowerAniLayer, 0);
                    break;
                case LowerAniClip.Aim:
                    animator.CrossFade(Animator.StringToHash("Aim"), 0.1f, lowerAniLayer, 0);
                    break;
            }
        }
    }

    enum WholeAniClip
    {
        Empty,
        Roll,
        Jump,
        Fall,
    }
    WholeAniClip curWholeAniClip = WholeAniClip.Empty;
    //设置全身动作
    void SetWholeAniClip(WholeAniClip clip, bool reset = false)
    {
        if (curWholeAniClip == clip && !reset)
        {
            return;
        }
        else
        {
            curUpperAniClip = UpperAniClip.Empty;
            curLowerAniClip = LowerAniClip.Empty;
            curWholeAniClip = clip;
            SwitchBodyMode(false);
            switch (clip)
            {
                case WholeAniClip.Roll:
                    animator.CrossFade(Animator.StringToHash("Roll"), 0.1f, wholeAniLayer, 0);
                    break;
                case WholeAniClip.Jump:
                    animator.CrossFade(Animator.StringToHash("JumpUp"), 0.1f, wholeAniLayer, 0);
                    break;
                case WholeAniClip.Fall:
                    animator.CrossFade(Animator.StringToHash("Fall"), 0.1f, wholeAniLayer, 0);
                    break;
            }
        }
    }

    void UpperSuitLowerAnimation()
    {
        UpperAniClip clip = UpperAniClip.Empty;
        switch (curLowerAniClip)
        {
            case LowerAniClip.Aim:
                clip = UpperAniClip.Aim;
                break;
            case LowerAniClip.Idle:
                clip = UpperAniClip.Idle;
                break;
            case LowerAniClip.Move:
                clip = UpperAniClip.Move;
                break;
            case LowerAniClip.Strafe:
                clip = UpperAniClip.Strafe;
                break;
        }
        SetUpperAniClip(clip);
    }

    public bool shootIk = false;
    public bool footIk = false;
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
