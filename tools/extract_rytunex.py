#!/usr/bin/env python3

from pathlib import Path
import re


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "third_party/RyTuneX/Helpers/OptimizationOptions.cs"
OUTPUT = ROOT / "src/GoatTweaks.Runner/RyTuneX/OptimizationOptions.Extracted.cs"
SIGNATURES = (
    "internal static async Task<int> StartInCmd(string command)",
    "internal static async Task ExecuteToggleActionAsync(string? tag, bool isOn, CancellationToken ct = default)",
)


def read_source(path):
    with path.open("r", encoding="utf-8-sig", newline="") as stream:
        return stream.read()


def skip_string(source, start):
    index = start
    while index < len(source) and source[index] in "$@":
        index += 1
    if index >= len(source) or source[index] != '"':
        return None

    verbatim = "@" in source[start:index]
    quote_count = 1
    while index + quote_count < len(source) and source[index + quote_count] == '"':
        quote_count += 1
    if quote_count >= 3:
        delimiter = '"' * quote_count
        end = source.find(delimiter, index + quote_count)
        if end < 0:
            raise ValueError("Unterminated raw string literal")
        return end + quote_count

    index += 1
    while index < len(source):
        if verbatim and source.startswith('""', index):
            index += 2
        elif source[index] == '"':
            return index + 1
        elif not verbatim and source[index] == "\\":
            index += 2
        else:
            index += 1
    raise ValueError("Unterminated string literal")


def find_method_end(source, opening_brace):
    depth = 0
    index = opening_brace
    while index < len(source):
        if source.startswith("//", index):
            end = source.find("\n", index + 2)
            index = len(source) if end < 0 else end + 1
        elif source.startswith("/*", index):
            end = source.find("*/", index + 2)
            if end < 0:
                raise ValueError("Unterminated block comment")
            index = end + 2
        elif source[index] in "$@":
            end = skip_string(source, index)
            index = end if end is not None else index + 1
        elif source[index] == '"':
            end = skip_string(source, index)
            index = end
        elif source[index] == "'":
            index += 1
            while index < len(source):
                if source[index] == "\\":
                    index += 2
                elif source[index] == "'":
                    index += 1
                    break
                else:
                    index += 1
        elif source[index] == "{":
            depth += 1
            index += 1
        elif source[index] == "}":
            depth -= 1
            index += 1
            if depth == 0:
                return index
        else:
            index += 1
    raise ValueError("Could not find matching method brace")


def extract(source, signature):
    start = source.index(signature)
    brace = source.index("{", start + len(signature))
    end = find_method_end(source, brace)
    return source[start:end]


def main():
    source = read_source(SOURCE)
    namespace_match = re.search(r"^namespace\s+([\w.]+)\s*;", source, re.MULTILINE)
    if namespace_match is None:
        raise ValueError(f"Could not find file-scoped namespace in {SOURCE}")
    methods = [extract(source, signature) for signature in SIGNATURES]

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    generated = (
        "using System;\n"
        "using System.Diagnostics;\n"
        "using System.IO;\n"
        "using System.Threading;\n"
        "using System.Threading.Tasks;\n\n"
        f"namespace {namespace_match.group(1)};\n\n"
        "internal static partial class OptimizationOptions\n"
        "{\n"
        + "\n\n".join(methods)
        + "\n}\n"
    )
    OUTPUT.write_text(generated, encoding="utf-8", newline="")
    lengths = ", ".join(
        f"{name} ({len(method)} chars)"
        for name, method in zip(("StartInCmd", "ExecuteToggleActionAsync"), methods)
    )
    print(f"Extracted {lengths} from {SOURCE.relative_to(ROOT)}")
    print(f"Wrote {OUTPUT.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
