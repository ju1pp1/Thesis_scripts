using UnityEngine;
using UnityEngine.AI;

public class BT_Flee : BTNodeWithLifecycle
{
    private float _nextRepathTime;

    public BT_Flee(EnemyBlackboard bb) : base(bb) { }

    public override void OnEnter()
    {
        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.EnterMode("Flee", "InRange+LOS");

        AIEventLogger.Action(bb, "BT Enter Flee");
        _nextRepathTime = 0f;
    }

    protected override BTStatus TickRunning()
    {
        if (bb.agent == null) return BTStatus.Failure;

        // stop fleeing condition: reengage
        if (bb.HealthPct >= bb.reengageHealthPct)
            return BTStatus.Success; // flee finished

        if (bb.player == null)
        {
            bb.agent.ResetPath();
            return BTStatus.Running;
        }

        // Don’t spam SetDestination every frame
        if (Time.time < _nextRepathTime)
            return BTStatus.Running;

        _nextRepathTime = Time.time + 0.25f;

        Vector3 away = (bb.transform.position - bb.player.position).normalized;
        Vector3 dest = bb.transform.position + away * 8f;

        if (NavMesh.SamplePosition(dest, out var hit, 4f, NavMesh.AllAreas))
            bb.agent.SetDestination(hit.position);

        return BTStatus.Running;
    }

    public override void OnExit() { }
}
