public interface IGameStateManager
{
    void ChangeState(IGameState newState);
    void LoadSceneAndChangeState(string sceneName, IGameState newState);
}