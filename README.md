# Match-3 Puzzle Prototype (Unity)

A fully playable Match-3 puzzle prototype developed from scratch in **Unity 6**, with a focus on clean gameplay architecture, blocker mechanics, and deterministic game state management.

This project was created as a **portfolio prototype** to demonstrate my ability to design and implement core puzzle systems, build scalable gameplay logic, and take a feature from concept to a finished, playable state.

---

## Overview

The prototype implements a classic Match-3 core loop with a deliberately limited scope. All gameplay systems were designed and implemented from scratch, prioritizing clarity, correctness, and extensibility over heavy content or visual polish.

The goal of the project was not content volume, but to build a solid, production-ready foundation for a casual puzzle game.

---

## Gameplay Features

- Standard Match-3 mechanics
  - Swap validation
  - Match detection
  - Cascades, gravity, and refill
- Stone blocker system
  - Occupies grid cells and blocks movement and gravity
  - Cannot be swapped or matched directly
  - Breaks after **two adjacent matches**
- Objective-based progression
  - Color tile objectives
  - Stone-breaking objectives
- Move-limited gameplay
- Win / lose conditions with clear UI feedback

---

## Technical Focus

- Core gameplay systems implemented from scratch in C#
- Data-driven level configuration using `ScriptableObject`
- Clear separation of responsibilities between:
  - Game logic (Board, Cell)
  - Presentation (BoardView, TileView, StoneView)
  - State and rules (GameState)
- Deterministic gravity and refill behavior, including segmented gravity around blockers
- Scoped intentionally as a minimum viable prototype to demonstrate system design and maintainability

---

## Levels

- **Level 1**  
  Introduces core Match-3 mechanics with simple objectives and generous move limits.

- **Level 2**  
  Introduces stone blockers, reduced move counts, and increased strategic constraints.

---

## Tech Stack

- Unity 6
- C#
- ScriptableObject-based configuration
- WebGL build for fast playtesting and accessibility

---

## Assets

All visual assets used in this prototype are free-to-use assets sourced from publicly available asset packs (e.g. Kenney), and are used strictly for presentation purposes.

All gameplay logic, systems, and architecture were implemented from scratch.

---

## Documentation

A one-page game design document used during development is included in the `/GDD` folder.  
It outlines the core mechanics, goals, and scope of the prototype.

---

## Playable Build

A playable WebGL build and gameplay video are available via an external portfolio link.
