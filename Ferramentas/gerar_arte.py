# -*- coding: utf-8 -*-
"""
Gerador da arte original do Cinco Estrelas (Python puro, sem Pillow).

Gera, dentro de Assets/Resources/Sprites:
  * as folhas de sprites (PNG) e os fundos,
  * o .meta de cada PNG (TextureImporter do Unity 6: Sprite Single, 16 pixels
    por unidade, filtro Point, sem compressão),
  * o manifest.json com a posição e o pivô de cada sprite (o jogo usa para
    montar os sprites em tempo de execução, veja Assets/Scripts/Nucleo/Arte.cs).

Uso (na raiz do projeto):
    py Ferramentas/gerar_arte.py
    py Ferramentas/gerar_arte.py --previas PASTA   (também salva prévias ampliadas x4)
"""
import hashlib
import json
import os
import sys

AQUI = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, AQUI)

import arte_cenario as ac        # noqa: E402
import arte_interface as ai      # noqa: E402
import arte_personagens as ap    # noqa: E402
import png                       # noqa: E402
from pixelart import Canvas      # noqa: E402

RAIZ = os.path.dirname(AQUI)
SAIDA = os.path.join(RAIZ, "Assets", "Resources", "Sprites")
PPU = 16
MARGEM = 1  # pixels extras (copiados da borda) em volta de cada sprite: evita "linhas" entre tiles

PE = (0.5, 0.0)
CENTRO = (0.5, 0.5)
SEM = (0, 0, 0, 0)

# nomes que o código do jogo usa (o gerador falha se algum sumir)
NOMES_OBRIGATORIOS = (
    ["camisa10_parado_%d" % i for i in range(4)] + ["camisa10_correr_%d" % i for i in range(6)] +
    ["camisa10_pulo_0", "camisa10_queda_0"] + ["camisa10_chute_%d" % i for i in range(3)] +
    ["camisa10_carregar_%d" % i for i in range(2)] + ["camisa10_cavadinha_%d" % i for i in range(2)] +
    ["camisa10_bicicleta_%d" % i for i in range(4)] + ["camisa10_pedalada_%d" % i for i in range(4)] +
    ["camisa10_embaixadinha_%d" % i for i in range(4)] + ["camisa10_dano_0"] +
    ["camisa10_vitoria_%d" % i for i in range(2)] + ["camisa10_chamar_0"] +
    ["companheiro_parado_%d" % i for i in range(2)] + ["companheiro_correr_%d" % i for i in range(6)] +
    ["companheiro_passe_%d" % i for i in range(2)] + ["companheiro_comemorar_%d" % i for i in range(2)] +
    ["carrinho_andar_%d" % i for i in range(4)] +
    ["carrinho_preparar_0", "carrinho_sentado_0", "carrinho_atingido_0", "carrinho_distraido_0"] +
    ["carrinho_deslizar_%d" % i for i in range(2)] +
    ["saltador_parado_%d" % i for i in range(2)] +
    ["saltador_agachar_0", "saltador_pulo_0", "saltador_aterrissar_0", "saltador_atingido_0",
     "saltador_distraido_0"] +
    ["capitao_andar_%d" % i for i in range(4)] +
    ["capitao_preparar_0", "capitao_sentado_0", "capitao_aterrissar_0", "capitao_atingido_0",
     "capitao_distraido_0"] +
    ["capitao_salto_%d" % i for i in range(2)] + ["capitao_tonto_%d" % i for i in range(2)] +
    ["capitao_deslizar_%d" % i for i in range(2)] +
    ["goleiro_parado_%d" % i for i in range(2)] + ["goleiro_correr_%d" % i for i in range(4)] +
    ["goleiro_arremesso_%d" % i for i in range(2)] +
    ["goleiro_pronto_0", "goleiro_espalmar_0", "goleiro_segurar_0", "goleiro_levantar_0",
     "goleiro_comemorar_0", "goleiro_mergulho_0", "goleiro_caido_0"] +
    ["bola", "bola_goleiro"] + ["bola_fogo_%d" % i for i in range(4)] + ["coracao_%d" % i for i in range(2)] +
    ["caixote"] + ["lasca_%d" % i for i in range(3)] + ["barris"] + ["barril_pedaco_%d" % i for i in range(2)] +
    ["sino_%d" % i for i in range(3)] + ["sino_poste", "ponte", "ponte_ponta", "barco", "bandeira_off"] +
    ["bandeira_on_%d" % i for i in range(4)] +
    ["placa", "portao", "gol", "carta_canhao", "carta_caneleira", "estrela_cheia", "estrela_vazia",
     "estrela_grande_cheia", "estrela_grande_vazia", "luz_suave", "anel"] +
    ["brilho_%d" % i for i in range(4)] + ["poeira_%d" % i for i in range(5)] +
    ["fumaca_%d" % i for i in range(5)] + ["confete_%d" % i for i in range(3)] +
    ["rastro"] + ["gota_%d" % i for i in range(2)] +
    ["exclamacao", "interrogacao"] + ["estrelinha_%d" % i for i in range(2)] +
    ["seta", "icone_companheiro", "icone_vida"] +
    ["pedra_%d" % m for m in range(16)] + ["rua_%d" % m for m in range(16)] + ["grama_%d" % m for m in range(16)] +
    ["doca_" + t for t in "emdu"] + ["tabua_" + t for t in "emdu"] +
    ["agua_topo_%d" % i for i in range(4)] + ["agua_fundo_%d" % i for i in range(2)] +
    ["lampiao", "arvore", "banco", "barril", "refletor"] + ["mastro_%d" % i for i in range(3)] +
    ["ui_painel", "ui_botao", "ui_tecla", "ui_branco", "ui_coracao_cheio", "ui_coracao_vazio"] +
    ["fundo_ceu", "fundo_nuvens", "fundo_cidade", "fundo_gamla_stan", "estadio_arquibancada_0",
     "estadio_arquibancada_1"]
)

