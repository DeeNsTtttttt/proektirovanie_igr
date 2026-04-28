using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] waypoints;

    [Header("Ranges")]
    [SerializeField, Min(0f)] private float detectionRange = 10f;
    [SerializeField, Min(0f)] private float chaseRange = 12f;
    [SerializeField, Min(0f)] private float attackRange = 2f;

    [Header("Vision")]
    [SerializeField] private LayerMask visibilityMask = ~0;
    [SerializeField, Min(0f)] private float eyeHeight = 1.0f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField, Min(0.05f)] private float attackCooldown = 1f;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private State currentState = State.Patrol;
    private int currentWaypointIndex;
    private float lastAttackTime = -Mathf.Infinity;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        ConfigurePhysicsForNavAgent();
        TryFindPlayerByTag();
    }

    private void ConfigurePhysicsForNavAgent()
    {
        if (rb == null)
        {
            return;
        }

        // NavMeshAgent should drive movement, so physics impulses must not push the enemy away.
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            currentWaypointIndex = 0;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    public void SetWaypoints(Transform[] newWaypoints)
    {
        if (newWaypoints == null || newWaypoints.Length == 0)
        {
            waypoints = System.Array.Empty<Transform>();
            currentWaypointIndex = 0;
            return;
        }

        List<Transform> filtered = new List<Transform>(newWaypoints.Length);
        for (int i = 0; i < newWaypoints.Length; i++)
        {
            if (newWaypoints[i] != null)
            {
                filtered.Add(newWaypoints[i]);
            }
        }

        waypoints = filtered.ToArray();
        currentWaypointIndex = 0;

        if (agent != null && waypoints.Length > 0 && agent.isOnNavMesh)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void Update()
    {
        if (player == null)
        {
            TryFindPlayerByTag();
            Patrol();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                if (distanceToPlayer <= detectionRange && CanSeePlayer())
                {
                    currentState = State.Chase;
                }
                else
                {
                    Patrol();
                }
                break;

            case State.Chase:
                if (distanceToPlayer > chaseRange || !CanSeePlayer())
                {
                    currentState = State.Patrol;
                }
                else if (distanceToPlayer <= attackRange)
                {
                    currentState = State.Attack;
                }
                else
                {
                    Chase();
                }
                break;

            case State.Attack:
                if (distanceToPlayer > attackRange)
                {
                    currentState = State.Chase;
                }
                else
                {
                    Attack();
                }
                break;
        }
    }

    private void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void Chase()
    {
        agent.SetDestination(player.position);
    }

    private void Attack()
    {
        agent.ResetPath();
        FacePlayer();

        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;
        DealDamage();
    }

    private void DealDamage()
    {
        if (player == null)
        {
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = player.GetComponentInParent<PlayerHealth>();
        }
        if (playerHealth == null)
        {
            playerHealth = player.GetComponentInChildren<PlayerHealth>();
        }
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = GetPlayerTargetPoint();
        Vector3 toTarget = target - origin;
        float distance = toTarget.magnitude;

        if (distance <= 0.001f)
        {
            return true;
        }

        int mask = visibilityMask.value == 0 ? ~0 : visibilityMask.value;
        RaycastHit[] hits = Physics.RaycastAll(origin, toTarget.normalized, distance, mask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
            {
                continue;
            }

            return IsPlayerHit(hitTransform);
        }

        return false;
    }

    private bool IsPlayerHit(Transform hitTransform)
    {
        return hitTransform == player || hitTransform.IsChildOf(player);
    }

    private Vector3 GetPlayerTargetPoint()
    {
        Collider playerCollider = player.GetComponentInChildren<Collider>();
        if (playerCollider != null)
        {
            return playerCollider.bounds.center;
        }

        return player.position + Vector3.up * 0.9f;
    }

    private void FacePlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 12f);
    }

    private void TryFindPlayerByTag()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1f, 0.55f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
