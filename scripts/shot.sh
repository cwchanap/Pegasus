#!/usr/bin/env bash
# Render a scene off-screen and save a screenshot (Linux, needs Xvfb).
# Usage: scripts/shot.sh out.png [res://path/scene.tscn] [frames] [dev args...]
set -euo pipefail
out=$(realpath -m "$1"); scene=${2:-}; frames=${3:-90}; shift $(( $# < 3 ? $# : 3 ))
if [ -f /opt/lavapipe/lvp_icd.json ]; then unset VK_DRIVER_FILES; export VK_ICD_FILENAMES=/opt/lavapipe/lvp_icd.json; fi
cd "$(dirname "$0")/.."
xvfb-run -a -s "-screen 0 1280x720x24" godot --rendering-driver vulkan $scene -- --shot="$out" --shot-frames="$frames" "$@"
