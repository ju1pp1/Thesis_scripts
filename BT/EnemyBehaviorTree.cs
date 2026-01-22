using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyBlackboard))]
public class EnemyBehaviorTree : MonoBehaviour
{
    private EnemyBlackboard bb;
    private BTNode root;

    [Header("Sensing")]
    public bool requireLineOfSight = true;
    public LayerMask losBlockers = ~0;

    void Awake()
    {
        bb = GetComponent<EnemyBlackboard>();
    }

    void Start()
    {
        // Make sure Blackboard has been wired by EnemyController (player, agent, stats, etc.)
        // Build the BT once
        BuildTree();
    }

    void Update()
    {
        if (bb == null || bb.player == null || bb.selfStats == null) return;

        // refresh context each tick
        bb.RefreshSenses(requireLineOfSight, losBlockers);

        // tick the tree
        root?.Tick();
    }

    void BuildTree()
    {
        root = new Selector(bb,

            // 1) Flee
            new Sequence(bb,
                new Condition(bb, () => bb.HealthPct <= bb.fleeHealthPct),
                new BT_Flee(bb)
            ),

            // 2) Combat (only if player detected!)
            new Sequence(bb,
                new Condition(bb, () => bb.DistanceToPlayer <= bb.controller.lookRadius && bb.HasLoS),
                new Selector(bb,

                    // Attack only if in attack range
                    new Sequence(bb,
                        new Condition(bb, () =>
                        {
                            float stopRange =
                                bb.controller.kind == EnemyController.EnemyKind.Melee ? bb.controller.meleeStopRange :
                                bb.controller.kind == EnemyController.EnemyKind.Ranged ? bb.controller.rangedStopRange :
                                bb.controller.casterStopRange;

                            return bb.DistanceToPlayer <= stopRange + 0.5f;
                        }),
                        new BT_Attack(bb)
                    ),

                    // Otherwise chase
                    new BT_Chase(bb)
                )
            ),

            // 3) Fallback patrol/idle
            new Selector(bb,
                new Sequence(bb,
                    new Condition(bb, () => bb.patrolRadius > 0f && bb.agent != null),
                    new BT_PatrolRandom(bb)
                ),
                new BT_Idle(bb)
            )
        );
    }
}
