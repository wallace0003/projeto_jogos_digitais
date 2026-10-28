# -*- coding: utf-8 -*-
"""
Personagens do Cinco Estrelas: o Camisa 10 e o companheiro (camisa amarela,
gola verde, calção azul, meião branco) e os adversários de 1958 (uniformes
ORIGINAIS: carrinho de vermelho, saltador de branco, capitão de grená e o
goleiro de preto com boné, como nos anos 50).

Todos usam o mesmo "boneco" com esqueleto: cada pose diz onde ficam cabeça,
pescoço, quadril, mãos, cotovelos, joelhos e tornozelos (em pixels de um
quadro 32x32, olhando para a direita, pés na última linha). O desenho é
montado por camadas (braço e perna de trás mais escuros), com contorno de 1 px.
"""
import math

from pixelart import Canvas, cor, misturar

K = cor('#1e1622')
OLHO_B = cor('#f7f3e8')
PUPILA = cor('#1d1a2c')
BOCA = cor('#6e2430')
DENTE = cor('#f4f0e8')

# ------------------------------------------------------------------ uniformes
CAMISA10 = {
    'camisa': cor('#ffd21f'), 'camisa_s': cor('#d9a000'), 'camisa_h': cor('#fff07a'),
    'gola': cor('#1f9d48'), 'punho': cor('#1f9d48'),
    'calcao': cor('#2557c9'), 'calcao_s': cor('#173a8f'),
    'meiao': cor('#f4f4f4'), 'meiao_s': cor('#c4c4d2'), 'faixa_meiao': cor('#1f9d48'),
    'chuteira': cor('#20202a'), 'chuteira_h': cor('#5a5a6a'),
    'manga_longa': False, 'luva': None,
}

CARRINHO = {
    'camisa': cor('#d8352f'), 'camisa_s': cor('#9e1f22'), 'camisa_h': cor('#f26a52'),
    'gola': cor('#f4f4f4'), 'punho': cor('#f4f4f4'),
    'calcao': cor('#f2f2f6'), 'calcao_s': cor('#b8b8c8'),
    'meiao': cor('#d8352f'), 'meiao_s': cor('#9e1f22'), 'faixa_meiao': cor('#f4f4f4'),
    'chuteira': cor('#2a1c18'), 'chuteira_h': cor('#5c4436'),
    'manga_longa': False, 'luva': None,
}

SALTADOR = {
    'camisa': cor('#f6f6fa'), 'camisa_s': cor('#bfc4d6'), 'camisa_h': cor('#ffffff'),
    'gola': cor('#1d2a5e'), 'punho': cor('#1d2a5e'),
    'calcao': cor('#1d2a5e'), 'calcao_s': cor('#111a3c'),
    'meiao': cor('#1d2a5e'), 'meiao_s': cor('#111a3c'), 'faixa_meiao': cor('#f6f6fa'),
    'chuteira': cor('#20202a'), 'chuteira_h': cor('#4a4a5a'),
    'manga_longa': False, 'luva': None,
}

CAPITAO = {
    'camisa': cor('#8c1f3c'), 'camisa_s': cor('#5e1026'), 'camisa_h': cor('#b8435f'),
    'gola': cor('#f2e6c8'), 'punho': cor('#f2e6c8'),
    'calcao': cor('#f2f2f6'), 'calcao_s': cor('#b8b8c8'),
    'meiao': cor('#8c1f3c'), 'meiao_s': cor('#5e1026'), 'faixa_meiao': cor('#f2e6c8'),
    'chuteira': cor('#20202a'), 'chuteira_h': cor('#4a4a5a'),
    'manga_longa': False, 'luva': None, 'bracadeira': cor('#ffcf33'),
}

GOLEIRO = {
    'camisa': cor('#3a3e4c'), 'camisa_s': cor('#262833'), 'camisa_h': cor('#62687c'),
    'gola': cor('#e6e6ec'), 'punho': cor('#e6e6ec'),
    'calcao': cor('#a2a8b8'), 'calcao_s': cor('#727890'),
    'meiao': cor('#3a3e4c'), 'meiao_s': cor('#262833'), 'faixa_meiao': cor('#e6e6ec'),
    'chuteira': cor('#20202a'), 'chuteira_h': cor('#4a4a5a'),
    'manga_longa': True, 'luva': (cor('#efe6cf'), cor('#bfb294')),
}

PELES = {
    'morena': (cor('#b9794f'), cor('#8c5536'), cor('#d99a6c')),
    'escura': (cor('#8a5634'), cor('#653a22'), cor('#a96e46')),
    'clara': (cor('#f0c49c'), cor('#c9946c'), cor('#ffdcbc')),
    'media': (cor('#d9a074'), cor('#a8724c'), cor('#f0bf94')),
    'rosada': (cor('#f2b496'), cor('#c98468'), cor('#ffd0b8')),
}

CABELOS = {
    'preto': (cor('#1c1820'), cor('#3e3644')),
    'castanho': (cor('#5a3418'), cor('#7e4e2a')),
    'loiro': (cor('#e8b84a'), cor('#ffe08a')),
    'ruivo': (cor('#a84a1c'), cor('#d06a2c')),
}

BOLA_B = cor('#f7f7f2')
BOLA_S = cor('#c9c6bc')
BOLA_P = cor('#2a2630')


# ------------------------------------------------------------------ geometria
def _norm(vx, vy):
    d = math.sqrt(vx * vx + vy * vy)
    if d < 1e-6:
        return 0.0, 1.0
    return vx / d, vy / d


def _ik(a, b, l1, l2, frente):
    """Junta do meio (joelho/cotovelo) entre a e b; frente=1 dobra para +x, -1 para -x."""
    ax, ay = a
    bx, by = b
    dx, dy = bx - ax, by - ay
    d = math.sqrt(dx * dx + dy * dy)
    if d < 1e-6:
        return (ax + frente * l1, ay)
    if d >= l1 + l2 - 1e-3:
        t = l1 / (l1 + l2)
        return (ax + dx * t, ay + dy * t)
    # lei dos cossenos
    cos_a = (l1 * l1 + d * d - l2 * l2) / (2 * l1 * d)
    cos_a = max(-1.0, min(1.0, cos_a))
    ang = math.acos(cos_a)
    base = math.atan2(dy, dx)
    s1 = (ax + l1 * math.cos(base + ang), ay + l1 * math.sin(base + ang))
    s2 = (ax + l1 * math.cos(base - ang), ay + l1 * math.sin(base - ang))
    if frente > 0:
        return s1 if s1[0] >= s2[0] else s2
    return s1 if s1[0] <= s2[0] else s2


