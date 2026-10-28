# -*- coding: utf-8 -*-
"""
Leitura e escrita de PNG em Python puro (só zlib + struct), para gerar a arte
do Cinco Estrelas sem instalar nada (nem Pillow).

As imagens são listas de linhas; cada pixel é uma tupla RGBA (0-255) ou None
(transparente). A linha 0 é a de CIMA.
"""
import struct
import zlib


def _chunk(tipo, dados):
    corpo = tipo + dados
    return struct.pack(">I", len(dados)) + corpo + struct.pack(">I", zlib.crc32(corpo) & 0xFFFFFFFF)


def salvar(caminho, largura, altura, pixels):
    """Salva um PNG RGBA 8 bits. pixels[y][x] = (r, g, b, a) ou None."""
    bruto = bytearray()
    vazio = (0, 0, 0, 0)
    for y in range(altura):
        bruto.append(0)  # filtro "nenhum" na linha
        linha = pixels[y]
        for x in range(largura):
            c = linha[x]
            if c is None:
                c = vazio
            bruto.extend((int(c[0]) & 255, int(c[1]) & 255, int(c[2]) & 255, int(c[3]) & 255))
    cabecalho = struct.pack(">IIBBBBB", largura, altura, 8, 6, 0, 0, 0)
    dados = b"\x89PNG\r\n\x1a\n"
    dados += _chunk(b"IHDR", cabecalho)
    dados += _chunk(b"IDAT", zlib.compress(bytes(bruto), 9))
    dados += _chunk(b"IEND", b"")
    with open(caminho, "wb") as f:
        f.write(dados)


def salvar_ampliado(caminho, largura, altura, pixels, escala=4, fundo=None):
    """Salva uma cópia ampliada (vizinho mais próximo), útil para conferir a arte."""
    novas = []
    for y in range(altura):
        linha = []
        for x in range(largura):
            c = pixels[y][x]
            if c is None or c[3] == 0:
                c = fundo
            elif fundo is not None and c[3] < 255:
                a = c[3] / 255.0
                c = (int(c[0] * a + fundo[0] * (1 - a)), int(c[1] * a + fundo[1] * (1 - a)),
                     int(c[2] * a + fundo[2] * (1 - a)), 255)
            linha.extend([c] * escala)
        for _ in range(escala):
            novas.append(linha)
    salvar(caminho, largura * escala, altura * escala, novas)


def ler(caminho):
    """Lê um PNG RGBA/RGB de 8 bits sem entrelaçamento (só para conferências)."""
    with open(caminho, "rb") as f:
        dados = f.read()
    if dados[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError("não é PNG: " + caminho)
    pos = 8
    idat = b""
    largura = altura = tipo_cor = 0
    while pos < len(dados):
        n = struct.unpack(">I", dados[pos:pos + 4])[0]
        tipo = dados[pos + 4:pos + 8]
        corpo = dados[pos + 8:pos + 8 + n]
        pos += 12 + n
        if tipo == b"IHDR":
            largura, altura, bits, tipo_cor = struct.unpack(">IIBB", corpo[:10])
            if bits != 8 or tipo_cor not in (2, 6):
                raise ValueError("formato de PNG não suportado")
        elif tipo == b"IDAT":
            idat += corpo
        elif tipo == b"IEND":
            break
    canais = 4 if tipo_cor == 6 else 3
    bruto = zlib.decompress(idat)
    passo = largura * canais
    linhas = []
    anterior = bytearray(passo)
    i = 0
    for _ in range(altura):
        filtro = bruto[i]
        atual = bytearray(bruto[i + 1:i + 1 + passo])
        i += 1 + passo
        for x in range(passo):
            a = atual[x - canais] if x >= canais else 0
            b = anterior[x]
            c = anterior[x - canais] if x >= canais else 0
            if filtro == 1:
                atual[x] = (atual[x] + a) & 255
            elif filtro == 2:
                atual[x] = (atual[x] + b) & 255
            elif filtro == 3:
                atual[x] = (atual[x] + ((a + b) >> 1)) & 255
            elif filtro == 4:
                p = a + b - c
                pa, pb, pc = abs(p - a), abs(p - b), abs(p - c)
                pr = a if (pa <= pb and pa <= pc) else (b if pb <= pc else c)
                atual[x] = (atual[x] + pr) & 255
        linha = []
        for x in range(largura):
            k = x * canais
            px = tuple(atual[k:k + canais])
            if canais == 3:
                px = px + (255,)
            linha.append(px if px[3] > 0 else None)
        linhas.append(linha)
        anterior = atual
    return largura, altura, linhas
