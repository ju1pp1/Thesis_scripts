using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;

public static class AICsvLogger
{
    private static string _path;
    private static bool _initialized;
    private static readonly StringBuilder _buffer = new StringBuilder(64 * 1024);
    private static float _lastFlushTime;
    private const float FlushIntervalSeconds = 2f;

    private static readonly string _sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

    public static void InitIfNeeded()
    {
        if (_initialized) return;

        var dir = Application.persistentDataPath;
        _path = Path.Combine(dir, $"ai_log_{_sessionId}.csv");

        _buffer.AppendLine(
            "{session};{system};{npc};{npcId};{eventType};{action};{time};{hp};{dist};{inLookRadius};{chaseDuration}"
            );

        _initialized = true;

        // Ensure flush when exiting playmode / build
        Application.quitting += FlushToDisk;
    }

    public static void Row(
        string systemTag,
        float time,
        EnemyBlackboard bb,
        string eventType,
        string fromState,
        string toState,
        string action,
        string reason)
    {
        if (bb == null) return;
        InitIfNeeded();

        var pos = bb.transform.position;

        //int los = bb.HasLoS ? 1 : 0;
        int inLookRadius = bb.InLookRadius ? 1 : 0;
        var m = bb.GetComponent<AIMetrics>();
        float chaseDuration = (m != null) ? m.AvgChaseDuration() : 0f;
        // Keep it CSV-safe: replace commas/newlines in free text
        static string Clean(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace(",", ";").Replace("\n", " ").Replace("\r", " ");
            return s;
        }

        _buffer.Append(_sessionId).Append(';')
            .Append(Clean(systemTag)).Append(';')
            .Append(Clean(bb.name)).Append(';')
            .Append(bb.GetInstanceID()).Append(';')
            .Append(eventType).Append(';')
            .Append(Clean(action)).Append(';')
            //.Append(Clean(reason)).Append(';')
            .Append(time.ToString("0.000")).Append(';')
            .Append(bb.HealthPct.ToString("0.000")).Append(';')
            .Append(bb.DistanceToPlayer.ToString("0.00")).Append(';')
            //.Append(los).Append(";")
            .Append(inLookRadius).Append(';')
            .Append(chaseDuration.ToString("0.000"))
            .AppendLine();

        // Periodic flush
        if (Time.time - _lastFlushTime >= FlushIntervalSeconds)
            FlushToDisk();
    }

    public static void FlushToDisk()
    {
        if (!_initialized) return;
        if (_buffer.Length == 0) return;

        try
        {
            File.AppendAllText(_path, _buffer.ToString());
            _buffer.Clear();
            _lastFlushTime = Time.time;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AICsvLogger] Failed to write CSV: {e.Message}");
        }
    }

    public static string GetPath()
    {
        InitIfNeeded();
        return _path;
    }
}
