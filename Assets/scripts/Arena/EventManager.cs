using System;
using System.Collections.Generic;

public static class EventManager
{
    private static Dictionary<string, Action<object>> eventTable = new Dictionary<string, Action<object>>();

    public static void Subscribe(string eventName, Action<object> listener)
    {
        if (!eventTable.ContainsKey(eventName))
            eventTable[eventName] = delegate { };

        eventTable[eventName] += listener;
    }

    public static void Unsubscribe(string eventName, Action<object> listener)
    {
        if (eventTable.ContainsKey(eventName))
            eventTable[eventName] -= listener;
    }

    public static void Trigger(string eventName, object param = null)
    {
        if (eventTable.ContainsKey(eventName))
            eventTable[eventName].Invoke(param);
    }

    // Optional for debugging
    public static void ClearAllListeners()
    {
        eventTable.Clear();
    }
}
