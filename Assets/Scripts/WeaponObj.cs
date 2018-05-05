using UnityEngine;
using System.Collections;


public class WeaponObj : MonoBehaviour
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

    public delegate void OnHitCallback(Collider other);
    public OnHitCallback onHit;
    void OnTriggerEnter(Collider other)
    {
        if (onHit != null)
            onHit(other);
    }
}
