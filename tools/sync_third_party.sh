#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
src_root="${1:-$HOME/repos/src}"
third_party="$repo_root/third_party"

declare -A pins=(
    [RyTuneX]=957885f53beaacfa7322581cafdb9eba5df5726a
    [WinSux]=91adbfd63b7ed2eb5fcf0821fd53405a3cf63573
    [Ultimate]=a0a33a57078434201f582b0db5107dcbb502292a
    [winutil]=2f0bdc8fca52e1ebe31a8d98216428061d3f8956
)

for name in "${!pins[@]}"; do
    source="$src_root/$name"
    actual="$(git -C "$source" rev-parse HEAD)"
    if [[ "$actual" != "${pins[$name]}" ]]; then
        printf 'Unexpected %s revision: expected %s, found %s\n' "$name" "${pins[$name]}" "$actual" >&2
        exit 1
    fi
done

mkdir -p "$third_party"

copy_files() {
    local source="$1"
    local target="$2"
    shift 2
    local file rel
    while IFS= read -r -d '' file; do
        rel="${file#"$source"/}"
        mkdir -p "$target/$(dirname "$rel")"
        cp -- "$file" "$target/$rel"
    done < <(find "$source" -type f "$@" -print0)
}

copy_files "$src_root/Ultimate" "$third_party/Ultimate" \
    -not -path "$src_root/Ultimate/.git/*"
copy_files "$src_root/WinSux" "$third_party/WinSux" \
    -not -path "$src_root/WinSux/.git/*"

copy_files "$src_root/RyTuneX" "$third_party/RyTuneX" \
    -not -path "$src_root/RyTuneX/.git/*" \
    \( -name '*.cs' -o -name 'LICENSE.md' \)

copy_files "$src_root/winutil/functions" "$third_party/winutil/functions" \
    -name '*.ps1'
copy_files "$src_root/winutil/config" "$third_party/winutil/config" \
    -name '*.json'
copy_files "$src_root/winutil" "$third_party/winutil" \
    -maxdepth 1 -name 'LICENSE'

cp -- "$src_root/RyTuneX/LICENSE.md" "$repo_root/LICENSE.md"

(
    cd "$third_party"
    find . -type f ! -name SHA256SUMS -print0 |
        sort -z |
        while IFS= read -r -d '' file; do
            sha256sum "$file"
        done > SHA256SUMS
)

printf 'Synchronized pinned upstream files into %s\n' "$third_party"
