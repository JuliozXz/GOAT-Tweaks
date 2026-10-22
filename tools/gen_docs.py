#!/usr/bin/env python3
"""Renders docs/FUNCTION-MAP.md (every action -> exact upstream origin) and docs/CONFLICTS.md from the generated catalog files."""
import json
from pathlib import Path
ROOT = Path(__file__).resolve().parent.parent
cat = json.load(open(ROOT / "catalog/catalog.json", encoding="utf-8"))
conf = json.load(open(ROOT / "catalog/conflicts.lead.json", encoding="utf-8"))
REPO = {"RyTuneX": "https://github.com/rayenghanmi/RyTuneX", "WinSux": "https://github.com/FR33THYFR33THY/WinSux",
        "Ultimate": "https://github.com/FR33THYFR33THY/Ultimate", "winutil": "https://github.com/christitustech/winutil"}
def esc(x): return str(x or "").replace("|", "\\|").replace("\n", " ")
L = ["# GOAT Tweaks — mapa função → origem", "", "Gerado por `tools/gen_docs.py` a partir de `catalog/catalog.json`. Commits upstream fixados:", ""]
for r, c in cat["upstream"].items(): L.append(f"- {r}: [{REPO[r]}]({REPO[r]}/tree/{c}) @ `{c}`")
from collections import Counter
L += ["", "| Origem | Itens |", "|---|---|"] + [f"| {k} | {v} |" for k, v in Counter(i["source"] for i in cat["items"]).items()]
L += ["", "Legenda: Admin = exige administrador; Reversão = como desfazer (vazio = sem reversão automática); Flags = áreas do sistema afetadas (análise estática).", ""]
for src in ["RyTuneX", "WinSux", "Ultimate", "winutil"]:
    L += [f"## {src}", "", "| ID | Título | Categoria | Tipo | Arquivo de origem | Admin | Risco | Flags | Reversão | Downloads |", "|---|---|---|---|---|---|---|---|---|---|"]
    for i in [x for x in cat["items"] if x["source"] == src]:
        L.append("| " + " | ".join(esc(v) for v in [i["id"], i["title"], i["category"], i["kind"], i["sourceFile"], "sim" if i["requiresAdmin"] else "não",
                                                    i["risk"], ", ".join(i["flags"]), i["revert"] or "—", len(i.get("downloads", [])) or ""]) + " |")
    L.append("")
(ROOT / "docs/FUNCTION-MAP.md").write_text("\n".join(L), encoding="utf-8")
C = ["# Conflitos e duplicações entre os quatro materiais", "",
     "Gerado de `catalog/conflicts.lead.json` (`tools/gen_conflicts.py`). Detecção estática de `reg add`, `reg.reg` e registro JSON do winutil (lado \"aplicar\"), mais grupos semânticos manuais. Não cobre `Set-ItemProperty`, serviços nem `bcdedit` (limitação documentada).", "",
     "- **exclusive**: precisa rodar sozinho no lote.", "- **user-choice**: valores incompatíveis; o app impede enfileirar juntos e pede para escolher um (decisão registrada no log).", "- **duplicate**: mesmo valor; apenas informativo (\"Também alterado por\"), nenhuma função é ocultada.", ""]
for dec in ["exclusive", "user-choice", "duplicate"]:
    gs = [g for g in conf["groups"] if g["decision"] == dec]
    C += [f"## {dec} ({len(gs)})", "", "| Configuração | Itens | Valores | Nota |", "|---|---|---|---|"]
    C += [f"| {esc(g['setting'])} | {esc(', '.join(g['items']))} | {esc('; '.join(g.get('values', [])))[:400]} | {esc(g['note'])} |" for g in gs] + [""]
(ROOT / "docs/CONFLICTS.md").write_text("\n".join(C), encoding="utf-8")
print("ok", len(L), len(C))