# ------------------------------------------------------------------ peças
def _membro(c, a, b, cor_fn, esp):
    c.linha_grossa(a[0], a[1], b[0], b[1], cor_fn, esp)


def _perna(c, quadril, joelho, tornozelo, dir_pe, kit, pele, sombra, esc):
    pele_c = pele[1] if sombra else pele[0]
    meiao = kit['meiao_s'] if sombra else kit['meiao']
    faixa = kit['faixa_meiao']
    esp = 2.6 * esc

    def coxa(t, lado):
        return pele_c

    def canela(t, lado):
        if t < 0.16:
            return faixa
        if not sombra and lado < -0.5:
            return misturar(meiao, (255, 255, 255, 255), 0.25)
        return meiao

    _membro(c, quadril, joelho, coxa, esp)
    _membro(c, joelho, tornozelo, canela, esp * 0.92)
    # chuteira: do calcanhar até a ponta, na direção dir_pe
    dx, dy = _norm(dir_pe[0], dir_pe[1])
    ch = kit['chuteira']
    x0, y0 = tornozelo[0] - dx * 0.5 * esc, tornozelo[1] - dy * 0.5 * esc
    x1, y1 = tornozelo[0] + dx * 2.2 * esc, tornozelo[1] + dy * 2.2 * esc
    c.linha_grossa(x0, y0, x1, y1, lambda t, l: ch, 1.9 * esc)
    if not sombra:
        c.set(x1, y1 - 0.6, kit['chuteira_h'])


def _braco(c, ombro, cotovelo, mao, kit, pele, sombra, esc, bracadeira=False):
    pele_c = pele[1] if sombra else pele[0]
    manga = kit['camisa_s'] if sombra else kit['camisa']
    longa = kit.get('manga_longa')
    esp = 2.2 * esc

    def superior(t, lado):
        if longa or t < 0.55:
            if bracadeira and 0.3 < t < 0.55 and kit.get('bracadeira'):
                return kit['bracadeira']
            return manga
        return pele_c

    def antebraco(t, lado):
        if longa:
            return manga if t < 0.82 else kit['punho']
        return pele_c

    _membro(c, ombro, cotovelo, superior, esp)
    _membro(c, cotovelo, mao, antebraco, esp * 0.92)
    luva = kit.get('luva')
    if luva:
        c.elipse(mao[0], mao[1], 1.5 * esc, 1.5 * esc, luva[1] if sombra else luva[0])
    else:
        c.elipse(mao[0], mao[1], 1.05 * esc, 1.05 * esc, pele[2] if not sombra else pele[1])


def _tronco(c, pescoco, quadril, kit, esc, largura):
    """Camisa como um polígono ao longo do eixo pescoço-quadril, com luz e sombra."""
    ax, ay = _norm(quadril[0] - pescoco[0], quadril[1] - pescoco[1])
    px, py = -ay, ax  # perpendicular ("frente" do corpo quando ele está em pé olhando para a direita: -px...)
    w_o = largura * 0.5 * esc
    w_q = largura * 0.45 * esc
    topo = (pescoco[0] - ax * 0.4 * esc, pescoco[1] - ay * 0.4 * esc)
    pts = [
        (topo[0] + px * w_o, topo[1] + py * w_o),
        (quadril[0] + px * w_q, quadril[1] + py * w_q),
        (quadril[0] - px * w_q, quadril[1] - py * w_q),
        (topo[0] - px * w_o, topo[1] - py * w_o),
    ]
    comp = math.sqrt((quadril[0] - topo[0]) ** 2 + (quadril[1] - topo[1]) ** 2)

    def tinta(x, y):
        # projeção no eixo perpendicular: lado "de trás" mais escuro
        rx, ry = x + 0.5 - topo[0], y + 0.5 - topo[1]
        lado = (rx * px + ry * py) / max(w_o, 0.5)
        ao_longo = (rx * ax + ry * ay) / max(comp, 0.5)
        # px aponta para a esquerda quando o eixo vai para baixo (em pé): lado > 0 = costas
        if lado > 0.55:
            return kit['camisa_s']
        if lado < -0.45 and ao_longo < 0.45:
            return kit['camisa_h']
        return kit['camisa']

    c.poligono(pts, tinta)
    # gola em "V" na frente do pescoço
    g = kit['gola']
    gx, gy = pescoco[0] - px * 0.9 * esc, pescoco[1] - py * 0.9 * esc
    c.set(gx, gy, g)
    c.set(gx - px * 1.0, gy - py * 1.0, g)
    c.set(gx + ax * 1.0, gy + ay * 1.0, g)
    c.set(pescoco[0], pescoco[1], g)
    return (ax, ay, px, py)


def _calcao(c, quadril, eixo, kit, esc, largura):
    ax, ay, px, py = eixo
    w = largura * 0.5 * esc + 0.2
    ini = (quadril[0] - ax * 1.2 * esc, quadril[1] - ay * 1.2 * esc)
    fim = (quadril[0] + ax * 2.6 * esc, quadril[1] + ay * 2.6 * esc)
    pts = [
        (ini[0] + px * w, ini[1] + py * w),
        (fim[0] + px * (w + 0.4), fim[1] + py * (w + 0.4)),
        (fim[0] - px * (w + 0.4), fim[1] - py * (w + 0.4)),
        (ini[0] - px * w, ini[1] - py * w),
    ]

    def tinta(x, y):
        rx, ry = x + 0.5 - ini[0], y + 0.5 - ini[1]
        lado = (rx * px + ry * py) / max(w, 0.5)
        ao_longo = (rx * ax + ry * ay)
        if lado > 0.45 or ao_longo > 3.0 * esc:
            return kit['calcao_s']
        return kit['calcao']

    c.poligono(pts, tinta)


def _cabeca(c, centro, pele, esc):
    cx, cy = centro
    rx, ry = 3.7 * esc, 3.9 * esc

    def tinta(x, y, nx, ny):
        if nx < -0.55 or ny > 0.66:
            return pele[1]
        if nx > 0.2 and ny < -0.15:
            return pele[2]
        return pele[0]

    c.elipse_fn(cx, cy, rx, ry, tinta)
    c.set(cx + rx - 0.2, cy + 0.6 * esc, pele[0])          # nariz
    c.set(cx - 0.9 * esc, cy + 0.5 * esc, pele[1])          # orelha
    return (cx, cy, rx, ry)


