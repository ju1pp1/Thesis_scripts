using UnityEngine;

public class BT_PatrolRandom : BTNodeWithLifecycle
{
    public BT_PatrolRandom(EnemyBlackboard bb) : base(bb) { }

    public override void OnEnter()
    {
        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.EnterMode("Patrol", "InRange+LOS");

        AIEventLogger.Action(bb, "BT Enter Patrol");
        if(bb.agent != null) bb.agent.stoppingDistance = 0.2f;
        PickNewPatrolPoint();
    }

    protected override BTStatus TickRunning()
    {
        if (bb.DistanceToPlayer <= bb.controller.lookRadius && bb.HasLoS)
            return BTStatus.Failure;

        if(bb.agent == null) return BTStatus.Failure;

        if(bb.hasPatrolTarget && bb.ReachedDestination())
        {
            if(Time.time < bb.dwellUntil) return BTStatus.Running;

            bb.dwellUntil = Time.time + Random.Range(bb.dwellMin, bb.dwellMax);
            PickNewPatrolPoint();
        }

        if (!bb.agent.pathPending && bb.agent.velocity.sqrMagnitude < 0.01f && !bb.ReachedDestination())
            PickNewPatrolPoint();

        return BTStatus.Running;
    }
    void PickNewPatrolPoint()
    {
        if (bb.agent == null) return;

        var center = bb.PatrolOrigin;
        var distFromCenter = Vector3.Distance(bb.transform.position, center);

        if(bb.leashRadius > 0f && distFromCenter > bb.leashRadius * 1.1f)
        {
            var dir = (center - bb.transform.position).normalized;
            var biased = bb.transform.position + dir * (bb.patrolRadius * 0.75f);

            if(bb.GetRandomPointOnNavMesh(biased, bb.patrolRadius * 0.5f, out var leashTarget))
            {
                bb.lastPatrolTarget = leashTarget;
                bb.hasPatrolTarget = true;
                bb.agent.SetDestination(leashTarget);
                return;
            }
        }
        if(bb.GetRandomPointOnNavMesh(center, bb.patrolRadius, out var p))
        {
            bb.lastPatrolTarget = p;
            bb.hasPatrolTarget = true;
            bb.agent.SetDestination(p);
        }
        else
        {
            bb.hasPatrolTarget = false;
        }
    }
    public override void OnExit()
    {
        
    }
}
