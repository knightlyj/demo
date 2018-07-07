using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MobileInput : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    WeaponType lastWeaponType = WeaponType.Empty;
    [SerializeField]
    Transform lockTarget = null;
    [SerializeField]
    Text txtAimAndBlock = null;
    // Update is called once per frame
    void Update()
    {
        Player p = GlobalVariables.localPlayer;
        if (p != null)
        {
            if (lastWeaponType != p.weaponType)
            {
                if (p.weaponType == WeaponType.Melee)
                {
                    lockTarget.gameObject.SetActive(true);
                    txtAimAndBlock.text = "格档";
                }
                else if (p.weaponType == WeaponType.Pistol)
                {
                    lockTarget.gameObject.SetActive(false);
                    txtAimAndBlock.text = "瞄准";
                }
                lastWeaponType = p.weaponType;
            }
        }
        else
        {
            lastWeaponType = WeaponType.Empty;
        }
    }
}
