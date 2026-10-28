# -*- coding: utf-8 -*-
"""
Confere a "costura" do projeto Unity sem abrir o Unity:

  * todo arquivo dentro de Assets/ tem .meta (e não sobra .meta sem arquivo);
  * os GUIDs são únicos;
  * as cenas apontam para scripts que existem (pelo GUID do .meta), as referências
    internas delas (fileID) existem e os campos preenchidos existem nos scripts;
  * a lista de cenas da Build bate com as cenas e os GUIDs delas;
  * cada MonoBehaviour/ScriptableObject está num arquivo com o mesmo nome da classe;
  * os .asmdef são JSON válido e só referenciam assemblies que existem;
  * o manifest.json das sprites é válido, cada folha tem o PNG e o .meta e todo
    sprite cabe dentro da folha.

Uso (na raiz do projeto):  py Ferramentas/verificar_projeto.py
Sai com código 1 se encontrar algum problema.
"""
import io
import json
import os
import re
import struct
import sys

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ASSETS = os.path.join(RAIZ, "Assets")

# GUIDs de scripts que vêm dos pacotes do Unity (URP)
GUIDS_DE_PACOTES = {
    "a79441f348de89743a2939f4d699eac1": "UniversalAdditionalCameraData (URP)",
    "073797afb82c5a1438f328866b10b3f0": "Light2D (URP)",
    "a97c105638bdf8b4a8650670310a4cd3": "Sprite-Lit-Default (URP)",
    "9dfc825aed78fcd4ba02077103263b40": "Sprite-Unlit-Default (URP)",
}

# assemblies do Unity que os .asmdef podem referenciar
ASSEMBLIES_DO_UNITY = {
    "Unity.InputSystem", "UnityEngine.UI", "UnityEngine.TestRunner", "UnityEditor.TestRunner",
    "Unity.RenderPipelines.Universal.Runtime", "Unity.RenderPipelines.Core.Runtime",
}

BASES_UNITY = ("MonoBehaviour", "ScriptableObject", "MaskableGraphic", "Graphic", "TileBase", "Inimigo")

problemas = []


def problema(msg):
    problemas.append(msg)


def rel(c):
    return os.path.relpath(c, RAIZ).replace("\\", "/")


def ler(c):
    with io.open(c, encoding="utf-8") as f:
        return f.read()


def guid_de_meta(meta):
    m = re.search(r"^guid: ([0-9a-f]{32})\s*$", ler(meta), re.M)
    return m.group(1) if m else None


def verificar_metas():
    guids = {}
    for raiz, pastas, arquivos in os.walk(ASSETS):
        pastas[:] = [p for p in pastas if not p.startswith(".")]
        nomes = set(pastas) | set(a for a in arquivos if not a.endswith(".meta") and not a.startswith("."))
        for n in nomes:
            c = os.path.join(raiz, n)
            if not os.path.exists(c + ".meta"):
                problema("sem .meta: " + rel(c))
        for a in arquivos:
            if not a.endswith(".meta"):
                continue
            meta = os.path.join(raiz, a)
            alvo = meta[:-5]
            if not os.path.exists(alvo):
                problema(".meta sem arquivo: " + rel(meta))
                continue
            g = guid_de_meta(meta)
            if g is None:
                problema("GUID inválido em " + rel(meta))
            elif g in guids:
                problema("GUID repetido %s em %s e %s" % (g, guids[g], rel(meta)))
            else:
                guids[g] = rel(meta)
    return guids


