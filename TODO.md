# RPG Dungeon — Competition Polish TODO
Last updated: 2026-04-06

Tick off with [x] as we go.

---

## BUGS

- [x] **NG+ slime/skeleton/archer/shroom/clone damage scaling** — All enemies now use additive NG+ damage bonus: NG+1=1dmg, NG++=2dmg, NG+++=3dmg capped. DamageDealer children now scaled by NGPlusEnemyScaler. ToxicGasCloud and ArrowProjectile self-scale on Start(). Clone double-scaling fixed.

- [x] **Heart Container buys heal instead of adding max HP** — AddMaxHP no longer modifies currentHp.

- [x] **Inventory disappears on scene transition** — InventoryManager now DontDestroyOnLoad.

- [x] **NG+ skips main pig dialogue / zone flow broken** — Zone1 requires dialogue in NG+1, free in NG++. Zone3 skippable in NG+. Zone7→cave free in NG+. Zone1MainDone resets on SetGameCleared and death. PigShopkeeper flags reset on SetGameCleared and death.

- [x] **Dialogue marked as complete after death/respawn** — PigShopkeeper static flags now reset on death.

- [ ] **Cave tile palette collider covering player** — A collider from the cave tile palette is rendering/sorting on top of the player, hiding the sprite. Likely a sorting layer/order issue on the tilemap.

---

## FEATURES & POLISH

- [ ] **Change iron item → diamond** — The item that grants the damage buff should be renamed/replaced: iron → diamond. Update ItemData asset name, icon, and any references.

- [x] **Heart Container "Sold Out" when cap reached** — When the player hits the HC limit, the store slot should show a "Sold Out" label/indicator instead of remaining purchasable.

- [ ] **Extend max hearts cap to 15 or 20** — Increase the maximum allowed hearts from current cap to 15 or 20. Update Health + StoreManager logic.

- [x] **Remove Space as dialogue skip — E only** — Done. Prompt text updated too.

- [x] **Dying in cave restarts in cave** — RestartGame now always loads "Main Scene".

- [x] **Hearts/gold UI broken on restart** — Consequence of above, fixed.

- [x] **ESC inventory not working in cave** — PauseMenu no longer intercepts ESC when HUD is active.

- [x] **Healing on scene transition** — NGPlusManager now snapshots and restores currentHp exactly. No heal on transition.

- [x] **Sword showing in inventory after death** — ClearInventory now also clears equippedSword.

- [ ] **NG+ content: more dialogues per tier** — Write and create DialogueData assets for NG+, NG++, NG+++ NPC variants (PigShopkeeper fields already exist).

- [ ] **NG+ content: more enemies per tier** — Scale enemy count upward with each NG+ cycle, not just stats.

- [ ] **NG+ content: move mushroom positions** — Reposition bombshrooms in NG+ to keep the layout feeling fresh.

- [ ] **Post processing pass** — Evaluate and add post processing (bloom, vignette, color grading) to improve visual feel.

- [ ] **Balance pass: player stats vs enemy stats** — Full review of damage numbers, HP values, XP thresholds, crafting buffs across all zones and NG+ tiers. Try to transform all initial stats to a 0 baseline.

- [ ] **Campfire sound** — Add ambient/SFX sound for the campfire in the cave scene.

- [ ] **"Again, below you" clone line in NG++ onwards** — Clone should say this line (or variant) during the boss fight in NG++ and beyond.

- [ ] **Dialogue box continue prompt position** — The [E] continue/close prompt is in the wrong position on the dialogue box. Needs repositioning.

- [ ] **PigShopkeeper position** — Move the PigShopkeeper GameObject a bit to the left in the editor.

- [ ] **Clone death dialogue** — When the clone dies, it should say a line before the ending sequence. CloneAI hooks into Health.OnDeath, calls DialogueManager.StartDialogue(), delay ending sequence until dialogue finishes.

---

## CONTENT (from afterfinish.txt)

- [ ] **Boss death dialogue** — When player dies during clone fight, clone says a line. CloneAI hooks into Health.OnDeath, calls DialogueManager.StartDialogue(), delay game over screen. Pick name (pig / pig? / humberto), create DialogueData asset.

- [ ] **UI design pass** — Replace all default Unity UI elements with custom art: health bar, XP bar, inventory panel, store, dialogue box, game over screen, victory screen.

- [ ] **NG+ dialogues: write content** — All NPC dialogue variants for NG+/NG++/NG+++ tiers need actual written content and DialogueData assets created.
