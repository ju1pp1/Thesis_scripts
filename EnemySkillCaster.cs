using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Stats))]
public class EnemySkillCaster : MonoBehaviour
{
    [Header("Skill")]
    public SkillData skill;                       // e.g. your Fireball SkillData
    [Tooltip("Optional: where the projectile spawns from")]
    public Transform castPoint;                   // if null, falls back to a sensible default

    [Header("Usage Conditions")]
    public float minRange = 3f;
    public float maxRange;                  // typically match/relate to skill.castingRange
    public bool requireLineOfSight = true;
    public LayerMask losBlockers = ~0;

    [Header("Behavior")]
    public bool stopToCast = true;
    public float faceTurnSpeed = 8f;
    [Header("After-cast behavior")]
    [Tooltip("Time after a cast during which normal attacks are disabled.")]
    public float postCastLockout = 0.6f;
    public bool IsCasting { get; private set; }
    public bool IsBusy => IsCasting || Time.time < _actionLockUntil;
    
    float _actionLockUntil;
    Stats _stats;
    NavMeshAgent _agent;
    Animator _anim;
    float _nextReadyTime;
    Transform _player;

    void Awake()
    {
        _stats = GetComponent<Stats>();
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();
    }
    void Start()
    {
        maxRange = skill.castingRange;
        // Cache the player once (you already do this elsewhere, this is just local convenience)
        var pm = PlayerManager.instance;
        _player = pm != null ? pm.player?.transform : null;

        // If castPoint not assigned, try to find a child named CastPoint or ShootPoint
        if (!castPoint)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Equals("CastPoint") || t.name.Equals("ShootPoint"))
                { castPoint = t; break; }
            }
        }
    }

    public bool TryCastIfReady(Stats target)
    {
        if (skill == null || target == null) return false;

        // If we’re already casting, tell caller to skip normal attacks this frame.
        if (IsCasting) { FaceTarget(target.transform); return true; }

        // Cooldown
        if (Time.time < _nextReadyTime) return false;

        // Range gate
        float dist = Vector3.Distance(transform.position, target.transform.position);
        if (dist < minRange || dist > maxRange) return false;

        // LoS gate
        if (requireLineOfSight && !HasLineOfSight(target.transform, dist + 0.5f)) return false;

        // Mana gate
        if (_stats.currentMana < skill.manaCost) return false;

        // Start casting
        StartCoroutine(CastRoutine(target));
        return true;
    }

    bool HasLineOfSight(Transform t, float dist)
    {
        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Vector3 dest = t.position + Vector3.up * 1.2f;
        if (Physics.Raycast(origin, (dest - origin).normalized, out var hit, dist, losBlockers, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform != t) return false;
        }
        return true;
    }

    IEnumerator CastRoutine(Stats target)
    {
        IsCasting = true;

        // Stop & face
        if (stopToCast && _agent) { _agent.ResetPath(); }
        FaceTarget(target.transform);

        // Optional animation: reuse your existing triggers
        if (_anim) _anim.SetTrigger("startCasting");

        float castTime = Mathf.Max(0f, skill.castingTime);
        float elapsed = 0f;
        while (elapsed < castTime)
        {
            elapsed += Time.deltaTime;
            FaceTarget(target.transform);
            yield return null;
        }

        if (_anim) _anim.SetTrigger("finishCasting");
        /*
        // Spend mana
        _stats.ModifyMana(-skill.manaCost);

        // Fire projectile / apply skill
        FireSkillProjectileAt(target);

        // Set cooldown
        _nextReadyTime = Time.time + Mathf.Max(0f, skill.cooldown);

        // Post-cast lockout so we dont instantly basic-attack
        _actionLockUntil = Time.time + postCastLockout;
        
        IsCasting = false; */
    }
    public void Anim_UseSpellSkillEvent()
    {
        Debug.Log($"Anim_UseSpellSkillEvent on {name} ({GetInstanceID()})");
        if (!IsCasting || skill == null) return;

        if (_stats.currentMana < skill.manaCost) { EndCast(); return; }
        _stats.ModifyMana(-skill.manaCost);

        // Use current player as target, or cache the target when you began casting
        var pm = PlayerManager.instance;
        var playerStats = pm != null ? pm.player?.GetComponent<Stats>() : null;
        if (playerStats != null) FireSkillProjectileAt(playerStats);

        _nextReadyTime = Time.time + Mathf.Max(0f, skill.cooldown);
        _actionLockUntil = Time.time + postCastLockout;

        EndCast();
    }
    void EndCast()
    {
        IsCasting = false;
    }

    void FaceTarget(Transform t)
    {
        if (t == null) return;
        Vector3 to = (t.position - transform.position);
        to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;
        Quaternion look = Quaternion.LookRotation(to);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * faceTurnSpeed);
    }

    void FireSkillProjectileAt(Stats target)
    {
        if (skill.projectilePrefab == null)
        {
            Debug.LogWarning($"{name}: Skill {skill.skillName} has no projectilePrefab.");
            return;
        }

        Vector3 spawnPos = castPoint ? castPoint.position : (transform.position + Vector3.up * 1.3f);
        Vector3 aimAt = target.transform.position; //+ Vector3.up * 1.0f;

        var go = Instantiate(skill.projectilePrefab, spawnPos, Quaternion.LookRotation(aimAt - spawnPos));
        var proj = go.GetComponent<SpellProjectile>();
        if (proj != null)
        {
            // Your projectile already knows how to apply damage/effects from SkillData
            proj.SetTarget(aimAt, _stats, skill);
        }

        // Optional: self/hand VFX for enemies (if you want)
        // if (skill.selfCastEffectPrefab) Instantiate(skill.selfCastEffectPrefab, spawnPos, Quaternion.identity, transform);
    }
}
