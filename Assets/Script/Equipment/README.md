# Unit equipment

Open GearTest.unity in this folder and enter Play Mode. The scene includes the existing inventory UI and initializer, two Unit objects, and authored equipment UI. Use Previous/Next to select a unit, or call EquipmentController.SelectUnit(unit) from other UI. Existing UnitClickable events also select the unit.

- Set ItemDefinition.ActionType to Equip and EquipmentType to Weapon or Armor. ItemType remains the display/sort category. The existing sword/hat SOs are configured accordingly.
- Each Unit exposes Unit.Equipment, which obtains or creates its own UnitEquipment component. Slot 0 is the weapon; slots 1-5 are armor with no body-part restrictions.
- Select Equip in the inventory context menu. The selected instance moves into the first empty compatible equipment slot. Occupied weapon slots and full armor slots reject the request without replacing items.
- Left-click equipment for its description and stat bonuses. Right-click for Unequip. Unequip returns the same ItemInstance to inventory. A full inventory disables the menu button and the transfer method also rejects the operation without changing either container.
- UnitEquipment.TryEquip(item, inventory, out message) and TryUnequip(slot, inventory, out message) provide the transfer API. Changed updates the selected unit's UI.
- Gear slots and selection are independent for every Unit. Equipment is held at runtime; saving/loading and recovery when a Unit is destroyed are not implemented.
- This change handles item ownership transfer and equipment inspection. Stat bonuses are displayed; applying them to UnitStatus and gameplay effects remains a separate integration.

New scripts and this scene live in geartest. Unit.cs exposes the component, ItemDefinition declares the equipment category, and InventoryController passes the existing equip request to EquipmentController.
