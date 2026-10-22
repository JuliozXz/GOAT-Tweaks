#!/usr/bin/env python3
"""Finds registry values written by actions of different upstream sources (apply side) and emits catalog/conflicts.json.
Same data across sources => "duplicate"; different data => "user-choice" (both cannot be queued together)."""
import json, re, sys
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SRC = Path(sys.argv[1] if len(sys.argv) > 1 else Path.home() / "repos/src")
cat = json.load(open(ROOT / "catalog/catalog.json", encoding="utf-8"))

HIVES = {"HKEY_LOCAL_MACHINE": "HKLM", "HKLM:": "HKLM", "HKEY_CURRENT_USER": "HKCU", "HKCU:": "HKCU", "HKEY_CLASSES_ROOT": "HKCR", "HKEY_USERS": "HKU"}
def norm_path(p):
    p = p.replace("`", "").replace('"', "").replace("Registry::", "").strip().rstrip("\\")
    for k, v in HIVES.items():
        if p.upper().startswith(k.upper()): p = v + p[len(k):]; break
    p = p.replace("HKLM:", "HKLM").replace("HKCU:", "HKCU")
    p = re.sub(r"\\ControlSet00\d\\", r"\\CurrentControlSet\\", p, flags=re.I)
    return re.sub(r"\\+", r"\\", p).lower()
def norm_data(d):
    d = d.replace("`", "").replace('"', "").strip().lower()
    if re.fullmatch(r"0x[0-9a-f]+", d): d = str(int(d, 16))
    return d

REG_ADD = re.compile(r'reg(?:\.exe)?\s+add\s+\\?`?"?(?P<p>[^"`]+?)`?\\?"?\s+/v\s+\\?`?"?(?P<n>[^"`/]+?)`?\\?"?\s+/t\s+(?P<t>\w+)\s+/d\s+\\?`?"?(?P<d>[^"`/\s]*)', re.I)
def from_text(t):
    out = []
    for m in REG_ADD.finditer(t):
        out.append((norm_path(m["p"]), m["n"].strip().lower(), norm_data(m["d"])))
    return out
def from_regfile(t):
    out, key = [], None
    for line in t.splitlines():
        line = line.strip()
        if line.startswith("[") and line.endswith("]"): key = None if line.startswith("[-") else norm_path(line[1:-1])
        elif key and "=" in line and line.startswith('"'):
            n, d = line.split("=", 1)
            d = d.strip()
            if d.lower().startswith("dword:"): d = str(int(d[6:], 16))
            out.append((key, n.strip('"').lower(), norm_data(d)))
    return out

def ultimate_apply(t):
    i = t.find("switch ($choice)")
    if i < 0: return t
    a = re.search(r"\n\s*1 \{", t[i:])
    if not a: return t
    b = re.search(r"\n\s*2 \{", t[i + a.end():])
    return t[i + a.end(): i + a.end() + b.start()] if b else t[i + a.end():]

helper = (SRC / "RyTuneX/Helpers/OptimizeSystemHelper.cs").read_text(encoding="utf-8")
def method_body(name):
    m = re.search(r"static\s+async\s+Task\s+" + name + r"\s*\(", helper)
    if not m: return ""
    i = helper.index("{", m.end()); d = 0
    for j in range(i, len(helper)):
        d += helper[j] == "{"; d -= helper[j] == "}"
        if d == 0: return helper[m.start():j + 1]
    return ""
def cs_unescape(t): return t.replace('\\"', '"').replace("\\\\", "\\")

writes = defaultdict(list)  # (path,name) -> [(item, data)]
for it in cat["items"]:
    src, rows = it["source"], []
    if src == "winutil" and it["kind"] == "toggle": continue  # two-way state switch: user picks the state, never a forced conflict
    if src == "Ultimate":
        rows = from_text(ultimate_apply((SRC / "Ultimate" / it["sourceFile"]).read_text(encoding="utf-8", errors="replace")))
    elif src == "WinSux":
        base = SRC / "WinSux/WinSux"
        rows = from_text("".join((base / f).read_text(encoding="utf-8", errors="replace") for f in ["winsux.ps1", "stepone.ps1", "steptwo.ps1"]))
        rows += from_regfile((base / "reg.reg").read_text(encoding="utf-16" if (base / "reg.reg").read_bytes()[:2] == b"\xff\xfe" else "utf-8", errors="replace"))
    elif src == "RyTuneX" and it.get("methods"):
        rows = from_text(cs_unescape(method_body(it["methods"][0])))
    elif src == "winutil" and it["id"].startswith("winutil:tweak:"):
        key = it["id"].split(":", 2)[2]
        tw = json.loads((SRC / "winutil/config/tweaks.json").read_text(encoding="utf-8"), strict=False)[key]
        rows = [(norm_path(r["Path"]), r["Name"].lower(), norm_data(str(r.get("Value", r.get("value", ""))))) for r in tw.get("registry", []) if "Name" in r]
        rows += from_text(" ".join(tw.get("InvokeScript", [])))
    for p, n, d in set(rows):
        writes[(p, n)].append((it["id"], src, d))