# pivôs dos itens que ficam apoiados no chão (os outros itens usam o centro)
PIVO_ITENS = {
    'caixote': PE, 'barris': PE, 'sino_poste': PE, 'bandeira_off': PE, 'placa': PE, 'portao': PE,
    'gol': (8.0 / 48.0, 0.0),
}
BORDAS_UI = {'ui_painel': (8, 8, 8, 8), 'ui_botao': (5, 5, 5, 5), 'ui_tecla': (4, 4, 4, 4), 'ui_branco': (1, 1, 1, 1)}


def guid_de(texto):
    """GUID determinístico (32 hex) para o .meta não mudar entre execuções."""
    return hashlib.md5(("cincoestrelas:" + texto).encode("utf-8")).hexdigest()


def _f(v):
    s = ("%.4f" % v).rstrip("0").rstrip(".")
    return s if s else "0"


def meta_textura(guid, nome):
    """Texto do .meta (TextureImporter) no formato do Unity 6: Sprite (Single), PPU 16, Point, sem compressão."""
    linhas = []
    add = linhas.append
    add("fileFormatVersion: 2")
    add("guid: %s" % guid)
    add("TextureImporter:")
    add("  internalIDToNameTable: []")
    add("  externalObjects: {}")
    add("  serializedVersion: 13")
    add("  mipmaps:")
    add("    mipMapMode: 0")
    add("    enableMipMap: 0")
    add("    sRGBTexture: 1")
    add("    linearTexture: 0")
    add("    fadeOut: 0")
    add("    borderMipMap: 0")
    add("    mipMapsPreserveCoverage: 0")
    add("    alphaTestReferenceValue: 0.5")
    add("    mipMapFadeDistanceStart: 1")
    add("    mipMapFadeDistanceEnd: 3")
    add("  bumpmap:")
    add("    convertToNormalMap: 0")
    add("    externalNormalMap: 0")
    add("    heightScale: 0.25")
    add("    normalMapFilter: 0")
    add("    flipGreenChannel: 0")
    add("  isReadable: 0")
    add("  streamingMipmaps: 0")
    add("  streamingMipmapsPriority: 0")
    add("  vTOnly: 0")
    add("  ignoreMipmapLimit: 0")
    add("  grayScaleToAlpha: 0")
    add("  generateCubemap: 6")
    add("  cubemapConvolution: 0")
    add("  seamlessCubemap: 0")
    add("  textureFormat: 1")
    add("  maxTextureSize: 2048")
    add("  textureSettings:")
    add("    serializedVersion: 2")
    add("    filterMode: 0")
    add("    aniso: 1")
    add("    mipBias: 0")
    add("    wrapU: 1")
    add("    wrapV: 1")
    add("    wrapW: 1")
    add("  nPOTScale: 0")
    add("  lightmap: 0")
    add("  compressionQuality: 50")
    add("  spriteMode: 1")
    add("  spriteExtrude: 1")
    add("  spriteMeshType: 0")
    add("  alignment: 9")
    add("  spritePivot: {x: 0.5, y: 0.5}")
    add("  spritePixelsToUnits: %d" % PPU)
    add("  spriteBorder: {x: 0, y: 0, z: 0, w: 0}")
    add("  spriteGenerateFallbackPhysicsShape: 1")
    add("  alphaUsage: 1")
    add("  alphaIsTransparency: 1")
    add("  spriteTessellationDetail: -1")
    add("  textureType: 8")
    add("  textureShape: 1")
    add("  singleChannelComponent: 0")
    add("  flipbookRows: 1")
    add("  flipbookColumns: 1")
    add("  maxTextureSizeSet: 0")
    add("  compressionQualitySet: 0")
    add("  textureFormatSet: 0")
    add("  ignorePngGamma: 0")
    add("  applyGammaDecoding: 0")
    add("  swizzle: 50462976")
    add("  cookieLightType: 0")
    add("  platformSettings:")
    for alvo in ("DefaultTexturePlatform", "Standalone", "WebGL", "Android"):
        add("  - serializedVersion: 4")
        add("    buildTarget: %s" % alvo)
        add("    maxTextureSize: 2048")
        add("    resizeAlgorithm: 0")
        add("    textureFormat: -1")
        add("    textureCompression: 0")
        add("    compressionQuality: 50")
        add("    crunchedCompression: 0")
        add("    allowsAlphaSplitting: 0")
        add("    overridden: 0")
        add("    ignorePlatformSupport: 0")
        add("    androidETC2FallbackOverride: 0")
        add("    forceMaximumCompressionQuality_BC6H_BC7: 0")
    add("  spriteSheet:")
    add("    serializedVersion: 2")
    add("    sprites: []")
    add("    outline: []")
    add("    customData: ")
    add("    physicsShape: []")
    add("    bones: []")
    add("    spriteID: %s" % guid_de(nome + "#main"))
    add("    internalID: 0")
    add("    vertices: []")
    add("    indices: ")
    add("    edges: []")
    add("    weights: []")
    add("    secondaryTextures: []")
    add("    spriteCustomMetadata:")
    add("      entries: []")
    add("    nameFileIdTable: {}")
    add("  mipmapLimitGroupName: ")
    add("  pSDRemoveMatte: 0")
    add("  userData: ")
    add("  assetBundleName: ")
    add("  assetBundleVariant: ")
    return "\n".join(linhas) + "\n"


