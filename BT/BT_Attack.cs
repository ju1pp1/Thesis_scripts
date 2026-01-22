using UnityEngine;

public class BT_Attack : BTNodeWithLifecycle
{
    private float _nextAttackCheck;

    public BT_Attack(EnemyBlackboard bb) : base(bb) { }

    public override void OnEnter()
    {
        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.EnterMode("Attack", "InRange+LOS");

        AIEventLogger.Action(bb, "BT Enter Attack");
        if (bb.agent != null) bb.agent.ResetPath();
        _nextAttackCheck = 0f;
    }

    protected override BTStatus TickRunning()
    {
        if (bb.player == null || bb.combat == null) return BTStatus.Failure;

        if (bb.HealthPct <= bb.fleeHealthPct) return BTStatus.Failure; // let flee branch run
        if (!bb.HasLoS) return BTStatus.Failure;

        float stopRange =
            bb.controller.kind == EnemyController.EnemyKind.Melee ? bb.controller.meleeStopRange :
            bb.controller.kind == EnemyController.EnemyKind.Ranged ? bb.controller.rangedStopRange :
            bb.controller.casterStopRange;

        // If player got away, attack no longer valid -> fail so Chase can run
        if (bb.DistanceToPlayer > stopRange + 0.5f)
            return BTStatus.Failure;

        // face target
        Vector3 to = bb.player.position - bb.transform.position; to.y = 0f;
        if (to.sqrMagnitude > 0.001f)
            bb.transform.rotation = Quaternion.Slerp(bb.transform.rotation, Quaternion.LookRotation(to),
                Time.deltaTime * bb.controller.faceTurnSpeed);

        // caster: try skill first
        if (bb.controller.kind == EnemyController.EnemyKind.Caster && bb.skillCaster != null)
        {
            var pStats = bb.player.GetComponent<Stats>();
            if (pStats != null && bb.skillCaster.TryCastIfReady(pStats)) return BTStatus.Running;
            if (bb.skillCaster.IsBusy) return BTStatus.Running;
        }

        // do basic attack at a reasonable tick rate
        if (Time.time >= _nextAttackCheck)
        {
            _nextAttackCheck = Time.time + 0.1f;

            var tgtStats = bb.player.GetComponent<Stats>();
            if (tgtStats != null) bb.combat.Attack(tgtStats);
        }

        return BTStatus.Running;
    }

    public override void OnExit() { }
}
