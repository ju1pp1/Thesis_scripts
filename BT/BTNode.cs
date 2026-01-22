using UnityEngine;

public abstract class BTNode
{
    protected readonly EnemyBlackboard bb;
    protected BTNode(EnemyBlackboard bb)
    {
        this.bb = bb;
    }
    protected abstract BTStatus OnTick();
    public BTStatus Tick()
    {
        return OnTick();
    }
}
