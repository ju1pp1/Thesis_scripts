using System;
using UnityEngine;

public class Condition : BTNode
{
    private readonly Func<bool> _predicate;

    public Condition(EnemyBlackboard bb, Func<bool> predicate) : base(bb)
    {
        _predicate = predicate;
    }

    protected override BTStatus OnTick()
    {
        return _predicate() ? BTStatus.Success : BTStatus.Failure;
    }
}
