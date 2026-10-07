# Unit equipment

Open Assets/Scene/explore.unity and form a party. Click a character sidebar to inspect equipment and current stats.

- Drag an inventory equipment item onto a living character's Party HUD card to equip it immediately. An occupied part is replaced, and the old item returns to the incoming item's inventory slot. Replacement works even when inventory is full.
- Left-click inventory or equipment slots for descriptions and stat bonuses. Exploration does not open right-click menus or a character selection dialog.
- Drag an equipped item outside the character information window to return the same ItemInstance to inventory. Dropping inside the window cancels. Full inventory rejects unequip and keeps the original equipment.
- Dropping inventory equipment elsewhere or on an empty/dead party member leaves it unchanged. Escape, closing its window, and battle entry cancel a drag. The pointer preview never removes an item by itself.
- Set ItemDefinition.ActionType to Equip and EquipmentType to Weapon or Armor. Armor also requires ArmorPart: Hat, Top, Bottom or Shoes. ItemType remains the display/sort category.
- Each Unit exposes Unit.Equipment. Slot 0 is Weapon; slots 1-4 are Hat, Top, Bottom and Shoes. Sidebar icons appear for equipped armor above the HP bar.
- UnitEquipment.TryEquip(item, inventory, out message) and TryUnequip(slot, inventory, out message) remain the transfer API. Changed refreshes equipment and stat displays.
- UnitStatus.GetFinalStat adds all equipped item bonuses to base stats. Equip, replacement and unequip take effect immediately. Raising maximum HP does not heal; lowering it clamps current HP. Dead units remain at 0 HP.
- Gear is held at runtime. Saving/loading is not implemented.

ExplorationItemDragAndDrop is attached to Exploration in explore.unity and shares inventory, equipment, and character window references. ExplorationPartyHUD registers a drop target on each member card. Other scenes without this component retain the existing controller click actions.

## Weapon classes

- Sword → Knight (검 → 나이트), Bow → Archer (활 → 아처), Dagger → Assassin (단검 → 어쌔신), Staff → Caster (스태프 → 캐스터), Orb → Shaman (오브 → 샤먼).
- ItemDefinition.WeaponType determines WeaponClass. Class is available from Unit.Equipment.Class. No extra class stat or skill bonuses are applied.
- Main's PartyFormationController.startingWeapons supplies the five choices. Each selected party slot has a separate weapon button. Confirm creates a distinct ItemInstance and equips it before exploration.
- ExplorationEntry.Confirm requires every selected member to have a valid weapon, then locks that member's class. UnitEquipment.TryEquip rejects weapons of a different class before either container is changed.
- Unequipping a weapon leaves the exploration class locked. Armor does not affect class. Encounter battles preserve the same lock; leaving exploration releases it.
- Weapon assets are testitem1.asset (existing sword), Bow.asset, Dagger.asset, Staff.asset and Orb.asset under Assets/Script/Item. All currently provide +3 AT for testing. Exploration's initializer includes all five weapons and the existing hat.
