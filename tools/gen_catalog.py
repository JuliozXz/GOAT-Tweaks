#!/usr/bin/env python3
"""Builds catalog.json (every GOAT Tweaks action + its exact upstream origin) from the four pinned upstream checkouts."""
import hashlib, json, re, subprocess, sys
from pathlib import Path

SRC = Path(sys.argv[1] if len(sys.argv) > 1 else Path.home() / "repos/src")
OUT = Path(sys.argv[2] if len(sys.argv) > 2 else Path(__file__).resolve().parent.parent / "catalog/catalog.json")

def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def rev(repo): return subprocess.check_output(["git", "-C", str(SRC / repo), "rev-parse", "HEAD"], text=True).strip()
def jload(p): return json.loads(p.read_text(encoding="utf-8-sig"), strict=False)

FLAGS = {
    "registry": r"\breg(\.exe)?\s+(add|delete|import)|Set-ItemProperty|New-ItemProperty|Remove-ItemProperty|Registry::",
    "services": r"\bsc(\.exe)?\s+(config|stop|start|delete)|Set-Service|Stop-Service|New-Service",
    "boot": r"\bbcdedit\b",
    "power": r"\bpowercfg\b",
    "drivers": r"\bpnputil\b|Display Driver Uninstaller|\bDDU\b|\.inf\b",
    "network": r"\bnetsh\b|NetAdapter|Set-DnsClient|Disable-NetAdapterBinding",
    "security": r"Defender|EnableLUA|SmartScreen|HypervisorEnforcedCodeIntegrity|VulnerableDriverBlocklist|Set-MpPreference|FeatureSettingsOverride|\bnx\b|firewall",
    "updates": r"wuauserv|WindowsUpdate|UsoSvc|WaaSMedicSvc",
    "appx": r"Remove-AppxPackage|Remove-AppxProvisionedPackage",
    "features": r"Disable-WindowsOptionalFeature|Enable-WindowsOptionalFeature|dism",
    "fileDeletion": r"Remove-Item|\bdel\b|\brd\b|rmdir",
    "reboot": r"Restart-Computer|shutdown(\.exe)?\s+/r|shutdown -r",
    "trustedInstaller": r"function Run-Trusted|Run-Trusted ",
    "bitlocker": r"manage-bde|BitLocker",
}
def flags(text): return sorted(k for k, rx in FLAGS.items() if re.search(rx, text, re.I))
def urls(text): return sorted(set(u.rstrip("\"'`),;") for u in re.findall(r"https?://[^\s\"'`<>]+", text)))

def risk(fl, extra=""):
    high = {"boot", "security", "drivers", "trustedInstaller", "bitlocker", "updates"}
    if set(fl) & high or re.search(r"Factory|Reinstall|Autounattend|Convert|Activation|Keys", extra, re.I): return "high"
    if set(fl) & {"services", "appx", "features", "fileDeletion", "network", "power"}: return "medium"
    return "low"

items = []

# ---------------- Ultimate ----------------
U = SRC / "Ultimate"
for f in sorted(U.glob("[0-9] */*.ps1"), key=lambda p: (p.parent.name, int(p.name.split()[0]))):
    t = f.read_text(encoding="utf-8", errors="replace")
    rel = f.relative_to(U).as_posix()
    opts = [(int(n), s.strip().rstrip("\\n").replace("`n", "").strip())
            for n, s in re.findall(r'Write-Host\s+"(\d+)\.\s+([^"]*)"', t)]
    seen, menu = set(), []
    for n, s in opts:
        if n not in seen: seen.add(n); menu.append({"key": str(n), "label": s})
    looping = bool(re.search(r"function show-menu", t))
    exit_key = next((o["key"] for o in menu if o["label"].lower() == "exit"), None)
    free = [m for m in re.findall(r'Read-Host\s+(?:-Prompt\s+)?"([^"]*)"', t) if m.strip()]
    fl = flags(t)
    admin = "IsInRole" in t
    items.append({
        "id": "ultimate:" + rel, "source": "Ultimate", "sourceFile": rel, "sha256": sha(f),
        "category": f.parent.name, "title": re.sub(r"^\d+\s+", "", f.stem),
        "kind": "menu" if menu else "oneshot", "looping": looping, "exitKey": exit_key,
        "options": [o for o in menu if o["key"] != exit_key],
        "freeInputs": free, "requiresAdmin": admin, "flags": fl, "downloads": urls(t),
        "needsInternet": bool(urls(t)) and bool(re.search(r"Invoke-WebRequest|Get-FileFromWeb|Start-BitsTransfer|DownloadFile|\biwr\b", t)),
        "revert": next((f"option {o['key']} ({o['label']})" for o in menu if re.search(r"default|on\b|enable", o["label"], re.I) and not re.search(r"recommended", o["label"], re.I)), None),
        "risk": risk(fl, f.stem),
    })

