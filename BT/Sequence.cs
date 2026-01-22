using UnityEngine;

public class Sequence : BTNode
{
    private readonly BTNode[] _children;
    public Sequence(EnemyBlackboard bb, params  BTNode[] children) : base(bb)
    {
        _children = children;
    }

    protected override BTStatus OnTick()
    {
        foreach(var c in _children)
        {
            var s = c.Tick();
            if (s != BTStatus.Success) return s;
        }
        return BTStatus.Success;
    }
}
