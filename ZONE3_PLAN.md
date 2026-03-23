# Zone 3 — Clone Boss Fight Plan

## What We Did Today

### Concept
The final boss is the pig NPC (shopkeeper from earlier zones) who used the mushroom ingredients
the player collected to brew a potion that turns him human — but it transforms him into whoever
he's closest to, which is the player. So the final fight is 1v1: you vs a clone of yourself.

The clone inherits the player's actual runtime stats (speed, dashForce, attackPushForce) read
from PlayerStats.Instance on Awake. Damage is separate — set manually in Inspector.

---

### Scripts Written

#### `CloneAnimator.cs`
- Sits on the Visual child of the Clone prefab (same GO as the Animator)
- Drives the Player Animator Controller (reuses it 100%, zero new art)
- Handles all animation events: EnableHitbox, DisableHitbox, LightAttack2Window,
  LightAttack1Ended, LightAttack2Ended
- Exposes `Attack2WindowOpen` and `ComboFinished` flags polled by CloneAI

#### `CloneAI.cs`
Full state machine. States:

| State | Description |
|---|---|
| Idle | Waiting for StartFight() call |
| MirrorStance | Hub state. Reads player, shuffles, retreats briefly, then commits |
| ComboApproach | Walks toward player, does full 1-2 combo on arrival |
| DashStrike | Impulse burst toward player, then full 1-2 combo on arrival |
| Swinging | Executes the combo: Attack1 → wait for window → Attack2 + second lunge |
| Dodge | Diagonal escape when player swings (chance-based, cooldown) |
| Cooldown | Brief pause after any attack |
| WallEscape | 8-ray open-space detection → diagonal dash to most open direction |
| Phase2Talk | Stops at 50% HP, plays dialogue, then resumes as Phase2 |
| Dead | Triggers death anim, hides boss bar, calls VictoryRoutine |

Key behaviors:
- **MirrorStance retreat**: 1s max when player rushes in, then commits to attack
- **Shuffle**: lateral drift every 3-5s so it doesn't look static
- **Wall escape**: triggered any time clone is within 1.5 units of any wall (except during
  Swinging, Dodge, Phase2Talk). Uses 8-directional raycast to find most open direction.
- **ChooseAttack**: DashStrike if player is retreating, ComboApproach if player advancing.
  Phase2 increases DashStrike weight.
- **Dodge**: diagonal (not cardinal), 28% chance normal / 45% Phase2, 4s cooldown between dodges.
  Direction weighted away from player but picks most open diagonal.
- **Combo lunge**: both attacks apply an impulse toward the player (attackPushForce),
  so the clone body-weights into the hit instead of standing still.
- **Phase2**: shorter patience (2-4s), shorter cooldown, higher dodge chance, higher dash weight.

#### `BossHealthBarUI.cs`
- Singleton. Call `RevealBar()` to trigger the formation sequence.
- Formation: panel fades in → bar fills left to right (smooth, 1.6s) → Y, O, U letters
  appear one at a time with a scale punch (0 → 1.2 → 1.0).
- Tracks clone HP automatically in Update via `cloneHealth` reference.
- Call `Hide()` on clone death — fades out.

---

## Full Unity Setup Checklist (Do This Tomorrow)

### Clone Prefab
Build this hierarchy in Unity:

```
CloneRoot  (Empty GameObject)
├── Components: CloneAI, Health, GenericEnemyHitEffect, Rigidbody2D, Collider2D
│
└── Visual  (Empty child)
    ├── Components: CloneAnimator, Animator, SpriteRenderer
    └── Hitbox  (Empty child)
        └── Components: DamageDealer, Collider2D (trigger)
```

**CloneRoot — Inspector assignments:**
- CloneAI:
  - `cloneAnimator` → drag Visual child
  - `wallLayer` → set to whatever layer walls use
  - `playerAttackState1` → MUST match exact name of LightAttack1 state in Player Animator Controller
    (open the Player Animator Controller asset and check — probably "LightAttack1" or "Player_LightAttack1")
  - `playerAttackState2` → same for attack 2
  - `phase2Dialogue` → create a DialogueData asset (Assets > Create > Dialogue > Dialogue Data),
    write the 50% HP funny lines, drag here
