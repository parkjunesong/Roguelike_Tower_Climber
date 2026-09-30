using UnityEngine;
using System;

[Flags]
public enum EffectTag
{
    None = 0,

    Attack = 1 << 0,
    Recovery = 1 << 1,
    Defense = 1 << 2,
    Resource = 1 << 3,
    Draw = 1 << 4,
    Control = 1 << 5,
    Utility = 1 << 6,

    All =
        Attack |
        Recovery |
        Defense |
        Resource |
        Draw |
        Control |
        Utility
}  

public abstract class EffectDefinition : ScriptableObject
{
    public abstract EffectTag Tags { get; }
}