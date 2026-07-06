using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private static readonly Dictionary<Type, List<Delegate>> _toAdd = new();
    private static readonly Dictionary<Type, List<Delegate>> _toRemove = new();


    private static readonly object _lock = new();


    private static int _publishDepth = 0;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
    {
        if (handler == null)
        {
            Debug.LogWarning("[EventBus] Tried to subscribe a null handler.");
            return;
        }

        var type = typeof(T);

        lock (_lock)
        {
            var target = _publishDepth > 0 ? _toAdd : _subscribers;

            if (!target.ContainsKey(type))
                target[type] = new List<Delegate>();


            if (target[type].Contains(handler))
            {
                Debug.LogWarning($"[EventBus] Handler already subscribed to {type.Name}. " +
                                 "Duplicate subscription ignored. If using lambdas, cache them to avoid this.");
                return;
            }

            target[type].Add(handler);
        }
    }

    public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
    {
        if (handler == null)
        {
            Debug.LogWarning("[EventBus] Tried to unsubscribe a null handler.");
            return;
        }

        var type = typeof(T);

        lock (_lock)
        {
            if (_publishDepth > 0)
            {
                if (!_toRemove.ContainsKey(type))
                    _toRemove[type] = new List<Delegate>();
                _toRemove[type].Add(handler);
            }
            else
            {
                if (_subscribers.TryGetValue(type, out var list))
                {

                    int removed = list.RemoveAll(d => d.Equals(handler));
                    if (removed == 0)
                        Debug.LogWarning($"[EventBus] Unsubscribe called for {type.Name} but handler was not found. " +
                                         "Check for mismatched Subscribe/Unsubscribe calls.");
                }
                else
                {
                    Debug.LogWarning($"[EventBus] Unsubscribe called for {type.Name} but no subscribers exist for that event.");
                }
            }
        }
    }

    public static void Publish<T>(T eventData) where T : IGameEvent
    {
        var type = typeof(T);


        List<Delegate> snapshot;
        lock (_lock)
        {
            if (!_subscribers.TryGetValue(type, out var handlers) || handlers.Count == 0)
                return;

            snapshot = new List<Delegate>(handlers);
            _publishDepth++;
        }

        try
        {
            int count = snapshot.Count;
            for (int i = 0; i < count; i++)
            {
                if (snapshot[i] is Action<T> action)
                {
                    try
                    {
                        action.Invoke(eventData);
                    }
                    catch (Exception e)
                    {

                        Debug.LogException(e);
                    }
                }
            }
        }
        finally
        {
            lock (_lock)
            {
                _publishDepth--;

                if (_publishDepth == 0)
                    ProcessPending();
            }
        }
    }


    public static void Clear()
    {
        lock (_lock)
        {
            _subscribers.Clear();
            _toAdd.Clear();
            _toRemove.Clear();
            _publishDepth = 0;
        }
    }


    // NOTE: Must be called while holding _lock.
    private static void ProcessPending()
    {
        foreach (var kvp in _toAdd)
        {
            if (!_subscribers.ContainsKey(kvp.Key))
                _subscribers[kvp.Key] = new List<Delegate>();

            foreach (var del in kvp.Value)
            {
                if (_subscribers[kvp.Key].Contains(del))
                {
                    Debug.LogWarning($"[EventBus] Deferred subscribe for {kvp.Key.Name}: duplicate handler ignored.");
                    continue;
                }
                _subscribers[kvp.Key].Add(del);
            }
        }
        _toAdd.Clear();

        foreach (var kvp in _toRemove)
        {
            if (_subscribers.TryGetValue(kvp.Key, out var list))
            {
                foreach (var del in kvp.Value)
                {
                    int removed = list.RemoveAll(d => d.Equals(del));
                    if (removed == 0)
                        Debug.LogWarning($"[EventBus] Deferred unsubscribe for {kvp.Key.Name}: handler not found.");
                }
            }
        }
        _toRemove.Clear();
    }



#if UNITY_EDITOR

    public static Dictionary<string, int> GetSubscriberCounts()
    {
        lock (_lock)
        {
            var counts = new Dictionary<string, int>();
            foreach (var kvp in _subscribers)
                counts[kvp.Key.Name] = kvp.Value.Count;
            return counts;
        }
    }

    public static void LogSubscriberCounts()
    {
        lock (_lock)
        {
            if (_subscribers.Count == 0)
            {
                Debug.Log("[EventBus] No active subscribers.");
                return;
            }

            var sb = new System.Text.StringBuilder("[EventBus] Active subscribers:\n");
            foreach (var kvp in _subscribers)
                sb.AppendLine($"  {kvp.Key.Name}: {kvp.Value.Count} handler(s)");

            Debug.Log(sb.ToString());
        }
    }
#endif
}