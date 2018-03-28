using UnityEngine;
using System.Collections;

public class WatchPoint : MonoBehaviour {
    public Transform player = null;
	// Use this for initialization
	void Start () {
	
	}

    int fixedCount = 0;
    void FixedUpdate()
    {
        fixedCount++;
    }

	// Update is called once per frame
	void Update () {
        // chase player
        Vector3 toPlayer = player.position - transform.position;
        float step = Mathf.Max(toPlayer.magnitude * fixedCount * 0.1f, 0.001f);
        transform.position = Vector3.MoveTowards(transform.position, player.position, step);
        fixedCount = 0;
    }
}