def meta_texto(guid):
    return ("fileFormatVersion: 2\nguid: %s\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n"
            "  assetBundleName: \n  assetBundleVariant: \n" % guid)


def _pot(v):
    p = 1
    while p < v:
        p *= 2
    return p


def empacotar(sprites, largura_max=256):
    """
    Empacota sprites em prateleiras (os mais altos primeiro). Cada sprite ganha
    MARGEM px de cada lado. Devolve {nome: (x, y)} do canto superior esquerdo
    do conteúdo e o tamanho final da textura (potência de 2).
    """
    ordem = sorted(sprites, key=lambda s: (-s[1].h, -s[1].w))
    x = y = 0
    altura_linha = 0
    pos = {}
    largura_usada = 0
    for nome, canvas, pivo, borda in ordem:
        w = canvas.w + 2 * MARGEM
        h = canvas.h + 2 * MARGEM
        if w > largura_max:
            raise ValueError("sprite %s mais largo que a folha" % nome)
        if x + w > largura_max:
            x = 0
            y += altura_linha
            altura_linha = 0
        pos[nome] = (x + MARGEM, y + MARGEM)
        x += w
        largura_usada = max(largura_usada, x)
        altura_linha = max(altura_linha, h)
    return pos, _pot(largura_usada), _pot(y + altura_linha)


def _colar_extrudado(folha, canvas, x0, y0):
    """Cola o sprite e repete as bordas na margem (evita sangrar a cor vizinha)."""
    for y in range(-MARGEM, canvas.h + MARGEM):
        for x in range(-MARGEM, canvas.w + MARGEM):
            sx = min(max(x, 0), canvas.w - 1)
            sy = min(max(y, 0), canvas.h - 1)
            c = canvas.p[sy][sx]
            if c is not None:
                folha.p[y0 + y][x0 + x] = c


def _escrever_png_e_meta(nome_arquivo, canvas):
    caminho = os.path.join(SAIDA, nome_arquivo + ".png")
    canvas.salvar(caminho)
    with open(caminho + ".meta", "w", encoding="utf-8", newline="\n") as f:
        f.write(meta_textura(guid_de("Assets/Resources/Sprites/" + nome_arquivo + ".png"), nome_arquivo))
    return caminho


