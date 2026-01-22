using UnityEngine;

public class Selector : BTNode
{
    private readonly BTNode[] _children;
    public Selector(EnemyBlackboard bb, params BTNode[] children) : base(bb)
    {
        _children = children;
    }

    protected override BTStatus OnTick()
    {
        foreach (var c in _children)
        {
            var s = c.Tick();
            if (s != BTStatus.Failure) return s;
        }
        return BTStatus.Failure;
    }
}
