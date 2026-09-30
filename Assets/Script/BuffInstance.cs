using System;

[Serializable]
public class BuffInstance
{
    public int RemainingTurns;
    public int Magnitude;

    public BuffInstance(int remainingTurns, int magnitude)
    {
        RemainingTurns = remainingTurns;
        Magnitude = magnitude;
    }
}