def salvar_folha(nome_folha, sprites, manifest, previas=None, largura_max=256):
    """sprites: lista de (nome, canvas, (pivoX, pivoY), (bordaE, bordaB, bordaD, bordaC))."""
    nomes = [s[0] for s in sprites]
    if len(set(nomes)) != len(nomes):
        raise ValueError("nomes repetidos na folha " + nome_folha)
    pos, W, H = empacotar(sprites, largura_max)
    folha = Canvas(W, H)
    infos = []
    for nome, canvas, pivo, borda in sprites:
        x, y = pos[nome]
        _colar_extrudado(folha, canvas, x, y)
        infos.append({"n": nome, "x": x, "y": H - y - canvas.h, "w": canvas.w, "h": canvas.h,
                      "px": round(pivo[0], 4), "py": round(pivo[1], 4), "b": list(borda)})
    _escrever_png_e_meta(nome_folha, folha)
    manifest["folhas"].append({"folha": nome_folha, "sprites": infos})
    if previas:
        folha.salvar_ampliado(os.path.join(previas, "folha_" + nome_folha + ".png"), 4, (96, 120, 160, 255))
    print("  %-26s %4dx%-4d %3d sprites" % (nome_folha + ".png", W, H, len(infos)))


def salvar_imagem_unica(nome, canvas, pivo, manifest, previas=None):
    """Imagens grandes (fundos do parallax): uma por arquivo."""
    _escrever_png_e_meta(nome, canvas)
    manifest["folhas"].append({
        "folha": nome,
        "sprites": [{"n": nome, "x": 0, "y": 0, "w": canvas.w, "h": canvas.h,
                     "px": pivo[0], "py": pivo[1], "b": [0, 0, 0, 0]}],
    })
    if previas:
        canvas.salvar_ampliado(os.path.join(previas, "fundo_" + nome + ".png"), 2, (40, 40, 60, 255))
    print("  %-26s %4dx%-4d (imagem única)" % (nome + ".png", canvas.w, canvas.h))


def limpar_antigos(gerados):
    """Apaga PNGs que não fazem mais parte do jogo (e os .meta deles)."""
    for nome in os.listdir(SAIDA):
        if not nome.endswith(".png") or nome[:-4] in gerados:
            continue
        for arquivo in (nome, nome + ".meta"):
            caminho = os.path.join(SAIDA, arquivo)
            if os.path.exists(caminho):
                os.remove(caminho)
        print("  removido (antigo): %s" % nome)


def conferir(manifest):
    """Todos os nomes que o jogo usa existem? Nenhum nome repetido entre folhas?"""
    vistos = {}
    for folha in manifest["folhas"]:
        for s in folha["sprites"]:
            if s["n"] in vistos:
                raise ValueError("sprite %s repetido (%s e %s)" % (s["n"], vistos[s["n"]], folha["folha"]))
            vistos[s["n"]] = folha["folha"]
    faltando = [n for n in NOMES_OBRIGATORIOS if n not in vistos]
    if faltando:
        raise ValueError("faltam sprites: " + ", ".join(faltando))
    return len(vistos)


def main():
    previas = None
    if "--previas" in sys.argv:
        previas = sys.argv[sys.argv.index("--previas") + 1]
        os.makedirs(previas, exist_ok=True)
    os.makedirs(SAIDA, exist_ok=True)

    manifest = {"ppu": PPU, "folhas": []}
    print("Gerando a arte do Cinco Estrelas...")
    salvar_folha("jogador", [(n, c, PE, SEM) for n, c in ap.quadros_jogador()], manifest, previas)
    salvar_folha("adversarios", [(n, c, PE, SEM) for n, c in ap.quadros_adversarios()], manifest, previas)
    itens = [(n, c, PIVO_ITENS.get(n, PE if n.startswith('bandeira_on') else CENTRO), SEM)
             for n, c in ai.quadros_itens()]
    salvar_folha("itens", itens, manifest, previas)
    cenario = [(n, c, CENTRO, SEM) for n, c in ac.tiles_chao() + ac.tiles_plataformas() + ac.tiles_agua()]
    cenario += [(n, c, PE, SEM) for n, c in ac.quadros_decoracao()]
    salvar_folha("cenario", cenario, manifest, previas)
    salvar_folha("ui", [(n, c, CENTRO, BORDAS_UI.get(n, SEM)) for n, c in ai.quadros_ui()], manifest, previas)
    for nome, canvas, pivo in ac.fundos():
        salvar_imagem_unica(nome, canvas, pivo, manifest, previas)

    total = conferir(manifest)
    limpar_antigos(set(f["folha"] for f in manifest["folhas"]))

    caminho = os.path.join(SAIDA, "manifest.json")
    with open(caminho, "w", encoding="utf-8", newline="\n") as f:
        json.dump(manifest, f, ensure_ascii=False, separators=(",", ":"))
    with open(caminho + ".meta", "w", encoding="utf-8", newline="\n") as f:
        f.write(meta_texto(guid_de("Assets/Resources/Sprites/manifest.json")))
    print("Pronto: %d arquivos de imagem, %d sprites (todos os %d nomes obrigatórios presentes)." %
          (len(manifest["folhas"]), total, len(NOMES_OBRIGATORIOS)))


if __name__ == "__main__":
    main()
