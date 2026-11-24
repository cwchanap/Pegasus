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

Dialogue: Yarn Spinner for Godot is vendored in `addons/YarnSpinner-Godot` (MIT, v0.3.23, upstream commit b0abe88); do not edit it. Scripts live in `content/dialogue/*.yarn` (one `dialogue.yarnproject`). `ui/dialogue/conversation.tscn` (autoload `Conversation`) runs them through our own `ui/dialogue/DialogueBox`. Start one with `Conversation.Start("Node")` or an `world/npc/npc.tscn` with a `CharacterProfile` (`content/characters/*.tres`; register it in the Conversation scene's Characters list). Yarn reads and writes game state only through `dialogue/YarnBindings.cs` (`met("...")`, `<<stat>>`, `<<give>>`, `<<flag>>`, `<<bond>>`...); never keep game facts in Yarn `$variables`. Choices tagged `#needs:guts>=2` stay visible but locked (write the condition with no spaces, and no `<`, `$` or `#`, e.g. `#needs:guts>=2|item:flashlight`); `<<if>>` on a choice hides it instead. Every choice group needs at least one choice the player can always take: if all are locked or hidden the box shows them and then ends the conversation (the vendored runner cannot fall through). Input ownership: `core/InputLock` (dialogue, fades, later menus and puzzles).

Day places: to add one, make a `LocationData` in `content/locations/*.tres`, a scene whose root script is `day/DayLocation` (or derives from it) with that data, an exit, an instance of `ui/hud/day_hud.tscn` set as its `Hud`, and a `world/bounds/Bounds` node around the walkable area, then add the `.tres` to the `Locations` list in `day/city_map/city_map.tscn`. Every scene with open edges needs a `Bounds` node. Activities: an `ActivityData` in `content/activities/*.tres` (title, verb, gains), offered by a `day/ActivitySpot` in the scene and listed in the place's `LocationData.Activities` for the map. Doing one spends a slot behind a fade (`SceneRouter.Interlude`). A hangout is an activity with `With` (a CharacterProfile), `Bond = 1` and a `YarnScene` prefix: it plays `{YarnScene}1` to `3` by the bond level it raises, then `{YarnScene}Again`; the activity adds the bond afterwards, so hangout scripts don't `<<bond>>`. Place the person as `world/npc/npc.tscn` with no `YarnNode` (Talk stays off) and an `ActivitySpot` at the same spot. Stats and bonds cap at 3 (`GameState.MaxStat`, `MaxBond`).

UI: all styling lives in `ui/theme.tres`, the project's default theme. Give nodes a type variation from it (`NavyPanel`, `CreamPanel`, `TagOrange`, `KeyCap`, `PrimaryButton`, `ChoiceButton`...) rather than colour or stylebox overrides, so the A6 UI kit is a theme swap. Build new screens as `.tscn` scenes.

Also: `sandbox/` grey-box test scene, `dev/` dev-only helpers, `tests/` xUnit tests for pure logic only.

State and rules: `core/state/` (GameState, SaveData) and `conditions/` are pure C# with no Godot types, so `tests/` can compile them directly. Keep Godot code out of those folders. Read and change state through `Game.State` (`core/Game.cs`); gates use condition strings such as `"guts >= 2 | item:flashlight"` (`conditions/ConditionParser.cs` has the grammar).

Conventions: snake_case folders and scene files, PascalCase C# files, namespace `Pegasus.<Module>`. Game state is plain C# data kept apart from nodes; scenes rebuild from it on load. Grey-box art and placeholder sound until real assets land.

## Commands

- Build: `dotnet build`
- Tests: `dotnet test`
- Run headless: `godot --headless --quit-after 120 [res://scene.tscn]` (main scene is `core/boot.tscn`: continues the save; add `-- --new-game` to start fresh)
- Screenshot: `[SHOT_SIZE=1600x700] scripts/shot.sh out.png [res://scene.tscn] [frames] [extra dev args, e.g. --hold=move_right@1-40 --tap=interact@60]` (see `dev/DevDriver.cs`; `--state=/abs/state.json` starts from a save-format fixture, and each shot prints the player position and state)
- After adding new assets, import once: `godot --headless --import`
