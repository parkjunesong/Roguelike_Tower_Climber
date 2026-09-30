using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveData
{
    public List<UnitData> UnitData = new List<UnitData>();

    public bool IsBossWave;
}

[CreateAssetMenu(menuName = "Game/Data/Battle")]
public class BattleData : ScriptableObject
{
    public UnitData Player;
    public List<WaveData> Waves = new List<WaveData>();

    public int WaveCount => Waves.Count;
}