# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**NeoPokemonTCG_Unity** is a Unity 6 project recreating the Pokemon Trading Card Game engine up to (but excluding) the eCard sets. This is a non-commercial, educational project with a sophisticated modular architecture.

## Architecture & Code Structure

### Assembly Structure
- **Main Assembly**: `GimGim.asmdef` - Core game logic
- **Test Assembly**: `Tests.asmdef` - Unit tests with NUnit framework, editor-only

### Core Systems

#### ActionSystem (`Assets/Scripts/ActionSystem/`)
Command pattern implementation managing game action execution and sequencing. All game state changes flow through this system.
- `ActionSystem.cs` - Main coordinator with coroutine-based execution
- `GameAction.cs` - Base class for all game actions
- `GameActions/` - Specific action implementations

#### GameplaySystems (`Assets/Scripts/GameplaySystems/`)
Core gameplay logic built on modular aspect system:
- `GameplaySystem.cs` - Base class for all gameplay systems
- `GameDataSystem.cs` - Manages game data and profiles
- `GameStateSystem.cs` - Handles game state transitions
- `MatchSystem.cs` - Match-level logic and coordination

#### Model (`Assets/Scripts/Model/`)
Data models representing game entities (Card, Player, Match, Zone classes).

#### Event System (`Assets/Scripts/NotificationEventSystem/`)
Observer pattern implementation for decoupled communication between systems.

#### AspectContainer (`Assets/Scripts/AspectContainer/`)
Component system providing modular functionality composition.

### Data Management

#### Profiles System (`Assets/Scripts/Data/Profiles/`, `Assets/Resources/Profiles/`)
JSON-based data storage for:
- Card profiles (Pokemon, Trainer, Energy)
- Deck configurations
- Set definitions
- Test data (isolated in `TestData/` subdirectory)

#### Factories (`Assets/Scripts/Data/Factories/`)
Factory pattern implementations for creating game objects from profile data.

### Utilities

#### Logging (`Assets/Scripts/Utility/Logger/`)
Custom logging system with color-coded output for different systems.

#### Python Scripts (`Assets/Scripts/HelperPythonScripts/`)
Data processing pipeline for downloading and organizing Pokemon card data.

## Key Patterns & Conventions

### Namespacing
All code uses `GimGim.*` namespace hierarchy reflecting directory structure.

### Event-Driven Architecture
- Use `NotificationEventSystem.PostEventAndExecute()` for immediate event processing
- Systems subscribe to events via `GameplayAspect` base class
- Events flow through the ActionSystem when they modify game state

### Data Flow
1. **Profile Loading**: JSON → Profile factories → Game objects
2. **Action Execution**: User input → GameAction → ActionSystem → Model updates → Events
3. **System Reactions**: Events → Subscribed systems → Reactions → More actions (if needed)

### Testing
- Unit tests in `Assets/Tests/` with separate assembly definition
- Test data isolated in `TestData/` subdirectories
- Use NUnit framework with Unity Test Runner integration

## Development Guidelines

### Code Organization
- New gameplay systems inherit from `GameplaySystem`
- New actions inherit from `GameAction` 
- Use existing logging, serialization, and utility classes
- Follow established namespace patterns

### Data Management
- Card data goes in appropriate `Profiles/` subdirectories
- Test-specific data goes in `TestData/` subdirectories
- Use JSON serialization with existing profile classes

### Event Handling
- Subscribe to events in system `Awake()` methods
- Unsubscribe in system `Destroy()` methods
- Post events through `NotificationEventSystem`

## Unity-Specific Notes

- **Unity Version**: 6000.0.43f1 (Unity 6)
- **Test Runner**: Unity Test Framework with NUnit
- **Assembly Definitions**: Used for modular compilation
- **Resources**: Runtime data loading via Unity Resources system
- **2D Features**: Project configured for 2D game development