# ---------------- WinSux ----------------
W = SRC / "WinSux/WinSux"
st = (W / "steptwo.ps1").read_text(encoding="utf-8", errors="replace")
sections = re.findall(r'Write-Host "([A-Z ]+)`n"', st)
alltext = "".join((W / n).read_text(encoding="utf-8", errors="replace") for n in ["winsux.ps1", "stepone.ps1", "steptwo.ps1"])
items.append({
    "id": "winsux:pipeline", "source": "WinSux", "sourceFile": "WinSux/winsux.ps1", "sha256": sha(W / "winsux.ps1"),
    "relatedFiles": {n: sha(W / n) for n in ["stepone.ps1", "steptwo.ps1", "reg.reg", "settimerresolutionservice.cs", "start2.txt"]},
    "category": "WinSux", "title": "WinSux (full pipeline)", "kind": "pipeline", "options": [], "freeInputs": [],
    "requiresAdmin": True, "flags": flags(alltext), "downloads": urls(alltext), "needsInternet": True,
    "steptwoSections": sections, "revert": None, "risk": "high",
})

# ---------------- winutil ----------------
C = SRC / "winutil/config"
tw = jload(C / "tweaks.json")
for k, v in tw.items():
    txt = json.dumps(v)
    has_undo = bool(v.get("UndoScript")) or any("OriginalValue" in r for r in v.get("registry", [])) or any("OriginalType" in s for s in v.get("service", []))
    items.append({
        "id": "winutil:tweak:" + k, "source": "winutil", "sourceFile": "config/tweaks.json#" + k, "category": v.get("category", ""),
        "title": v.get("Content", k), "description": v.get("Description", ""), "kind": "toggle" if v.get("Type") == "Toggle" else ("combo" if v.get("ComboItems") else "tweak"),
        "options": [], "freeInputs": [], "requiresAdmin": True, "flags": flags(txt + " ".join(v.get("InvokeScript", []))) + (["registry"] if v.get("registry") else []) + (["services"] if v.get("service") else []),
        "downloads": urls(txt), "revert": "winutil undo (OriginalValue/OriginalType/UndoScript)" if has_undo else None, "risk": "high" if "Advanced" in v.get("category", "") else "medium",
    })
for k, v in jload(C / "feature.json").items():
    items.append({"id": "winutil:feature:" + k, "source": "winutil", "sourceFile": "config/feature.json#" + k, "category": v.get("category", "Features"),
                  "title": v.get("Content", k), "description": v.get("Description", ""), "kind": "feature" if v.get("feature") else "fix",
                  "options": [], "freeInputs": [], "requiresAdmin": True, "flags": flags(json.dumps(v)) + (["features"] if v.get("feature") else []), "downloads": urls(json.dumps(v)),
                  "revert": "Disable-WindowsOptionalFeature (manual)" if v.get("feature") else None, "risk": "medium"})
for k, v in jload(C / "appx.json").items():
    items.append({"id": "winutil:appx:" + k, "source": "winutil", "sourceFile": "config/appx.json#" + k, "category": "AppX removal", "title": v.get("Content", k),
                  "description": v.get("Description", ""), "kind": "appx", "packageId": v.get("PackageId"), "storeId": v.get("StoreId"), "options": [], "freeInputs": [],
                  "requiresAdmin": True, "flags": ["appx"], "downloads": [], "revert": "reinstall from Microsoft Store (StoreId)" if v.get("StoreId") else None, "risk": "medium"})
