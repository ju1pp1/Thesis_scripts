using UnityEngine;

public class PatrolState : IEnemyState
{
    readonly EnemyBlackboard bb; readonly EnemyStateMachine fsm;
    public string Name => "Patrol";
    public PatrolState(EnemyBlackboard bb, EnemyStateMachine fsm) { this.bb = bb; this.fsm = fsm; }
    public void OnEnter() 
    {
        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.EnterMode("Patrol", "InRange+LOS");

        AIEventLogger.Action(bb, "Enter Patrol");
        bb.agent.stoppingDistance = 0.2f; PickNewPatrolPoint();
    }
    void PickNewPatrolPoint()
    {
        var center = bb.PatrolOrigin;
        var distFromCenter = Vector3.Distance(bb.transform.position, center);
        if (bb.leashRadius > 0f && distFromCenter > bb.leashRadius * 1.1f)
        {
            var dir = (center - bb.transform.position).normalized;
            var biased = bb.transform.position + dir * (bb.patrolRadius * 0.75f);
            if (bb.GetRandomPointOnNavMesh(biased, bb.patrolRadius * 0.5f, out var leashTarget))
            {
                bb.lastPatrolTarget = leashTarget;
                bb.hasPatrolTarget = true;
                bb.agent.SetDestination(leashTarget);
                return;
            }
        }
        if (bb.GetRandomPointOnNavMesh(center, bb.patrolRadius, out var p))
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

    public void Tick()
    {
        if (bb.DistanceToPlayer <= bb.controller.lookRadius && bb.HasLoS)
        {
            if (bb.HealthPct <= bb.fleeHealthPct) { fsm.ChangeState(new FleeState(bb, fsm)); return; }
            fsm.ChangeState(new ChaseState(bb, fsm)); return;
        }

        if (bb.hasPatrolTarget && bb.ReachedDestination())
        {
            if (Time.time >= bb.dwellUntil)
            {
                bb.dwellUntil = Time.time + Random.Range(bb.dwellMin, bb.dwellMax);
            }
            else
            {
                return;
            }
            PickNewPatrolPoint();
        }
        if (!bb.agent.pathPending && bb.agent.velocity.sqrMagnitude < 0.01f && !bb.ReachedDestination())
        {
            PickNewPatrolPoint();
        }
    }
    public void OnExit() { }
}
