using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyKind { Melee, Ranged, Caster}
    [Header("Archetype")]
    [Header("NPC Base Damage (when not using an item)")]
    [Min(0)] public int npcMinDamage = 2;
    [Min(0)] public int npcMaxDamage = 5;
    public EnemyKind kind = EnemyKind.Melee;
    [Header("Awareness")]
    public float lookRadius = 10f;
    public bool requireLineOfSight = true;
    public LayerMask losBlockers = ~0;

    [Header("Preferred Ranges")]
    public float meleeStopRange = 2.0f;
    public float rangedStopRange = 9.0f;
    public float casterStopRange = 10.0f;
    
    [Header("Movement")]
    public float faceTurnSpeed = 5f;

    Transform target;
    NavMeshAgent agent;
    private CharacterCombat combat;
    private Stats myStats;
    private Transform currentTarget;

    private float nextAttackTime;

    private EnemySkillCaster _skillCaster;

    EnemyBlackboard bb;
    EnemyStateMachine fsm;
    private void Awake()
    {
        myStats = GetComponent<Stats>();
        combat = GetComponent<CharacterCombat>();
        _skillCaster = GetComponent<EnemySkillCaster>();
        Debug.Log("[AI CSV] " + AICsvLogger.GetPath());
    }
    void Start()
    {
        target = PlayerManager.instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
        myStats = GetComponent<Stats>();

        if (npcMaxDamage < npcMinDamage) npcMaxDamage = npcMinDamage;

        if (CompareTag("RangedEnemy"))
        {
            kind = EnemyKind.Ranged;
            combat.ForceWeaponModeForNPC(
        ranged: true, magic: false,
        damageType: DamageType.Physical,
        projectilePrefab: combat.arrowPrefab,
        projectileSpeed: combat.arrowSpeed,
        explosionPrefab: null,
        explosionRadius: 0f,
        minDamage: npcMinDamage,
        maxDamage: npcMaxDamage
    );
        }
        else if(CompareTag("CasterEnemy"))
        {
            kind = EnemyKind.Caster;
            combat.ForceWeaponModeForNPC(
        ranged: false, magic: true,
        damageType: combat.boltDamageType,
        projectilePrefab: combat.boltPrefab,
        projectileSpeed: combat.boltSpeed,
        explosionPrefab: combat.boltExplosionPrefab,
        explosionRadius: combat.boltExplosionRadius,
        minDamage: npcMinDamage,
        maxDamage: npcMaxDamage
    );
        }
        else
        {
            kind = EnemyKind.Melee;
            combat.ForceWeaponModeForNPC(
                ranged: false, magic: false,
                damageType: DamageType.Physical,
                minDamage: npcMinDamage,
                maxDamage: npcMaxDamage
                );
        }
            agent.stoppingDistance = GetPreferredStopRange();

        // FSM wiring
        bb = gameObject.GetComponent<EnemyBlackboard>() ?? gameObject.AddComponent<EnemyBlackboard>();
        fsm = gameObject.GetComponent<EnemyStateMachine>() ?? gameObject.AddComponent<EnemyStateMachine>();

        bb.player = target;
        bb.selfStats = myStats;
        bb.agent = agent;
        bb.combat = combat;
        bb.skillCaster = GetComponent<EnemySkillCaster>();
        bb.controller = this;

        // initial state
        fsm.ChangeState(new IdleState(bb, fsm));
    }

    void Update()
    {

        if (myStats != null && myStats.faction == Stats.Faction.Ally) return;
        if(!target || !myStats) return;
        bb.RefreshSenses(requireLineOfSight, losBlockers);
        fsm.Tick();

    }

    float GetPreferredStopRange()
    {
        switch (kind)
        {
            case EnemyKind.Ranged: return rangedStopRange;
            case EnemyKind.Caster: return casterStopRange;
            default: return meleeStopRange;
        }
    }

    bool HasLineOfSight(Transform t, float dist)
    {
        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Vector3 dest = t.position + Vector3.up * 1.6f;
        if (Physics.Raycast(origin, (dest - origin).normalized, out var hit, dist + 0.25f, losBlockers, QueryTriggerInteraction.Ignore))
        {
            // if we hit something that is not the target, LoS is blocked
            if (hit.transform != t) return false;
        }
        return true;
    }

    void FaceTarget()
    {
        Vector3 to = (target.position - transform.position);
        to.y = 0f;
        if (to.sqrMagnitude < 0.001f) return;
        Quaternion look = Quaternion.LookRotation(to);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * faceTurnSpeed);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = (kind == EnemyKind.Caster) ? Color.cyan :
                       (kind == EnemyKind.Ranged) ? Color.yellow : Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);

        Gizmos.color = Color.white;
        float r = GetPreferredStopRange();
        Gizmos.DrawWireSphere(transform.position, r);
    }
}
