# -*- coding: utf-8 -*-
"""
Cenário do Cinco Estrelas (Suécia 1958): tiles com auto-tiling de 16
combinações (cais de pedra, rua de paralelepípedo de Gamla Stan e o gramado
do estádio), doca de madeira, tábuas atravessáveis, o canal animado, as
decorações e as camadas de fundo (céu, nuvens, silhueta de Estocolmo, casas
de Gamla Stan e a arquibancada do estádio Råsunda).
"""
import math
import random

from pixelart import Canvas, cor, misturar, com_alpha, ruido_periodico

K = cor('#1e1622')
BRANCO = cor('#ffffff')

CIMA, DIREITA, BAIXO, ESQUERDA = 1, 2, 4, 8

PEDRA = [cor('#cfd5df'), cor('#a9b1bf'), cor('#8a93a4'), cor('#6c7487'), cor('#4c5365')]
RUA = [cor('#c4b6a6'), cor('#a69684'), cor('#8a7a6a'), cor('#6c5d50'), cor('#4a3f37')]
MURO = [cor('#b8a48e'), cor('#9a8672'), cor('#7e6c5a'), cor('#5c4c40'), cor('#3e332c')]
TERRA = [cor('#7a6050'), cor('#634c3e'), cor('#4e3a2f'), cor('#382a22')]
GRAMA = [cor('#9ee06a'), cor('#6cc24c'), cor('#4ea23e'), cor('#2f7a34')]
MADEIRA = [cor('#e0b07a'), cor('#c08a52'), cor('#9a6a3c'), cor('#6e4826'), cor('#4a2e18')]
AGUA = [cor('#bfe6f7'), cor('#7cc0e6'), cor('#4f97cc'), cor('#2f6fa8'), cor('#1f5185'), cor('#173c66')]
FERRO = [cor('#7c8496'), cor('#545b6c'), cor('#363a47'), cor('#24262f')]


def _bordas(c, mascara, escuro, mais_escuro, topo_livre=0):
    """Bordas escuras (e contorno) nos lados em que o vizinho NÃO é chão."""
    aberto_dir = not (mascara & DIREITA)
    aberto_baixo = not (mascara & BAIXO)
    aberto_esq = not (mascara & ESQUERDA)
    if aberto_baixo:
        for x in range(16):
            c.set(x, 13, escuro)
            c.set(x, 14, mais_escuro)
            c.set(x, 15, K)
    if aberto_esq:
        for y in range(topo_livre, 16):
            c.set(1, y, escuro)
            c.set(0, y, K)
    if aberto_dir:
        for y in range(topo_livre, 16):
            c.set(14, y, escuro)
            c.set(15, y, K)
    if aberto_baixo and aberto_esq:
        c.set(0, 15, None)
        c.set(1, 14, K)
    if aberto_baixo and aberto_dir:
        c.set(15, 15, None)
        c.set(14, 14, K)


