using UnityEngine;

public class BattleSceneInitializer : MonoBehaviour
{
    public BattleData battleData;

    void Start()
    {
        if (battleData == null)
            return;

        var battleManager = GetComponent<BattleManager>();
        battleManager.Init(battleData);
        battleManager.BattleStart();       
    }  
}
