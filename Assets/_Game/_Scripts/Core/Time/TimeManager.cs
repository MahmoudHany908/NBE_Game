using System;
using VContainer;

namespace VaultSurvivors.Core.Time
{
    public class TimeManager : ITimeManager, System.IDisposable
    {
        private readonly IEventSubscriber _eventSubscriber;
        private readonly IEventPublisher _eventPublisher;

        private bool _hasBooted = false;

        [Inject]
        public TimeManager(IEventSubscriber eventSubscriber, IEventPublisher eventPublisher)
        {
            _eventSubscriber = eventSubscriber;
            _eventPublisher = eventPublisher;

            _eventSubscriber.Subscribe<SaveDataLoadedEvent>(OnSaveDataLoaded);
        }

        public void Dispose()
        {
            _eventSubscriber?.Unsubscribe<SaveDataLoadedEvent>(OnSaveDataLoaded);
        }

        private void OnSaveDataLoaded(SaveDataLoadedEvent evt)
        {
            if (_hasBooted) return;
            _hasBooted = true;

            if (evt.Data != null && evt.Data.lastSeenTimestamp > 0)
            {
                long currentTimestamp = GetCurrentUnixTimestamp();
                float secondsElapsed = CalculateElapsedSeconds(evt.Data.lastSeenTimestamp, currentTimestamp);

                if (secondsElapsed > 0)
                {
                    _eventPublisher.Publish(new AppReturnedOnlineEvent
                    {
                        SecondsElapsedOffline = secondsElapsed
                    });
                }
            }
        }

        public long GetCurrentUnixTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public float CalculateElapsedSeconds(long startTimestamp, long endTimestamp)
        {
            return Math.Max(0, (endTimestamp - startTimestamp));
        }

        public bool IsTimerComplete(long endTimeTimestamp)
        {
            return GetCurrentUnixTimestamp() >= endTimeTimestamp;
        }
    }
}