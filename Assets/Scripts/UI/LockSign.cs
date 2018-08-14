using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LockSign : MonoBehaviour
{

    Image imgComp = null;
    void Awake()
    {
        imgComp = GetComponent<Image>();
    }

    // Use this for initialization
    void Start()
    {

    }

    [HideInInspector]
    public Transform target = null;
    // Update is called once per frame
    void Update()
    {
        bool show = false;
        if (target)
        {
            CreatureCommon cc = target.GetComponent<CreatureCommon>();
            if (cc)
            {
                Vector3 worldPos = cc.aim.position;
                Vector3 viewPortPos = Camera.main.WorldToViewportPoint(worldPos);
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                if (viewPortPos.z > Camera.main.nearClipPlane)
                {
                    show = true;
                    RectTransform rt = transform as RectTransform;
                    rt.position = screenPos;
                }
                else
                {
                    show = false;
                }
            }
        }

        imgComp.enabled = show;

    }
}
