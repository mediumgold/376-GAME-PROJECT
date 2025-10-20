using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public Transform player;
    [Header("Sight")]
    public float viewRadius = 12f;
    [Range(0, 180)] public float viewAngle = 60f;
    public LayerMask obstacleMask; 
    public LayerMask playerMask;

    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float patrolSpeed = 2f;
    public float loseSightAfter = 2f;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private float lastSeenTime = Mathf.NegativeInfinity;
    private enum State { Patrol, Chase }
    private State state = State.Patrol;

    void Awake() => agent = GetComponent<NavMeshAgent>();

    void Start()
    {
             agent = GetComponent<NavMeshAgent>();

    if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        agent.Warp(hit.position);

    if (patrolPoints != null && patrolPoints.Length > 0)
        agent.SetDestination(patrolPoints[0].position);
    }

    void Update()
    {
        bool canSee = CanSeePlayer();

        if (canSee)
        {
            state = State.Chase;
            lastSeenTime = Time.time;
        }
        else if (state == State.Chase && Time.time - lastSeenTime > loseSightAfter)
        {
            state = State.Patrol;
            GoToNextPoint();
        }

        if (state == State.Patrol && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            GoToNextPoint();

        if (state == State.Chase)
            agent.SetDestination(player.position);
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;
        currentIndex = (currentIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentIndex].position);
    }

    bool CanSeePlayer()
    {
        if (!player) return false;
        Vector3 dirToPlayer = player.position - transform.position;
        float dist = dirToPlayer.magnitude;
        if (dist > viewRadius) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer.normalized);
        if (angle > viewAngle) return false;

        if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer.normalized, out RaycastHit hit, viewRadius, obstacleMask | playerMask))
            return hit.transform.CompareTag("Player");

        return false;
    }
}