for k, v in jload(C / "dns.json").items():
    items.append({"id": "winutil:dns:" + k, "source": "winutil", "sourceFile": "config/dns.json#" + k, "category": "DNS", "title": k, "kind": "dns", "dns": v,
                  "options": [], "freeInputs": [], "requiresAdmin": True, "flags": ["network"], "downloads": [], "revert": "DNS 'Default' (DHCP)", "risk": "medium"})
for k, v in jload(C / "applications.json").items():
    items.append({"id": "winutil:app:" + k, "source": "winutil", "sourceFile": "config/applications.json#" + k, "category": "Install: " + v.get("category", ""),
                  "title": v.get("content", k), "description": v.get("description", ""), "kind": "app", "winget": v.get("winget"), "choco": v.get("choco"), "link": v.get("link"),
                  "options": [], "freeInputs": [], "requiresAdmin": False, "flags": [], "downloads": [], "needsInternet": True, "revert": "uninstall via winget/choco", "risk": "low"})
presets = jload(C / "preset.json")

# ---------------- RyTuneX (toggles) ----------------
R = SRC / "RyTuneX"
res = {}
for m in re.finditer(r'<data name="([^"]+)"[^>]*>\s*<value>(.*?)</value>', (R / "Strings/en-us/Resources.resw").read_text(encoding="utf-8"), re.S):
    res[m.group(1)] = m.group(2)
oo = (R / "Helpers/OptimizationOptions.cs").read_text(encoding="utf-8")
sw = oo[oo.index("internal static async Task ExecuteToggleActionAsync"):]
cases = {}
for m in re.finditer(r'case "([^"]+)":(.*?)break;', sw, re.S):
    cases.setdefault(m.group(1), re.findall(r"OptimizeSystemHelper\.(\w+)\(", m.group(2)))
helper = (R / "Helpers/OptimizeSystemHelper.cs").read_text(encoding="utf-8")
def method_body(name):
    m = re.search(r"static\s+async\s+Task\s+" + name + r"\s*\(", helper)
    if not m: return ""
    i = helper.index("{", m.end()); d = 0
    for j in range(i, len(helper)):
        d += helper[j] == "{"; d -= helper[j] == "}"
        if d == 0: return helper[m.start():j + 1]
    return ""
for page in ["OptimizeSystemPage", "FeaturesPage", "PrivacyPage"]:
    x = (R / f"Views/{page}.xaml").read_text(encoding="utf-8")
    for m in re.finditer(r'<controls:SettingsCard x:Uid="([^"]+)"(?:(?!</controls:SettingsCard>).)*?<ToggleSwitch[^>]*Tag="([^"]+)"', x, re.S):
        uid, tag = m.group(1), m.group(2)
        meths = cases.get(tag, [])
        body = "\n".join(method_body(n) for n in meths)
        items.append({"id": "rytunex:" + tag, "source": "RyTuneX", "sourceFile": f"Views/{page}.xaml + Helpers/OptimizationOptions.cs#ExecuteToggleActionAsync",
                      "category": page.replace("Page", ""), "title": res.get(uid + ".Header", tag), "description": res.get(uid + ".Description", ""),
                      "kind": "toggle", "tag": tag, "methods": meths, "dispatchFound": tag in cases, "options": [], "freeInputs": [],
                      "requiresAdmin": True, "flags": flags(body), "downloads": urls(body), "revert": "toggle off (opposite method)" if len(meths) >= 2 else None,
                      "risk": risk(flags(body))})

for m in re.finditer(r'case "(Default|Security|Manually|Disabled)":\s*await OptimizeSystemHelper\.(\w+)\(', oo):
    pass
