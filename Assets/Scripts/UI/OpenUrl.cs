using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


public class OpenUrl : MonoBehaviour, IPointerClickHandler
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [SerializeField]
    string url = null;
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if(url != null)
        {
            Application.OpenURL(url);
        }
    }
}
