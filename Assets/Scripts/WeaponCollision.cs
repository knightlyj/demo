using UnityEngine;
using System.Collections;


public class WeaponCollision : MonoBehaviour
{
    new Collider collider = null;
    void Awake()
    {
        collider = GetComponent<Collider>();
    }

    public bool colliderEanbled
    {
        set
        {
            collider.enabled = value;
        }
        get
        {
            return collider.enabled;
        }
    }

    public delegate void OnHitCallback(Collision collision);
    public OnHitCallback onHit;
    void OnCollisionEnter(Collision collision)
    {
        if (onHit != null)
            onHit(collision);
    }
}
