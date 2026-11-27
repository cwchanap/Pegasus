# Building fronts

Flat, straight-on paintings of the south face of the grey-box boxes, which is the only face the camera sees. 256 px per metre. Each exterior has a `{name}_summer.png` and a `{name}_snow.png` (3 AM under heavy snow, same layout); the interiors are summer only. Skies are in `content/art/skies/`.

| Front | Size (m) | Pixels | Box it covers |
|---|---|---|---|
| `apartment_block` | 12 × 7 | 3072×1792 | night `Home/Apartment` |
| `university_hall` | 20 × 8 | 5120×2048 | night `University/Hall` |
| `university_gate_pillar` | 0.8 × 3 | 205×768 | night `University/GateL` and `GateR` |
| `drugstore` | 4 × 4 | 1024×1024 | night `Arcade/ShopsN` 0–4 m from the west end; day `ShopA` |
| `kissa_mori` | 4 × 4 | 1024×1024 | `ShopsN` 4–8 m; day `ShopB` |
| `books` | 4 × 4 | 1024×1024 | `ShopsN` 8–12 m; day `ShopC` |
| `winter_gear` | 4 × 4 | 1024×1024 | `ShopsN` 12–16 m; day `ShopD` |
| `shrine_hall` | 5 × 2.6 | 1280×666 | `Shrine/ShrineHall` (night), `Geometry/Hall` (day) |
| `house_row_low` | 8 × 3, tiles | 2048×768 | night `Blocks/HousesSW` (18 m), `MiddleS` (16 m) |
| `house_row_tall` | 8 × 6, tiles | 2048×1536 | night `Blocks/CornerNE` (10 m), `HousesNW` (8 × 5, raise to 6) |
| `apartment_room_wall` | 8 × 3 | 2048×768 | day apartment `Room/BackWall` |
| `university_library_wall` | 8 × 4 | 2048×1024 | day university `BackWall`, west half |
| `university_gym_wall` | 8 × 4 | 2048×1024 | day university `BackWall`, east half |

The day shop boxes are 4 × 3.2 m today; the shopfronts are painted for 4 × 4.

## Where the openings are

Metres from the left edge of the face, and up from the street or floor.

- `apartment_block`: door 5.3–6.7, 0–2.2. Windows 1.8–3.2 and 8.8–10.2, both 3.65–4.75 (`Home/Door`, `Window0`, `Window1`). The left window and the door lamp are lit in the snow version.
- `university_hall`: entrance 8.4–11.6, 0–2.8. The UNIVERSITY plate is 7–13, 3.0–3.8, so the `University/Sign` label is no longer needed.
- `kissa_mori`: door 0.1–1.5, 0–2.2; window 1.55–3.95, 1.0–2.2. In the night scene that puts the door centre at x 38.8 and the window centre at x 40.75; `KissaDoor` (38) and `KissaWindow` (40.5) need to move there. The sign carries the name, so `KissaSign` is no longer needed either. The day version has a hiring notice in the window; the snow version is closed and dark.
- `shrine_hall`: doors 1.7–3.3, 0–2.0 (`Shrine/HallDoor`).
- `apartment_room_wall`: window frame 2.7–4.85, 1.15–2.45, a little larger than the grey-box `Room/Window` (1.6 × 1 m).
- Library and gym walls: the lower 2.4 m is plain wainscot behind the shelves, treadmill and rack.

## Tiling

The house rows and skies repeat left to right. The seam was blended across a plain strip of wall (or sky) at both edges. HousesSW (18 m) and CornerNE (10 m) are not multiples of 8 m, so stretch the repeat a little (9 m or 10 m per tile) rather than ending mid-house.
