# Pirate Game — Act 1 Vertical Slice Plan

## Phase 1 — World Foundation
1. **Expand the map** (100×100 or similar) with camera follow. Add ocean tiling or color.
2. **Multi-location system** — Define locations (Base Port, Port 2, Port 3, Secret Island, Boss Arena) with positions, discovery state, and colliders.
3. **Minimap/compass** — Simple UI showing direction arrows to known locations so navigation isn't blind.

## Phase 2 — Quest & Story System
4. **Quest data model** — `Quest` class with id, title, description, type (main/side), objectives, state (locked/active/complete).
5. **Quest Manager** — Tracks active quests, progress, triggers. Holds the Act 1 quest chain.
6. **Dialogue system** — Queue of text lines + speaker name displayed in a bottom panel. Triggered by quest events or location entry.
7. **Quest tracker UI** — Small HUD element: current quest name + objective text.

## Phase 3 — Act 1 Main Quest Line
8. **Quest: "Set Sail"** — Tutorial. Sail to Port 2 (marked on compass). Arrive → dialogue with harbormaster.
9. **Quest: "Rescue the Gunsmith"** — Harbormaster says gunsmith captured. Sail to marked area, defeat enemy wave, gunsmith rescued → unlocks cannon upgrades in shop.
10. **Quest: "The Pilot"** — At Port 3, recruit Helmsman/Pilot. Gives permanent speed boost. Dialogue + crew added.
11. **Quest: "The Gunner"** — Gunsmith sends you to find his apprentice (Gunner). Rescue mission → recruit. Gives fire rate boost.
12. **Quest: "The Final Threat"** — All crew assembled. Sail to Boss Arena. Boss fight.

## Phase 4 — Enemy Ships
13. **Enemy ship prefab** — Sprite, AI that circles and fires cannons at player (unlike monsters that charge).
14. **Enemy ship spawning** — Used in rescue quests and boss arena. Different difficulty tiers.

## Phase 5 — Boss
15. **Boss enemy** — Large ship or sea creature. Multi-phase (e.g., phase 1: charges, phase 2: spawns adds, phase 3: rapid fire). Health bar at top of screen.
16. **Victory screen** — "Act 1 Complete" with stats.

## Phase 6 — Side Quest
17. **Secret Island discovery** — Hidden location. Entering trigger zone → cutscene dialogue → side quest activates.
18. **Island defense mission** — 3-minute survival. Enemies target the island (health bar for island). Player must intercept. Success → unique reward (crew member or special upgrade).

## Phase 7 — Shop & Economy Rework
19. **Multi-resource costs** — Shop items cost combos (e.g., Extra Cannons: 100 gold + 5 metal). Update `ShopItem` with resource fields. Update shop UI to show all costs.
20. **Locked items** — Cannon upgrades locked until gunsmith rescued. Show padlock + "Requires: Rescue the Gunsmith" text.
21. **Crew tab rework** — Story crew (Gunner, Pilot) obtained via quests only, shown as recruited. Shop crew are optional extras.

## Phase 8 — Polish & Flow
22. **Larger wave system** — Waves trigger contextually (sailing between locations, during quests) not just at game start.
23. **Enemy scaling** — HP/speed multiplier increases as story progresses.
24. **Balancing pass** — Tune gold income, shop prices, enemy difficulty, quest pacing.
