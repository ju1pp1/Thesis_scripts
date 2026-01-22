using UnityEngine;

public class ChaseState : IEnemyState
{
    readonly EnemyBlackboard bb; readonly EnemyStateMachine fsm;
    public string Name => "Chase";
    public ChaseState(EnemyBlackboard bb, EnemyStateMachine fsm) { this.bb = bb; this.fsm = fsm;}
    public void OnEnter() 
    {
        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.EnterMode("Chase", "InRange+LOS");

        AIEventLogger.Action(bb, "Enter Chase"); 
    }
    public void Tick()
    {
        if (bb.HealthPct <= bb.fleeHealthPct) { fsm.ChangeState(new FleeState(bb, fsm)); return; }

        if (bb.DistanceToPlayer > bb.chaseRadius || !bb.HasLoS)
        {
            fsm.ChangeState(new IdleState(bb, fsm)); return;
        }

        float stopRange = bb.controller.kind == EnemyController.EnemyKind.Melee
            ? bb.controller.meleeStopRange
            : (bb.controller.kind == EnemyController.EnemyKind.Ranged ? bb.controller.rangedStopRange : bb.controller.casterStopRange);

        //Implementing
        /*if (bb.DistanceToPlayer > stopRange)
        {
            bb.agent.stoppingDistance = stopRange;
            bb.agent.SetDestination(bb.player.position);
            return;
        }
        if (!bb.agent.pathPending) bb.agent.ResetPath();
        */
        if(bb.DistanceToPlayer > stopRange)
        {
            bb.agent.stoppingDistance = stopRange;
            bb.agent.SetDestination(bb.player.position);
            return;
        }
        bb.agent.ResetPath();

        if (bb.controller.kind == EnemyController.EnemyKind.Caster && bb.skillCaster != null)
        {
            if (bb.skillCaster.TryCastIfReady(bb.player.GetComponent<Stats>())) return;
            if (bb.skillCaster.IsBusy) return;
        }
        else
        {
            fsm.ChangeState(new AttackState(bb, fsm));
        }
    }
    public void OnExit() { }
}
