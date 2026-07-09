public struct GameStateChangedEvent : IGameEvent
{
    public IGameState NewState;
}