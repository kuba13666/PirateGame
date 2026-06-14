# Biome 1 — "The Cursed Shallows" — Design Document

*Plot, quest progression, map geography, and boss design for the first biome.*

---

## 0. Game Structure (where Biome 1 sits)

The game spans **multiple biomes** (cursed seas), each a self-contained arc:
its own waters, two minibosses that recruit crew/grant power, and a **guardian
boss** at a gateway. Defeating a biome's guardian does **not** end the curse —
it opens the passage to the **next, deeper sea**.

**Breaking the curse — "Break the Loop" — is the final quest of the FINAL
biome, not Biome 1.** Biome 1, "The Cursed Shallows," is only the first sea.
Its finale (the Kraken at the Maelstrom) is a cliffhanger: the whirlpool is a
door, and it swallows you down into Biome 2.

*(The full curse-break finale dialogue — "the loop is broken, the sea is
calm" — is reserved for the last biome and intentionally NOT used here.)*

---

## 1. Plot Summary — The Curse of Davy Jones

You are **Davy Jones**, cursed to die at sea and be reborn, endlessly. The sea
spits you back every time you fall. The **Voice of the Deep** promises a way
out: assemble a crew of legends and conquer **The Maelstrom**. But the
Maelstrom is no exit — it is a passage, guarded by the **Kraken**, that opens
onto deeper, stranger seas. Biome 1 is only the first of them; the true source
of the curse lies far below, biomes away.

Two other cursed legends haunt these waters, mirrors of your own damnation:

- **Captain Hendrick van der Decken**, "The Flying Dutchman" — swore during a
  storm to round the Cape *"even if it takes until doomsday"* and was condemned
  to sail forever, never able to make port. You are cursed to die and return;
  he is cursed to sail and never stop.
- **Mocha Dick**, the white whale — the legendary albino whale that survived a
  hundred whalers, now corrupted by the Kraken's curse. Following the old
  Aspidochelone legend, his barnacled back drifts disguised as an uncharted
  islet, luring sailors to land on him. Inside him, kept alive by the curse,
  is **Israel Hands** — Blackbeard's old second-in-command, Flint's gunner,
  the finest cannon hand of the golden age, swallowed while hunting the beast.

Defeating the two minibosses recruits the two **legendary crew members**
(Pilot and Gunner) required to enter and survive the Maelstrom.

### Characters

| Character | Role | Mythological source |
|---|---|---|
| Davy Jones (player) | Cursed captain, dies and returns | Sailor folklore: the sea-devil, "Davy Jones' locker" |
| Voice of the Deep | Narrator/guide, ambiguous ally | Original |
| Harbormaster | Quest giver at Trader's Cove | Original |
| Gunsmith | Regular crew: shop/upgrades vendor | Original |
| **Van der Decken** | **Legendary PILOT** (miniboss → crew) | The Flying Dutchman legend (1821 Blackwood's version) |
| **Israel Hands** | **Legendary GUNNER** (freed from miniboss → crew) | Real pirate (Blackbeard's 2nd-in-command); *Treasure Island* (Flint's gunner) |
| **Mocha Dick** | Miniboss: cursed white whale | Real albino whale (~1810–1838) that inspired *Moby-Dick*; island disguise from the Aspidochelone/Jasconius legend |
| **The Kraken** | Final boss, source of the curse | Norse/sailor legend |

---

## 2. Quest Progression

Linear main chain. One quest active at a time (current QuestManager model).

### Q0 — The Awakening *(exists, unchanged)*
- **Objective:** Survive the onslaught (completed by first death).
- **Beat:** You die. The Voice reveals you are Davy Jones, cursed to return.
  Build a crew, face the Maelstrom, break the loop.

### Q1 — Set Sail *(exists, dialogue updated)*
- **Objective:** TravelTo `traders_cove`.
- **Beat:** The Harbormaster recognizes the cursed captain. NEW: he seeds both
  legends — *"Two terrors haunt these waters, Jones. A green ghost-light in
  the northern fog that lures ships to their doom... and a white island that
  appears on no chart and moves between sightings. Legends say a man still
  lives inside it."*
- **Unlocks:** `dutchmans_drift` and `white_island` POIs become discoverable.

### Q2 — Rescue the Gunsmith *(exists, anchored to map)*
- **Objective:** TravelTo `gunsmith_wreck` (shipwreck POI, eastern Hunting
  Grounds) + DefeatEnemies 10 near it.
- **Beat:** Free the Gunsmith from the beasts surrounding his wrecked ship.
  He becomes your shop/upgrade vendor and points you onward: his old friend
  **Israel Hands** vanished hunting the white island; and only a legendary
  pilot could ever steer into the Maelstrom.
- **Reward:** Gunsmith joins (shop upgrades; existing weapon-upgrade hook).

### Q3 — The Ghost Light *(REPLACES "The Pilot")*
- **Objective:** TravelTo `dutchmans_drift` (fog bank, deep NW water) →
  boss arena → DefeatBoss `flying_dutchman`.
- **Start dialogue (sketch):**
  - Harbormaster: "The ghost-light took another ship last night. Whatever
    burns green out there, it's no lighthouse."
  - Voice of the Deep: "A cursed soul, like you. The Dutchman cannot stop.
    Make him."
- **Fight:** Ghost ship duel (see §4.1).
- **Complete dialogue (sketch):**
  - Van der Decken: "Three hundred years... and the sea finally lets me drop
    anchor. You did this, Jones?"
  - Van der Decken: "Then my wheel is yours. If breaking YOUR curse breaks
    mine, I'll steer you through hell itself. And captain — the Maelstrom IS
    hell. No hand but mine can hold a wheel through it."
- **Reward:** **PILOT joins** — move speed/handling buff; required to open
  the Maelstrom in Q5.

### Q4 — The White Island *(REPLACES "The Gunner")*
- **Objective:** TravelTo `white_island` (uncharted islet near Forgotten
  Isle) → island sinks (reveal) → boss arena → DefeatBoss `mocha_dick`.
- **Start dialogue (sketch):**
  - Gunsmith: "Hands swore he'd kill the white whale or die trying. Knowing
    that stubborn old gunner, he managed neither."
  - Voice of the Deep: "The island that moves. Land on it, and it will take
    you where the lost ones go. Perhaps that is the way in."
- **Reveal beat (at POI):** "The 'island' shudders. Barnacles crack. The
  white mass beneath you opens one ancient, hateful eye — and dives."
- **Fight:** Corrupted whale (see §4.2).
- **Complete dialogue (sketch):**
  - *(the dying whale convulses and disgorges a man, furious and dripping)*
  - Israel Hands: "TWELVE YEARS! Twelve years in that stinking gut, and you
    kill my whale in ten minutes?!"
  - Israel Hands: "...Davy Jones, eh? Blackbeard shot me in the knee for
    less than what the sea's done to you. Fine. My cannons are yours,
    captain. Let's go kill something bigger."
- **Reward:** **GUNNER joins** — fire rate/damage buff.

### Q5 — Into the Maelstrom *(Biome 1 finale — guardian boss, NOT the curse-break)*
- **Quest id:** `into_the_maelstrom` (was "break_the_loop").
- **Requires:** Q3 + Q4 complete (pilot + gunner aboard).
- **Objective:** TravelTo `boss_arena` (Maelstrom entrance, N) → arena →
  DefeatBoss `kraken`.
- **Beat:** Van der Decken steers through the storm wall (the rock-ring gap
  "opens" only now). The Kraken — the Maelstrom's **guardian** — rises and is
  slain. But killing it does **not** free you: the whirlpool yawns wider and
  drags the ship **down into Biome 2**. The curse holds. Cliffhanger.
- **Sets:** `GameManager.biome1Complete = true`. (No Biome 2 content yet — the
  chain ends here as "to be continued".)

**Side quest (existing, unchanged):** Forgotten Isle treasure hook stays
available as optional content.

---

## 3. Map Geography — Biome 1 (100×100 world)

Difficulty flows from the safe SW/center outward to the deadly N.

```
        ┌─────────────────────────────────────────────┐
        │ ≈≈ THE DEEP ≈≈        ╔═══════════╗   ☠☠    │
        │ (storms, all enemies) ║ MAELSTROM ║ rock    │
        │  ░ DUTCHMAN'S DRIFT   ║ (Q5 gate) ║ ring    │
        │  ░ (fog POI, Q3)      ╚═══╤═══════╝         │
        │                           │gap (opens Q5)   │
        │ TRADER'S COVE        ▲▲ rock belt ▲▲        │
        │  ⌂ (port+shop)                              │
        │    \  TRADE ROUTE         HUNTING GROUNDS   │
        │     \ (islet channel,     (E: harpies,      │
        │      crabs+harpies)       mermaids, ships)  │
        │                           ⚓ GUNSMITH WRECK  │
        │        SAFE HARBOR        (Q2)              │
        │         🏝 (home, calm)    ◌ WHITE ISLAND   │
        │                           (Q4, uncharted)   │
        │  ·· sandbars ··           🏝 FORGOTTEN ISLE │
        │        NAVY WATERS (SE: enemy ships)        │
        │         ⌂ NAVAL OUTPOST                     │
        └─────────────────────────────────────────────┘
```

### Spawn zones

| Zone | Area | Enemies | Density |
|---|---|---|---|
| Home Waters | radius ~18 around Safe Harbor | crabs only | sparse |
| Trade Route | W/NW channel to Trader's Cove | crabs, harpies | light |
| Hunting Grounds | E third | harpies, mermaids, some ships | medium |
| Navy Waters | SE quadrant | enemy ships heavy | medium |
| The Deep | N third | everything | dense |

### Decorative geography
- 10–15 small islets (3–4 PixelLab variants: palm islet, rocky crag, sandbar)
  with colliders, shaping sailing lanes.
- Rock/reef obstacles in belts (esp. ring around the Maelstrom with a single
  gap that visually opens in Q5).
- POI sprites: shipwreck (Q2), fog bank (Q3), uncharted white islet (Q4).

---

## 4. Boss Design

### Arena architecture (all 3 bosses)
Bosses are fought in **separate arenas off the main map** (same Unity scene,
pockets at e.g. (200,0), (300,0), (400,0), ~30×20, walled by rock rings):

1. Player enters boss POI trigger → screen fade out.
2. Teleport ship + camera to arena spawn; normal spawners pause.
3. Boss spawns; fight runs; arena walls confine both parties.
4. Victory → fade → teleport back beside the POI; quest progresses.
5. Death → existing death/respawn-at-home flow (curse rebirth fits the plot).

Driven by one `BossArenaManager`. Guarantees bosses never follow the player
to the open sea and arena size is independent of map geography.

### 4.1 The Flying Dutchman (Q3 miniboss)
Spectral ghost galleon (green/teal glow treatment).
- **Phase 1 — Duel:** keeps broadside distance, fires volleys.
- **Phase 2 (≤66% HP) — Ghost crew:** summons 2–3 spectral adds (tinted
  existing enemies), keeps firing.
- **Phase 3 (≤33% HP) — Doomsday storm:** speed up, ram attempts, heavy
  volleys; fog patches limit visibility.

### 4.2 Mocha Dick, the White Island (Q4 miniboss)
Giant albino sperm whale, sickly green "infection" growths (corruption).
- **Phase 1 — Surface:** slow ram charges; vulnerable.
- **Phase 2 (≤66%) — Dives:** submerges (invulnerable), moving shadow
  telegraph, breaches under the player; parasite adds (sickly-tinted crabs/
  mermaids) spill out.
- **Phase 3 (≤33%) — Frenzy:** faster charges, tail-slam shockwave rings,
  more parasites. Only damageable while surfaced.

### 4.3 The Kraken (Q5 — Biome 1 **guardian** boss) — *separate milestone*
The leviathan coiled at the Maelstrom's mouth. It is **not** the source of the
curse — it is the gate. Slaying it opens the passage down into Biome 2.
Multi-part preferred: central body + independent tentacles with own HP that
attack/grab; body vulnerable only after tentacles are down. Detailed design
deferred to its own phase.

---

## 7. Implementation Status

- **Phase A — Map geography:** ✅ done. PixelLab asset pack, 250×250 world
  (scaled 2.5×), islets/rock belts/Maelstrom ring, 3 story POIs, mist border,
  port/island art, minimap terrain. Islands/rocks block the ship (Terrain
  layer).
- **Phase B — Spawn zones:** ✅ done. `ZoneSpawnManager` (Home / Trade Route /
  Hunting Grounds / Navy / Deep / Open), ahead-of-heading off-screen spawning,
  threat-aware density caps. The Awakening is its own off-map onslaught.
- **Phase C — Quest chain anchored to map:** ✅ done. `DefeatBoss` objective +
  `ReportBossDefeated`; Q2 anchored to the wreck; Q3 "The Ghost Light"
  (Dutchman → Pilot, speed ×1.5); Q4 "The White Island" (Mocha Dick → Israel
  Hands, fire ×0.7); Q5 "Into the Maelstrom" (Kraken, cliffhanger). Crew buffs
  persist across re-equips (GameManager multipliers). Gold quest marker on
  minimap/edge arrows. Progressive location reveals.
- **Phase D — Boss arena framework:** ⏳ pending (§4 arena architecture).
- **Phase E — The two minibosses:** ⏳ pending (§4.1 Dutchman, §4.2 Mocha Dick).
- **Phase F — Kraken finale + Maelstrom gate open:** ⏳ pending (§4.3).

Final quest ids (as built): `the_awakening` → `set_sail` → `rescue_gunsmith`
→ `ghost_light` → `white_island_hunt` → `into_the_maelstrom`.

---

## 5. Crew Reward Summary

| Crew member | Source | Mechanical reward |
|---|---|---|
| Harbormaster | Q1 | opens Trader's Cove (port/shop) |
| Gunsmith | Q2 | shop upgrades vendor (+weapon upgrade) |
| **Van der Decken (Pilot)** | Q3 miniboss | +move speed/handling; unlocks Maelstrom entry |
| **Israel Hands (Gunner)** | Q4 miniboss | +fire rate/damage |

*(Note: Q3/Q4 dialogue already promises speed/fire-rate buffs — implementation
must be verified/added when reworking the quests.)*

---

## 6. Mythology Sources

- Flying Dutchman / van der Decken: https://en.wikipedia.org/wiki/Flying_Dutchman
- Israel Hands: https://en.wikipedia.org/wiki/Israel_Hands
- Mocha Dick: https://en.wikipedia.org/wiki/Mocha_Dick
- Aspidochelone (island-whale): https://en.wikipedia.org/wiki/Aspidochelone