wu = dict(re.findall(r'case "(Default|Security|Manually|Disabled)":\s*await OptimizeSystemHelper\.(SetWindowsUpdates\w+)\(', oo))
wu.setdefault("Default", "SetWindowsUpdatesDefault")
wubody = "\n".join(method_body(v) for v in wu.values())
items.append({"id": "rytunex:WindowsUpdates", "source": "RyTuneX", "sourceFile": "Views/OptimizeSystemPage.xaml (ComboBox) + Helpers/OptimizeSystemHelper.cs",
              "category": "OptimizeSystem", "title": res.get("Feature_WindowsUpdates.Header", "Windows Updates"), "description": res.get("Feature_WindowsUpdates.Description", ""),
              "kind": "combo", "tag": "WindowsUpdates", "methods": [wu[k] for k in ["Default", "Security", "Manually", "Disabled"] if k in wu],
              "options": [{"key": k, "label": res.get("Feature_WindowsUpdates_" + k + ".Content", k)} for k in ["Default", "Security", "Manually", "Disabled"]],
              "freeInputs": [], "requiresAdmin": True, "flags": flags(wubody), "downloads": [], "revert": "option Default", "risk": "high", "dispatchFound": True})
# ---------------- winutil Updates tab (public functions, no UI deps) ----------------
for fn, title in [("Invoke-WPFUpdatesdefault", "Updates - Default"), ("Invoke-WPFUpdatessecurity", "Updates - Security (Recommended)"), ("Invoke-WPFUpdatesdisable", "Updates - Disable All")]:
    fp = SRC / "winutil/functions/public" / (fn + ".ps1")
    t = fp.read_text(encoding="utf-8")
    items.append({"id": "winutil:updates:" + fn, "source": "winutil", "sourceFile": "functions/public/" + fn + ".ps1", "sha256": sha(fp), "category": "Updates",
                  "title": title, "description": "", "kind": "function", "function": fn, "options": [], "freeInputs": [], "requiresAdmin": True,
                  "flags": flags(t), "downloads": urls(t), "revert": None if fn == "Invoke-WPFUpdatesdefault" else "winutil:updates:Invoke-WPFUpdatesdefault", "risk": "high"})

# ---------------- RyTuneX tool buttons: exact literals pinned by file:line ----------------
V = R / "Views"
def lit(rel, line, rx):
    text = (R / rel).read_text(encoding="utf-8").split("\n")[line - 1]
    m = re.search(rx, text)
    assert m, (rel, line, text)
    return cs_unescape_lit(m.group(1))
def cs_unescape_lit(x): return x.replace('\\"', '"').replace("\\\\", "\\")
SIC = r'StartInCmd\(\$?"((?:[^"\\]|\\.)*)"\)'
ARG = r'Arguments = "((?:[^"\\]|\\.)*)"'
tools = [
 ("FlushDNS", "Network", "Flush DNS cache", "StartInCmd", [("Views/NetworkPage.xaml.cs", 439, SIC)], False, "low", None),
 ("ReleaseRenewIP", "Network", "Release / renew IP", "StartInCmd", [("Views/NetworkPage.xaml.cs", 464, SIC), ("Views/NetworkPage.xaml.cs", 465, SIC)], True, "medium", None),
 ("ResetTCPIP", "Network", "Reset TCP/IP and Winsock", "StartInCmd", [("Views/NetworkPage.xaml.cs", 492, SIC), ("Views/NetworkPage.xaml.cs", 493, SIC)], True, "high", None),
 ("ResetDefaultDNS", "Network", "Reset DNS to DHCP (adapter)", "StartInCmd", [("Views/NetworkPage.xaml.cs", 409, SIC), ("Views/NetworkPage.xaml.cs", 410, SIC)], True, "medium", None),
 ("MemoryDiagRestart", "Repair", "Windows Memory Diagnostic (restart now)", "StartInCmd", [("Views/RepairPage.xaml.cs", 995, SIC)], True, "high", None),
 ("MemoryDiagNextBoot", "Repair", "Windows Memory Diagnostic (next boot)", "StartInCmd", [("Views/RepairPage.xaml.cs", 1001, SIC)], True, "medium", None),
 ("DefenderQuickScan", "Security", "Microsoft Defender quick scan", "Process", [("Views/SecurityPage.xaml.cs", 540, ARG)], True, "low", None),
 ("DefenderUpdateSignatures", "Security", "Update Defender signatures", "Process", [("Views/SecurityPage.xaml.cs", 584, ARG)], True, "low", None),
]
for tid, cat_, title, launcher, refs, admin, rk, rev_ in tools:
    cmds = [lit(f, n, rx) for f, n, rx in refs]
    item = {"id": "rytunex:tool:" + tid, "source": "RyTuneX", "sourceFile": "; ".join(f"{f}:{n}" for f, n, _ in refs), "category": cat_, "title": title, "description": "",
            "kind": "command", "launcher": launcher, "commands": cmds, "options": [], "freeInputs": [], "requiresAdmin": admin,
            "flags": flags(" ".join(cmds)), "downloads": [], "revert": rev_, "risk": rk}
    if launcher == "Process": item["fileName"] = "powershell.exe"
    if tid == "ResetDefaultDNS": item["freeInputs"] = ["nic"]; item["inputValidation"] = {"nic": "must equal the name of an existing network adapter (enumerated at runtime); reject \" & | < > ^ % and CR/LF"}
    items.append(item)
