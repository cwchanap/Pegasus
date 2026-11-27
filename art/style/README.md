# Snow Dream art style

One look for every image in the game: **clean TV-anime characters over soft, painted anime-film backgrounds.** Every later `[Art]` ticket starts from this folder. Paste the [prompt preamble](#prompt-preamble) first, and pass the matching character sheet to Codex as a reference image.

`art/` has an empty `.gdignore`, so Godot does not import these files. Finished game assets live under `content/`.

## The look

- **Day** (summer Osaka neighbourhood): warm and bright. Cream, sunlit orange, sky blue, leaf green.
- **Night** (the same streets buried in snow at 3 AM): cold navy, ice blue and snow white. Lantern, window and fire light are the only warm colours, so warmth reads as safety. (The red torii and the vending-machine glow are landmarks and may stay warm, but keep them small.)
- Both palettes sit on top of the UI theme (navy panels, cream dialogue boxes, orange tags), so art and UI match.

## Rules

**Characters**

- Clean dark-brown line art (`#3B2418`), even weight, slightly heavier outer contour.
- Two-tone cel shading: one flat base colour, one hard-edged shadow tone, one flat highlight band on hair. No gradients, airbrush or texture noise.
- Large simple anime eyes with two flat highlights, simplified hands.
- About 5 to 6 heads tall so faces still read at game-camera distance. Relative height: Kenta (tallest) > protagonist > Aoi. The three sheets are not drawn at the same scale.
- A clear silhouette that still reads at about 140 px tall. No baked ground shadow (the game draws one).

**Backgrounds and props**

- Painted, soft gradients, detailed skies.
- No hard baked cast shadows; the 3D scenes light them.
- No text, logos or watermarks unless a ticket asks for them.

## Palette

Day, night and cast values are measured from the approved art. Fire orange (not in the frames yet), line brown, skin shade and the UI colours are targets. Swatches are on `style_sheet.png`.

| Day | Hex | Night | Hex |
|---|---|---|---|
| Cream | `#FBF1E3` | Cold navy | `#0F1F46` |
| Plaster | `#F5DBC4` | Dusk blue | `#1F3D7B` |
| Sunlit orange | `#EFA269` | Shade blue | `#34508C` |
| Timber | `#9E7F51` | Ice blue | `#6D90D7` |
| Leaf green | `#6A8C41` | Pale ice | `#99B4ED` |
| Deep leaf | `#3B5B32` | Snow white | `#E6EEFF` |
| Sky blue | `#3D98EA` | Amber glow | `#EDC07F` |
| Haze blue | `#BCD6F4` | Fire orange | `#FF8A3D` |

| Shared | Hex | Cast (measured from the sheets) | Hex |
|---|---|---|---|
| Line brown | `#3B2418` | Protagonist tee | `#F96C3E` |
| Ink navy (UI panel) | `#1C1F3B` | Protagonist down coat | `#AE4333` |
| UI orange (tag) | `#FF5A3C` | Aoi skirt | `#423F54` |
| UI yellow | `#FFD24D` | Aoi hair | `#3D3944` |
| Vermilion (torii, omamori) | `#D8432F` | Kenta jacket | `#2C825B` |
| Skin | `#FBDBC0` | Kenta hair | `#6E4934` |
| Skin shade | `#E8B896` | Khaki trousers | `#D9C098` |

## The cast

- **Protagonist** (no name in the game): 20, ordinary and likeable, a little sleepy-eyed. Short messy black hair with one cowlick (his silhouette mark), navy canvas bag strap across the body. Summer: orange tee, khaki cropped chinos, white sneakers. Down coat: deep-red hip-length puffer with horizontal baffles, cream scarf, dark trousers and boots.
- **Aoi**: 19, the folklore club's only member, earnest and a little guarded. Straight blue-black bob with blunt bangs, white short-sleeve blouse, indigo pleated skirt, cream canvas tote with a notebook and a small red omamori charm. Reserved posture, arms close.
- **Kenta**: 20, loud and friendly, collects Osaka urban legends. Messy spiky chestnut hair, wide grin, green zip jacket with sleeves pushed up over a cream graphic tee, dark cargo shorts, navy high-tops, a sticker-covered sling bag. The bag hangs on his right hip in every view, strap over his left shoulder.

## Files

| File | What |
|---|---|
| `style_sheet.png` | Palettes with hex values, line and shading rules on one prop, day and night mood frames of one street |
| `protagonist_sheet.png` | Front, side, back in summer clothes and in the down coat |
| `aoi_sheet.png` | Front, side, back, and the expressions neutral, smile, worried |
| `kenta_sheet.png` | Same layout as Aoi |
| `parts/sample_prop.png` | Stone lantern: line, base, shade, final (used in the style sheet) |
| `parts/mood_day.png`, `parts/mood_night.png` | The two mood frames (used in the style sheet) |

## Game assets (step 2)

Finished images live in `content/characters/`, and the game shows them (the grey-box placeholders are gone). Their `.import` files turn mipmaps on and keep the PNGs lossless: the game draws sprites at about a quarter of their size and portraits at a quarter at 720p, and line art shimmers without mipmaps. For a new character image, let Godot write its `.import` file (open the editor or run `godot --headless --import`), then set the same values in it: `mipmaps/generate=true`, `compress/mode=0` and `detect_3d/compress_to=0`. Don't copy another image's `.import` file, which would duplicate its UID.

**World sprites:** `player_{summer|coat}_{front|back|side}.png` and `{aoi|kenta}_{front|side}.png`.

- 256×512, transparent. The feet end on row 500 (12 px margin) and are centred horizontally. Side views face right; the game flips them for left. No ground shadow.
- Cut straight from the approved character sheets (background removed, then scaled), so faces and outfits are identical to the sheets.
- The cut leaves a pale cream halo outside the dark line. It is cleaned up so the sprites sit on the navy night backdrop: edge pixels outside the line take the line colour and fade by how much of the line they cover, and stray low-alpha specks are cleared. Do the same for any sprite cut from a light sheet.
- Figure heights follow the cast, so one pixel size keeps their relative heights: Kenta 484 px (hair spikes included), protagonist 468, Aoi 452.
- At 1280×720 a 512 px canvas is about 136 px tall.
- Aoi's tote sits on her left in the front and back views and on the near side in her side view. The game flips side sprites, so nobody will notice.

**Dialogue portraits:** `{aoi|kenta}_portrait_{neutral|smile|worried}.png`.

- 1000×2240, transparent, waist-up, facing the viewer. The game shows them at 250×560, bottom-left of the dialogue box.
- Made with Codex from the character sheets: neutral first, then smile and worried as expression-only edits of the neutral one. Codex's image tool would not make 1000×2240, so each came out at 838×1877 (same ratio) and was scaled to 94% of the canvas width and bottom-anchored, leaving a side margin.
- Framing: crown at about 17 to 18% from the top, chin at about 51% (Aoi) and 47% (Kenta), widest row at 94% of the width.

## Scenes, screens and icons (step 3)

Puzzle close-ups live in `content/art/closeups/{lanterns|tablet|cafe}/`, everything else in `content/art/ui/`. The game shows them all. Their `.import` files keep them lossless with mipmaps on (`mipmaps/generate=true`, `detect_3d/compress_to=0`), and the project draws 2D textures linear with mipmaps, so a 512 px icon stays clean at 26 px. Set the same for any new image here.

**Close-up views:** `lanterns/lantern_path.png`, `tablet/main_hall.png`, `cafe/{counter|stove|door|booth}.png`.

- 2172×1044, which is 2× the 1086×522 view area of the close-up. View coordinates in a puzzle scene × 2 give pixels.
- Painted over a clean guide rendered from the view's ColorRects in its `.tscn` (no UI outlines), so every object sits under its hotspot. The style came from `parts/mood_night.png`, and the three later Kissa Mori views also got the finished counter so the room stays the same room.
- Codex returned 1808×870 (its image tool stops near 1.57 megapixels), scaled up with Lanczos.

**Overlays:** full-canvas transparent PNGs the size of their view, drawn straight over it.

- Lanterns: `lantern_glow_{1..5}.png`, one per lit lantern, each kept to its own lantern.
- Stove, in drawing order: `stove.png` (door shut), `stove_open.png` (door open, empty firebox), `stove_kindling.png` (paper, sticks and charcoal, only until it is lit), `stove_lit.png` (the fire and the warm room, drawn over the open state; it covers the whole view).
- Made by asking Codex to edit the finished base ("change nothing else"), then solving each pixel for the least alpha that turns the base into the edit, inside a soft box around the change. Each overlay is relative to the state below it, so kindling and lit were edits of the open state.

**Cut-outs:** square, transparent, 2× their square in the scene.

- `tablet/glyph_{gate|cold|sleep|fire|wake}.png`: 116×116 carved stone tiles. Each is a 58 square centred on its 130×58 sign button: top-left at (514, 100 + 62 × index) in view units, centre at (543, 129 + 62 × index).
- `cafe/dial_{gate|cold|sleep|fire|wake}.png`: 112×112 brass dial faces covering the three 56-square dial buttons, whose top-left corners are at x 460, 524 and 588, y 194 in view units. The pointer notch is at the top.
- The glyph strokes follow the scene's stroke rectangles.

**Screens:** 2560×1440 (Codex returned 1672×941).

- `title.png`: the protagonist at his window at 3 AM. The left third is calm for the logo and menu.
- `logo.png`: the transparent "Snow Dream" wordmark, trimmed to its edges plus a 24 px margin.
- `city_map.png`: follows the map drawing (river, three roads, five blocks), with each landmark under its marker and no text. One change: the shrine's park moved off the river into the grid cell south of the arcade, at (567, 336) to (800, 456) in map coordinates, and the river runs straight. The shrine marker sits at (685, 395).
- `end_card.png`: Kissa Mori with the stove lit, the window frost melting, and a figure outside under a dead streetlight. The centre stays dark for the text and button.

**Icons:** `ui/icons/items/{item id}.png`, `ui/icons/stats/{knowledge|guts|fitness}.png`, `ui/icons/hud/{warmth|bike_light}.png`.

- 512×512, transparent. The icon fills 87.5% on its longer side, centred.
- Thick dark outline with a cream rim, so they read on both the navy and the cream panels. Checked at 48 and 96 px.

**Codex notes for this batch:**

- When two runs share the machine, Codex can copy the other run's image. Take the file from that run's own `~/.codex/generated_images/<session id>/` folder.
- Ask for 1808×870 or 1672×941 and scale up; it does not make exact sizes.

## Fronts, skies and prop references (step 4)

Building fronts live in `content/art/fronts/` (its README lists each front's size, box and openings), skies in `content/art/skies/`, and the Tripo Studio prop references in `art/props/`.

- Fronts are flat, straight-on, evenly lit, at 256 px per metre of the box face. Fronts with doors or windows that the scene uses were painted over a guide of flat blocks at the right size and place, with `parts/mood_day.png` as the style reference.
- Each snow front is an edit of the summer one ("change nothing else"), with `parts/mood_night.png` as a second reference, so the layout matches.
- Tiling fronts and skies asked Codex for a plain strip at both edges; the last 7% (8% for skies) was cross-faded into the first and dropped, so the image repeats. Skies came at 3:1 and were cropped from the top to 4:1.
- Prop references are 2048×2048 on light grey, three-quarter front from above, flat colour with no line art, painted like the BASE column of `parts/sample_prop.png`. Side views are edits of the three-quarter view and show the same side. The three pilot props went in as `-i` references for the other 13 so the set matches. The models and their size and face budgets are listed in `content/models/props/README.md`.

**Codex notes for this batch:**

- The image tool keeps to about 1.57 MP and 3:1 at most, so the pillar (1:3.75) was painted on a 1:3 canvas with grey either side and cropped.
- An edit comes back in the input's aspect ratio only when that ratio is one the tool likes; for the pillar, pad the input to a wider canvas first.
- A straight side view of a torii needs spelling out (one pillar, a T on top, nothing sticking out), or the beam ends come out lopsided.
- In a side view of the right side, the front is on the right of the image. The bicycle's prompt put the basket on the left, which drew a mirrored bike, so the image was flipped (it has no text).

## Prompt preamble

Paste this block first in every Codex art prompt, then the item-specific text.

```text
Style: clean TV-anime character art over soft, painted anime-film backgrounds, for a 2.5D game set in a summer Osaka neighbourhood that is buried in snow at 3 AM. Characters: clean dark-brown line art (#3B2418), even line weight with a slightly heavier outer contour, two-tone cel shading (one flat base colour plus one hard-edged shadow tone, one flat highlight band on hair), large simple anime eyes with two flat highlights, simplified hands, about 5 to 6 heads tall, a clear silhouette that still reads at 140 px tall. No gradients, airbrush or texture noise on characters, no baked ground shadow. Backgrounds and props: painted, soft gradients, detailed skies, no hard baked cast shadows (the game lights the scene). No text, logos or watermarks unless the prompt asks for specific short labels. Day palette (warm and bright): cream #FBF1E3, plaster #F5DBC4, sunlit orange #EFA269, timber #9E7F51, leaf green #6A8C41, deep leaf #3B5B32, sky blue #3D98EA, haze blue #BCD6F4. Night palette (cold, the same streets under heavy snow at 3 AM): cold navy #0F1F46, dusk blue #1F3D7B, shade blue #34508C, ice blue #6D90D7, pale ice #99B4ED, snow white #E6EEFF; amber #EDC07F and fire orange #FF8A3D (lantern, window and fire light) are the only warm colours at night, so warmth reads as safety; the only exceptions are two small warm landmarks, the red torii (vermilion #D8432F) and the vending-machine glow. UI match: navy #1C1F3B panels, cream #FBF1E3 dialogue boxes, orange #FF5A3C tags, yellow #FFD24D accents.
```

## Running Codex

This is how the sheets were made:

```sh
REPO=~/path/to/pegasus   # your checkout
codex exec --model gpt-6-luna -c model_reasoning_effort=xhigh -s workspace-write -C "$REPO" -i reference.png -o last.txt - < prompt.txt
```

Drop `-i reference.png` when there is no reference image.

- The prompt is a short header (generate one image with the built-in image tool, do not draw it with code, save it as a PNG at this path), then the preamble, then the item brief.
- Codex's built-in `image_gen` tool writes to `~/.codex/generated_images`, and Codex copies the result to the path you named. No resizing happens, so check the pixel size.
- To keep a character on model, pass its sheet with `-i` and say to match it exactly. Style sheet, mood frames and sheets for new characters should reference an existing sheet for line and shading.
- The image tool returns true transparent PNGs when asked for a transparent background, and it will not make exactly 1000×2240; it gave 838×1877. Tell Codex to call the image tool exactly once, or it keeps retrying on its own.
- Look at every image before committing it. Retry only for real defects (wrong layout, missing limbs, garbled labels, off-palette colours, stray text).