def _cabelo(c, estilo, cab, cores, esc):
    cx, cy, rx, ry = cab
    escuro, claro = cores
    if estilo == 'crespo':
        # cabelo curto e crespo, com volume em cima
        for y in range(int(cy - ry - 3), int(cy + 1)):
            for x in range(int(cx - rx - 2), int(cx + rx + 2)):
                nx = (x + 0.5 - cx) / (rx + 0.9)
                ny = (y + 0.5 - (cy - 0.6)) / (ry + 1.1)
                if nx * nx + ny * ny <= 1.0 and (ny < -0.28 or nx < -0.48):
                    c.set(x, y, claro if (x * 3 + y * 5) % 7 == 0 else escuro)
    elif estilo == 'black':
        # black power arredondado
        for y in range(int(cy - ry - 5), int(cy + 2)):
            for x in range(int(cx - rx - 3), int(cx + rx + 2)):
                nx = (x + 0.5 - (cx - 0.6)) / (rx + 1.9)
                ny = (y + 0.5 - (cy - 1.7)) / (ry + 1.9)
                if nx * nx + ny * ny <= 1.0 and (ny < -0.05 or nx < -0.42):
                    c.set(x, y, claro if (x * 5 + y * 3) % 9 == 0 else escuro)
    elif estilo == 'liso':
        # cabelo liso repartido de lado, com franja para a frente
        for y in range(int(cy - ry - 2), int(cy + 1)):
            for x in range(int(cx - rx - 1), int(cx + rx + 2)):
                nx = (x + 0.5 - cx) / (rx + 0.6)
                ny = (y + 0.5 - (cy - 0.4)) / (ry + 0.8)
                if nx * nx + ny * ny <= 1.0 and (ny < -0.42 or nx < -0.5):
                    c.set(x, y, claro if y == int(cy - ry - 1) and x % 2 == 0 else escuro)
        c.set(cx + rx, cy - ry + 1.2 * esc, escuro)
        c.set(cx + rx - 1, cy - ry + 1.6 * esc, escuro)
    elif estilo == 'curto':
        for y in range(int(cy - ry - 1), int(cy)):
            for x in range(int(cx - rx - 1), int(cx + rx + 1)):
                nx = (x + 0.5 - cx) / (rx + 0.4)
                ny = (y + 0.5 - (cy - 0.3)) / (ry + 0.6)
                if nx * nx + ny * ny <= 1.0 and (ny < -0.45 or nx < -0.55):
                    c.set(x, y, claro if (x + y) % 4 == 0 else escuro)
    elif estilo == 'calvo':
        # entradas (careca na frente) e cabelo dos lados/atrás
        for y in range(int(cy - ry - 1), int(cy + 1)):
            for x in range(int(cx - rx - 1), int(cx + rx + 1)):
                nx = (x + 0.5 - cx) / (rx + 0.5)
                ny = (y + 0.5 - (cy - 0.2)) / (ry + 0.6)
                if nx * nx + ny * ny <= 1.0 and nx < -0.35 and ny < 0.3:
                    c.set(x, y, escuro)
    elif estilo == 'bone':
        # boné de goleiro dos anos 50 (copa e aba para a frente)
        bone = cor('#2e3a5a')
        bone_h = cor('#4a5a86')
        for y in range(int(cy - ry - 2), int(cy - 0.4 * esc)):
            for x in range(int(cx - rx - 1), int(cx + rx + 1)):
                nx = (x + 0.5 - cx) / (rx + 0.5)
                ny = (y + 0.5 - (cy - 0.6)) / (ry + 0.7)
                if nx * nx + ny * ny <= 1.0 and ny < -0.18:
                    c.set(x, y, bone_h if ny < -0.8 else bone)
        aba_y = cy - 0.9 * esc
        c.hline(cx + 0.5 * esc, cx + rx + 2.4 * esc, aba_y, bone)
        c.set(cx + rx + 2.4 * esc, aba_y + 0.8, bone)
        # cabelo aparecendo atrás
        c.set(cx - rx + 0.2, cy, escuro)
        c.set(cx - rx + 0.6, cy + 0.8, escuro)


def _rosto(c, cab, expressao, brava, pele, esc, bigode=None):
    cx, cy, rx, ry = cab
    ex = cx + 1.4 * esc
    ey = cy - 0.4 * esc
    mx = cx + 1.6 * esc
    my = cy + 2.3 * esc
    if expressao == 'tonto':
        c.set(ex, ey - 1, PUPILA); c.set(ex + 1, ey - 1, PUPILA)
        c.set(ex + 1, ey, PUPILA); c.set(ex, ey + 1, PUPILA)
        c.set(ex, ey, OLHO_B)
        c.hline(mx - 1, mx + 1, my, BOCA)
        c.set(mx + 1, my - 1, BOCA)
    elif expressao == 'dor':
        c.set(ex - 1, ey - 1, PUPILA); c.set(ex + 1, ey - 1, PUPILA)
        c.set(ex, ey, PUPILA)
        c.set(ex - 1, ey + 1, PUPILA); c.set(ex + 1, ey + 1, PUPILA)
        c.hline(mx - 1, mx + 1, my, BOCA)
    elif expressao == 'fechado':
        c.hline(ex - 1, ex + 1, ey + 0.5, PUPILA)
        c.hline(mx - 1, mx, my, BOCA)
    else:
        # olho: branco + pupila virada para a frente
        c.rect(ex - 1, ey - 1, 2, 2, OLHO_B)
        c.set(ex, ey - 1, PUPILA)
        c.set(ex, ey, PUPILA)
        if expressao == 'cima':
            c.set(ex, ey, OLHO_B)
            c.set(ex - 1, ey - 1, PUPILA)
        if brava or expressao in ('grito', 'concentrado'):
            c.linha(ex - 2, ey - 3, ex + 1, ey - 2, K)
        else:
            c.hline(ex - 1, ex, ey - 2.4, misturar(K, pele[1], 0.3))
        if expressao == 'feliz':
            c.hline(mx - 2, mx + 1, my, BOCA)
            c.set(mx - 2, my - 1, BOCA)
            c.set(mx + 1, my - 1, BOCA)
        elif expressao == 'grito':
            c.rect(mx - 1, my - 1, 2, 2, BOCA)
            c.set(mx, my - 1, DENTE)
        elif expressao == 'susto':
            c.rect(mx - 1, my - 1, 2, 2, BOCA)
        else:
            c.hline(mx - 1, mx, my, BOCA)
    if bigode is not None:
        c.hline(mx - 1.5, mx + 1, my - 1, bigode)
        c.set(mx - 2, my, bigode)