rep = (V / "RepairPage.xaml.cs").read_text(encoding="utf-8").split("\n")
assert '"/Online /Cleanup-Image /RestoreHealth" : "/Online /Cleanup-Image /ScanHealth"' in rep[212] and '"/scannow" : "/verifyonly"' in rep[213] and '"echo Y|chkdsk {DriveRoot} /f"' in rep[215]
for tid, title, exe, args, rk in [("DISMScan", "DISM scan health", "dism.exe", "/Online /Cleanup-Image /ScanHealth", "low"), ("DISMRepair", "DISM restore health", "dism.exe", "/Online /Cleanup-Image /RestoreHealth", "medium"),
                                ("SFCVerify", "SFC verify only", "sfc.exe", "/verifyonly", "low"), ("SFCRepair", "SFC scannow", "sfc.exe", "/scannow", "medium"),
                                ("CHKDSKScan", "CHKDSK scan (read-only)", "chkdsk.exe", "", "low"), ("CHKDSKSchedule", "CHKDSK /f on next boot (system drive)", "cmd", "echo Y|chkdsk {DriveRoot} /f", "medium")]:
    items.append({"id": "rytunex:tool:" + tid, "source": "RyTuneX", "sourceFile": "Views/RepairPage.xaml.cs:213-216,605-611", "category": "Repair", "title": title, "description": "",
                  "kind": "command", "launcher": "SystemTool" if exe != "cmd" else "StartInCmd", "fileName": exe, "commands": [args],
                  "options": [], "freeInputs": [], "requiresAdmin": True, "flags": [], "downloads": [], "revert": None, "risk": rk,
                  "note": "{DriveRoot} = system drive root, e.g. C:\\ (as RyTuneX)" if "{DriveRoot}" in args else ("CHKDSK stdin 'N' as RyTuneX" if exe == "chkdsk.exe" else "")})

CONSOLE_REQUIRED = {"3 Setup/12 Updates Pause.ps1", "5 Graphics/3 Driver Updated Install & Settings.ps1", "5 Graphics/4 Driver Debloat Install & Settings.ps1",
    "6 Windows/6 Signout Lockscreen Wallpaper Black.ps1", "8 Advanced/10 Priority.ps1", "8 Advanced/18 Start Search Shell Mobsync.ps1",
    "8 Advanced/8 Smt Ht.ps1", "8 Advanced/9 Core 1 Thread 1.ps1"}
for it in items:
    if it["source"] == "Ultimate":
        it["consoleRequired"] = it["sourceFile"] in CONSOLE_REQUIRED
assert sum(1 for it in items if it.get("consoleRequired")) == len(CONSOLE_REQUIRED), [it["sourceFile"] for it in items if it["source"] == "Ultimate"][:5]

OUT.parent.mkdir(parents=True, exist_ok=True)
OUT.write_text(json.dumps({"upstream": {r: rev(r) for r in ["RyTuneX", "WinSux", "Ultimate", "winutil"]}, "winutilPresets": presets, "items": items}, indent=1, ensure_ascii=False), encoding="utf-8")
from collections import Counter
print(Counter(i["source"] for i in items), Counter(i["kind"] for i in items))
print("rytunex tags without dispatch:", [i["tag"] for i in items if i["source"] == "RyTuneX" and i.get("kind") in ("toggle","combo") and not i.get("dispatchFound")])
print("rytunex dispatch cases without UI:", sorted(set(cases) - {i.get("tag") for i in items}))
