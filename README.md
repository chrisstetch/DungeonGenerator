# Procedural Dungeon Generator

## Overview
A Unity-based tool designed for fast, validated generation of grid based dungeon environments. The system gives control over the design through a simple UI, allowing for instant adjustments of complexity, density, and size, while guaranteeing map traversability using the A* pathfinding algorithm.

## Key Features
* **Parametric Control:** Users control room count and min/max room dimensions via UI sliders.
*  **Dynamic Scaling:** The underlying grid size automatically scales based on input parameters to minimize void space and maximize room placement success.
*  **Guaranteed Connectivity:** Rooms are linked sequentially using a randomized L shaped corridor technique using Manhattan distance.
*  **A\* Validation**: The system calculates and visually displays the optimal path between the Player spawn and Exit to validate that the generated map is solvable.
*  **Editor Utility:** Includes functions to use seeded generation, and ability to save the generated dungeon structure as a reusable Unity Prefab asset.
