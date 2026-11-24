#!/usr/bin/env bash
# Render a scene off-screen and save a screenshot (Linux, needs Xvfb).
# Usage: [SHOT_SIZE=1600x700] scripts/shot.sh /tmp/out.png [res://path/scene.tscn] [frames] [dev args...]
# Write outside the checkout: a PNG under the project would be imported as a game texture.
set -euo pipefail
out=$(realpath -m "$1"); scene=(${2:+"$2"}); frames=${3:-90}; shift $(( $# < 3 ? $# : 3 ))
if [ -f /opt/lavapipe/lvp_icd.json ]; then unset VK_DRIVER_FILES; export VK_ICD_FILENAMES=/opt/lavapipe/lvp_icd.json; fi
cd "$(dirname "$0")/.."
size=${SHOT_SIZE:-1280x720}
xvfb-run -a -s "-screen 0 ${size}x24" godot --rendering-driver vulkan --resolution "$size" "${scene[@]}" -- --shot="$out" --shot-frames="$frames" "$@"
