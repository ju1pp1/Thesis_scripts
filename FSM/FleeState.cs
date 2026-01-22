using UnityEngine;
using UnityEngine.AI;

public class FleeState : IEnemyState
{
    readonly EnemyBlackboard bb; readonly EnemyStateMachine fsm;
    public string Name => "Flee";
    public FleeState(EnemyBlackboard bb, EnemyStateMachine fsm) { this.bb = bb; this.fsm = fsm; }
    public void OnEnter() 
    {
        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.EnterMode("Flee", "InRange+LOS");

        AIEventLogger.Action(bb, "Enter Flee"); 
    }
    public void Tick()
    {
        if (bb.HealthPct >= bb.reengageHealthPct) { fsm.ChangeState(new IdleState(bb, fsm)); return; }
        if (bb.player == null) { bb.agent.ResetPath(); return; }

        Vector3 away = (bb.transform.position - bb.player.position).normalized;
        Vector3 dest = bb.transform.position + away * 8f;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dest, out hit, 4f, NavMesh.AllAreas))
            bb.agent.SetDestination(hit.position);
    }
    public void OnExit() { }
}