def futebolista(pose, kit, pele='morena', cabelo=('crespo', 'preto'), largura_corpo=7.4, brava=False,
                tamanho=(32, 32), esc=1.0, bigode=False, desloc=(0, 0)):
    """
    Desenha um jogador numa pose. As coordenadas da pose são de um quadro
    32x32 (olhando para a direita); 'esc' aumenta o boneco (capitão) e
    'desloc' empurra a pose dentro de quadros mais largos (carrinho, mergulho).

    Chaves da pose: pescoco, quadril, cabeca (opcional), mao_t/mao_f, cot_t/cot_f
    (opcional, senão IK), pe_t/pe_f (tornozelos), joe_t/joe_f (opcional),
    dir_t/dir_f (direção da ponta da chuteira), rosto.
    """
    w, h = tamanho
    c = Canvas(w, h)
    p = PELES[pele]
    ox, oy = desloc

    def P(pt):
        return (pt[0] * esc + ox, pt[1] * esc + oy)

    pescoco = pose['pescoco']
    quadril = pose['quadril']
    ax, ay = _norm(quadril[0] - pescoco[0], quadril[1] - pescoco[1])
    px, py = -ay, ax
    cab = pose.get('cabeca', (pescoco[0] + 0.6 - ax * 4.0 + (ax * 0), pescoco[1] - ay * 4.0))
    # ombros e quadris (pontas de cada lado do eixo do corpo)
    ombro_t = (pescoco[0] + ax * 1.4 + px * 1.3, pescoco[1] + ay * 1.4 + py * 1.3)
    ombro_f = (pescoco[0] + ax * 1.4 - px * 1.1, pescoco[1] + ay * 1.4 - py * 1.1)
    quad_t = (quadril[0] + ax * 1.2 + px * 1.3, quadril[1] + ay * 1.2 + py * 1.3)
    quad_f = (quadril[0] + ax * 1.2 - px * 1.3, quadril[1] + ay * 1.2 - py * 1.3)

    mao_t, mao_f = pose['mao_t'], pose['mao_f']
    pe_t, pe_f = pose['pe_t'], pose['pe_f']
    cot_t = pose.get('cot_t') or _ik(ombro_t, mao_t, 3.6, 3.6, -1)
    cot_f = pose.get('cot_f') or _ik(ombro_f, mao_f, 3.6, 3.6, -1)
    joe_t = pose.get('joe_t') or _ik(quad_t, pe_t, 4.3, 4.3, 1)
    joe_f = pose.get('joe_f') or _ik(quad_f, pe_f, 4.3, 4.3, 1)
    dir_t = pose.get('dir_t', (1, 0))
    dir_f = pose.get('dir_f', (1, 0))

    _braco(c, P(ombro_t), P(cot_t), P(mao_t), kit, p, True, esc, False)
    _perna(c, P(quad_t), P(joe_t), P(pe_t), dir_t, kit, p, True, esc)
    eixo = _tronco(c, P(pescoco), P(quadril), kit, esc, largura_corpo)
    _calcao(c, P(quadril), eixo, kit, esc, largura_corpo)
    _perna(c, P(quad_f), P(joe_f), P(pe_f), dir_f, kit, p, False, esc)
    # pescoço
    pc = P(pescoco)
    c.elipse(pc[0] - eixo[0] * 0.8, pc[1] - eixo[1] * 0.8, 1.1 * esc, 1.1 * esc, p[1])
    cabeca = _cabeca(c, P(cab), p, esc)
    estilo, tom = cabelo
    _cabelo(c, estilo, cabeca, CABELOS[tom], esc)
    _rosto(c, cabeca, pose.get('rosto', 'normal'), brava, p, esc,
           CABELOS[tom][0] if bigode else None)
    _braco(c, P(ombro_f), P(cot_f), P(mao_f), kit, p, False, esc, kit.get('bracadeira') is not None)
    for extra in pose.get('extras', []):
        extra(c, P)
    c.contorno(K)
    return c


def bola_na_mao(pos):
    """Extra de pose: a bola segura nas mãos (goleiro)."""
    def f(c, P):
        x, y = P(pos)
        c.elipse(x, y, 2.6, 2.6, BOLA_B)
        c.set(x - 0.5, y - 0.5, BOLA_P)
        c.set(x + 1, y + 0.8, BOLA_P)
        c.set(x - 1.5, y + 1, BOLA_S)
    return f


# ======================================================================
# POSES BÁSICAS (quadro 32x32)
# ======================================================================
def em_pe(bob=0, mao_t=(13, 21), mao_f=(19.5, 21), pe_t=(13.5, 30), pe_f=(18, 30), rosto='normal',
          incl=0.0, cabeca=None, **extra):
    d = {
        'pescoco': (16 + incl, 13.5 + bob), 'quadril': (15.5, 20.8 + bob),
        'mao_t': (mao_t[0], mao_t[1] + bob), 'mao_f': (mao_f[0], mao_f[1] + bob),
        'pe_t': pe_t, 'pe_f': pe_f, 'rosto': rosto,
    }
    if cabeca:
        d['cabeca'] = cabeca
    d.update(extra)
    return d


CORRIDA = [
    # (pé de trás, pé da frente, mão de trás, mão da frente, bob)
    ((9.5, 29), (21.5, 30), (10, 20), (21.5, 18), 0),
    ((11, 27), (19.5, 30), (11, 21), (21, 19.5), 1),
    ((14, 26), (17, 30), (12.5, 21.5), (19.5, 21), 1),
    ((21.5, 30), (9.5, 29), (21.5, 18.5), (10.5, 20), 0),
    ((19.5, 30), (11, 27), (20.5, 20), (11.5, 21), 1),
    ((17, 30), (14, 26), (19.5, 21), (12.5, 21.5), 1),
]


