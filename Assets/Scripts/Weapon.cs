using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    BoxCollider boxCollider = null;
    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    public bool colliderEanbled
    {
        set
        {
            boxCollider.enabled = value;
        }
        get
        {
            return boxCollider.enabled;
        }
    }

    public delegate void OnHitCallback(Collider other);
    public OnHitCallback onHit;
    void OnTriggerEnter(Collider other)
    {
        if (onHit != null)
            onHit(other);
    }
}
