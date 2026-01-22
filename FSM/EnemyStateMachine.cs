using UnityEngine;

public interface IEnemyState
{
    string Name { get; }
    void OnEnter();
    void Tick();
    void OnExit();

}

public class EnemyStateMachine : MonoBehaviour
{
    public IEnemyState Current {  get; private set; }

    public void ChangeState(IEnemyState next)
    {
        if (Current == next) return;
        Current?.OnExit();
        Current = next;
        Current?.OnEnter();
    }

    public void Tick() => Current?.Tick();
}
