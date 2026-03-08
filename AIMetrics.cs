using UnityEngine;
using System.Collections.Generic;

public class AIMetrics : MonoBehaviour
{
    [Header("Runtime summary")]
    public string currentMode;
    public float modeEnterTime;

    // counters
    public int transitions;
    public int attacksRequested;
    public int skillsCast;

    // time per mode/state
    private readonly Dictionary<string, float> timeInMode = new();

    // reaction milestones
    private float firstDetectedTime = -1f;
    private float firstAttackTime = -1f;

    public int modeEnters;
    private float chaseStartTime = -1f;
    private float totalChaseTime = 0f;
    private int chaseCount = 0;

    public void EnterMode(string to, string reason = "")
    {
        modeEnters++;                 // how often BT leaf entered
        OnTransition(currentMode, to, reason);
    }
    public void OnTransition(string from, string to, string reason)
    {
        transitions++;

        if (to == "Chase")
        {
            chaseStartTime = Time.time;
        }

        if (from == "Chase")
        {
            if (chaseStartTime >= 0f)
            {
                totalChaseTime += Time.time - chaseStartTime;
                chaseCount++;
                chaseStartTime = -1f;
            }
        }
    }
    public float AvgChaseDuration()
    {
        if (chaseCount == 0) return 0f;
        return totalChaseTime / chaseCount;
    }

    public void OnAction(string action, string reason)
    {
        // keep this simple: count key actions
        if (action.Contains("Attack")) attacksRequested++;
        if (action.Contains("Cast") || action.Contains("Skill")) skillsCast++;

        // if you treat �Enter Attack� as first attack start:
        if (action.Contains("Enter Attack") && firstAttackTime < 0f)
            firstAttackTime = Time.time;
    }

    public float ReactionTime_DetectToAttack()
    {
        if (firstDetectedTime < 0f || firstAttackTime < 0f) return -1f;
        return firstAttackTime - firstDetectedTime;
    }

    // Optional: show a summary in console when object is destroyed
    void OnDestroy()
    {
        var bb = GetComponent<EnemyBlackboard>();
        // close timer
        if (!string.IsNullOrEmpty(currentMode))
        {
            float dt = Time.time - modeEnterTime;
            if (!timeInMode.ContainsKey(currentMode)) timeInMode[currentMode] = 0f;
            timeInMode[currentMode] += dt;
        }

        Debug.Log($"[AI-METRICS] {name} transitions={transitions} attacksRequested={attacksRequested} skillsCast={skillsCast} " +
          $"avgChaseDuration={AvgChaseDuration():0.00}s " +
          $"reactDetectToAttack={ReactionTime_DetectToAttack():0.00}s");

        AICsvLogger.Row(
    AIEventLogger.SystemTag,
    Time.time,
    bb,
    "metric",
    "",
    "",
    "avgChaseDuration",
    AvgChaseDuration().ToString()
);
    }
}
