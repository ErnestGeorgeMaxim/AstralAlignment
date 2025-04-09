# Astral Alignment

**Astral Alignment** is a memory-matching game built in C# and WPF using MVVM. Players match image pairs within a time limit, with user profiles, game saving/loading, and statistics tracking.

---

## Key Features

- **User Accounts**
  - Create/select profiles
  - Associate avatars (image path stored)
  
- **Game Modes**
  - *Standard*: 4x4 board
  - *Custom*: User-defined MxN board (even number of tiles, M/N ∈ [2,6])

- **Randomized Layout**
  - New tile distribution every game

- **Timer-Based Gameplay**
  - Game ends when time runs out
  - Remaining time is always displayed

- **Save & Load**
  - Save game state (category, board, time, etc.)
  - Resume only from the same user profile

- **Statistics Tracking**
  - Tracks games played and won per user
  - Stats displayed as: `Username – Games Played – Games Won`

- **User Deletion**
  - Removes user profile, avatar, saved games, and stats
