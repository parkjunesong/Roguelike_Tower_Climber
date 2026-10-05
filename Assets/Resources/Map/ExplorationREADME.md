# Scenario exploration

- Main's scenario 3 entry loads Assets/Scene/explore.unity through ScenarioFlow.
- Assets/Scenario/#3/#3.asset contains an Explore step. Its Map Prefab references Prefabs/CaveExplorationMap.prefab. Assign another prefab with ExplorationMap to choose another map per scenario step.
- ExplorationMap stores width, height, viewport width, player spawn and edge padding. Child ParallaxLayer components are bound to the exploration camera after loading.
- The scene owns the player, camera and inventory/equipment UI; the map prefab owns the scenery. The scene preview map supports direct scene play.
- A/D or Left/Right Arrow moves. I toggles inventory plus equipment. Escape closes the UI. Opening the UI suspends movement and preserves inventory/gear state.
- Finish Exploration completes the current step; the final step returns to Main.
- The initial party and items currently reuse GearTest and InventoryTestInitializer samples. Inventory and gear are retained while opening/closing UI, but are not saved across scene exits.
