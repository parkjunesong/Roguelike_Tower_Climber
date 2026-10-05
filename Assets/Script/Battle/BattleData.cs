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
    [Tooltip("Battle 씬에서 새로 생성할 플레이어입니다. 빈 슬롯은 건너뛰며 최소 한 명이 필요합니다. 탐사 전투에서는 전역 파티를 사용합니다.")]
    public List<UnitData> Players = new List<UnitData>();
    public List<WaveData> Waves = new List<WaveData>();

    public int WaveCount => Waves.Count;
}