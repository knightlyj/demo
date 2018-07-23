using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [SerializeField]
    Light shootLight = null;
    // Use this for initialization
    void Start()
    {

    }

    float lightOffTimer = 0f;
    // Update is called once per frame
    void Update()
    {
        lightOffTimer -= Time.deltaTime;
        if (lightOffTimer <= 0)
        {
            shootLight.enabled = false;
        }
        
    }
    

    [SerializeField]
    Transform muzzle = null;
    public bool Shoot(Vector3 targetPoint, LayerMask layerMask, out RaycastHit hitInfo)
    {
        if (muzzle != null)
        {
            //显示射击光效
            shootLight.enabled = true;
            lightOffTimer = 0.05f;

            LevelManager lm = UnityHelper.GetLevelManager();
            if(lm != null)
            {
                lm.CreateParticleEffect(LevelManager.ParticleEffectType.Shoot, muzzle.position, -transform.right);
            }

            if (Physics.Raycast(muzzle.position, targetPoint - muzzle.position, out hitInfo, 100f, layerMask))
            {
                return true;
            }
        }
        hitInfo = new RaycastHit();
        return false;
    }
}
