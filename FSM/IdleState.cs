using UnityEngine;

public class IdleState : IEnemyState
{
    readonly EnemyBlackboard bb; readonly EnemyStateMachine fsm;
    public string Name => "Idle";
    public IdleState(EnemyBlackboard bb, EnemyStateMachine fsm) { this.bb = bb; this.fsm = fsm; }
    public void OnEnter() 
    {
        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.EnterMode("Idle", "InRange+LOS");

        bb.agent.ResetPath();
        AIEventLogger.Action(bb, "Enter Idle");
    }
    public void Tick()
    {
        if (bb.DistanceToPlayer <= bb.controller.lookRadius && bb.HasLoS)
        {
            if (bb.HealthPct <= bb.fleeHealthPct) { fsm.ChangeState(new FleeState(bb, fsm)); return; }
            fsm.ChangeState(new ChaseState(bb, fsm));
            return;
        }
        if(bb.patrolRadius > 0f && bb.agent != null)
        {
            fsm.ChangeState(new PatrolState(bb, fsm));
            return;
        }
    }
    public void OnExit() { }
}
