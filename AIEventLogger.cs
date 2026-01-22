using UnityEngine;

public static class AIEventLogger
{
    public static string SystemTag = "BT";
    /*
    public static void Log(EnemyBlackboard bb, string evt)
    {
        //Debug.Log($"[AI] {bb.name} -> {evt} | hp%={bb.HealthPct:0.00} dist={bb.DistanceToPlayer:0.0}");


    } */

    public static void Transition(EnemyBlackboard bb, string from, string to, string reason = "")
    {
        Debug.Log($"[AI][{SystemTag}] t={Time.time:0.00} npc={bb.name} from={from} to={to} " + $"hp={bb.HealthPct:0.00} dist={bb.DistanceToPlayer:0.0} los={(bb.HasLoS ? 1 : 0)}");

        // CSV
        AICsvLogger.Row(SystemTag, Time.time, bb, "transition", from, to, "", reason);

        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.OnTransition(from, to, reason);
    }
    public static void Action(EnemyBlackboard bb, string action, string reason = "")
    {
        Debug.Log($"[AI][{SystemTag}] t={Time.time:0.00} npc={bb.name} action={action} " + $"hp={bb.HealthPct:0.00} dist={bb.DistanceToPlayer:0.0} los={(bb.HasLoS ? 1 : 0)}");

        // CSV
        AICsvLogger.Row(SystemTag, Time.time, bb, "action", "", "", action, reason);

        var m = bb.GetComponent<AIMetrics>();
        if (m != null) m.OnAction(action, reason);
    }
}
