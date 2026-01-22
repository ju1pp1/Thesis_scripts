using UnityEngine;

public class BT_Chase : BTNodeWithLifecycle
{
    public BT_Chase(EnemyBlackboard bb) : base(bb) { }

    public override void OnEnter()
    {
        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.EnterMode("Chase", "InRange+LOS");

        AIEventLogger.Action(bb, "BT Enter Chase");
    }

    protected override BTStatus TickRunning()
    {
        if (bb.player == null || bb.agent == null) return BTStatus.Failure;

        // If we should flee, let higher priority branch handle it
        if (bb.HealthPct <= bb.fleeHealthPct) return BTStatus.Failure;

        // Chase invalid -> fail out so fallback branch runs (idle/patrol)
        if (bb.DistanceToPlayer > bb.chaseRadius || !bb.HasLoS)
            return BTStatus.Failure;

        float stopRange =
            bb.controller.kind == EnemyController.EnemyKind.Melee ? bb.controller.meleeStopRange :
            bb.controller.kind == EnemyController.EnemyKind.Ranged ? bb.controller.rangedStopRange :
            bb.controller.casterStopRange;

        // If not yet in preferred range, keep chasing
        if (bb.DistanceToPlayer > stopRange)
        {
            bb.agent.stoppingDistance = stopRange;
            bb.agent.SetDestination(bb.player.position);
            return BTStatus.Running;
        }

        // In range: chase achieved
        if (!bb.agent.pathPending) bb.agent.ResetPath();
        return BTStatus.Success;
    }

    public override void OnExit() { }
}
