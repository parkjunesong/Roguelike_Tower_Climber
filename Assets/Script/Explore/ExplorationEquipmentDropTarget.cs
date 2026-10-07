using UnityEngine;
using UnityEngine.EventSystems;

namespace CaveParallaxDemo
{
    public class ExplorationEquipmentDropTarget : MonoBehaviour, IDropHandler
    {
        private ExplorationPartyHUD hud;
        private int slot;

        public void Initialize(ExplorationPartyHUD owner, int index)
        {
            hud = owner;
            slot = index;
        }

        public void OnDrop(PointerEventData data) => hud.DropEquipment(slot, data);
    }
}
