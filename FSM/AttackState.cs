using UnityEngine;

public class AttackState : IEnemyState
{
    readonly EnemyBlackboard bb; readonly EnemyStateMachine fsm;
    public string Name => "Attack";
    float _nextCheck;
    public AttackState(EnemyBlackboard bb, EnemyStateMachine fsm) { this.bb = bb; this.fsm = fsm; }
    public void OnEnter() 
    {
        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.EnterMode("Attack", "InRange+LOS");

        AIEventLogger.Action(bb, "Enter Attack");
        bb.agent.ResetPath();
    }
    public void Tick()
    {
        if (bb.HealthPct <= bb.fleeHealthPct) { fsm.ChangeState(new FleeState(bb, fsm)); return; }
        if (bb.player == null) {  fsm.ChangeState(new IdleState(bb, fsm)); return; }

        Vector3 to = bb.player.position - bb.transform.position; to.y=0;
        if (to.sqrMagnitude > 0.001f) bb.transform.rotation = Quaternion.Slerp(bb.transform.rotation, Quaternion.LookRotation(to), Time.deltaTime * bb.controller.faceTurnSpeed);

        if (bb.controller.kind == EnemyController.EnemyKind.Caster && bb.skillCaster != null)
        {
            var pStats = bb.player.GetComponent<Stats>();
            if (bb.skillCaster.TryCastIfReady(pStats)) return;
            if (bb.skillCaster.IsBusy) return;
        }

        if (Time.time >= _nextCheck)
        {
            _nextCheck = Time.time + 0.1f;
            var tgtStats = bb.player.GetComponent<Stats>();
            if (tgtStats != null) bb.combat.Attack(tgtStats);
        }

        float stopRange = bb.controller.kind == EnemyController.EnemyKind.Melee
            ? bb.controller.meleeStopRange
            : (bb.controller.kind == EnemyController.EnemyKind.Ranged ? bb.controller.rangedStopRange : bb.controller.casterStopRange);

        if (bb.DistanceToPlayer > stopRange + 0.5f || !bb.HasLoS)
            fsm.ChangeState(new ChaseState(bb, fsm));
    }
    public void OnExit() { }
}
