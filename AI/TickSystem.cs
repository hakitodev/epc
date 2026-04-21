using UnityEngine;

public class TickSystem : MonoBehaviour
{
    public ulong Tick;
    public ulong ticks;
    public double timer;
    public int tickRate;
    public static double tickInterval;
    const int MAX_TICKS_PER_FRAME = 4;

    public static TickSystem TS;
    private void Awake()
    {
        if (TS is not null && TS != this) return;
        TS = this;
        SetTickRate(tickRate);
    }
    private void OnValidate()
    {
        SetTickRate(tickRate);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        int loops = 0;

        while (timer >= tickInterval && loops < MAX_TICKS_PER_FRAME)
        {
            timer -= tickInterval;
            Tick++;
            loops++;
            OnTick();
        }
    }
    private void OnTick()
    {
        TickManager.RunTick(Tick);
    }
    public void SetTickRate(int newRate)
    {
        tickRate = Mathf.Max(1, newRate);
        RecalculateInterval();
    }

    private void RecalculateInterval()
    {
        tickInterval = 1.0 / tickRate;
    }
}