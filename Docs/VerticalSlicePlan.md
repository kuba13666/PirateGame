# Pirate Game — Act 1 Vertical Slice Plan
> **Lore:** You are Davy Jones, cursed to die and be reborn. Build a crew, break the loop.

## Phase 1 — World Foundation ✅
1. ✅ **Expand the map** (100×100) with camera follow. Ocean tiling/color.
2. ✅ **Multi-location system** — 5 locations (Safe Harbor, Trader's Cove, Naval Outpost, Secret Island, Boss Arena / The Maelstrom) with positions, discovery state, colliders.
3. ✅ **Minimap/compass** — Direction arrows + minimap panel showing discovered locations.

## Phase 2 — Quest & Story System ✅
4. ✅ **Quest data model** — `Quest` class with id, title, description, type (main/side), objectives (TravelTo/DefeatEnemies/Survive/TalkTo/CollectItems), state, prerequisite, dialogue.
5. ✅ **Quest Manager** — Tracks active quests, progress, triggers. Holds the Davy Jones quest chain with rewards & location discovery.
6. ✅ **Dialogue system** — Bottom panel with speaker name + text. Click to advance. Uses unscaled time (works while paused).
7. ✅ **Quest tracker UI** — HUD element (top-left below health): current quest name + objective text.

## Phase 3 — Act 1 Main Quest Line ✅
8. ✅ **Quest: "The Awakening"** — Impossible wave floods the screen (60+ enemies, infinite spawn). Player dies → black overlay → rebirth dialogue introduces the Davy Jones curse. Respawn at Safe Harbor.
9. ✅ **Quest: "Set Sail"** — Voice of the Deep directs you to Trader's Cove (NW). Arrive → dialogue with harbormaster about the curse.
10. ✅ **Quest: "Rescue the Gunsmith"** — Harbormaster says gunsmith captured by sea creatures. Defeat 10 enemies → gunsmith rescued → +50% damage.
11. ✅ **Quest: "The Pilot"** — Sail to Naval Outpost. Recruit helmsman → +50% speed.
12. ✅ **Quest: "The Gunner"** — Clear 15 enemies to rescue gunsmith's apprentice → improved fire rate (0.7×).
13. ✅ **Quest: "Break the Loop"** — All crew assembled. Sail to The Maelstrom + defeat the Kraken. Victory → curse broken.

**Death flow:** Die → black overlay + pause → death/quest dialogue → overlay hides → Safe Harbor shop opens → close shop → waves restart.
**Progressive discovery:** Completing quests reveals the next location on the compass.

## Phase 3.5 — Death Penalty & Wave Scaling ✅
14. ✅ **Run loot system** — Loot collected during a sail is tracked as "run loot" shown in brackets (e.g. `500 (+12)`). Entering port banks run loot safely. Death only loses un-banked run loot; banked resources persist.
15. ✅ **Wave escalation** — Each successive wave increases enemy HP/speed/count. Scaling resets on death (back to wave 1) but the base difficulty increases with story progress.

## Phase 4 — Enemy Ships
16. **Enemy ship prefab** — Sprite, AI that circles and fires cannons at player (unlike monsters that charge).
17. **Enemy ship spawning** — Used in rescue quests and boss arena. Different difficulty tiers.

## Phase 5 — Boss
18. **Boss enemy (Kraken)** — Large sea creature. Multi-phase (phase 1: charges, phase 2: spawns adds, phase 3: rapid attacks). Health bar at top of screen.
19. **Victory screen** — "The Curse is Broken" with stats (deaths, kills, time).

## Phase 6 — Side Quest
20. **Secret Island discovery** — Hidden location. Entering trigger zone → cutscene dialogue → side quest activates.
21. **Island defense mission** — 3-minute survival. Enemies target the island (health bar for island). Player must intercept. Success → unique reward (crew member or special upgrade).

## Phase 7 — Shop & Economy Rework
22. **Multi-resource costs** — Shop items cost combos (e.g., Extra Cannons: 100 gold + 5 metal). Update `ShopItem` with resource fields. Update shop UI to show all costs.
23. **Locked items** — Cannon upgrades locked until gunsmith rescued. Show padlock + "Requires: Rescue the Gunsmith" text.
24. **Crew tab rework** — Story crew (Gunner, Pilot) obtained via quests only, shown as recruited. Shop crew are optional extras.

## Phase 8 — Polish & Flow
25. **Larger wave system** — Waves trigger contextually (sailing between locations, during quests) not just at game start.
26. **Enemy scaling** — HP/speed multiplier increases as story progresses.
27. **Balancing pass** — Tune gold income, shop prices, enemy difficulty, quest pacing.