# ------------------------------------------------------------ cais de pedra
def tile_pedra(mascara):
    """Blocos de granito do cais, em fiadas desencontradas; topo com capeamento claro."""
    rnd = random.Random(mascara * 13 + 7)
    c = Canvas(16, 16)
    tons = {}
    for y in range(16):
        fiada = y // 5
        desloc = 0 if fiada % 2 == 0 else 5
        for x in range(16):
            junta_h = y % 5 == 4
            junta_v = (x + desloc) % 10 == 9
            if junta_h or junta_v:
                cc = PEDRA[3]
            else:
                bloco = (fiada, (x + desloc) // 10)
                if bloco not in tons:
                    v = rnd.random()
                    tons[bloco] = PEDRA[1] if v < 0.5 else (PEDRA[2] if v < 0.8 else misturar(PEDRA[1], PEDRA[0], 0.5))
                cc = tons[bloco]
                if y % 5 == 0 or (x + desloc) % 10 == 0:
                    cc = misturar(cc, PEDRA[0], 0.45)      # luz na quina do bloco
                elif y % 5 == 3:
                    cc = misturar(cc, PEDRA[3], 0.35)      # sombra embaixo
                elif rnd.random() < 0.06:
                    cc = misturar(cc, PEDRA[3], 0.4)       # pintinhas do granito
            c.set(x, y, cc)
    _bordas(c, mascara, PEDRA[3], PEDRA[4])
    if not (mascara & CIMA):
        # capeamento: laje clara com quina arredondada
        for x in range(16):
            c.set(x, 0, PEDRA[0])
            c.set(x, 1, PEDRA[0] if x % 7 else PEDRA[1])
            c.set(x, 2, PEDRA[1])
            c.set(x, 3, PEDRA[3])
            c.set(x, 4, PEDRA[4] if x % 3 == 0 else PEDRA[3])
        if not (mascara & ESQUERDA):
            for y in range(0, 5):
                c.set(0, y, K)
            c.set(0, 0, None)
            c.set(1, 0, K)
        if not (mascara & DIREITA):
            for y in range(0, 5):
                c.set(15, y, K)
            c.set(15, 0, None)
            c.set(14, 0, K)
    return c


# ------------------------------------------------------------ rua de Gamla Stan
def tile_rua(mascara):
    """Paralelepípedos arredondados no topo; embaixo, base de pedra e terra."""
    rnd = random.Random(mascara * 29 + 3)
    c = Canvas(16, 16)
    # muro de pedra do cais de Gamla Stan (blocos grandes em tom quente)
    tons = {}
    for y in range(16):
        fiada = y // 6
        desloc = 0 if fiada % 2 == 0 else 6
        for x in range(16):
            if y % 6 == 5 or (x + desloc) % 12 == 11:
                cc = MURO[3]
            else:
                bloco = (fiada, (x + desloc) // 12)
                if bloco not in tons:
                    v = rnd.random()
                    tons[bloco] = MURO[1] if v < 0.5 else (MURO[2] if v < 0.85 else MURO[0])
                cc = tons[bloco]
                if y % 6 == 0:
                    cc = misturar(cc, MURO[0], 0.45)
                elif rnd.random() < 0.05:
                    cc = misturar(cc, MURO[3], 0.4)
            c.set(x, y, cc)
    _bordas(c, mascara, MURO[3], MURO[4])
    if not (mascara & CIMA):
        # duas fileiras de paralelepípedos (4x3) desencontradas
        for y in range(0, 7):
            for x in range(16):
                c.set(x, y, RUA[4])
        for linha, (y0, desloc) in enumerate(((0, 0), (3, 2))):
            for bx in range(-4, 16, 4):
                x0 = bx + desloc
                tom = RUA[1] if (bx // 4 + linha + mascara) % 3 else RUA[2]
                for yy in range(y0, y0 + 3):
                    for xx in range(x0, x0 + 4):
                        if xx < 0 or xx > 15:
                            continue
                        borda = xx == x0 + 3 or yy == y0 + 2
                        cc = RUA[3] if borda else tom
                        if yy == y0 and xx < x0 + 3:
                            cc = RUA[0] if xx == x0 else misturar(tom, RUA[0], 0.5)
                        c.set(xx, yy, cc)
        c.hline(0, 15, 6, RUA[4])
        if not (mascara & ESQUERDA):
            for y in range(0, 7):
                c.set(0, y, K)
            c.set(0, 0, None)
        if not (mascara & DIREITA):
            for y in range(0, 7):
                c.set(15, y, K)
            c.set(15, 0, None)
    return c


# ------------------------------------------------------------ gramado do estádio
def tile_grama(mascara):
    rnd = random.Random(mascara * 17 + 11)
    c = Canvas(16, 16)
    for y in range(16):
        for x in range(16):
            v = rnd.random()
            cc = TERRA[1]
            if v < 0.10:
                cc = TERRA[2]
            elif v < 0.17:
                cc = TERRA[0]
            c.set(x, y, cc)
    _bordas(c, mascara, TERRA[2], TERRA[3])
    if not (mascara & CIMA):
        for x in range(16):
            faixa = GRAMA[1] if x < 8 else GRAMA[2]   # listras do corte da grama
            fundo = 4 + (1 if (x * 7 + mascara) % 5 == 0 else 0)
            for y in range(0, fundo + 1):
                cc = faixa
                if y == 0:
                    cc = GRAMA[0] if x < 8 else GRAMA[1]
                if y == fundo:
                    cc = GRAMA[3]
                c.set(x, y, cc)
        # pontinhas de grama
        for x in range(1, 16, 3):
            if (x + mascara) % 2:
                c.set(x, 0, None)
                c.set(x, 1, GRAMA[0] if x < 8 else GRAMA[1])
        if not (mascara & ESQUERDA):
            for y in range(0, 6):
                c.set(0, y, K)
            c.set(0, 0, None)
        if not (mascara & DIREITA):
            for y in range(0, 6):
                c.set(15, y, K)
            c.set(15, 0, None)
    return c


def tiles_chao():
    q = []
    for m in range(16):
        q.append(('pedra_%d' % m, tile_pedra(m)))
    for m in range(16):
        q.append(('rua_%d' % m, tile_rua(m)))
    for m in range(16):
        q.append(('grama_%d' % m, tile_grama(m)))
    return q


# ------------------------------------------------------------ doca de madeira
def tile_doca(tipo):
    """Tablado da doca: tábuas em cima e estacas descendo até a água."""
    c = Canvas(16, 16)
    esq = tipo in ('e', 'u')
    dir_ = tipo in ('d', 'u')
    # estacas (atrás)
    for ex in (3, 12):
        for y in range(6, 16):
            c.set(ex, y, MADEIRA[3])
            c.set(ex + 1, y, MADEIRA[4])
            if y % 4 == 1:
                c.set(ex, y, MADEIRA[2])
    # viga sob o tablado
    c.rect(0, 5, 16, 2, MADEIRA[4])
    # tábuas do tablado (veio horizontal), emenda desencontrada
    for y in range(0, 5):
        for x in range(16):
            cc = MADEIRA[1]
            if y == 0:
                cc = MADEIRA[0]
            elif y == 4:
                cc = MADEIRA[3]
            elif (x * 5 + y * 3) % 11 == 0:
                cc = MADEIRA[2]
            c.set(x, y, cc)
    emenda = 7
    c.vline(emenda, 1, 3, MADEIRA[3])
    c.set(emenda, 0, MADEIRA[2])
    # pregos
    c.set(2, 2, FERRO[1])
    c.set(13, 2, FERRO[1])
    if esq:
        c.vline(0, 0, 6, K)
        c.vline(1, 0, 4, MADEIRA[2])
    if dir_:
        c.vline(15, 0, 6, K)
        c.vline(14, 0, 4, MADEIRA[2])
    c.hline(0, 15, 7, K)
    for ex in (3, 12):
        c.set(ex - 1, 15, None)
    # contorno das estacas
    tmp = c.copia()
    for y in range(8, 16):
        for ex in (3, 12):
            tmp.set(ex - 1, y, K)
            tmp.set(ex + 2, y, K)
    return tmp


# ------------------------------------------------------------ tábua/toldo atravessável
def tile_tabua(tipo):
    c = Canvas(16, 16)
    esq = tipo in ('e', 'u')
    dir_ = tipo in ('d', 'u')
    for y in range(0, 5):
        for x in range(16):
            cc = MADEIRA[1]
            if y == 0:
                cc = MADEIRA[0]
            elif y >= 3:
                cc = MADEIRA[3]
            if (x + 3) % 8 == 0 and 0 < y < 3:
                cc = MADEIRA[2]
            c.set(x, y, cc)
    c.hline(0, 15, 5, K)
    # mão-francesa de ferro embaixo (só nas pontas e no meio)
    suportes = []
    if esq:
        suportes.append(3)
    if dir_:
        suportes.append(12)
    if tipo == 'm':
        suportes.append(8)
    for sx in suportes:
        for i in range(4):
            c.set(sx + (i if sx < 8 else -i), 6 + i, FERRO[2])
        c.vline(sx, 6, 9, FERRO[1])
    if esq:
        c.vline(0, 0, 5, K)
    if dir_:
        c.vline(15, 0, 5, K)
    c.hline(0, 15, 0, MADEIRA[0])
    return c


# ------------------------------------------------------------ canal
def tile_agua(topo, fase):
    c = Canvas(16, 16)
    rnd = random.Random(7 + fase * 3 + (100 if topo else 0))
    for y in range(16):
        for x in range(16):
            cc = AGUA[3] if y < 8 or not topo else AGUA[4]
            if not topo:
                cc = AGUA[4] if (x + y * 2 + fase * 3) % 13 else AGUA[5]
            c.set(x, y, com_alpha(cc, 235))
    if topo:
        for x in range(16):
            onda = int(round(math.sin((x + fase * 4) * math.pi / 8.0)))
            y0 = 2 + onda
            for y in range(0, y0):
                c.set(x, y, None)
            c.set(x, y0, com_alpha(AGUA[0], 240))
            c.set(x, y0 + 1, com_alpha(AGUA[1], 235))
            c.set(x, y0 + 2, com_alpha(AGUA[2], 235))
        # reflexos
        for _ in range(3):
            px, py = rnd.randint(0, 13), rnd.randint(6, 13)
            c.hline(px, px + 2, py, com_alpha(AGUA[1], 200))
    else:
        for _ in range(2):
            px, py = rnd.randint(0, 13), rnd.randint(0, 15)
            c.hline(px, px + 1, py, com_alpha(AGUA[3], 220))
    return c


def tiles_agua():
    q = []
    for f in range(4):
        q.append(('agua_topo_%d' % f, tile_agua(True, f)))
    for f in range(2):
        q.append(('agua_fundo_%d' % f, tile_agua(False, f)))
    return q


def tiles_plataformas():
    q = []
    for t in ('e', 'm', 'd', 'u'):
        q.append(('doca_' + t, tile_doca(t)))
    for t in ('e', 'm', 'd', 'u'):
        q.append(('tabua_' + t, tile_tabua(t)))
    return q


# ------------------------------------------------------------ decoração (pivô no pé)
def lampiao():
    """Poste de luz antigo de ferro, com lanterna acesa."""
    c = Canvas(16, 48)
    preto = FERRO[2]
    # base
    c.rect(5, 44, 6, 4, preto)
    c.rect(6, 41, 4, 3, preto)
    # coluna com anéis
    for y in range(14, 41):
        c.set(7, y, FERRO[1])
        c.set(8, y, preto)
    for y in (20, 30, 38):
        c.hline(6, 9, y, preto)
    # braço e lanterna
    c.hline(5, 10, 13, preto)
    c.rect(5, 5, 6, 8, cor('#ffe38a'))
    c.rect(6, 6, 4, 6, cor('#fff4c8'))
    c.vline(8, 5, 12, cor('#f5c24a'))
    c.hline(4, 11, 4, preto)
    c.hline(5, 10, 3, preto)
    c.set(7, 2, preto)
    c.set(8, 2, preto)
    c.vline(4, 5, 12, preto)
    c.vline(11, 5, 12, preto)
    c.contorno(K)
    return c


def arvore():
    """Tília de praça: tronco e copa arredondada."""
    c = Canvas(32, 48)
    rnd = random.Random(42)
    # tronco
    for y in range(28, 48):
        larg = 2 if y < 40 else 3
        for x in range(16 - larg, 16 + larg):
            c.set(x, y, MADEIRA[3] if x < 16 else MADEIRA[4])
    c.linha(16, 32, 11, 26, MADEIRA[3])
    c.linha(16, 30, 21, 25, MADEIRA[3])
    verde = [cor('#8fd45e'), cor('#5fae48'), cor('#3f8a3c'), cor('#2a6432')]
    for (cx, cy, r) in ((16, 15, 11), (9, 20, 7), (23, 20, 7), (16, 8, 7), (11, 11, 6), (21, 11, 6)):
        def tinta(x, y, nx, ny, r=r):
            v = nx * 0.6 + ny * 0.8
            if v > 0.55:
                return verde[3]
            if v > 0.05:
                return verde[2]
            if v < -0.55:
                return verde[0]
            return verde[1]
        c.elipse_fn(cx, cy, r, r * 0.9, tinta)
    for _ in range(14):
        x, y = rnd.randint(6, 26), rnd.randint(3, 24)
        if c.get(x, y) is not None:
            c.set(x, y, verde[0] if rnd.random() < 0.5 else verde[2])
    c.contorno(K)
    return c


def banco():
    c = Canvas(24, 16)
    for y in (6, 8):
        c.hline(1, 22, y, MADEIRA[1])
        c.hline(1, 22, y + 1, MADEIRA[2])
    c.hline(1, 22, 11, MADEIRA[1])
    c.hline(1, 22, 12, MADEIRA[3])
    for x in (3, 20):
        c.vline(x, 6, 15, FERRO[2])
        c.vline(x + 1, 10, 15, FERRO[1])
    c.contorno(K)
    return c


def mastro(fase):
    """Mastro com a bandeira da Suécia (país-sede) tremulando."""
    c = Canvas(16, 64)
    for y in range(4, 64):
        c.set(3, y, cor('#f4f4f4'))
        c.set(4, y, cor('#c8ccd8'))
    c.rect(2, 61, 4, 3, FERRO[1])
    c.set(3, 3, cor('#ffd21f'))
    c.set(4, 3, cor('#ffd21f'))
    azul = cor('#2a6ab8')
    amarelo = cor('#ffcd2a')
    for x in range(5, 16):
        onda = int(round(math.sin((x - 5) * 0.75 + fase * 2.1) * 1.1))
        for y in range(5, 13):
            yy = y + onda
            cc = azul
            if x in (8, 9) or y in (8, 9):
                cc = amarelo
            if (x - 5) > 6 and (x + y + fase) % 5 == 0:
                cc = misturar(cc, K, 0.15)
            c.set(x, yy, cc)
    c.contorno(K)
    return c


def barril():
    c = Canvas(16, 16)
    for y in range(2, 16):
        meio = abs(y - 9) / 7.0
        larg = 6 - int(meio * 1.5)
        for x in range(8 - larg, 8 + larg):
            cc = MADEIRA[2]
            if x < 8 - larg + 2:
                cc = MADEIRA[3]
            elif x > 8 + larg - 3:
                cc = MADEIRA[1]
            if (x - 8) % 3 == 0:
                cc = misturar(cc, MADEIRA[4], 0.3)
            c.set(x, y, cc)
    for y in (4, 13):
        c.hline(2, 13, y, FERRO[1])
    c.hline(3, 12, 2, MADEIRA[1])
    c.contorno(K)
    return c


def refletor():
    """Torre de refletor do estádio (treliça de ferro e painel de lâmpadas)."""
    c = Canvas(32, 96)
    cinza = [cor('#c9ccd6'), cor('#9aa0ae'), cor('#6c7282')]
    # pernas da torre
    for y in range(24, 96):
        t = (y - 24) / 72.0
        xe = int(round(14 - t * 6))
        xd = int(round(17 + t * 6))
        c.set(xe, y, cinza[1])
        c.set(xd, y, cinza[2])
        # treliça em X
        if (y - 24) % 12 < 6:
            k = ((y - 24) % 12) / 6.0
            c.set(int(xe + (xd - xe) * k), y, cinza[2])
        else:
            k = 1 - ((y - 24) % 12 - 6) / 6.0
            c.set(int(xe + (xd - xe) * k), y, cinza[2])
        if (y - 24) % 12 == 0:
            c.hline(xe, xd, y, cinza[1])
    # painel de lâmpadas
    c.rect(4, 4, 24, 18, cinza[2])
    for ly in range(6, 20, 4):
        for lx in range(6, 26, 4):
            c.rect(lx, ly, 3, 3, cor('#fff6c8'))
            c.set(lx, ly, BRANCO)
    c.hline(4, 27, 22, cinza[1])
    c.contorno(K)
    return c


def quadros_decoracao():
    q = [('lampiao', lampiao()), ('arvore', arvore()), ('banco', banco()), ('barril', barril()),
         ('refletor', refletor())]
    for f in range(3):
        q.append(('mastro_%d' % f, mastro(f)))
    return q


# ------------------------------------------------------------ fundos (parallax)
def _gradiente(c, cores, y0=0, y1=None):
    if y1 is None:
        y1 = c.h
    n = len(cores) - 1
    for y in range(y0, y1):
        t = (y - y0) / max(1.0, (y1 - y0 - 1))
        i = min(n - 1, int(t * n))
        k = t * n - i
        cc = misturar(cores[i], cores[i + 1], k)
        for x in range(c.w):
            c.set(x, y, cc)


def fundo_ceu():
    """Céu de fim de tarde do verão sueco (o jogo estica e repete na horizontal)."""
    c = Canvas(64, 216)
    _gradiente(c, [cor('#4f7fcf'), cor('#6f9fe0'), cor('#9cc4ec'), cor('#d6e2ea'), cor('#f6dcb0'), cor('#f4c98e')])
    return c


def _nuvem(c, cx, cy, larg, rnd, corpo, sombra, luz):
    """Cúmulo: vários "bolos" redondos sobre uma base reta, com sombra embaixo e luz em cima."""
    partes = []
    n = max(3, int(larg / 9))
    for i in range(n):
        px = cx - larg / 2.0 + (i + 0.5) * larg / n
        meio = 1 - abs(i - (n - 1) / 2.0) / (n / 2.0)
        r = 6 + meio * 9 + rnd.random() * 3
        partes.append((px, cy - r * 0.45, r))
    W = c.w
    base = cy + 5
    for (px, py, r) in partes:
        for y in range(int(py - r - 1), int(base) + 1):
            for xx in range(int(px - r - 1), int(px + r + 1)):
                dx = (xx + 0.5 - px) / r
                dy = (y + 0.5 - py) / r
                if dx * dx + dy * dy <= 1.0 and y <= base:
                    cc = corpo
                    if y >= base - 2 or dy > 0.55:
                        cc = sombra
                    elif dy < -0.5 and dx < 0.3:
                        cc = luz
                    c.set(xx % W, y, cc)


def fundo_nuvens():
    c = Canvas(512, 96)
    rnd = random.Random(1958)
    corpo = cor('#fbf6ee', 235)
    sombra = cor('#dcd8e6', 225)
    luz = cor('#ffffff', 245)
    for (cx, cy, larg) in ((44, 40, 70), (160, 70, 48), (250, 36, 86), (350, 66, 56), (440, 44, 74)):
        _nuvem(c, cx, cy, larg, rnd, corpo, sombra, luz)
    return c


def fundo_cidade():
    """Silhueta distante de Estocolmo: Prefeitura (três coroas), Riddarholmskyrkan, torres e cúpulas."""
    W, H = 512, 128
    c = Canvas(W, H)
    base = cor('#8296bc')
    luz = cor('#a2b3d2')
    escuro = cor('#6f82aa')
    janela = cor('#c8d4ea')
    ouro = cor('#f2cf5a')
    ruido = ruido_periodico(5, W, 3)

    def coluna(x, topo, cc=base):
        for y in range(int(topo), H):
            c.set(x % W, y, cc)

    # morros de Södermalm ao fundo
    for x in range(W):
        topo = 92 - ruido(x) * 22
        coluna(x, topo, escuro)
    # telhados e prédios baixos
    rnd = random.Random(8)
    x = 0
    while x < W:
        larg = rnd.randint(10, 24)
        alto = rnd.randint(14, 30)
        for xx in range(x, x + larg):
            coluna(xx, H - 22 - alto, base)
        # telhado inclinado
        for i in range(larg // 2):
            c.set((x + i) % W, H - 22 - alto - i // 2, base)
            c.set((x + larg - 1 - i) % W, H - 22 - alto - i // 2, base)
        for jy in range(H - 18 - alto, H - 4, 6):
            for jx in range(x + 2, x + larg - 2, 4):
                if rnd.random() < 0.35:
                    c.set(jx % W, jy, janela)
        x += larg + rnd.randint(0, 3)

    # Prefeitura (Stadshuset): torre quadrada alta com lanterna e três coroas
    tx = 70
    for xx in range(tx, tx + 14):
        coluna(xx, 24, luz if xx < tx + 4 else base)
    for xx in range(tx + 2, tx + 12):
        coluna(xx, 16, base)
    for xx in range(tx + 4, tx + 10):
        coluna(xx, 10, base)
    c.rect(tx + 6, 4, 2, 6, base)
    for i, dx in enumerate((-3, 0, 3)):
        c.rect(tx + 6 + dx, 1, 2, 2, ouro)
    for jy in range(30, 70, 7):
        c.hline(tx + 4, tx + 9, jy, janela)
    # corpo da prefeitura
    for xx in range(tx - 30, tx + 50):
        coluna(xx, 62 + (0 if tx - 30 < xx < tx + 50 else 4), base)

    # Riddarholmskyrkan: agulha de ferro vazada
    rx = 200
    for y in range(8, 70):
        larg = int((y - 8) / 62.0 * 6) + 1
        for xx in range(rx - larg, rx + larg + 1):
            if y < 60 and (xx + y) % 3 == 0 and abs(xx - rx) < larg:
                continue
            c.set(xx, y, escuro if xx < rx else base)
    for xx in range(rx - 9, rx + 10):
        coluna(xx, 70, base)
    for xx in range(rx - 22, rx + 24):
        coluna(xx, 80, base)

    # Storkyrkan: torre com coruchéu em bulbo
    sx = 300
    for xx in range(sx - 5, sx + 6):
        coluna(xx, 40, base)
    c.elipse(sx + 0.5, 37, 5, 4, base)
    c.rect(sx, 26, 1, 8, base)
    c.elipse(sx + 0.5, 31, 2, 3, luz)

    # Palácio Real: bloco comprido e baixo
    for xx in range(330, 420):
        coluna(xx, 68, luz if (xx // 6) % 2 == 0 else base)
    for jx in range(334, 418, 5):
        for jy in (73, 81, 89):
            c.set(jx, jy, janela)

    # cúpula da Katarina kyrka
    kx = 470
    for xx in range(kx - 10, kx + 11):
        coluna(xx, 62, base)
    c.elipse(kx + 0.5, 56, 9, 9, base)
    c.elipse(kx - 2.5, 52, 3, 4, luz)
    c.rect(kx, 40, 1, 8, base)
    c.rect(kx - 1, 44, 3, 3, base)
    # bruma perto da água
    for y in range(H - 10, H):
        for x in range(W):
            if c.get(x, y) is not None:
                c.set(x, y, misturar(c.get(x, y), cor('#c2d0e6'), (y - (H - 10)) / 12.0))
    return c


CASAS = [cor('#d9a441'), cor('#d97a3a'), cor('#b8452e'), cor('#e8c45a'), cor('#e09a7a'), cor('#a8653c'),
         cor('#e8d4a8'), cor('#7c9a86')]


def fundo_gamla_stan():
    """Casas coloridas de Gamla Stan, com frontões escalonados, janelas e chaminés."""
    W, H = 512, 112
    c = Canvas(W, H)
    rnd = random.Random(1253)
    telhado = cor('#5a3a32')
    telhado_cobre = cor('#5f8f7a')
    x = 0
    while x < W - 6:
        larg = rnd.choice((18, 20, 22, 24, 26))
        if x + larg > W:
            larg = W - x
        alto = rnd.randint(58, 92)
        cc = CASAS[rnd.randint(0, len(CASAS) - 1)]
        sombra = misturar(cc, K, 0.25)
        luz = misturar(cc, BRANCO, 0.25)
        topo = H - alto
        # fachada
        for xx in range(x, x + larg):
            for y in range(topo, H):
                tom = cc
                if xx == x:
                    tom = luz
                elif xx >= x + larg - 2:
                    tom = sombra
                c.set(xx, y, tom)
        # frontão escalonado ou telhado inclinado
        if rnd.random() < 0.55:
            degraus = 4
            for d in range(degraus):
                y0 = topo - (d + 1) * 4
                margem = d * (larg // (2 * degraus + 1)) + 1
                for xx in range(x + margem, x + larg - margem):
                    for y in range(y0, y0 + 4):
                        c.set(xx, y, cc if xx < x + larg - margem - 1 else sombra)
        else:
            cor_t = telhado_cobre if rnd.random() < 0.3 else telhado
            for i in range(larg // 2 + 2):
                y = topo - i
                for xx in range(x - 1 + i, x + larg + 1 - i):
                    c.set(xx, y, cor_t if (xx + y) % 5 else misturar(cor_t, K, 0.2))
            # chaminé
            cx = x + larg // 3
            c.rect(cx, topo - larg // 4 - 6, 3, 7, cor('#7a4a3a'))
        # janelas (moldura clara, vidro escuro)
        for jy in range(topo + 6, H - 14, 12):
            for jx in range(x + 3, x + larg - 4, 6):
                c.rect(jx, jy, 4, 6, cor('#f4ecd8'))
                c.rect(jx + 1, jy + 1, 2, 4, cor('#3a4a66'))
                c.set(jx + 1, jy + 1, cor('#7f96c0'))
        # porta
        px = x + larg // 2 - 2
        c.rect(px, H - 12, 5, 12, cor('#4a2e22'))
        c.rect(px + 1, H - 11, 3, 2, cor('#7a5038'))
        # parede divisória
        c.vline(x + larg, topo, H - 1, misturar(cc, K, 0.45))
        x += larg + 1
    # bruma azulada: empurra as casas para o fundo e destaca os personagens
    bruma = cor('#9cc4ec')
    for y in range(H):
        for x in range(W):
            px = c.get(x, y)
            if px is not None:
                c.set(x, y, misturar(px, bruma, 0.22))
    return c


TORCIDA = [cor('#ffd21f'), cor('#1f9d48'), cor('#2557c9'), cor('#f4f4f4'), cor('#2a6ab8'), cor('#ffcd2a'),
           cor('#d8352f'), cor('#f0c49c'), cor('#8a5634')]


def estadio_arquibancada(fase):
    """Arquibancada do Råsunda com cobertura e torcida (2 quadros: a torcida pula)."""
    W, H = 256, 128
    c = Canvas(W, H)
    rnd = random.Random(58)
    concreto = [cor('#d6dae4'), cor('#b4bac8'), cor('#8e95a6'), cor('#6a7182')]
    # estrutura e cobertura
    for x in range(W):
        for y in range(18, H):
            c.set(x, y, concreto[2])
    for x in range(W):
        c.set(x, 8, concreto[0])
        for y in range(9, 16):
            c.set(x, y, concreto[1] if y < 13 else concreto[3])
        if x % 32 == 6:
            for y in range(16, H):
                c.set(x, y, concreto[3])
                c.set(x + 1, y, concreto[2])
    # degraus com torcida
    for linha, y in enumerate(range(22, H - 8, 7)):
        for x in range(W):
            c.set(x, y + 5, concreto[1])
            c.set(x, y + 6, concreto[3])
        for x in range(1, W - 2, 4):
            pulo = 1 if (fase == 1 and (x // 4 + linha) % 3 == 0) else 0
            cabeca = TORCIDA[7] if rnd.random() < 0.6 else TORCIDA[8]
            setor = (x // 40 + linha // 3) % 4
            paleta = [(0, 1, 3), (4, 5, 3), (0, 2, 3), (1, 0, 6)][setor]
            camisa = TORCIDA[paleta[rnd.randint(0, 2)]]
            yy = y + 1 - pulo
            c.set(x, yy, cabeca)
            c.set(x + 1, yy, cabeca)
            c.rect(x, yy + 1, 2, 3, camisa)
            c.set(x + 1, yy + 3, misturar(camisa, K, 0.3))
            if pulo and (x // 4) % 2 == 0:
                c.set(x + 2, yy - 1, cabeca)   # braço para cima
    # bandeiras na arquibancada: verde-amarelas e azul-amarelas
    for i, bx in enumerate(range(20, W - 10, 42)):
        by = 30 + (i * 17) % 50
        onda = 1 if fase == 1 else 0
        if i % 2 == 0:
            c.rect(bx, by + onda, 12, 8, cor('#1f9d48'))
            c.poligono([(bx + 6, by + 1 + onda), (bx + 11, by + 4 + onda), (bx + 6, by + 7 + onda),
                        (bx + 1, by + 4 + onda)], cor('#ffd21f'))
        else:
            c.rect(bx, by + onda, 12, 8, cor('#2a6ab8'))
            c.rect(bx + 3, by + onda, 2, 8, cor('#ffcd2a'))
            c.rect(bx, by + 3 + onda, 12, 2, cor('#ffcd2a'))
        c.vline(bx - 1, by - 2, by + 9, concreto[3])
    # cores da torcida um pouco apagadas (o gramado e os jogadores ficam em primeiro plano)
    for y in range(18, H - 8):
        for x in range(W):
            px = c.get(x, y)
            if px is not None:
                c.set(x, y, misturar(px, concreto[2], 0.18))
    # muro e placa do estádio na frente
    for x in range(W):
        for y in range(H - 8, H):
            c.set(x, y, cor('#2f7a34') if y < H - 6 else cor('#1f5a28'))
        c.set(x, H - 8, cor('#e8e4d8'))
    return c


def fundos():
    """Imagens únicas (uma por arquivo)."""
    return [('fundo_ceu', fundo_ceu(), (0.5, 0.5)), ('fundo_nuvens', fundo_nuvens(), (0.5, 0.5)),
            ('fundo_cidade', fundo_cidade(), (0.5, 0.5)), ('fundo_gamla_stan', fundo_gamla_stan(), (0.5, 0.5)),
            ('estadio_arquibancada_0', estadio_arquibancada(0), (0.5, 0.0)),
            ('estadio_arquibancada_1', estadio_arquibancada(1), (0.5, 0.0))]
