using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {
    [SerializeField]
    Bar healthBar = null;
    [SerializeField]
    Bar energyBar = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    public void SetPlayerHealth(float ratio)
    {
        healthBar.SetRatio(ratio);
    }

    public void SetPlayerEnergy(float ratio)
    {
        energyBar.SetRatio(ratio);
    }

    [SerializeField]
    Transform mobileUI = null;
    void OnEnable()
    {
        if(Application.platform == RuntimePlatform.Android || GlobalVariables.mobileUIOnPC)
        {
            mobileUI.gameObject.SetActive(true);
        }
        else
        {
            mobileUI.gameObject.SetActive(false);
        }
    }
}
