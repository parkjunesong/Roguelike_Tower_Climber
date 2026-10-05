# Unit equipment

Open Assets/Scene/explore.unity and form a party. Click a character sidebar to inspect equipment and current stats. Inventory equip requests open a separate character selection dialog.

- Set ItemDefinition.ActionType to Equip and EquipmentType to Weapon or Armor. Armor also requires ArmorPart: Hat, Top, Bottom or Shoes. ItemType remains the display/sort category. The existing hat SO is configured as Hat.
- Each Unit exposes Unit.Equipment. Slot 0 is Weapon; slots 1-4 are Hat, Top, Bottom and Shoes, each holding one item. Sidebar icons appear for equipped armor parts above the HP bar.
- Right-click an inventory item, select Equip, then choose a living party member. No item moves until a target is confirmed. Cancel leaves both containers unchanged. Occupied parts are replaced, with the previous item returned to the incoming item's inventory slot. Replacement works even with a full inventory.
- Left-click equipment for its description and stat bonuses. Right-click for Unequip. Unequip returns the same ItemInstance to inventory. A full inventory disables the menu button and the transfer method also rejects the operation without changing either container.
- UnitEquipment.TryEquip(item, inventory, out message) and TryUnequip(slot, inventory, out message) provide the transfer API. Changed updates the selected unit's UI.
- Gear slots and selection are independent for every Unit. Equipment is held at runtime; saving/loading and recovery when a Unit is destroyed are not implemented.
- UnitStatus.GetFinalStat adds all equipped item bonuses to the unchanged base stats. Equip, replacement and unequip take effect immediately. Maximum HP is at least 1. Raising maximum HP does not heal; lowering it clamps current HP to the new maximum. Dead units remain at 0 HP. Fixed-value damage effects still use their configured damage value.

Unit.cs exposes the component, ItemDefinition declares the equipment category and armor part, and InventoryController passes the equip request to EquipmentController. EquipTargetOverlay and its buttons are authored in the explore scene.
