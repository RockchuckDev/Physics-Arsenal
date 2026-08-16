using System;
using System.Collections.Generic;

/// <summary> A custom event bus that works just like signals in Godot, but is completely decoupled and is not affected by refactoring function names.
/// Custom event types can be defined anywhere, but they must be a struct and are conventionally defined in [[EventTypes.cs]] and end with "Event"
///  </summary>
public static class EventBusDeluxe
{
    private static readonly Dictionary<Type, List<Delegate>> subscriberDict = new Dictionary<Type, List<Delegate>>();

    /// <summary> Subscribes an action to the event bus for a given event type</summary>
    public static void Subscribe<TEvent>(Action<TEvent> action)
    {
        Type eventType = typeof(TEvent);
        if (!subscriberDict.ContainsKey(eventType))
        {
            subscriberDict[eventType] = new List<Delegate>();
        }
        subscriberDict[eventType].Add(action);
    }
    /// <summary> Unsubscribes an action from the event bus for a given event type </summary>
    public static void Unsubscribe<TEvent>(Action<TEvent> action)
    {
        Type eventType = typeof(TEvent);
        if (subscriberDict.ContainsKey(eventType))
        {
            subscriberDict[eventType].Remove(action);
        }
    }

    /// <summary> Fires an event of a given type, invoking all subscribed actions with the provided event data </summary>
    public static void Fire<TEvent>(TEvent eventData)
    {
        Type eventType = typeof(TEvent);
        if (subscriberDict.ContainsKey(eventType))
        {
            foreach (var action in subscriberDict[eventType])
            {
                (action as Action<TEvent>)?.Invoke(eventData);
            }
        }
    }
}