def correndo(i, rosto='normal'):
    pb, pf, mb, mf, b = CORRIDA[i]
    d = {
        'pescoco': (17.2, 13.6 + b), 'quadril': (15.8, 20.8 + b),
        'mao_t': (mb[0], mb[1] + b), 'mao_f': (mf[0], mf[1] + b),
        'pe_t': pb, 'pe_f': pf, 'rosto': rosto,
    }
    # o pé que está no ar aponta um pouco para baixo
    if pb[1] < 30:
        d['dir_t'] = (1, 0.5)
    if pf[1] < 30:
        d['dir_f'] = (1, 0.5)
    return d


ANDAR = [
    ((12, 30), (19.5, 30), (12, 21), (20.5, 20.5), 0),
    ((13.5, 29), (18, 30), (13, 21.5), (19.5, 21), 1),
    ((19, 30), (13, 30), (14, 21), (18.5, 21.5), 0),
    ((17.5, 30), (14.5, 29), (13, 21.5), (19.5, 21), 1),
]


def andando(i, rosto='normal', incl=0.4):
    pb, pf, mb, mf, b = ANDAR[i]
    d = {
        'pescoco': (16 + incl, 13.5 + b), 'quadril': (15.5, 20.8 + b),
        'mao_t': (mb[0], mb[1] + b), 'mao_f': (mf[0], mf[1] + b),
        'pe_t': pb, 'pe_f': pf, 'rosto': rosto,
    }
    if pb[1] < 30:
        d['dir_t'] = (1, 0.4)
    if pf[1] < 30:
        d['dir_f'] = (1, 0.4)
    return d


# ======================================================================
# CAMISA 10
# ======================================================================
def camisa10(pose, tamanho=(32, 32), desloc=(0, 0)):
    return futebolista(pose, CAMISA10, 'morena', ('crespo', 'preto'), tamanho=tamanho, desloc=desloc)


def quadros_camisa10():
    q = []
    for i in range(4):
        b = 1 if i in (1, 2) else 0
        q.append(('camisa10_parado_%d' % i, camisa10(em_pe(bob=b))))
    for i in range(6):
        q.append(('camisa10_correr_%d' % i, camisa10(correndo(i))))
    # pulo: joelhos para cima, braços abertos
    q.append(('camisa10_pulo_0', camisa10({
        'pescoco': (16.5, 12.5), 'quadril': (15.5, 20.5),
        'mao_t': (10, 13), 'mao_f': (22.5, 12.5), 'pe_t': (12.5, 27), 'pe_f': (20, 26.5),
        'dir_t': (1, 0.6), 'dir_f': (1, 0.6), 'rosto': 'feliz'})))
    # queda: pernas esticadas, braços para cima
    q.append(('camisa10_queda_0', camisa10({
        'pescoco': (16, 13), 'quadril': (15.5, 21),
        'mao_t': (9.5, 15), 'mao_f': (22.5, 14.5), 'pe_t': (13, 30), 'pe_f': (19.5, 29),
        'dir_f': (1, 0.4), 'rosto': 'susto'})))
    # chute: preparação (perna atrás), batida (perna esticada) e acompanhamento (perna alta)
    q.append(('camisa10_chute_0', camisa10({
        'pescoco': (15.5, 13.5), 'quadril': (15.5, 20.8),
        'mao_t': (19.5, 18), 'mao_f': (11.5, 19), 'pe_t': (16, 30), 'pe_f': (8.5, 25),
        'dir_f': (0.4, 1), 'rosto': 'concentrado'})))
    q.append(('camisa10_chute_1', camisa10({
        'pescoco': (16.5, 13.5), 'quadril': (15.5, 20.8),
        'mao_t': (9.5, 18), 'mao_f': (21, 20), 'pe_t': (14, 30), 'pe_f': (23.5, 28),
        'dir_f': (1, -0.2), 'rosto': 'grito'})))
    q.append(('camisa10_chute_2', camisa10({
        'pescoco': (15.5, 13.8), 'quadril': (15.5, 20.8),
        'mao_t': (9, 17), 'mao_f': (20.5, 22), 'pe_t': (14, 30), 'pe_f': (24, 21.5),
        'dir_f': (1, -0.6), 'rosto': 'feliz'})))
    # carregando: perna lá atrás, corpo inclinado, tremendo
    for i in range(2):
        q.append(('camisa10_carregar_%d' % i, camisa10({
            'pescoco': (17 + i * 0.5, 14 + i * 0.3), 'quadril': (15.5, 20.8),
            'mao_t': (12, 17 - i), 'mao_f': (22, 19 + i * 0.5), 'pe_t': (17, 30), 'pe_f': (7 + i, 24),
            'dir_f': (0.3, 1), 'rosto': 'grito'})))
    # cavadinha: o pé entra por baixo da bola e sobe
    q.append(('camisa10_cavadinha_0', camisa10({
        'pescoco': (15.5, 13.8), 'quadril': (15.5, 20.8),
        'mao_t': (10, 18.5), 'mao_f': (20.5, 19.5), 'pe_t': (14, 30), 'pe_f': (22, 29.5),
        'dir_f': (1, -0.7), 'rosto': 'concentrado'})))
    q.append(('camisa10_cavadinha_1', camisa10({
        'pescoco': (15.2, 14), 'quadril': (15.5, 20.8),
        'mao_t': (9.5, 17), 'mao_f': (21, 18), 'pe_t': (14, 30), 'pe_f': (22.5, 25),
        'dir_f': (0.6, -1), 'rosto': 'feliz'})))
    # bicicleta: cai de costas e chuta por cima da cabeça
    q.append(('camisa10_bicicleta_0', camisa10({
        'pescoco': (14.5, 15), 'quadril': (17, 22),
        'cabeca': (12.5, 11.5), 'mao_t': (9, 17), 'mao_f': (19, 14),
        'pe_t': (19, 30), 'pe_f': (22, 23), 'dir_f': (1, -0.8), 'rosto': 'concentrado'})))
    q.append(('camisa10_bicicleta_1', camisa10({
        'pescoco': (11.5, 22), 'quadril': (18.5, 21),
        'cabeca': (7.5, 23.5), 'mao_t': (8, 28), 'mao_f': (13.5, 27.5),
        'pe_t': (24, 25.5), 'pe_f': (21.5, 13), 'dir_t': (1, 0.3), 'dir_f': (0.6, -1), 'rosto': 'grito'})))
    q.append(('camisa10_bicicleta_2', camisa10({
        'pescoco': (12, 24), 'quadril': (17.5, 19.5),
        'cabeca': (9, 27), 'mao_t': (7.5, 29), 'mao_f': (14, 29.5),
        'pe_t': (20, 25), 'pe_f': (24, 11.5), 'dir_t': (1, 0.4), 'dir_f': (1, -0.3), 'rosto': 'grito'})))
    q.append(('camisa10_bicicleta_3', camisa10({
        'pescoco': (13.5, 18.5), 'quadril': (17.5, 24),
        'cabeca': (11, 15.5), 'mao_t': (8.5, 22), 'mao_f': (14, 24),
        'pe_t': (16, 30), 'pe_f': (22.5, 26), 'dir_f': (1, 0.3), 'rosto': 'feliz'})))
    # pedalada: a perna da frente passa por cima da bola (por fora) e volta
    q.append(('camisa10_pedalada_0', camisa10(em_pe(
        mao_t=(11, 19.5), mao_f=(21, 19), pe_t=(13, 30), pe_f=(19.5, 26.5), dir_f=(1, 0.3),
        rosto='concentrado', incl=0.6))))
    q.append(('camisa10_pedalada_1', camisa10(em_pe(
        mao_t=(10.5, 18.5), mao_f=(21.5, 18), pe_t=(13, 30), pe_f=(24, 25.5), dir_f=(1, 0.6),
        rosto='concentrado', incl=1.0))))
    q.append(('camisa10_pedalada_2', camisa10(em_pe(
        mao_t=(11.5, 19), mao_f=(21, 20), pe_t=(14.5, 30), pe_f=(25, 29.5), dir_f=(1, 0),
        rosto='concentrado', incl=1.2))))
    q.append(('camisa10_pedalada_3', camisa10(em_pe(
        mao_t=(12.5, 20), mao_f=(20, 20.5), pe_t=(12, 29), pe_f=(19, 30), dir_t=(1, 0.5),
        rosto='feliz', incl=0.6))))
    # embaixadinha: o pé sobe (quadro 0) e desce
    for i in range(4):
        pe = (21, 25) if i == 0 else ((20, 27.5) if i in (1, 3) else (19, 30))
        q.append(('camisa10_embaixadinha_%d' % i, camisa10(em_pe(
            mao_t=(10.5, 19.5), mao_f=(22, 19.5), pe_t=(14, 30), pe_f=pe,
            dir_f=(1, -0.3) if i == 0 else (1, 0), rosto='cima'))))
    # dano: jogado para trás
    q.append(('camisa10_dano_0', camisa10({
        'pescoco': (14.5, 14.5), 'quadril': (16, 22),
        'mao_t': (9, 13), 'mao_f': (21, 13), 'pe_t': (12, 30), 'pe_f': (19, 29.5),
        'dir_f': (1, 0.3), 'rosto': 'dor'})))
    # vitória: braços para cima
    for i in range(2):
        q.append(('camisa10_vitoria_%d' % i, camisa10(em_pe(
            bob=-i, mao_t=(10, 6.5 + i), mao_f=(22.5, 6 + i), pe_t=(13, 30), pe_f=(19, 30), rosto='feliz'))))
    # chamando o companheiro: braço levantado, boca aberta
    q.append(('camisa10_chamar_0', camisa10(em_pe(
        mao_t=(10.5, 20), mao_f=(21, 8), pe_t=(13.5, 30), pe_f=(18.5, 30), rosto='grito'))))
    return q


