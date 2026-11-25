#!/usr/bin/env bash
# On-demand smoke route (Linux, needs Xvfb), not part of CI: drives the real scenes with the dev
# driver and asserts on the final scene and state. Fixtures in tests/smoke/ never write the save.
#  1. Kissa Mori with the stove open and loaded: go to the Stove wall, pick the matchbox, use it
#     on the firebox, talk through to the Ending; expects the end card and the `ended` flag.
#  2. An ended save: entering the world shows the end card again.
# Click positions are in the 1280x720 viewport; move them if the close-up layout changes.
set -uo pipefail
cd "$(dirname "$0")/.."
out=$(mktemp -d)
fail=0

run() { # name, expected scene, expected flag, shot args...
  local name=$1 scene=$2 flag=$3; shift 3
  local log
  log=$(SHOT_SIZE=1280x720 scripts/shot.sh "$out/$name.png" "$@" 2>&1)
  if grep -q "^Scene: $scene$" <<<"$log" && grep -q "^State: .*\"$flag\"" <<<"$log"; then
    echo "PASS $name"
  else
    echo "FAIL $name (wanted $scene with $flag)"; grep -E "^(Scene|State):" <<<"$log"; fail=1
  fi
}

run cafe_to_ending res://ui/ending/end_card.tscn ended \
  res://night/snow_area/snow_area.tscn 900 --state="$PWD/tests/smoke/cafe_stove_ready.json" \
  --open=res://puzzles/cafe/cafe.tscn --talk --click=1232,320@80 --click=486,652@100 --click=647,420@115
run ended_save res://ui/ending/end_card.tscn ended \
  "" 60 --state="$PWD/tests/smoke/ended.json"

rm -rf "$out"
exit $fail
