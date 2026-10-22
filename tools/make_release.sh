#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GOAT_REPO="${GOAT_REPO:-JuliozXz/GOAT-Tweaks}"
GOAT_VERSION="${GOAT_VERSION:-v2.0.2}"
pwsh -NoProfile -File "$ROOT/tools/Publish-Release.ps1" -Repository "$GOAT_REPO" -Version "$GOAT_VERSION"