groups = []
for (p, n), ws in sorted(writes.items()):
    if len({s for _, s, _ in ws}) < 2: continue
    datas = {d for _, _, d in ws}
    groups.append({"id": f"reg:{p}\\{n}", "setting": f"{p}\\{n}", "items": sorted({i for i, _, _ in ws}),
                   "values": sorted({f"{i}={d}" for i, _, d in ws}),
                   "decision": "duplicate" if len(datas) == 1 else "user-choice",
                   "note": "Same value written by several sources" if len(datas) == 1 else "Sources write different values; GOAT Tweaks will not queue them together, the user picks one"})
MANUAL = [
 ("exclusive", "winsux:pipeline", ["winsux:pipeline"], "WinSux reboots into Safe Mode and chains stepone/steptwo via RunOnce; nothing else may share its batch"),
 ("user-choice", "Timer resolution", ["rytunex:GlobalTimerResolution", "ultimate:6 Windows/30 Timer Resolution.ps1"], "Both change the system timer resolution mechanism (registry request vs installed service)"),
 ("user-choice", "Game Bar / Game DVR", ["rytunex:GameBar", "ultimate:6 Windows/19 Gamebar.ps1"], "Different key sets; Ultimate also uses TrustedInstaller"),
 ("user-choice", "Windows Update policy", ["rytunex:WindowsUpdates", "ultimate:3 Setup/12 Updates Pause.ps1", "ultimate:2 Refresh/5 Updates Drivers Block.ps1", "winutil:feature:WPFFixesUpdate"], "Incompatible update policies (security-only/manual/disabled vs pause vs driver block vs full reset)"),
 ("user-choice", "Widgets", ["rytunex:Widgets", "winutil:tweak:WPFTweaksWidget", "ultimate:6 Windows/8 Widgets.ps1"], "Disable via policy vs package removal"),
 ("user-choice", "Microsoft Edge", ["winutil:tweak:WPFTweaksRemoveEdge", "winutil:tweak:WPFTweaksEdgeDebloat", "ultimate:6 Windows/20 Edge & WebView.ps1", "ultimate:3 Setup/10 Edge Settings.ps1"], "Removing Edge makes Edge settings/debloat pointless; run one approach"),
 ("user-choice", "Windows Defender", ["ultimate:8 Advanced/1 Defender.ps1", "ultimate:6 Windows/33 Defender Optimize.ps1"], "Off vs optimize write different MitigationOptions/real-time values"),
 ("duplicate", "Hibernation", ["rytunex:Hibernation", "winutil:tweak:WPFTweaksHiber", "ultimate:6 Windows/29 Power Plan.ps1"], "All disable hibernation (Power Plan does much more)"),
 ("duplicate", "Copilot", ["rytunex:CoPilotAI", "ultimate:6 Windows/9 Copilot.ps1", "winutil:appx:WPFAppxMicrosoft_Copilot"], "Disable vs remove package"),
 ("duplicate", "IPv6", ["winutil:tweak:WPFTweaksDisableIPv6", "ultimate:6 Windows/27 Network IPv4 Only.ps1"], "Both turn IPv6 off by different mechanisms"),
 ("duplicate", "Telemetry", ["rytunex:TelemetryServices", "winutil:tweak:WPFTweaksTelemetry"], "Overlapping telemetry services/keys"),
]
ids = {i["id"] for i in cat["items"]}
for dec, setting, its, note in MANUAL:
    missing = [i for i in its if i not in ids]
    assert not missing, missing
    groups.append({"id": "manual:" + setting, "setting": setting, "items": its, "values": [], "decision": dec, "note": note})
json.dump({"groups": groups}, open(ROOT / "catalog/conflicts.lead.json", "w", encoding="utf-8"), indent=1, ensure_ascii=False)
from collections import Counter
print(len(groups), Counter(g["decision"] for g in groups))
for g in [g for g in groups if g["decision"] == "user-choice"][:25]: print(g["setting"], "|", "; ".join(g["values"])[:260])
