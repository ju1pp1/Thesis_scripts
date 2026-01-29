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

    public void EnterMode(string to, string reason = "")
    {
        modeEnters++;
        OnTransition(currentMode, to, reason);
    }
    public void OnTransition(string from, string to, string reason)
    {
        if (string.IsNullOrEmpty(to)) return;

        // Don't count "transitions" to the same mode
        if (!string.IsNullOrEmpty(currentMode) && currentMode == to)
            return;

        transitions++;

        // close previous mode timer
        if (!string.IsNullOrEmpty(currentMode))
        {
            float dt = Time.time - modeEnterTime;
            if (!timeInMode.ContainsKey(currentMode)) timeInMode[currentMode] = 0f;
            timeInMode[currentMode] += dt;
        }

        currentMode = to;
        modeEnterTime = Time.time;

        // milestones
        if ((to == "Chase" || to == "Attack") && firstDetectedTime < 0f)
            firstDetectedTime = Time.time;

        if (to == "Attack" && firstAttackTime < 0f)
            firstAttackTime = Time.time;
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

    // Show a summary in console when object is destroyed
    void OnDestroy()
    {
        // close timer
        if (!string.IsNullOrEmpty(currentMode))
        {
            float dt = Time.time - modeEnterTime;
            if (!timeInMode.ContainsKey(currentMode)) timeInMode[currentMode] = 0f;
            timeInMode[currentMode] += dt;
        }

        Debug.Log($"[AI-METRICS] {name} transitions={transitions} attacksRequested={attacksRequested} skillsCast={skillsCast} " +
                  $"reactDetectToAttack={ReactionTime_DetectToAttack():0.00}s");
    }
}
