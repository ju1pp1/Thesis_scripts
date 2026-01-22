using UnityEngine;

public abstract class BTNodeWithLifecycle : BTNode
{
    private bool _started;
    protected BTNodeWithLifecycle(EnemyBlackboard bb) : base(bb) { }
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    protected abstract BTStatus TickRunning();
    protected override BTStatus OnTick()
    {
        if (!_started)
        {
            _started = true;
            OnEnter();
        }

        var status = TickRunning();

        if (status != BTStatus.Running)
        {
            OnExit();
            _started = false;
        }

        return status;
    }
}
