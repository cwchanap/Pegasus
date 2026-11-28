# Props

3D props made in Tripo Studio from the references in `art/props/` (`{name}.png`, plus `{name}_side.png` where a side view helps). The references are painted like props in a Japanese anime film background, to match the building fronts: real proportions, painted detail and gentle wear, no outline.

Each `{name}.glb` here is Y up, with its base centre on Y = 0, facing +Z (toward the camera), in metres, and has one base colour texture (1024 px JPEG) and no baked lighting.

Tripo exports every model about 1 unit across, with 4096 px base colour, normal and roughness maps. Each export here was scaled uniformly to the size in the table and moved so its base centre sits on the origin. Only its base colour was kept, at 1024 px, because the toon shader uses nothing else. The raw exports are not in the repo.

| Prop | Used in | Target size (m) | Face budget | GLB triangles | GLB size (m, W × H × D) |
|---|---|---|---|---|---|
| `torii` | Shrine | 3.2 × 3.7 high | 3,000 | 2,150 | 3.86 × 3.70 × 0.59 |
| `stone_lantern` | Shrine (5 times) | 0.7 × 1.6 high | 1,500 | 1,398 | 0.77 × 1.60 × 0.84 |
| `offering_box` | Shrine | 1.0 × 0.8 high | 800 | 984 | 1.00 × 0.73 × 0.93 |
| `cedar_tree` | Shrine, streets | 2 × 5 high | 3,000 | to come | |
| `vending_machine` | Streets | 1.0 × 1.8 high | 1,500 | 4,132 | 1.03 × 1.80 × 1.01 |
| `street_lamp` | Streets | 0.4 × 4.0 high | 800 | 738 | 1.23 × 4.00 × 1.12 |
| `utility_pole` | Streets (no wires) | 0.4 × 8.0 high | 1,500 | 1,164 | 1.02 × 8.00 × 2.31 |
| `bicycle` | Streets, apartment | 1.7 long × 1.0 high | 3,000 | to come | |
| `bench` | Streets, shrine | 1.6 × 0.8 high | 800 | 890 | 1.60 × 0.95 × 0.81 |
| `post_box` | Streets | 0.5 × 1.3 high | 800 | 1,486 | 0.56 × 1.30 × 0.62 |
| `shop_counter` | Arcade | 2.0 × 1.0 high | 1,000 | 748 | 2.00 × 1.03 × 0.90 |
| `bed` | Apartment | 2.0 long × 1.0 wide, 0.6 high | 1,500 | 1,768 | 1.17 × 0.74 × 2.00 |
| `desk` | Apartment (with its lamp) | 1.2 × 1.3 high | 1,500 | 3,819 | 1.30 × 1.23 × 0.83 |
| `bookshelf` | Apartment, library | 1.0 × 1.8 high | 1,500 | 4,723 | 0.98 × 1.80 × 0.50 |
| `treadmill` | University gym | 1.8 long × 1.4 high | 2,000 | 1,258 | 0.85 × 1.29 × 1.80 |
| `weight_rack` | University gym | 1.4 × 1.4 high | 2,000 | 4,462 | 1.40 × 1.24 × 0.93 |

Notes for placing them:

- **Scale:** tall props were scaled by height and long or low ones by length: offering box, bench, shop counter and weight rack by width, bed and treadmill by length. The desk was scaled so its top sits at 0.73 m (lamp top 1.23 m), the bench seat lands at 0.46 m, and the counter top at 1.03 m.
- **Facing:** every prop shows the face from its reference image toward +Z. The bed and the treadmill run along Z, with the foot of the bed and the back of the belt toward the camera; turn them 90° to lie along a wall. The utility pole's cross-arm runs along Z.
- **Triangles:** Tripo counts faces as quads, so a few models came out at 2 to 3 times the triangle budget (vending machine, desk, bookshelf, weight rack). The 14 together are about 30,000 triangles, and the shrine's torii, five lanterns, offering box and bench about 11,000, so none were reduced.

Side views: `torii_side`, `vending_machine_side`, `bicycle_side`.
