# Inventory

Open Assets/Scene/invenTest.unity. The Inventory object owns InventoryController. Canvas, grid slots, scrollable item details, context menu and sort button are authored in the scene and editable in Unity. InventorySlot.prefab supplies extra slots when Columns/Rows are increased before Play Mode.

- The inventory starts empty. External code calls controller.AddItem(itemDefinition) or controller.AddItem(new ItemInstance(itemDefinition)); the bool result indicates success. Each call with an SO creates a distinct instance. Null, duplicate instances and full capacity are rejected.
- Left-click a slot to display the item's SO description and stat bonuses. Right-click to show its action and discard options. Click outside the menu or press Escape to close it.
- ItemDefinition.ActionType determines which request is sent: None disables the action, Equip emits ItemEquipRequested, Use emits ItemUseRequested. UseSelectedItem is only a connection point for future gameplay; it does not equip, consume, or apply stats.
- DiscardSelectedItem removes the selected item and emits ItemDiscarded.
- SortByType groups items by ItemType, then DisplayName, with empty slots last. Selected item identity is preserved.
- ItemInstance contains a shared ItemDefinition reference. It has no click counter or duplicated definition data.
- InventoryGrid is the UI-independent N x M model. InventorySlotUI forwards left/right pointer clicks and renders existing scene/prefab controls. InventoryController coordinates inventory actions and scene UI.

There are no automatic sample items, test buttons, runtime BuildUI, legacy InventoryItem, InventoryTestController, InventoryUI or TestInventoryItem. The existing testitem1/testitem2 SOs are retained for external AddItem calls. Inventory is not persisted between sessions.

In explore.unity, ExplorationItemDragAndDrop replaces right-click menus with equipment dragging: drop onto a Party HUD card to equip; drag equipped gear outside the character information window to unequip. Left-click details and sorting remain available. Drops outside a valid inventory equipment target cancel without discarding items.
