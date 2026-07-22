namespace VaultSurvivors.Core.Time
{
    public interface ITimeManager
    {
        long GetCurrentUnixTimestamp();
        float CalculateElapsedSeconds(long startTimestamp, long endTimestamp);
        bool IsTimerComplete(long endTimeTimestamp);
    }
}
