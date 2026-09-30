using UnityEngine;

[CreateAssetMenu(menuName = "Game/Effect/Damage")]
public class DamageEffectDefinition : EffectDefinition
{    
    public override EffectTag Tags => EffectTag.Attack;        
}