using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class EnemyBlackboard : MonoBehaviour
{
    [Header("Dynamic")]
    public Transform player;
    public Stats selfStats;
    public NavMeshAgent agent;
    public CharacterCombat combat;
    public EnemySkillCaster skillCaster;
    public EnemyController controller;

    [Header("Patrol random-walk (FSM baseline)")]
    public Transform patrolCenter;
    public float patrolRadius = 12f;
    public float dwellMin = 0.5f;
    public float dwellMax = 2.0f;
    public float leashRadius = 18f;

    [HideInInspector] public Vector3 spawnPos;
    [HideInInspector] public float dwellUntil;
    [HideInInspector] public Vector3 lastPatrolTarget;
    [HideInInspector] public bool hasPatrolTarget;

    [Range(0f, 1f)] public float fleeHealthPct = 0.2f;
    public float chaseRadius = 15f;
    public float reengageHealthPct = 0.4f; // When to stop fleeing?

    [Header("Runtime context (read-only for most states)")]
    public float DistanceToPlayer {  get; private set; }
    public bool HasLoS { get; private set; }
    public float HealthPct { get; private set; }

    private void Awake()
    {
        spawnPos = transform.position;
    }

    public Vector3 PatrolOrigin =>
        patrolCenter ? patrolCenter.position : spawnPos;

    public bool GetRandomPointOnNavMesh(Vector3 center, float radius, out Vector3 result)
    {
        // Try a few times to find a valid point
        for (int i = 0; i < 10; i++)
        {
            var random = center + Random.insideUnitSphere * radius;
            random.y = center.y;
            if (NavMesh.SamplePosition(random, out var hit, 2.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }

    public bool ReachedDestination(float extraStop = 0.2f)
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > Mathf.Max(agent.stoppingDistance, extraStop)) return false;
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.01f) return false;
        return true;
    }
    public void RefreshSenses(bool requireLoS, LayerMask losBlockers)
    {
        if (player == null || selfStats == null) return;
        DistanceToPlayer = Vector3.Distance(player.position, transform.position);
        HealthPct = Mathf.Approximately(selfStats.maxHealth, 0) ? 0f : (selfStats.currentHealth / (float)selfStats.maxHealth);

        if (requireLoS)
        {
            Vector3 origin = transform.position + Vector3.up * 1.6f;
            Vector3 dest = player.position + Vector3.up * 1.4f;
            if (Physics.Raycast(origin, (dest - origin).normalized, out var hit, DistanceToPlayer + 0.25f, losBlockers, QueryTriggerInteraction.Ignore))
                HasLoS = (hit.transform == player);
            else
                HasLoS = true;
        }
        else
            HasLoS = true;
    }
}
