using UnityEngine;
using System.Collections;

public class WatchPoint : MonoBehaviour {
    public Transform player = null;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        // chase player
        Vector3 toPlayer = player.position - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, player.position, toPlayer.magnitude * Time.deltaTime);
    }
}
