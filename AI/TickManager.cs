using System;
using System.Collections.Generic;
using UnityEngine;

public static class TickManager
{
    private const int SIZE = 1024;
    private static readonly List<ScheduledEvent>[] wheel = new List<ScheduledEvent>[SIZE];
    private static ulong _lastTick = 0;

    static TickManager()
    {
        for (int i = 0; i < SIZE; i++)
        {
            wheel[i] = new List<ScheduledEvent>(4);
        }
    }

    public static void AddAction(ulong tick, Action action)
    {
        int index = (int)(tick & (SIZE - 1));
        wheel[index].Add(new ScheduledEvent { tick = tick, action = action });
    }
    public static void RunTick(ulong currentTick)
    {
        if (currentTick <= _lastTick) return;

        for (ulong t = _lastTick + 1; t <= currentTick; t++)
        {
            int index = (int)(t & (SIZE - 1));
            var list = wheel[index];

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var e = list[i];
                if (e.tick <= t)
                {
                    e.action();

                    int last = list.Count - 1;
                    list[i] = list[last];
                    list.RemoveAt(last);
                }
            }
        }
        _lastTick = currentTick;
    }

    private struct ScheduledEvent
    {
        public ulong tick;
        public Action action;
    }
}