using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Player : MonoBehaviour {
    public event UnityAction onPlayerDestroy;
    // Use this for initialization
    protected void Start () {
	
	}

    // Update is called once per frame
    protected void Update () {
	
	}

    protected void OnDestroy()
    {
        if (onPlayerDestroy != null)
        {
            onPlayerDestroy();
        }
    }
}