def verificar_cenas(guids):
    pasta = os.path.join(ASSETS, "Scenes")
    scripts = dict((g, os.path.join(RAIZ, m[:-5])) for g, m in guids.items() if m.endswith(".cs.meta"))
    cenas = {}
    for a in sorted(os.listdir(pasta)):
        if not a.endswith(".unity"):
            continue
        c = os.path.join(pasta, a)
        texto = ler(c)
        cenas["Assets/Scenes/" + a] = guid_de_meta(c + ".meta")
        for g in re.findall(r"guid: ([0-9a-f]{32})", texto):
            if g in GUIDS_DE_PACOTES or g in guids or g.startswith("0000000000000000"):
                continue
            problema("a cena %s aponta para um GUID que não existe: %s" % (a, g))
        # cada objeto da cena é um bloco "--- !u!<classe> &<fileID>"
        partes = re.split(r"^--- !u!(\d+) &(-?\d+).*$", texto, flags=re.M)
        blocos = [(partes[i], partes[i + 1], partes[i + 2]) for i in range(1, len(partes), 3)]
        ids = set(b[1] for b in blocos)
        for _, _, corpo in blocos:
            for ref in re.findall(r"\{fileID: (-?\d+)\}", corpo):
                if ref != "0" and ref not in ids:
                    problema("a cena %s aponta para um objeto que não existe nela: fileID %s" % (a, ref))
        for classe, _, corpo in blocos:
            m = re.search(r"m_Script: \{fileID: 11500000, guid: ([0-9a-f]{32})", corpo)
            if classe != "114" or not m or m.group(1) not in scripts:
                continue
            fonte = ler(scripts[m.group(1)])
            for campo in re.findall(r"^  (\w+):", corpo, re.M):
                if not campo.startswith("m_") and not re.search(r"\b%s\b\s*[=;]" % campo, fonte):
                    problema("a cena %s preenche o campo '%s', que não existe em %s" %
                             (a, campo, rel(scripts[m.group(1)])))
        if "orthographic: 1" not in texto:
            problema("a câmera da cena %s não é ortográfica" % a)
    build = ler(os.path.join(RAIZ, "ProjectSettings", "EditorBuildSettings.asset"))
    pares = re.findall(r"path: (\S+)\s+guid: ([0-9a-f]{32})", build)
    if [p for p, _ in pares] != ["Assets/Scenes/Menu.unity", "Assets/Scenes/Suecia1958.unity", "Assets/Scenes/FimDeJogo.unity"]:
        problema("a lista de cenas da Build não é Menu, Suecia1958, FimDeJogo: %s" % [p for p, _ in pares])
    for caminho, guid in pares:
        if cenas.get(caminho) != guid:
            problema("o GUID da cena %s na Build (%s) não bate com o .meta (%s)" % (caminho, guid, cenas.get(caminho)))
    return len(cenas)


def verificar_nomes_de_classe():
    padrao = re.compile(r"\bclass\s+(\w+)\s*:\s*([\w.]+)")
    n = 0
    for raiz, _, arquivos in os.walk(os.path.join(ASSETS, "Scripts")):
        for a in arquivos:
            if not a.endswith(".cs"):
                continue
            texto = ler(os.path.join(raiz, a))
            for classe, base in padrao.findall(texto):
                if base.split(".")[-1] in BASES_UNITY or base in ("ZagueiroCarrinho",):
                    n += 1
                    if classe + ".cs" != a:
                        problema("a classe %s (herda de %s) precisa ficar em %s.cs, não em %s" % (classe, base, classe, a))
    return n


def verificar_asmdefs():
    nomes = {}
    arquivos = []
    for raiz, _, lista in os.walk(ASSETS):
        for a in lista:
            if a.endswith(".asmdef"):
                c = os.path.join(raiz, a)
                try:
                    dados = json.loads(ler(c))
                except ValueError as e:
                    problema("asmdef inválido %s: %s" % (rel(c), e))
                    continue
                nomes[dados["name"]] = rel(c)
                arquivos.append((c, dados))
    for c, dados in arquivos:
        for r in dados.get("references", []):
            if r not in nomes and r not in ASSEMBLIES_DO_UNITY:
                problema("o asmdef %s referencia '%s', que não existe" % (rel(c), r))
    return len(arquivos)


def verificar_sprites():
    pasta = os.path.join(ASSETS, "Resources", "Sprites")
    try:
        manifest = json.loads(ler(os.path.join(pasta, "manifest.json")))
    except (IOError, ValueError) as e:
        problema("manifest.json inválido: %s" % e)
        return 0
    total = 0
    vistos = set()
    for folha in manifest.get("folhas", []):
        png = os.path.join(pasta, folha["folha"] + ".png")
        largura = altura = None
        if os.path.exists(png):
            largura, altura = tamanho_png(png)
        else:
            problema("a folha %s não tem o PNG" % folha["folha"])
        if not os.path.exists(png + ".meta"):
            problema("a folha %s não tem o .meta" % folha["folha"])
        for s in folha["sprites"]:
            total += 1
            if s["n"] in vistos:
                problema("sprite repetido no manifest: " + s["n"])
            vistos.add(s["n"])
            if largura is not None and (s["x"] < 0 or s["y"] < 0 or s["x"] + s["w"] > largura or
                                        s["y"] + s["h"] > altura):
                problema("o sprite %s sai da folha %s (%dx%d)" % (s["n"], folha["folha"], largura, altura))
    return total


def tamanho_png(c):
    """Largura e altura lidas do cabeçalho IHDR do PNG."""
    with open(c, "rb") as f:
        f.read(16)
        return struct.unpack(">II", f.read(8))


def main():
    guids = verificar_metas()
    cenas = verificar_cenas(guids)
    classes = verificar_nomes_de_classe()
    asmdefs = verificar_asmdefs()
    sprites = verificar_sprites()
    print("Conferido: %d arquivos com GUID, %d cenas, %d componentes, %d asmdef, %d sprites." %
          (len(guids), cenas, classes, asmdefs, sprites))
    if problemas:
        print("\n%d problema(s):" % len(problemas))
        for p in problemas:
            print("  - " + p)
        sys.exit(1)
    print("Nenhum problema encontrado.")


if __name__ == "__main__":
    main()
