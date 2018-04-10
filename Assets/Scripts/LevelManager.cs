using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    float gravityScale = 1.0f;
    // Use this for initialization
    void Start()
    {
        Physics.gravity = new Vector3(0f, -9.8f, 0f) * gravityScale; //设置重力
    }
    
    // Update is called once per frame
    void Update()
    {

    }
    
}
