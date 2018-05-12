using UnityEngine;
using System.Collections;

public class CreatureCommon : MonoBehaviour
{
    //*********锁定时的瞄准点**************
    [HideInInspector]
    public Transform aim { get { return this._aim; } }

    [SerializeField]
    Transform _aim = null;

    //***********视线点*************
    [HideInInspector]
    public Transform sight { get { return this._sight; } }

    [SerializeField]
    Transform _sight = null;
    
}
