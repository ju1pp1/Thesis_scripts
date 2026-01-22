using UnityEngine;

public class BT_Idle : BTNodeWithLifecycle
{
    public BT_Idle(EnemyBlackboard bb) : base(bb) { }
    public override void OnEnter()
    {
        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.EnterMode("Idle", "InRange+LOS");

        if (bb.agent != null) bb.agent.ResetPath();
        AIEventLogger.Action(bb, "BT Enter Idle");
    }

    protected override BTStatus TickRunning()
    {
        if (bb.DistanceToPlayer <= bb.controller.lookRadius && bb.HasLoS)
            return BTStatus.Failure;

        if(bb.patrolRadius > 0f && bb.agent != null)
            return BTStatus.Failure;

        return BTStatus.Running;
    }
    public override void OnExit()
    {
        
    }
}