- Health:
  - `maxHP` → set to 40 or 50
- GenericEnemyHitEffect:
  - `knockForce` → something low like 3-4 (clone should feel heavy)

**Visual child — Inspector assignments:**
- Animator:
  - Controller → drag the Player Animator Controller asset
- SpriteRenderer:
  - Sprite → player sprite (same sheet)

**Hitbox child:**
- DamageDealer:
  - `hitLayers` → Player layer
  - `damage` → 1 (tune later)
  - `knockbackForce` → whatever feels right
- Collider2D: set to Trigger, size it like player's hitbox

---

### Boss Health Bar UI
Build this in the Canvas:

```
BossBarPanel  (GameObject with CanvasGroup + BossHealthBarUI)
├── BarBackground  (Image — dark background)
├── BarFill  (Image — the actual fill)
│   └── Fill Method: Horizontal
│   └── Fill Origin: Left
│   └── fillAmount: 0 (starts empty)
└── LettersRow  (Horizontal Layout Group)
    ├── Y  (TextMeshProUGUI, alpha 0)
    ├── O  (TextMeshProUGUI, alpha 0)
    └── U  (TextMeshProUGUI, alpha 0)
```

**BossHealthBarUI Inspector:**
- `cloneHealth` → drag clone's Health component
- `rootGroup` → drag BossBarPanel's CanvasGroup
- `fillImage` → drag BarFill Image
- `letterY/O/U` → drag the three TMP objects

Position the bar wherever you want on screen (top center recommended).

---

### Pig Reveal Trigger
You need a trigger or dialogue moment where the pig "reveals" his plan. At the end of that
moment, call:
```csharp
CloneAI.Instance (or reference).StartFight();
BossHealthBarUI.Instance.RevealBar();
```
Play a boss music AudioClip at the same moment.

The pig NPC dialogue before the fight needs to be written. The phase2Dialogue
(at 50% HP) also needs to be written — it should be funny/self-aware (e.g., the clone
commenting on looking like you, or trash-talking).

---

### Phase 2 Dialogue Asset
- Assets > Create > Dialogue > Dialogue Data
- Name it something like "ClonePhase2Dialogue"
- Write 2-3 lines, speaker name can be "???" or your character's name
- Drag into CloneAI's `phase2Dialogue` field

---

### Player Attack State Names (Critical)
Open the Player Animator Controller. Find the two light attack animation states.
Their exact names go into CloneAI's `playerAttackState1` and `playerAttackState2` fields.
If these are wrong, dodge will never trigger.

---

### Zone 3 Scene / Arena
- Zone 3 needs a room big enough for the fight (not cramped)
- The pig NPC needs to be in the scene with a trigger zone or dialogue gate
- The Clone prefab spawns when the fight starts (either pre-placed and disabled,
  or instantiated from the pig's position)

---

## What's NOT Done / Open Questions

- Pig NPC reveal sequence (dialogue, animation, how clone spawns) — not scripted yet
- Boss music — not wired anywhere yet
- Zone 3 scene itself — not built yet
- Clone prefab — not built in Unity yet
- Player attack state names — need to be verified in editor
- Phase 2 and reveal dialogue lines — need to be written
- Victory screen content — ShowVictory() shows the existing victoryPanel,
  you may want a special ending instead

## Notes on Tuning (Do in Playtest)
- `attackRange`: if combo feels like it misses, increase slightly
- `dodgeChance`: if clone dodges too much even at 28%, lower it
- `dashForce` clone inherits from player — if dash feels too fast/slow, override it
  with a multiplier in CloneAI.Awake
- `wallThreshold` 1.5: increase if clone still gets stuck on walls
- Phase2 patience 2-4s: tighten more if Phase2 feels same as Phase1
