using UnityEngine;
using System.Collections;
using UnityEngine.Events;


public partial class Player
{
    public static class StateNameHash
    {
        public static readonly int idle = Animator.StringToHash("Idle");
        public static readonly int move = Animator.StringToHash("Move");
        public static readonly int aim = Animator.StringToHash("Aim");
        public static readonly int blockIdle = Animator.StringToHash("BlockIdle");
        public static readonly int blockGetHit = Animator.StringToHash("BlockGetHit");
        public static readonly int swapWeapon = Animator.StringToHash("SwapWeapon");
        public static readonly int shoot = Animator.StringToHash("Shoot");
        public static readonly int jump = Animator.StringToHash("Jump");
        public static readonly int fall = Animator.StringToHash("Fall");
        public static readonly int roll = Animator.StringToHash("Roll");
        public static readonly int attack1 = Animator.StringToHash("Attack1");
        public static readonly int attack2 = Animator.StringToHash("Attack2");
        public static readonly int getHitFront = Animator.StringToHash("GetHitFront");
        public static readonly int getHitBack = Animator.StringToHash("GetHitBack");
        public static readonly int aimStrafe = Animator.StringToHash("AimStrafe");
        public static readonly int meleeStrafe = Animator.StringToHash("MeleeStrafe");
        public static readonly int die = Animator.StringToHash("Die");
    }

    const int upperAniLayer = 1;
    const int lowerAniLayer = 2;
    const float defTransTime = 0.1f;

    public delegate void OnAnimationEvent(AnimationEvent aniEvent);
    public event OnAnimationEvent onAnimationEvent;

    //动画事件
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
    
    int curUpperAniStateHash = StateNameHash.idle;
    //设置上半身动作
    public void SetUpperAniState(int stateHash, bool reset = false, float transTime = defTransTime, float startTime = 0f)
    {
        if (curUpperAniStateHash == stateHash && !reset)
        {
            return;
        }
        else
        {
            curUpperAniStateHash = stateHash;
            animator.CrossFade(stateHash, transTime, upperAniLayer, startTime);
        }
    }

    int curLowerAniStateHash = StateNameHash.idle;
    //设置下半身动作
    public void SetLowerAniState(int stateHash, bool reset = false, float transTime = defTransTime, float startTime = 0f)
    {
        if (curLowerAniStateHash == stateHash && !reset)
        {
            return;
        }
        else
        {
            curLowerAniStateHash = stateHash;
            animator.CrossFade(stateHash, transTime, lowerAniLayer, startTime);
        }
    }

    public void UpperSuitLowerAnimation()
    {
        int state = StateNameHash.idle;
        if (curLowerAniStateHash == StateNameHash.aim)
            state = StateNameHash.aim;
        else if (curLowerAniStateHash == StateNameHash.idle)
            state = StateNameHash.idle;
        else if (curLowerAniStateHash == StateNameHash.move)
            state = StateNameHash.move;
        else if (curLowerAniStateHash == StateNameHash.aimStrafe)
            state = StateNameHash.aim;
        else if (curLowerAniStateHash == StateNameHash.meleeStrafe)
            state = StateNameHash.meleeStrafe;
        else if (curLowerAniStateHash == StateNameHash.blockIdle)
            state = StateNameHash.blockIdle;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(lowerAniLayer);
        SetUpperAniState(state, true, defTransTime, info.normalizedTime);
    }

    public void GetUpperAniState(out int stateHash, out float normTime)
    {
        if (!animator.IsInTransition(upperAniLayer))
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(upperAniLayer);
            stateHash = info.shortNameHash;
            normTime = info.normalizedTime;
        }
        else
        {
            AnimatorStateInfo info = animator.GetNextAnimatorStateInfo(upperAniLayer);
            stateHash = info.shortNameHash;
            normTime = info.normalizedTime;
        }
    }

    public void GetLowerAniState(out int stateHash, out float normTime)
    {
        if (!animator.IsInTransition(lowerAniLayer))
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(lowerAniLayer);
            stateHash = info.shortNameHash;
            normTime = info.normalizedTime;
        }
        else
        {
            AnimatorStateInfo info = animator.GetNextAnimatorStateInfo(lowerAniLayer);
            stateHash = info.shortNameHash;
            normTime = info.normalizedTime;
        }
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
            {   //脚下一定距离没有地面

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
            { //脚下一定距离没有地面

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
