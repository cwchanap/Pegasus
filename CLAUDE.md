# Snow Dream (Pegasus)

Godot 4.7 .NET (C# only, no GDScript), desktop. Hobby project: keep it simple (YAGNI, KISS), clean modules, no backward compatibility.

## Layout

Folders follow the architecture layers. A layer only uses layers below it.

| Layer | Folders |
|---|---|
| Scenes | `day/`, `night/`, `puzzles/`, `ui/` |
| Shared gameplay | `world/` (player, camera, interactables), `conditions/`, `dialogue/` |
| Core services (autoloads) | `core/` (GameState, SceneRouter, SaveService, AudioService) |
| Content (data, not code) | `content/` (Resource `.tres` files, `.yarn` scripts) |

Also: `sandbox/` grey-box test scene, `dev/` dev-only helpers, `tests/` xUnit tests for pure logic only.

Conventions: snake_case folders and scene files, PascalCase C# files, namespace `Pegasus.<Module>`. Game state is plain C# data kept apart from nodes; scenes rebuild from it on load. Grey-box art and placeholder sound until real assets land.

## Commands

- Build: `dotnet build`
- Tests: `dotnet test`
- Run headless: `godot --headless --quit-after 120 [res://scene.tscn]`
- Screenshot: `scripts/shot.sh out.png [res://scene.tscn] [frames]`
- After adding new assets, import once: `godot --headless --import`