# ======================================================================
# COMPANHEIRO DE EQUIPE
# ======================================================================
def companheiro(pose):
    return futebolista(pose, CAMISA10, 'escura', ('black', 'preto'))


def quadros_companheiro():
    q = []
    for i in range(2):
        q.append(('companheiro_parado_%d' % i, companheiro(em_pe(bob=i))))
    for i in range(6):
        q.append(('companheiro_correr_%d' % i, companheiro(correndo(i, 'feliz'))))
    q.append(('companheiro_passe_0', companheiro({
        'pescoco': (15.5, 13.5), 'quadril': (15.5, 20.8),
        'mao_t': (19.5, 18), 'mao_f': (11.5, 19.5), 'pe_t': (16, 30), 'pe_f': (10, 27),
        'dir_f': (0.5, 1), 'rosto': 'concentrado'})))
    q.append(('companheiro_passe_1', companheiro({
        'pescoco': (16.5, 13.5), 'quadril': (15.5, 20.8),
        'mao_t': (10, 18.5), 'mao_f': (21, 20), 'pe_t': (14, 30), 'pe_f': (22.5, 27.5),
        'dir_f': (1, 0.1), 'rosto': 'feliz'})))
    for i in range(2):
        q.append(('companheiro_comemorar_%d' % i, companheiro(em_pe(
            bob=-i, mao_t=(10.5, 7 + i), mao_f=(22, 6 + i), pe_t=(13, 30), pe_f=(19, 30), rosto='feliz'))))
    return q


# ======================================================================
# ZAGUEIRO CARRINHO (I01)
# ======================================================================
def carrinho(pose, tamanho=(32, 32), desloc=(0, 0)):
    return futebolista(pose, CARRINHO, 'clara', ('liso', 'castanho'), brava=True, largura_corpo=7.8,
                       tamanho=tamanho, desloc=desloc)


