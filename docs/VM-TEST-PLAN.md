# Windows VM test plan (disposable VM, user-approved; destructive tests allowed)
Inputs: GOATTweaks.exe (+ .sha256), goat.ps1, catalog.json, the 4 upstream repos at pinned SHAs (clone them on the VM for comparison).
Record OS build (winver), edition, PowerShell 5.1 version. Create a system restore point + export HKLM/HKCU snapshots (reg export) before each phase.

1. Startup: run exe unelevated -> window title exactly "GOAT Tweaks", no other branding; all nav sections render; search works. Screenshot.
2. Launcher: `Set-ExecutionPolicy` untouched; run `& .\goat.ps1` with a local-served release (or real repo when available): downloads, verifies hash, opens app; tamper exe -> refuses.
3. Permissions: non-admin item runs without UAC; batch of admin items -> exactly one UAC prompt; deny UAC -> clean error, nothing applied.
4. Per-engine equivalence (compare GOAT run vs original run on same key set, via reg query/Get-Service/powercfg/bcdedit before/after diff):
   - RyTuneX: 10 toggles across pages on/off (incl. MenuShowDelay, MouseAcceleration, GameBar, Telemetry services, VBS) ; WindowsUpdates combo all 4 options; tools FlushDNS, DISMScan, SFCVerify, CHKDSKScan.
   - winutil: 10 tweaks apply+undo (Telemetry, Services, Hiber, DisableIPv6, DarkMode toggle...), 1 feature, 1 appx removal, DNS Cloudflare + Default, winget install 7zip, Updates security + default.
   - Ultimate: 15 scripts incl. menu (UAC Off/Default, Power Plan 1/2, Write Cache 1/2, Gamebar w/ Run-Trusted), freeInputs (Priority with fake exe), console-mode (Updates Pause), launcher-only (Gamemode).
   - WinSux full pipeline last (snapshot first): verify typed confirmation, then actual run reaches safe mode + steptwo + reboot; document state.
5. Failure isolation: batch with one forced failure (e.g. invalid DNS adapter / missing network) -> others still run; JSONL log has all rows.
6. Conflicts: queue two user-choice items -> blocked + dialog; winsux with anything -> blocked.
7. Revert: every reversible item tested in 4 is reverted from app's Logs/Revert and registry diff returns to baseline (report exceptions).
Report: table item | expected (original) | observed (GOAT) | pass/fail | evidence path; screenshots; logs.
