# Procedural Dungeon Generator

## Overview
A Unity-based tool designed for fast, validated generation of grid based 2D dungeon environments. The system gives control over the design through a simple UI, allowing for instant adjustments of complexity, density, and size, while guaranteeing map traversability using the A* pathfinding algorithm.

### 1. Problem Space and Context
Level generation is a challenge in game development, requiring significant manual effort to ensure maps are playable, balance, and interesting. Random level generators often fail due to disconnected geometry and uncontrolled density. This system addresses these issues by transforming random geometric room placement into a guaranteed connected graph. It provides level designers with level prototypes that maintains structural integrity regardless of complexity.

### 2. System Contributions
* **Controlled Space Packing:** Uses collision-checked room placement alongside a dynamic scaling model to ensure rooms are placed efficiently.
* **A\* Validation Layer:** A dedicated pathfinding system to guarantee a path from start to end.
* **Designer Workflow:** Simplifies level design time and provides instant generation and visual feedback, and the ability to saved the generated level as a prefab.
  
## Key Features
* **Parametric Control:** Users control room count and min/max room dimensions via UI sliders.
*  **Dynamic Scaling:** The underlying grid size automatically scales based on input parameters to minimize void space and maximize room placement success.
*  **Guaranteed Connectivity:** Rooms are linked sequentially using a randomized L shaped corridor technique using Manhattan distance.
*  **A\* Validation**: The system calculates and visually displays the optimal path between the Player spawn and Exit to validate that the generated map is solvable.
*  **Editor Utility:** Includes functions to use seeded generation, and ability to save the generated dungeon structure as a reusable Unity Prefab asset.

## Installation and Setup
### Requirements
* Unity 2021 LTS or newer.

### Installation
1.  Import the provided `DungeonGenerator.unitypackage` via **Assets** $\rightarrow$ **Import Package** $\rightarrow$ **Custom Package**.
2.  Open the primary demonstration scene located at `Assets/Scenes/DungeonScene`

### Component Structure
| Slot | Required Component | Notes |
| :--- | :--- | :--- |
| **Pathfinder** | Generator (Pathfinder) | Drag the Generator object here. |
| **Path Tilemap** | PathMap (Tilemap) | The transparent layer for drawing the solution path. |
| **Wall/Floor Tiles** | TileBase Assets | Must be linked to Tile assets for generation. |

## Usage Guide

### 1. Generation Controls
| Control | Function |
| :--- | :--- |
| **Reset Defaults Button** | Instantly reverts all sliders and the seed field to the original values. |
| **Sliders** | Control Room Count, Min/Max Width/Height. |
| **Seed Input** | Enter a specific text string (e.g., "CASTLE") to regenerate the same map layout. |
| **Generate Button ** | Force immediate regeneration with current parameters. |

### 2. Utility and Validation
| Control | Function |
| :--- | :--- |
| **Show Path Toggle** | Toggles the visibility of the A\* solution path. Toggling **ON** recalculates and draws the shortest route between the Player and Exit. |
| **Save Dungeon Button** | Saves the entire current Grid and its Tilemaps as a reusable Prefab asset. The prefab will be stored in: `Assets/SavedDungeons/`. |

### 3. Using the Saved Prefab
The saved Prefab is the primary output for integration into a final game.

1. **Locate the Prefab:** Find the asset in your Project window at: `Assets/SavedDungeons/Dungeon_[SEED_OR_ID].prefab`
2. **Deployment:** Drag the saved dungeon Prefab directly into the Hierarchy of your main game scene.
3. **Customize**
    * **Cleanup:** Delete the debug entities (PlayerPrefab and ExitPrefab) contained within the Prefab instance.
    * **Art Integration:** Replace the placeholder tiles on the Tilemap components with your game's final art assets.