def quadros_carrinho():
    q = []
    for i in range(4):
        q.append(('carrinho_andar_%d' % i, carrinho(andando(i, 'concentrado'))))
    # preparando: agachado, corpo para a frente, braços para trás
    q.append(('carrinho_preparar_0', carrinho({
        'pescoco': (18.5, 16.5), 'quadril': (14.5, 23.5),
        'mao_t': (10, 22), 'mao_f': (13, 23.5), 'pe_t': (11, 30), 'pe_f': (20, 30),
        'rosto': 'grito'})))
    # carrinho rasteiro (48x32): corpo deitado para trás, perna esticada rente ao chão
    for i in range(2):
        q.append(('carrinho_deslizar_%d' % i, carrinho({
            'pescoco': (10.5 + i, 23.5), 'quadril': (18 + i, 27.5),
            'cabeca': (8 + i, 21),
            'mao_t': (6 + i, 29.5), 'mao_f': (14 + i, 25),
            'pe_t': (27 + i, 30), 'pe_f': (31 + i * 2, 29.5),
            'joe_t': (22 + i, 26.5), 'joe_f': (25 + i, 28.5),
            'dir_t': (1, 0.2), 'dir_f': (1, -0.4), 'rosto': 'grito'},
            tamanho=(48, 32), desloc=(8, 0))))
    # sentado no chão depois do carrinho (não machuca)
    q.append(('carrinho_sentado_0', carrinho({
        'pescoco': (13.5, 20), 'quadril': (14.5, 27.5),
        'mao_t': (10, 30), 'mao_f': (19, 26), 'pe_t': (22.5, 30), 'pe_f': (25, 30),
        'joe_t': (19, 26.5), 'joe_f': (21, 27), 'rosto': 'dor'})))
    q.append(('carrinho_atingido_0', carrinho({
        'pescoco': (14, 14.5), 'quadril': (16, 22),
        'mao_t': (9, 14), 'mao_f': (21, 12.5), 'pe_t': (12.5, 30), 'pe_f': (19.5, 29),
        'dir_f': (1, 0.4), 'rosto': 'dor'})))
    q.append(('carrinho_distraido_0', carrinho(em_pe(
        mao_t=(12.5, 21), mao_f=(19, 21.5), rosto='cima', cabeca=(16.2, 9.2)))))
    return q


# ======================================================================
# ZAGUEIRO SALTADOR (I04)
# ======================================================================
def saltador(pose):
    return futebolista(pose, SALTADOR, 'rosada', ('curto', 'loiro'), brava=True)


def quadros_saltador():
    q = []
    for i in range(2):
        q.append(('saltador_parado_%d' % i, saltador(em_pe(
            bob=i, mao_t=(12, 20 + i), mao_f=(20.5, 20 + i), pe_t=(12.5, 30), pe_f=(19, 30),
            rosto='concentrado'))))
    q.append(('saltador_agachar_0', saltador({
        'pescoco': (16.5, 17), 'quadril': (15, 24),
        'mao_t': (11, 24), 'mao_f': (20, 24.5), 'pe_t': (11.5, 30), 'pe_f': (20, 30),
        'rosto': 'concentrado'})))
    # no ar, cabeceando: braços para cima, corpo esticado
    q.append(('saltador_pulo_0', saltador({
        'pescoco': (17, 11.5), 'quadril': (15.5, 19.5),
        'cabeca': (18.6, 7.8), 'mao_t': (10.5, 9), 'mao_f': (22.5, 8),
        'pe_t': (13, 28.5), 'pe_f': (17.5, 29), 'dir_t': (0.6, 1), 'dir_f': (0.6, 1), 'rosto': 'grito'})))
    q.append(('saltador_aterrissar_0', saltador({
        'pescoco': (17, 18), 'quadril': (15, 25),
        'mao_t': (9.5, 21), 'mao_f': (22, 21), 'pe_t': (11, 30), 'pe_f': (21, 30),
        'rosto': 'concentrado'})))
    q.append(('saltador_atingido_0', saltador({
        'pescoco': (14, 14.5), 'quadril': (16, 22),
        'mao_t': (9, 13.5), 'mao_f': (21, 12.5), 'pe_t': (12.5, 30), 'pe_f': (19.5, 29),
        'dir_f': (1, 0.4), 'rosto': 'dor'})))
    q.append(('saltador_distraido_0', saltador(em_pe(
        mao_t=(12.5, 21), mao_f=(19, 21.5), rosto='cima', cabeca=(16.2, 9.2)))))
    return q


# ======================================================================
# CAPITÃO (I07) - quadros 40x40 (boneco 25% maior)
# ======================================================================
ESC_CAPITAO = 1.25


def capitao(pose, tamanho=(40, 40), desloc=(0, 0)):
    return futebolista(pose, CAPITAO, 'media', ('calvo', 'castanho'), brava=True, largura_corpo=8.6,
                       tamanho=tamanho, esc=ESC_CAPITAO, bigode=True, desloc=desloc)


def quadros_capitao():
    q = []
    for i in range(4):
        q.append(('capitao_andar_%d' % i, capitao(andando(i, 'concentrado', incl=0.6))))
    q.append(('capitao_preparar_0', capitao({
        'pescoco': (18.5, 16.5), 'quadril': (14.5, 23.5),
        'mao_t': (10, 22), 'mao_f': (13, 23.5), 'pe_t': (11, 30), 'pe_f': (20, 30),
        'rosto': 'grito'})))
    for i in range(2):
        q.append(('capitao_deslizar_%d' % i, capitao({
            'pescoco': (10.5 + i, 23.5), 'quadril': (18 + i, 27.5),
            'cabeca': (8 + i, 21),
            'mao_t': (6 + i, 29.5), 'mao_f': (14 + i, 25),
            'pe_t': (27 + i, 30), 'pe_f': (31 + i * 2, 29.5),
            'joe_t': (22 + i, 26.5), 'joe_f': (25 + i, 28.5),
            'dir_t': (1, 0.2), 'dir_f': (1, -0.4), 'rosto': 'grito'},
            tamanho=(56, 40), desloc=(8, 0))))
    q.append(('capitao_sentado_0', capitao({
        'pescoco': (13.5, 20), 'quadril': (14.5, 27.5),
        'mao_t': (10, 30), 'mao_f': (19, 26), 'pe_t': (22.5, 30), 'pe_f': (25, 30),
        'joe_t': (19, 26.5), 'joe_f': (21, 27), 'rosto': 'dor'})))
    # salto: subindo (joelhos encolhidos, braços para cima) e descendo (pernas para baixo)
    q.append(('capitao_salto_0', capitao({
        'pescoco': (16.5, 12.5), 'quadril': (15.5, 20.5),
        'mao_t': (10, 10), 'mao_f': (22.5, 9.5), 'pe_t': (12.5, 26.5), 'pe_f': (20, 25.5),
        'dir_t': (1, 0.6), 'dir_f': (1, 0.6), 'rosto': 'grito'})))
    q.append(('capitao_salto_1', capitao({
        'pescoco': (16.5, 13), 'quadril': (15.5, 21),
        'mao_t': (9, 16), 'mao_f': (23, 15), 'pe_t': (13, 30), 'pe_f': (19.5, 29.5),
        'dir_f': (1, 0.5), 'rosto': 'grito'})))
    q.append(('capitao_aterrissar_0', capitao({
        'pescoco': (17, 18.5), 'quadril': (15, 25),
        'mao_t': (9.5, 22), 'mao_f': (22, 22), 'pe_t': (10.5, 30), 'pe_f': (21.5, 30),
        'rosto': 'grito'})))
    for i in range(2):
        q.append(('capitao_tonto_%d' % i, capitao(em_pe(
            bob=i, mao_t=(10 + i, 20), mao_f=(21 - i, 19), pe_t=(13 + i, 30), pe_f=(19 - i, 30),
            rosto='tonto', incl=-0.8 + i * 1.6))))
    q.append(('capitao_atingido_0', capitao({
        'pescoco': (14, 14.5), 'quadril': (16, 22),
        'mao_t': (9, 14), 'mao_f': (21, 12.5), 'pe_t': (12.5, 30), 'pe_f': (19.5, 29),
        'dir_f': (1, 0.4), 'rosto': 'dor'})))
    q.append(('capitao_distraido_0', capitao(em_pe(
        mao_t=(12.5, 21), mao_f=(19, 21.5), rosto='cima', cabeca=(16.2, 9.2)))))
    return q


