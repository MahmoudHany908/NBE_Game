public struct ChangeStateCommand : IGameEvent
{
    public string TargetSceneName;
    public IGameState TargetState;
}