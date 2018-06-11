using UnityEngine;
using System.Collections;
using UnityEngine.Events;


public partial class Player
{
    const int upperAniLayer = 1;
    const int lowerAniLayer = 2;
    const float defTransTime = 0.1f;

    public delegate void OnAnimationEvent(AnimationEvent aniEvent);
    public event OnAnimationEvent onAnimationEvent;
    //动作完成
    void AnimationEventHandler(AnimationEvent aniEvent)
    {
        if (onAnimationEvent != null)
            onAnimationEvent(aniEvent);
    }

    //设置walk和run的blend比例
    public float walkRun
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

    //strafe的blend参数
    public float strafeForward
    {
        set
        {
            animator.SetFloat("StrafeForward", value);
        }
        get
        {
            return animator.GetFloat("StrafeForward");
        }
    }

    public float strafeRight
    {
        set
        {
            animator.SetFloat("StrafeRight", value);
        }
        get
        {
            return animator.GetFloat("StrafeRight");
        }
    }

    public enum UpperAniState
    {
        Empty,
        Idle,
        Move,
        Aim,
        Strafe,
        BlockIdle,
        MeleeStrafe,
        SwapWeapon,
        Shoot,
        Roll,
        Jump,
        Fall,
        Attack,
        GetHit,
    }
    UpperAniState curUpperAniState = UpperAniState.Empty;
    //设置上半身动作
    public void SetUpperAniState(UpperAniState state, bool reset = false, string stateName = null, float transTime = defTransTime, float startTime = 0f)
    {
        if (curUpperAniState == state && !reset)
        {
            return;
        }
        else
        {
            curUpperAniState = state;
            switch (state)
            {
                case UpperAniState.Idle:
                    animator.CrossFade(Animator.StringToHash("Idle"), transTime, upperAniLayer, startTime);
                    break;
                case UpperAniState.Move:
                    animator.CrossFade(Animator.StringToHash("Move"), transTime, upperAniLayer, startTime);
                    break;
                case UpperAniState.Aim:
                    animator.CrossFade(Animator.StringToHash("Aim"), transTime, upperAniLayer, startTime);
                    break;
                case UpperAniState.BlockIdle:
                    animator.CrossFade(Animator.StringToHash("BlockIdle"), transTime, upperAniLayer, startTime);
                    break;
                case UpperAniState.SwapWeapon:
                    animator.CrossFade(Animator.StringToHash("SwapWeapon"), transTime, upperAniLayer, startTime);
                    break;
                case UpperAniState.Shoot:
                    animator.CrossFade(Animator.StringToHash("Shoot"), transTime, upperAniLayer, startTime);
                    break;
                case UpperAniState.Jump:
                    animator.CrossFade(Animator.StringToHash("Jump"), transTime, upperAniLayer, startTime);
                    break;
                case UpperAniState.Fall:
                    animator.CrossFade(Animator.StringToHash("Fall"), transTime, upperAniLayer, startTime);
                    break;
                case UpperAniState.Roll:
                    animator.CrossFade(Animator.StringToHash("Roll"), transTime, upperAniLayer, startTime);
                    break;
                case UpperAniState.Attack:
                    if (stateName != null)
                        animator.CrossFade(Animator.StringToHash(stateName), transTime, upperAniLayer, startTime);
                    else
                        animator.CrossFade(Animator.StringToHash("Attack1"), transTime, upperAniLayer, startTime);
                    break;
                case UpperAniState.GetHit:
                    if (stateName != null)
                        animator.CrossFade(Animator.StringToHash(stateName), transTime, upperAniLayer, startTime);
                    else
                        animator.CrossFade(Animator.StringToHash("GetHitFront"), transTime, upperAniLayer, startTime);
                    break;
            }
        }
    }

    public enum LowerAniState
    {
        Empty,
        Idle,
        Move,
        Aim,
        BlockIdle,
        MeleeStrafe,
        AimStrafe,
        Roll,
        Jump,
        Fall,
        Attack,
        GetHit,
    }
    LowerAniState curLowerAniState = LowerAniState.Empty;
    //设置下半身动作
    public void SetLowerAniState(LowerAniState state, bool reset = false, string stateName = null, float transTime = defTransTime)
    {
        if (curLowerAniState == state && !reset)
        {
            return;
        }
        else
        {
            curLowerAniState = state;
            switch (state)
            {
                case LowerAniState.Idle:
                    animator.CrossFade(Animator.StringToHash("Idle"), transTime, lowerAniLayer, 0);
                    break;
                case LowerAniState.Move:
                    animator.CrossFade(Animator.StringToHash("Move"), transTime, lowerAniLayer, 0);
                    break;
                case LowerAniState.Aim:
                    animator.CrossFade(Animator.StringToHash("Aim"), transTime, lowerAniLayer, 0);
                    break;
                case LowerAniState.AimStrafe:
                    animator.CrossFade(Animator.StringToHash("AimStrafe"), transTime, lowerAniLayer, 0);
                    break;
                case LowerAniState.Jump:
                    animator.CrossFade(Animator.StringToHash("Jump"), transTime, lowerAniLayer, 0);
                    break;
                case LowerAniState.Roll:
                    animator.CrossFade(Animator.StringToHash("Roll"), transTime, lowerAniLayer, 0);
                    break;
                case LowerAniState.Fall:
                    animator.CrossFade(Animator.StringToHash("Fall"), transTime, lowerAniLayer, 0);
                    break;
                case LowerAniState.Attack:
                    if (stateName != null)
                        animator.CrossFade(Animator.StringToHash(stateName), transTime, lowerAniLayer, 0);
                    else
                        animator.CrossFade(Animator.StringToHash("Attack1"), transTime, lowerAniLayer, 0);
                    break;
                case LowerAniState.BlockIdle:
                    animator.CrossFade(Animator.StringToHash("BlockIdle"), transTime, lowerAniLayer, 0);
                    break;
                case LowerAniState.MeleeStrafe:
                    animator.CrossFade(Animator.StringToHash("MeleeStrafe"), transTime, lowerAniLayer, 0);
                    break;
                case LowerAniState.GetHit:
                    if (stateName != null)
                        animator.CrossFade(Animator.StringToHash(stateName), transTime, lowerAniLayer, 0);
                    else
                        animator.CrossFade(Animator.StringToHash("GetHitFront"), transTime, lowerAniLayer, 0);
                    break;
            }
        }
    }

    public void UpperSuitLowerAnimation()
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
            case LowerAniState.AimStrafe:
                clip = UpperAniState.Aim;
                break;
            case LowerAniState.MeleeStrafe:
                clip = UpperAniState.MeleeStrafe;
                break;
            case LowerAniState.BlockIdle:
                clip = UpperAniState.BlockIdle;
                break;
        }
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(lowerAniLayer);
        SetUpperAniState(clip, true, null, defTransTime, info.normalizedTime);
    }

    [HideInInspector]
    public bool shootIk = false;
    [HideInInspector]
    public bool footIk = false;

    [SerializeField]
    LayerMask footIKLayer = 0;
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
            if (Physics.Raycast(leftUp, Vector3.down, out hitInfo, bb, footIKLayer))
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
            if (Physics.Raycast(rightUp, Vector3.down, out hitInfo, bb, footIKLayer))
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
