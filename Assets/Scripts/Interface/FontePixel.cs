using System.Collections.Generic;
using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Fonte "pixel art" própria (maiúsculas, minúsculas e acentos do português).
    /// O desenho das letras fica em <see cref="GlifosFonte"/>; aqui o atlas é
    /// montado em tempo de execução, então o jogo não depende de arquivo de fonte
    /// (nem do TextMeshPro).
    /// </summary>
    public static class FontePixel
    {
        public const int AlturaGlifo = GlifosFonte.AlturaGlifo;
        const int CelulaL = 7;
        const int CelulaA = 12;
        const int TamanhoAtlas = 128;

        public struct Glifo
        {
            public int largura;
            /// <summary>Em coordenadas 0..1 do atlas.</summary>
            public Rect uv;
            /// <summary>Em pixels do atlas.</summary>
            public Rect pixels;
        }

        static Texture2D atlas;
        static Dictionary<char, Glifo> glifos;
        static Dictionary<char, Sprite> spritesGlifo;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            atlas = null;
            glifos = null;
            spritesGlifo = null;
        }

        public static Texture2D Atlas
        {
            get
            {
                Montar();
                return atlas;
            }
        }

        static void Montar()
        {
            if (atlas != null && glifos != null)
                return;

            atlas = new Texture2D(TamanhoAtlas, TamanhoAtlas, TextureFormat.RGBA32, false);
            atlas.name = "FontePixel (atlas)";
            atlas.filterMode = FilterMode.Point;
            atlas.wrapMode = TextureWrapMode.Clamp;
            var cores = new Color32[TamanhoAtlas * TamanhoAtlas];
            for (int i = 0; i < cores.Length; i++)
                cores[i] = new Color32(255, 255, 255, 0);

            glifos = new Dictionary<char, Glifo>();
            int indice = 0;
            int colunas = TamanhoAtlas / CelulaL;
            foreach (KeyValuePair<char, string[]> par in GlifosFonte.Todos())
            {
                string[] linhas = par.Value;
                int largura = linhas[0].Length;
                int cx = (indice % colunas) * CelulaL + 1;
                int cyTopo = TamanhoAtlas - 1 - (indice / colunas) * CelulaA;
                if (cyTopo - AlturaGlifo + 1 < 0)
                {
                    Debug.LogError("[CincoEstrelas] O atlas da fonte ficou pequeno para " + GlifosFonte.Todos().Count + " letras.");
                    break;
                }
                for (int r = 0; r < AlturaGlifo; r++)
                {
                    string l = linhas[r];
                    for (int c = 0; c < l.Length; c++)
                    {
                        if (l[c] != '#')
                            continue;
                        cores[(cyTopo - r) * TamanhoAtlas + cx + c] = new Color32(255, 255, 255, 255);
                    }
                }

                var g = new Glifo();
                g.largura = largura;
                g.pixels = new Rect(cx, cyTopo - AlturaGlifo + 1, largura, AlturaGlifo);
                g.uv = new Rect(g.pixels.x / TamanhoAtlas, g.pixels.y / TamanhoAtlas,
                    g.pixels.width / TamanhoAtlas, g.pixels.height / TamanhoAtlas);
                glifos[par.Key] = g;
                indice++;
            }

            atlas.SetPixels32(cores);
            atlas.Apply(false, false);
        }

        /// <summary>Converte o caractere para um que exista na fonte.</summary>
        public static char Normalizar(char c)
        {
            return GlifosFonte.Normalizar(c);
        }

        public static bool TentarGlifo(char c, out Glifo g)
        {
            Montar();
            return glifos.TryGetValue(c, out g);
        }

        /// <summary>Largura (em pixels da fonte) de uma linha de texto.</summary>
        public static int LarguraLinha(string linha)
        {
            Montar();
            int total = 0;
            for (int i = 0; i < linha.Length; i++)
            {
                char c = Normalizar(linha[i]);
                Glifo g;
                if (c == ' ')
                    total += GlifosFonte.LarguraEspaco;
                else if (glifos.TryGetValue(c, out g))
                    total += g.largura;
                if (i < linha.Length - 1)
                    total += 1;
            }
            return total;
        }

        /// <summary>
        /// Quebra o texto em linhas que caibam em <paramref name="larguraMaxima"/>
        /// pixels da fonte (respeitando os '\n' do texto).
        /// </summary>
        public static List<string> Quebrar(string texto, float larguraMaxima)
        {
            var linhas = new List<string>();
            foreach (string paragrafo in (texto ?? "").Split('\n'))
            {
                if (larguraMaxima <= 0f || LarguraLinha(paragrafo) <= larguraMaxima)
                {
                    linhas.Add(paragrafo);
                    continue;
                }
                string atual = "";
                foreach (string palavra in paragrafo.Split(' '))
                {
                    string tentativa = atual.Length == 0 ? palavra : atual + " " + palavra;
                    if (atual.Length > 0 && LarguraLinha(tentativa) > larguraMaxima)
                    {
                        linhas.Add(atual);
                        atual = palavra;
                    }
                    else
                    {
                        atual = tentativa;
                    }
                }
                linhas.Add(atual);
            }
            return linhas;
        }

        /// <summary>Sprite de um caractere (usado nos textos que aparecem no cenário).</summary>
        public static Sprite SpriteDoGlifo(char c)
        {
            Montar();
            if (spritesGlifo == null)
                spritesGlifo = new Dictionary<char, Sprite>();
            c = Normalizar(c);
            Sprite s;
            if (spritesGlifo.TryGetValue(c, out s) && s != null)
                return s;
            Glifo g;
            if (!glifos.TryGetValue(c, out g))
                return null;
            s = Sprite.Create(atlas, g.pixels, new Vector2(0f, 0f), Arte.PixelsPorUnidade, 0, SpriteMeshType.FullRect);
            s.name = "glifo_" + c;
            spritesGlifo[c] = s;
            return s;
        }
    }
}
