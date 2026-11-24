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

Flow: `core/Boot` enters the world from the state; `core/DayCycle` runs bed → 3 AM night → morning and saves at both; `core/SceneRouter` (autoload) changes scenes behind a fade and pauses the tree meanwhile. `ui/Toast` (autoload) shows one line of feedback.

Also: `sandbox/` grey-box test scene, `dev/` dev-only helpers, `tests/` xUnit tests for pure logic only.

State and rules: `core/state/` (GameState, SaveData) and `conditions/` are pure C# with no Godot types, so `tests/` can compile them directly. Keep Godot code out of those folders. Read and change state through `Game.State` (`core/Game.cs`); gates use condition strings such as `"guts >= 2 | item:flashlight"` (`conditions/ConditionParser.cs` has the grammar).

Conventions: snake_case folders and scene files, PascalCase C# files, namespace `Pegasus.<Module>`. Game state is plain C# data kept apart from nodes; scenes rebuild from it on load. Grey-box art and placeholder sound until real assets land.

## Commands

- Build: `dotnet build`
- Tests: `dotnet test`
- Run headless: `godot --headless --quit-after 120 [res://scene.tscn]` (main scene is `core/boot.tscn`: continues the save; add `-- --new-game` to start fresh)
- Screenshot: `scripts/shot.sh out.png [res://scene.tscn] [frames] [extra dev args, e.g. --hold=move_right@1-40 --tap=interact@60]` (see `dev/DevDriver.cs`)
- After adding new assets, import once: `godot --headless --import`
