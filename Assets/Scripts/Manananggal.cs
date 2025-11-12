using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
//using System.Numerics;

public class Manananggal : MonoBehaviour
{
    //public string clipName = "Flying";
    Animator anim;
    public Transform player;

    public float speed;
    public Vector3 offset = new Vector3(5f, 5f, 5f);
    
    //[SerializeField] private float moveThreshold = 0.05f; // m/s under which we consider "idle"
    //[SerializeField] private float speedDampTime = 0.1f;  // smoothing for Animator parameter
    //private Vector3 prevPos;
    
    void Awake()
    {
        anim = GetComponent<Animator>();
        var p = FindAnyObjectByType<Player>();
        player = p.transform;
        //prevPos = transform.position;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerTarget = player.position + offset;
        transform.position = Vector3.MoveTowards(transform.position, playerTarget + offset, speed * Time.deltaTime);
        playerTarget.y = transform.position.y;
        transform.LookAt(playerTarget);

        // // compute velocity magnitude (planar if you want)
        // Vector3 delta = transform.position - prevPos;
        // float vel = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        // // feed Animator (float)
        // anim.SetFloat("speed", vel, speedDampTime, Time.deltaTime);

        // // (optional) also set a bool if you prefer logic in Animator as bools
        // bool isMoving = vel > moveThreshold;
        // anim.SetBool("isMoving", isMoving);

        // prevPos = transform.position;


    }
    
    public void animDebug()
    {
        anim.SetBool("isFlying", true);
    }
}