# ======================================================================
# GOLEIRO (I08)
# ======================================================================
def goleiro(pose, tamanho=(32, 32), desloc=(0, 0)):
    return futebolista(pose, GOLEIRO, 'clara', ('bone', 'castanho'), largura_corpo=7.8,
                       tamanho=tamanho, desloc=desloc)


def quadros_goleiro():
    q = []
    for i in range(2):
        q.append(('goleiro_parado_%d' % i, goleiro({
            'pescoco': (16.5, 14.5 + i), 'quadril': (15.5, 22 + i),
            'mao_t': (12.5, 18 + i), 'mao_f': (21.5, 18.5 + i), 'pe_t': (12, 30), 'pe_f': (19.5, 30),
            'rosto': 'concentrado'})))
    q.append(('goleiro_pronto_0', goleiro({
        'pescoco': (17, 16.5), 'quadril': (15.5, 23.5),
        'mao_t': (11, 15), 'mao_f': (23, 14.5), 'pe_t': (11, 30), 'pe_f': (20.5, 30),
        'rosto': 'concentrado'})))
    for i in range(4):
        pb, pf, mb, mf, b = CORRIDA[(i * 3) // 2]
        q.append(('goleiro_correr_%d' % i, goleiro({
            'pescoco': (17.2, 13.6 + b), 'quadril': (15.8, 20.8 + b),
            'mao_t': (mb[0], mb[1] + b), 'mao_f': (mf[0] + 0.5, mf[1] + b - 1),
            'pe_t': pb, 'pe_f': pf, 'rosto': 'grito',
            'dir_t': (1, 0.5) if pb[1] < 30 else (1, 0), 'dir_f': (1, 0.5) if pf[1] < 30 else (1, 0)})))
    q.append(('goleiro_espalmar_0', goleiro({
        'pescoco': (16.5, 12.5), 'quadril': (15.5, 20.5),
        'mao_t': (12, 5.5), 'mao_f': (21, 5), 'pe_t': (13, 29), 'pe_f': (19, 28.5),
        'dir_t': (0.7, 1), 'dir_f': (0.7, 1), 'rosto': 'grito'})))
    q.append(('goleiro_segurar_0', goleiro({
        'pescoco': (16, 14), 'quadril': (15.5, 22),
        'mao_t': (17.5, 18.5), 'mao_f': (20, 18), 'pe_t': (12.5, 30), 'pe_f': (19, 30),
        'rosto': 'feliz', 'extras': [bola_na_mao((19.5, 17.5))]})))
    q.append(('goleiro_arremesso_0', goleiro({
        'pescoco': (15.5, 13.5), 'quadril': (15.5, 20.8),
        'mao_t': (21, 18), 'mao_f': (9.5, 9.5), 'pe_t': (12.5, 30), 'pe_f': (20, 30),
        'rosto': 'concentrado', 'extras': [bola_na_mao((9, 8))]})))
    q.append(('goleiro_arremesso_1', goleiro({
        'pescoco': (17.5, 14), 'quadril': (15.5, 20.8),
        'mao_t': (11, 20), 'mao_f': (24, 13), 'pe_t': (12, 30), 'pe_f': (20.5, 29.5),
        'rosto': 'grito'})))
    q.append(('goleiro_levantar_0', goleiro({
        'pescoco': (17.5, 18), 'quadril': (14.5, 24.5),
        'mao_t': (13, 28), 'mao_f': (21.5, 27.5), 'pe_t': (10.5, 30), 'pe_f': (20.5, 30),
        'joe_t': (14, 29.5), 'dir_t': (-0.3, 1), 'rosto': 'concentrado'})))
    q.append(('goleiro_comemorar_0', goleiro(em_pe(
        mao_t=(11, 20), mao_f=(21.5, 8.5), pe_t=(13, 30), pe_f=(19, 30), rosto='grito'))))
    # mergulho (48x32): voando de lado, braços esticados para a frente
    q.append(('goleiro_mergulho_0', goleiro({
        'pescoco': (20, 17), 'quadril': (12.5, 19.5),
        'cabeca': (24, 15.5), 'mao_t': (28, 12.5), 'mao_f': (30.5, 14.5),
        'pe_t': (2, 18), 'pe_f': (3.5, 22.5), 'joe_t': (7, 18.5), 'joe_f': (8, 21.5),
        'dir_t': (-0.3, 1), 'dir_f': (-0.3, 1), 'rosto': 'grito'},
        tamanho=(48, 32), desloc=(8, 0))))
    # caído no chão (48x32): deitado de lado
    q.append(('goleiro_caido_0', goleiro({
        'pescoco': (19.5, 26.5), 'quadril': (11.5, 27.5),
        'cabeca': (23.5, 26.5), 'mao_t': (26, 30), 'mao_f': (24.5, 23.5),
        'pe_t': (1.5, 30), 'pe_f': (3, 27.5), 'joe_t': (6.5, 29.5), 'joe_f': (7, 27),
        'dir_t': (-0.2, 1), 'dir_f': (-0.2, 1), 'rosto': 'tonto'},
        tamanho=(48, 32), desloc=(8, 0))))
    return q


def quadros_jogador():
    return quadros_camisa10() + quadros_companheiro()


def quadros_adversarios():
    return quadros_carrinho() + quadros_saltador() + quadros_capitao() + quadros_goleiro()
