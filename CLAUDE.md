# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 2D top-down RPG dungeon game built with Unity (2D/URP). The solution files are `RPG-Dungeon.sln` and `Dungeon Descent.sln`. All game scripts live in `Assets/Scripts/`.

## Build & Run

This is a Unity project — there are no CLI build commands for normal development. Open the project in the Unity Editor and use Play mode to test. The Unity version is tracked in `ProjectSettings/ProjectVersion.txt`.

Tests run via Unity's Test Runner (Window > General > Test Runner inside the editor). There are no custom CLI test commands.

## Architecture

### Singleton Managers

Several core systems use `static Instance` singletons:
- **`GameManager`** — wave spawning, enemy tracking, victory condition (4 enemies killed)
- **`UIManager`** — controls all UI panels (main menu, HUD, game over, victory, XP/level display)
- **`InventoryManager`** — manages a single sword slot + up to 10 ingredient slots; fires `OnInventoryChanged` event
- **`CraftingSystem`** — handles ingredient-to-sword fusion via `CraftingRecipe` ScriptableObjects; fires `OnFuseSuccess` event
- **`InventoryUI`** — inventory panel toggled with `I` key; pauses game (timeScale=0) while open

### Player GameObject (multi-component)

The player root holds multiple scripts that communicate directly via `GetComponent`:
- **`PlayerInput`** — reads Unity Input System (`PlayerInputActions`), routes to `PlayerController` and `PlayerCombat`
- **`PlayerController`** — Rigidbody2D movement + dash + impulse decay; `speed`, `attackPushForce`, `dashForce` are public so `CraftingSystem` and `XPManager` can buff them
- **`PlayerCombat`** — triggers attack animations via `PlayerAnimator`
- **`PlayerAnimator`** — drives Animator, manages 2-hit combo window via animation events, enables/disables `DamageDealer` hitboxes, plays death sequence
- **`XPManager`** — tracks XP/level; on level-up increases `PlayerController.speed` by 5
- **`PlayerDeathHandler`** — listens to `Health.OnDeath`; clears inventory and resets crafting buffs on death
- **`Health`** — shared component (used by both player and enemies); fires `OnDeath` Action event

### Enemy: SkeletonFighter

State machine with 6 states: `Approach → Orbit → DashPrep → DashMove → Hit → Cooldown`. All movement parameters (speed, orbit radius, cooldown) are randomized per instance in `Awake`. The attack animation drives `OnAttackAnimationEnd()` via animation event to transition out of `Hit` state. `EnemyTracker` calls `GameManager.UnregisterEnemy()` on `OnDestroy`.

### Combat & Damage

`DamageDealer` is a trigger collider child of the player (hitbox). It uses `IDamageable` interface and a `HashSet` to ensure one hit per swing. Animation events `EnableHitbox(i)` / `DisableHitbox(i)` activate it. Enemies deal damage directly through their own collider trigger logic.

### Loot & Inventory Flow

On enemy death → `EnemyAnimator` spawns an `ItemDrop` prefab using a `LootTable` ScriptableObject. Player walks into the drop → `ItemDrop.OnTriggerEnter2D` calls `InventoryManager.AddItem`. Player opens inventory (`I`) → selects an ingredient → clicks Fuse → `CraftingSystem.TryFuse` consumes the ingredient and calls `ApplyBuff` on the player.

### ScriptableObjects

- **`ItemData`** — item definition (name, icon, type, gold value). Create via `Inventory/Item Data` menu.
- **`CraftingRecipe`** — maps one `ItemData` ingredient to a `BuffType` (Damage, DashSpeed, Range, Knockback, MoveSpeed) + float value. Create via `Inventory/Crafting Recipe` menu.
- **`LootTable`** — weighted random drop table with overall drop chance. Create via `Inventory/Loot Table` menu.

### UI Flow

Game starts → main menu buttons visible, HUD hidden. `UIManager.HideMainMenu()` / `GameManager.StartGame()` called on Start button. HUD (health hearts, XP slider, level text) shows. On player death → `PlayerAnimator` calls `UIManager.ShowGameOver()` after death animation. On 4 kills → `GameManager.UnregisterEnemy` calls `UIManager.ShowVictory()`. Restart reloads the active scene.

## Key Conventions

- Hitbox enable/disable is driven entirely by Animator events — never toggle `DamageDealer` manually.
- Combo logic (2-hit) lives in `PlayerAnimator` via boolean flags set by animation events (`LightAttack2Window`, `LightAttack1Ended`, `LightAttack2Ended`).
- `Health.OnDeath` is the canonical death signal — subscribe to it rather than polling `IsDead`.
- `InventoryManager.OnInventoryChanged` is the canonical signal for UI refresh — `InventoryUI` subscribes in `Start` and unsubscribes in `OnDestroy`.
- Note: `InvetoryUI.cs` has a typo in the filename (missing 'n') — the class inside is correctly named `InventoryUI`.
