# -*- coding: utf-8 -*-
"""
Itens, objetos da fase, efeitos visuais e interface do Cinco Estrelas:
a bola (e a aura de fogo do especial), coração, caixote, pilha de barris,
sino e poste, ponte levadiça, barco, bandeira de checkpoint, placa, portão,
gol, cartas de poder (P01 e P05), estrelas, partículas e peças de UI.
"""
import math
import random

from pixelart import Canvas, cor, misturar, com_alpha

K = cor('#1e1622')
BRANCO = cor('#ffffff')
OURO = [cor('#fff7c2'), cor('#ffe066'), cor('#f5b62a'), cor('#c77a16'), cor('#7a4410')]
VERDE = cor('#1f9d48')
VERDE_S = cor('#13703a')
VERDE_H = cor('#5fd47e')
AMARELO = cor('#ffd21f')
AZUL = cor('#2557c9')
MADEIRA = [cor('#e0b07a'), cor('#c08a52'), cor('#9a6a3c'), cor('#6e4826'), cor('#4a2e18')]
FERRO = [cor('#9aa2b4'), cor('#6c7487'), cor('#454b5c'), cor('#2a2d38')]
BRONZE = [cor('#ffe6a0'), cor('#e8b34a'), cor('#c0862a'), cor('#8a5a1c'), cor('#5a3a12')]
NAVY = cor('#13233a')

# micro fonte 3x5 (para as cartas e a placa)
MICRO = {
    'P': ["###", "#.#", "###", "#..", "#.."], '0': ["###", "#.#", "#.#", "#.#", "###"],
    '1': [".#.", "##.", ".#.", ".#.", "###"], '5': ["###", "#..", "###", "..#", "###"],
    '!': [".#.", ".#.", ".#.", "...", ".#."],
}


def micro_texto(c, x, y, texto, cc):
    for ch in texto:
        g = MICRO[ch]
        for j, linha in enumerate(g):
            for i, p in enumerate(linha):
                if p == '#':
                    c.set(x + i, y + j, cc)
        x += 4


# ------------------------------------------------------------ bola
def bola():
    """Bola de couro clássica (10x10): branca, com as costuras dos gomos."""
    c = Canvas(10, 10)
    branco = cor('#f7f7f2')
    sombra = cor('#cfcabc')
    costura = cor('#8a8478')
    c.elipse_fn(5, 5, 4.4, 4.4, lambda x, y, nx, ny: sombra if nx + ny > 0.8 else branco)
    # costuras: uma curva horizontal e duas verticais (gomos de couro)
    for (x, y) in ((1, 4), (2, 5), (3, 5), (4, 5), (5, 5), (6, 5), (7, 4), (8, 4),
                   (4, 1), (4, 2), (3, 3), (3, 4), (6, 6), (6, 7), (5, 8)):
        c.set(x, y, costura)
    c.set(6, 2, costura)
    c.set(7, 2, costura)
    c.set(3, 2, BRANCO)
    c.set(2, 3, BRANCO)
    c.contorno(K)
    return c


def bola_goleiro():
    """Bola marrom antiga (a que o goleiro arremessa)."""
    c = Canvas(10, 10)
    c.elipse_fn(5, 5, 4.4, 4.4, lambda x, y, nx, ny: cor('#8a4a24') if nx + ny > 0.7 else cor('#c07a3c'))
    c.vline(5, 2, 7, cor('#f2e6c8'))
    for y in (3, 5, 7):
        c.set(4, y, cor('#f2e6c8'))
        c.set(6, y, cor('#f2e6c8'))
    c.set(3, 2, cor('#e8b07a'))
    c.contorno(K)
    return c


def bola_fogo(fase):
    """Aura de fogo (20x20) desenhada atrás da bola no especial da torcida: chamas subindo."""
    c = Canvas(20, 20)
    cores = [cor('#fff4b0', 235), cor('#ffd23a', 225), cor('#ff8a1e', 205), cor('#e23c18', 175)]
    cx, cy = 10.0, 11.5
    for y in range(20):
        for x in range(20):
            dx = x + 0.5 - cx
            dy = y + 0.5 - cy
            ang = math.atan2(dy, dx)
            # línguas de fogo: maiores para cima (dy < 0), tremulando com a fase
            lingua = 1.0 + 0.55 * max(0.0, -math.sin(ang)) + 0.22 * math.sin(ang * 5 + fase * 1.7)
            r = math.sqrt(dx * dx + dy * dy) / (6.2 * lingua)
            if r < 0.45:
                cc = cores[0]
            elif r < 0.68:
                cc = cores[1]
            elif r < 0.86:
                cc = cores[2]
            elif r < 1.0:
                cc = cores[3]
            else:
                continue
            c.set(x, y, cc)
    # faíscas
    for (x, y) in ((3 + fase % 2, 4), (15 - fase % 2, 3), (9 + (fase % 3), 1)):
        c.set(x, y, cores[1])
    return c


