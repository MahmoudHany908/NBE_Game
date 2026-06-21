public interface IGameState
{
    void EnterState(GameStateManager manager);
    void UpdateState(GameStateManager manager);
    void ExitState(GameStateManager manager);
}