# ------------------------------------------------------------ coração
def coracao(fase):
    c = Canvas(12, 12)
    esc = [1.0, 1.12][fase % 2]
    cx, cy = 6.0, 6.3
    for y in range(12):
        for x in range(12):
            px = (x + 0.5 - cx) / (3.9 * esc)
            py = -(y + 0.5 - cy) / (3.9 * esc)
            v = (px * px + py * py - 0.55) ** 3 - px * px * (py ** 3) * 1.2
            if v <= 0:
                cc = cor('#e8323e')
                if px < -0.25 and py > 0.15:
                    cc = cor('#ff8a8a')
                elif px + (-py) > 0.55:
                    cc = cor('#9a1a2e')
                c.set(x, y, cc)
    c.set(3, 3, BRANCO)
    c.contorno(K)
    if fase % 2:
        for (x, y) in ((0, 1), (11, 2), (11, 9)):
            c.set(x, y, OURO[0])
    return c


# ------------------------------------------------------------ caixote e barris
def caixote():
    c = Canvas(16, 16)
    for y in range(1, 16):
        for x in range(1, 15):
            cc = MADEIRA[1] if (y // 4) % 2 == 0 else MADEIRA[2]
            if y % 4 == 0:
                cc = MADEIRA[3]
            c.set(x, y, cc)
    # moldura e travessa em X
    for x in range(1, 15):
        c.set(x, 1, MADEIRA[0])
        c.set(x, 2, MADEIRA[2])
        c.set(x, 14, MADEIRA[3])
    for y in range(1, 16):
        c.set(1, y, MADEIRA[3])
        c.set(2, y, MADEIRA[2])
        c.set(13, y, MADEIRA[2])
        c.set(14, y, MADEIRA[3])
    c.linha(3, 3, 12, 13, MADEIRA[0])
    c.linha(3, 4, 12, 14, MADEIRA[3])
    for (x, y) in ((2, 2), (13, 2), (2, 13), (13, 13)):
        c.set(x, y, FERRO[1])
    c.contorno(K)
    return c


def lasca(fase):
    c = Canvas(4, 4)
    formas = [[(0, 1), (1, 1), (2, 2), (3, 2)], [(1, 0), (1, 1), (2, 2), (2, 3)], [(0, 0), (1, 1), (2, 1), (3, 1)]]
    for i, (x, y) in enumerate(formas[fase % 3]):
        c.set(x, y, MADEIRA[1] if i < 2 else MADEIRA[3])
    return c


def _barril_deitado(c, x0, y0, w, h):
    """Barril deitado (vista de lado) num retângulo w x h."""
    for y in range(y0, y0 + h):
        t = (y - y0) / max(1.0, h - 1)
        for x in range(x0, x0 + w):
            meio = abs((x - x0) / max(1.0, w - 1) - 0.5) * 2
            if (meio > 0.92 and (t < 0.12 or t > 0.88)):
                continue
            cc = MADEIRA[1]
            if t < 0.2:
                cc = MADEIRA[0]
            elif t > 0.75:
                cc = MADEIRA[3]
            elif t > 0.55:
                cc = MADEIRA[2]
            if (x - x0) in (3, w - 4):
                cc = FERRO[2]
            c.set(x, y, cc)


def _barril_em_pe(c, x0, y0, w, h):
    for y in range(y0, y0 + h):
        for x in range(x0, x0 + w):
            meio = abs((y - y0) / max(1.0, h - 1) - 0.5) * 2
            margem = 1 if meio > 0.8 else 0
            if x < x0 + margem or x > x0 + w - 1 - margem:
                continue
            cc = MADEIRA[2]
            if x < x0 + 2:
                cc = MADEIRA[3]
            elif x > x0 + w - 4:
                cc = MADEIRA[1]
            if (y - y0) in (2, h - 3):
                cc = FERRO[2]
            c.set(x, y, cc)


def barris():
    """Pilha de barris (2x5 tiles) amarrada com cordas: só cai com carga máxima."""
    c = Canvas(32, 80)
    corda = cor('#d8c08a')
    # camadas alternadas: barris deitados e barris em pé
    y = 80
    camada = 0
    while y > 0:
        if camada % 2 == 0:
            h = 15
            _barril_deitado(c, 1, y - h, 30, h)
        else:
            h = 17
            _barril_em_pe(c, 1, y - h, 15, h)
            _barril_em_pe(c, 16, y - h, 15, h)
        y -= h
        camada += 1
    # cordas cruzando a pilha
    for yy in range(4, 80):
        c.set(9 + (yy // 6) % 2, yy, corda)
        c.set(22 - (yy // 6) % 2, yy, corda)
    c.contorno(K)
    # placa de aviso "!" pregada
    c.rect(11, 34, 10, 10, cor('#f2d36a'))
    c.contorno_rect(11, 34, 10, 10, K)
    micro_texto(c, 14, 36, "!", cor('#c0281e'))
    return c


def barril_pedaco(fase):
    c = Canvas(8, 8)
    if fase == 0:
        c.linha(1, 6, 6, 1, MADEIRA[2])
        c.linha(2, 6, 7, 1, MADEIRA[1])
        c.set(4, 3, FERRO[2])
    else:
        c.rect(1, 3, 6, 2, MADEIRA[2])
        c.hline(1, 6, 3, MADEIRA[1])
        c.set(3, 4, FERRO[2])
    c.contorno(K)
    return c


# ------------------------------------------------------------ sino e poste
def sino(fase):
    """Sino de bronze (16x18) pendurado pelo topo: parado, balançando para um lado e para o outro."""
    c = Canvas(16, 18)
    ang = [0.0, -0.2, 0.2][fase % 3]
    ox, oy = 8.0, 1.2   # ponto onde ele pendura
    ca, sa = math.cos(ang), math.sin(ang)

    def gira(px, py):
        dx, dy = px - ox, py - oy
        return (ox + dx * ca - dy * sa, oy + dx * sa + dy * ca)

    # perfil do sino (meia-largura em cada altura), espelhado: coroa, cintura e boca
    perfil = [(0.0, 1.2), (1.0, 2.6), (2.0, 3.3), (4.0, 3.5), (7.0, 3.8), (10.0, 4.4), (12.0, 5.2),
              (13.3, 5.9), (14.2, 5.9)]
    pts = [gira(ox + r, oy + y) for (y, r) in perfil] + [gira(ox - r, oy + y) for (y, r) in reversed(perfil)]

    def tinta(x, y):
        # luz da esquerda, girando junto com o sino
        rx, ry = x + 0.5 - ox, y + 0.5 - oy
        lx = rx * ca + ry * sa
        if lx < -2.8:
            return BRONZE[1]
        if lx < -1.0:
            return BRONZE[0]
        if lx > 3.0:
            return BRONZE[3]
        return BRONZE[2]

    c.poligono(pts, tinta)
    # friso perto da boca
    f0, f1 = gira(ox - 5.0, oy + 12.6), gira(ox + 5.0, oy + 12.6)
    c.linha(f0[0], f0[1], f1[0], f1[1], BRONZE[3])
    # badalo aparecendo na boca
    bad = gira(ox, oy + 15.2)
    c.elipse(bad[0], bad[1], 1.3, 1.3, BRONZE[4])
    # argola no topo
    a = gira(ox, oy - 0.6)
    c.set(a[0], a[1], FERRO[2])
    c.contorno(K)
    return c


def sino_poste():
    """Poste de ferro (8x64) com braço no topo; o sino pendura no meio, no alto."""
    c = Canvas(8, 64)
    for y in range(4, 62):
        c.set(3, y, FERRO[1])
        c.set(4, y, FERRO[2])
    for y in (16, 30, 44):
        c.hline(2, 5, y, FERRO[2])
    # base
    c.rect(1, 60, 6, 4, FERRO[2])
    c.hline(1, 6, 60, FERRO[1])
    # braço do topo e gancho
    c.hline(0, 7, 2, FERRO[2])
    c.hline(1, 6, 3, FERRO[1])
    c.set(3, 1, FERRO[1])
    c.set(4, 1, FERRO[2])
    c.contorno(K)
    return c


# ------------------------------------------------------------ ponte levadiça
def ponte(ponta=False):
    c = Canvas(16, 16)
    for y in range(0, 6):
        for x in range(16):
            cc = MADEIRA[1]
            if y == 0:
                cc = MADEIRA[0]
            elif y == 5:
                cc = MADEIRA[3]
            if x % 8 == 7 and 0 < y < 5:
                cc = MADEIRA[3]
            c.set(x, y, cc)
    # viga de ferro e rebites
    c.rect(0, 6, 16, 2, FERRO[2])
    for x in (2, 6, 10, 14):
        c.set(x, 6, FERRO[0])
    if ponta:
        # dobradiça de ferro (eixo onde a ponte gira)
        c.elipse(3, 6, 3, 3, FERRO[1])
        c.elipse(3, 6, 1.4, 1.4, FERRO[3])
        c.rect(0, 2, 6, 3, FERRO[2])
    c.hline(0, 15, 8, K)
    return c


# ------------------------------------------------------------ barco
def barco():
    """Barcaça de Estocolmo (64x32). O convés fica exatamente na metade (y = 16 a partir de baixo)."""
    c = Canvas(64, 32)
    casco = cor('#8a2e24')
    casco_s = cor('#5e1c18')
    faixa = cor('#f2ead2')
    conves = MADEIRA[1]
    # casco abaixo do convés (linhas 16..31), afinando nas pontas
    for y in range(16, 30):
        t = (y - 16) / 14.0
        recuo = int(round(t * t * 9))
        for x in range(1 + recuo, 63 - recuo):
            cc = casco
            if y == 18 or y == 19:
                cc = faixa
            elif y > 25:
                cc = casco_s
            c.set(x, y, cc)
    # convés (linha 16 é a tábua de cima)
    for x in range(1, 63):
        c.set(x, 15, MADEIRA[0])
        c.set(x, 16, conves)
        c.set(x, 17, MADEIRA[3])
    # cabine e mastro acima do convés
    c.rect(36, 6, 14, 9, cor('#f2ead2'))
    c.rect(36, 4, 14, 2, cor('#3a4a66'))
    c.rect(39, 8, 3, 3, cor('#7fb6e0'))
    c.rect(44, 8, 3, 3, cor('#7fb6e0'))
    c.vline(18, 2, 14, MADEIRA[3])
    # flâmula azul e amarela no mastro
    for x in range(19, 25):
        for y in range(2, 5):
            c.set(x, y + (1 if x > 22 else 0), cor('#2a6ab8') if y != 3 else cor('#ffcd2a'))
    # boias e corda
    c.elipse(10, 12, 2.4, 2.4, cor('#e85a3a'))
    c.elipse(10, 12, 1, 1, cor('#f2ead2'))
    c.hline(4, 34, 14, cor('#d8c08a'))
    c.contorno(K)
    return c


# ------------------------------------------------------------ bandeira de checkpoint
def bandeira(ligada, fase=0):
    """Mastro com flâmula: cinza caída (desligada) ou verde e amarela tremulando (ligada)."""
    c = Canvas(16, 48)
    for y in range(6, 48):
        cc = cor('#f4f4f4') if (y // 4) % 2 == 0 else cor('#c9c9d6')
        c.set(4, y, cc)
        c.set(5, y, cor('#c9c9d6'))
    c.rect(2, 45, 6, 3, FERRO[2])
    c.set(4, 5, OURO[1])
    c.set(5, 5, OURO[2])
    if not ligada:
        for y in range(7, 17):
            larg = 3 if y < 14 else 2
            c.hline(6, 6 + larg, y, cor('#8a8894') if y % 3 else cor('#6e6c78'))
    else:
        # flâmula triangular verde com a ponta amarela e uma estrela
        for x in range(6, 16):
            onda = int(round(math.sin((x - 6) * 0.8 + fase * 1.6) * 1.2))
            meia = 5 - (x - 6) * 0.45
            for y in range(int(12 - meia), int(12 + meia) + 1):
                yy = y + onda
                cc = VERDE if x < 12 else AMARELO
                if y == int(12 - meia):
                    cc = VERDE_H if x < 12 else OURO[0]
                c.set(x, yy, cc)
        c.set(8, 11 + int(round(math.sin(2 * 0.8 + fase * 1.6) * 1.2)), AMARELO)
    c.contorno(K)
    return c


# ------------------------------------------------------------ placa
def placa():
    c = Canvas(16, 24)
    # poste
    c.rect(7, 12, 2, 12, MADEIRA[3])
    c.vline(7, 12, 23, MADEIRA[2])
    # tabuleta
    for y in range(1, 12):
        for x in range(1, 15):
            cc = MADEIRA[1]
            if y == 1:
                cc = MADEIRA[0]
            elif y >= 10:
                cc = MADEIRA[3]
            c.set(x, y, cc)
    c.hline(3, 12, 4, MADEIRA[3])
    c.hline(3, 10, 6, MADEIRA[3])
    c.hline(3, 11, 8, MADEIRA[3])
    c.rect(11, 3, 2, 2, cor('#ffd21f'))
    c.contorno(K)
    return c


# ------------------------------------------------------------ portão
def portao():
    """Portão de ferro alto (1x6 tiles) com lanças no topo."""
    c = Canvas(16, 96)
    for x in (2, 6, 10, 14):
        for y in range(6, 96):
            c.set(x - 1, y, FERRO[2])
            c.set(x, y, FERRO[1] if y % 9 else FERRO[0])
        # ponta de lança
        c.set(x - 1, 4, FERRO[1])
        c.set(x, 4, FERRO[1])
        c.set(x - 1, 3, FERRO[0])
        c.set(x, 2, OURO[1])
    for y in (10, 46, 86):
        c.rect(0, y, 16, 3, FERRO[2])
        c.hline(0, 15, y, FERRO[0])
    # brasão de ferro (sem símbolo real: um círculo com a estrela)
    c.elipse(8, 28, 5, 5, FERRO[2])
    c.elipse(8, 28, 3.5, 3.5, OURO[2])
    c.set(8, 26, OURO[0])
    c.hline(7, 9, 28, OURO[0])
    c.set(8, 29, OURO[0])
    c.contorno(K)
    return c


# ------------------------------------------------------------ gol
def gol():
    """
    Trave (48x56). O poste da frente fica em x = 8..9 (pivô x = 8/48), o
    travessão a 48 px do chão (3 tiles) e a rede vai até x = 40.
    """
    W, H = 48, 56
    c = Canvas(W, H)
    rede = cor('#e8eef6', 150)
    rede2 = cor('#c8d2e2', 120)
    chao = H - 1
    trave_y = H - 48
    # rede: lateral de cima e o fundo
    for y in range(trave_y, H):
        for x in range(10, 41):
            if (x - 10) % 4 == 0 or (y - trave_y) % 4 == 0:
                c.set(x, y, rede if x < 38 else rede2)
    # caimento da rede no topo (do travessão até o poste de trás)
    for x in range(10, 41):
        yy = trave_y - 6 + int((x - 10) / 30.0 * 6)
        c.set(x, yy, rede)
        for y in range(yy, trave_y):
            if (x - 10) % 4 == 0:
                c.set(x, y, rede2)
    # poste de trás e suporte
    c.vline(40, trave_y - 1, chao, cor('#c8cdd8'))
    c.linha(10, trave_y - 6, 40, trave_y - 1, cor('#c8cdd8'))
    # travessão e poste da frente (brancos, com sombra)
    c.rect(8, trave_y - 1, 34, 2, BRANCO)
    c.hline(8, 41, trave_y, cor('#c8cdd8'))
    c.rect(8, trave_y - 1, 2, H - trave_y + 1, BRANCO)
    c.vline(9, trave_y, chao, cor('#c8cdd8'))
    c.contorno(K)
    return c


# ------------------------------------------------------------ cartas de poder
def _carta(fundo, borda, desenho, codigo):
    c = Canvas(24, 32)
    for y in range(32):
        for x in range(24):
            c.set(x, y, borda)
    for y in range(2, 30):
        for x in range(2, 22):
            c.set(x, y, misturar(fundo, BRANCO, 0.25) if y < 8 else fundo)
    desenho(c)
    # faixa do código embaixo
    c.rect(3, 23, 18, 7, NAVY)
    micro_texto(c, 6, 24, codigo, OURO[0])
    for (x, y) in ((0, 0), (23, 0), (0, 31), (23, 31)):
        c.set(x, y, None)
    c.contorno(K)
    return c


def carta_canhao():
    def desenho(c):
        # bola em chamas indo para a direita
        cores = [cor('#ffd23a'), cor('#ff8a1e'), cor('#e23c18')]
        for i, cc in enumerate(cores):
            c.poligono([(3 + i, 13), (12, 8 - i), (12, 18 + i)], cc)
        b = bola()
        c.colar(b, 11, 8)
    return _carta(cor('#c0392b'), OURO[2], desenho, "P01")


def carta_caneleira():
    def desenho(c):
        # caneleira dourada
        c.poligono([(8, 5), (16, 5), (17, 9), (16, 20), (12, 22), (8, 20), (7, 9)], OURO[1])
        c.poligono([(9, 6), (11, 6), (10, 19), (9, 18)], OURO[0])
        c.hline(9, 15, 12, OURO[3])
        c.hline(9, 15, 15, OURO[3])
        c.linha(8, 5, 7, 9, OURO[3])
        c.linha(16, 5, 17, 9, OURO[3])
    return _carta(cor('#2a4a8a'), OURO[1], desenho, "P05")


# ------------------------------------------------------------ estrelas
def _estrela_pts(cx, cy, r_ext, r_int, rot=-90):
    pts = []
    for i in range(10):
        r = r_ext if i % 2 == 0 else r_int
        a = math.radians(rot + i * 36)
        pts.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
    return pts


def estrela(tamanho, cheia):
    c = Canvas(tamanho, tamanho)
    m = tamanho / 2.0
    pts = _estrela_pts(m, m + tamanho * 0.04, m - 0.8, (m - 0.8) * 0.42)
    if cheia:
        def tinta(x, y):
            v = (x + y) / float(tamanho)
            if v < 0.62:
                return OURO[0] if v < 0.35 else OURO[1]
            return OURO[2] if v < 1.25 else OURO[3]
        c.poligono(pts, tinta)
        if tamanho >= 32:
            # brilho
            for i in range(-3, 4):
                c.set(m - tamanho * 0.18 + i, m - tamanho * 0.12, com_alpha(BRANCO, 200))
    else:
        c.poligono(pts, lambda x, y: cor('#4a4f66') if (x + y) / float(tamanho) < 1.0 else cor('#3a3e52'))
    c.contorno(K)
    return c


# ------------------------------------------------------------ efeitos
def luz_suave():
    c = Canvas(32, 32)
    for y in range(32):
        for x in range(32):
            d = math.hypot(x + 0.5 - 16, y + 0.5 - 16) / 16.0
            if d < 1:
                a = int(255 * (1 - d) ** 2)
                if a > 0:
                    c.set(x, y, cor('#ffffff', a))
    return c


def anel():
    c = Canvas(32, 32)
    for y in range(32):
        for x in range(32):
            d = math.hypot(x + 0.5 - 16, y + 0.5 - 16)
            if 13.5 <= d <= 15.5:
                a = 255 if 14 <= d <= 15 else 120
                c.set(x, y, cor('#ffffff', a))
            elif 11.5 <= d < 13.5:
                c.set(x, y, cor('#ffffff', int(70 * (d - 11.5) / 2.0)))
    return c


def brilho(fase):
    c = Canvas(8, 8)
    r = [1, 2, 3, 2][fase % 4]
    col = [BRANCO, cor('#fff6b8'), cor('#ffe066'), cor('#ffe066')][fase % 4]
    for i in range(-r, r + 1):
        a = 255 if abs(i) < r else 170
        c.set(4 + i, 4, com_alpha(col, a))
        c.set(4, 4 + i, com_alpha(col, a))
    if fase in (1, 2):
        for (sx, sy) in ((1, 1), (-1, -1), (1, -1), (-1, 1)):
            c.set(4 + sx, 4 + sy, com_alpha(col, 170))
    return c


def poeira(fase):
    c = Canvas(16, 16)
    r = [2.0, 3.0, 3.8, 4.4, 4.8][fase % 5]
    a = [235, 210, 160, 105, 50][fase % 5]
    base = cor('#e8dcc4', a)
    c.elipse(8, 11, r, r * 0.7, base)
    c.elipse(4.5, 12, r * 0.6, r * 0.45, base)
    c.elipse(11.5, 12, r * 0.6, r * 0.45, base)
    return c


def fumaca(fase):
    c = Canvas(16, 16)
    r = [2.5, 3.5, 4.5, 5.2, 5.8][fase % 5]
    a = [220, 180, 130, 80, 40][fase % 5]
    c.elipse(8, 9, r, r * 0.85, cor('#d8d6e0', a))
    c.elipse(7, 8, r * 0.6, r * 0.5, cor('#ffffff', a))
    return c


def confete(fase):
    tam = [(3, 3), (2, 4), (4, 2)][fase % 3]
    c = Canvas(tam[0], tam[1])
    c.rect(0, 0, tam[0], tam[1], BRANCO)
    return c


def rastro():
    c = Canvas(6, 6)
    c.elipse(3, 3, 2.8, 2.8, cor('#ffffff', 150))
    c.elipse(3, 3, 1.6, 1.6, cor('#ffffff', 230))
    return c


def gota(fase):
    c = Canvas(4, 4)
    if fase == 0:
        c.rect(1, 1, 2, 2, cor('#9fd3f0'))
        c.set(1, 1, BRANCO)
    else:
        c.set(1, 0, cor('#9fd3f0'))
        c.rect(0, 1, 3, 2, cor('#5aa0d0'))
        c.set(1, 3, cor('#5aa0d0'))
    return c


def exclamacao():
    c = Canvas(6, 12)
    c.rect(2, 1, 2, 7, AMARELO)
    c.set(2, 1, OURO[0])
    c.rect(2, 9, 2, 2, AMARELO)
    c.contorno(K)
    return c


def interrogacao():
    c = Canvas(8, 12)
    c.ascii(1, 1, [".####.", "##..##", "....##", "...##.", "..##..", "......", "..##.."], {'#': BRANCO})
    c.contorno(K)
    return c


def estrelinha(fase):
    c = Canvas(7, 7)
    if fase == 0:
        c.set(3, 1, AMARELO)
        c.hline(1, 5, 3, AMARELO)
        c.set(2, 4, AMARELO)
        c.set(4, 4, AMARELO)
        c.set(3, 2, AMARELO)
        c.set(3, 3, BRANCO)
    else:
        c.set(3, 2, AMARELO)
        c.hline(2, 4, 3, AMARELO)
        c.set(3, 4, AMARELO)
        c.set(3, 3, BRANCO)
    c.contorno(K)
    return c


def seta():
    c = Canvas(12, 8)
    c.poligono([(1, 2.5), (6, 2.5), (6, 0.5), (11, 4), (6, 7.5), (6, 5.5), (1, 5.5)], AMARELO)
    c.hline(2, 6, 3, OURO[0])
    c.contorno(K)
    return c


def _rosto_icone(tamanho, pele, cabelo, estilo):
    c = Canvas(tamanho, tamanho)
    m = tamanho / 2.0
    c.elipse_fn(m, m + 0.5, m - 2.2, m - 1.8,
                lambda x, y, nx, ny: pele[1] if nx < -0.5 or ny > 0.6 else (pele[2] if nx > 0.2 and ny < -0.2 else pele[0]))
    if estilo == 'black':
        c.elipse_fn(m - 0.3, m - 1.2, m - 0.6, m - 1.4,
                    lambda x, y, nx, ny: cabelo if (ny < -0.15 or nx < -0.55) else None)
    else:
        c.elipse_fn(m, m - 0.8, m - 1.6, m - 1.8,
                    lambda x, y, nx, ny: cabelo if (ny < -0.3 or nx < -0.6) else None)
    ex, ey = int(m + 1), int(m)
    c.set(ex, ey, cor('#1d1a2c'))
    c.set(ex, ey + 1, cor('#1d1a2c'))
    c.set(ex - 1, ey, cor('#f7f3e8'))
    c.set(ex - 1, ey + 1, cor('#f7f3e8'))
    c.hline(ex - 1, ex, ey + 3, cor('#6e2430'))
    c.contorno(K)
    return c


def icone_vida():
    return _rosto_icone(12, (cor('#b9794f'), cor('#8c5536'), cor('#d99a6c')), cor('#1c1820'), 'crespo')


def icone_companheiro():
    return _rosto_icone(14, (cor('#8a5634'), cor('#653a22'), cor('#a96e46')), cor('#1c1820'), 'black')


# ------------------------------------------------------------ interface
def ui_painel():
    """Painel azul-marinho com moldura dourada (9-slice, borda de 8 px)."""
    c = Canvas(24, 24)
    fundo = cor('#13233a', 240)
    for y in range(24):
        for x in range(24):
            c.set(x, y, fundo)
    camadas = [K, cor('#8a6a1c'), cor('#f5c84c'), cor('#c79a2c'), cor('#0b1628')]
    for i, cc in enumerate(camadas):
        for x in range(i, 24 - i):
            c.set(x, i, cc)
            c.set(x, 23 - i, cc)
        for y in range(i, 24 - i):
            c.set(i, y, cc)
            c.set(23 - i, y, cc)
    for (x, y) in ((2, 2), (21, 2), (2, 21), (21, 21)):
        c.set(x, y, OURO[0])
    for (x, y) in ((0, 0), (23, 0), (0, 23), (23, 23)):
        c.set(x, y, None)
    return c


def ui_botao():
    """Botão verde com borda clara (9-slice, borda de 5 px)."""
    c = Canvas(16, 16)
    for y in range(16):
        for x in range(16):
            cc = VERDE
            if y in (1, 2):
                cc = VERDE_H
            if y in (12, 13):
                cc = VERDE_S
            c.set(x, y, cc)
    for x in range(16):
        c.set(x, 0, K)
        c.set(x, 15, K)
        c.set(x, 14, cor('#0c4a24'))
    for y in range(16):
        c.set(0, y, K)
        c.set(15, y, K)
    for x in range(2, 14):
        c.set(x, 3, com_alpha(AMARELO, 110))
    for (x, y) in ((0, 0), (15, 0), (0, 15), (15, 15)):
        c.set(x, y, None)
    return c


def ui_tecla():
    c = Canvas(12, 12)
    for y in range(12):
        for x in range(12):
            cc = cor('#e8e4dc')
            if y >= 9:
                cc = cor('#9a948c')
            if y <= 1:
                cc = BRANCO
            c.set(x, y, cc)
    for x in range(12):
        c.set(x, 0, K)
        c.set(x, 11, K)
    for y in range(12):
        c.set(0, y, K)
        c.set(11, y, K)
    for (x, y) in ((0, 0), (11, 0), (0, 11), (11, 11)):
        c.set(x, y, None)
    return c


def ui_branco():
    c = Canvas(4, 4)
    c.rect(0, 0, 4, 4, BRANCO)
    return c


def ui_coracao(cheio):
    c = Canvas(9, 8)
    forma = [".##.##.", "#######", "#######", ".#####.", "..###..", "...#..."]
    for j, linha in enumerate(forma):
        for i, ch in enumerate(linha):
            if ch == '#':
                if cheio:
                    cc = cor('#e8323e')
                    if j <= 1 and i <= 2:
                        cc = cor('#ff8a8a')
                    elif j >= 3:
                        cc = cor('#b8202e')
                else:
                    cc = cor('#4a3a4a')
                c.set(i + 1, j + 1, cc)
    c.contorno(K)
    return c


# ------------------------------------------------------------ listas
def quadros_itens():
    q = [('bola', bola()), ('bola_goleiro', bola_goleiro())]
    q += [('bola_fogo_%d' % i, bola_fogo(i)) for i in range(4)]
    q += [('coracao_%d' % i, coracao(i)) for i in range(2)]
    q += [('caixote', caixote())]
    q += [('lasca_%d' % i, lasca(i)) for i in range(3)]
    q += [('barris', barris())]
    q += [('barril_pedaco_%d' % i, barril_pedaco(i)) for i in range(2)]
    q += [('sino_%d' % i, sino(i)) for i in range(3)]
    q += [('sino_poste', sino_poste()), ('ponte', ponte(False)), ('ponte_ponta', ponte(True)), ('barco', barco())]
    q += [('bandeira_off', bandeira(False))]
    q += [('bandeira_on_%d' % i, bandeira(True, i)) for i in range(4)]
    q += [('placa', placa()), ('portao', portao()), ('gol', gol())]
    q += [('carta_canhao', carta_canhao()), ('carta_caneleira', carta_caneleira())]
    q += [('estrela_cheia', estrela(16, True)), ('estrela_vazia', estrela(16, False)),
          ('estrela_grande_cheia', estrela(48, True)), ('estrela_grande_vazia', estrela(48, False))]
    q += [('luz_suave', luz_suave()), ('anel', anel())]
    q += [('brilho_%d' % i, brilho(i)) for i in range(4)]
    q += [('poeira_%d' % i, poeira(i)) for i in range(5)]
    q += [('fumaca_%d' % i, fumaca(i)) for i in range(5)]
    q += [('confete_%d' % i, confete(i)) for i in range(3)]
    q += [('rastro', rastro())]
    q += [('gota_%d' % i, gota(i)) for i in range(2)]
    q += [('exclamacao', exclamacao()), ('interrogacao', interrogacao())]
    q += [('estrelinha_%d' % i, estrelinha(i)) for i in range(2)]
    q += [('seta', seta()), ('icone_companheiro', icone_companheiro()), ('icone_vida', icone_vida())]
    return q


def quadros_ui():
    return [('ui_painel', ui_painel()), ('ui_botao', ui_botao()), ('ui_tecla', ui_tecla()),
            ('ui_branco', ui_branco()), ('ui_coracao_cheio', ui_coracao(True)),
            ('ui_coracao_vazio', ui_coracao(False))]
