using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Camadas de física (os nomes aparecem em Project Settings &gt; Tags and Layers).
    /// </summary>
    public static class Camadas
    {
        public const int Chao = 6;
        public const int Jogador = 7;
        public const int Inimigo = 8;
        public const int Item = 9;
        public const int Perigo = 10;
        public const int Plataforma = 11;
        public const int Bola = 12;
        public const int Projetil = 13;

        /// <summary>Onde o jogador pisa: chão, barcos, ponte, caixotes e tábuas.</summary>
        public const int MascaraPisavel = (1 << Chao) | (1 << Plataforma);

        /// <summary>Somente chão e paredes sólidos.</summary>
        public const int MascaraSolida = 1 << Chao;

        /// <summary>
        /// Define quais camadas se enxergam. Os adversários só precisam
        /// "sentir" o jogador e a bola; a bola quica no chão e nas tábuas.
        /// </summary>
        public static void ConfigurarColisoes()
        {
            int[] todas = { Chao, Jogador, Inimigo, Item, Perigo, Plataforma, Bola, Projetil };
            foreach (int a in todas)
            {
                foreach (int b in todas)
                    Physics2D.IgnoreLayerCollision(a, b, true);
            }
            Ligar(Jogador, Chao, Plataforma, Inimigo, Item, Perigo, Projetil);
            Ligar(Bola, Chao, Plataforma, Inimigo, Item, Perigo);
            Ligar(Projetil, Chao);
        }

        static void Ligar(int camada, params int[] outras)
        {
            foreach (int o in outras)
                Physics2D.IgnoreLayerCollision(camada, o, false);
        }
    }

    /// <summary>Ordem de desenho (Sorting Order) de cada elemento.</summary>
    public static class Ordem
    {
        public const int Ceu = -1000;
        public const int Nuvens = -950;
        public const int Cidade = -900;
        public const int GamlaStan = -850;
        public const int Estadio = -700;
        public const int DecoracaoFundo = -60;
        public const int Decoracao = -10;
        public const int AguaFundo = -5;
        public const int Chao = 0;
        public const int Plataformas = 1;
        public const int Objetos = 5;
        public const int Bandeira = 8;
        public const int Itens = 10;
        public const int Gol = 15;
        public const int Inimigos = 20;
        public const int Goleiro = 22;
        public const int Companheiro = 25;
        public const int Jogador = 30;
        public const int Bola = 32;
        public const int AguaTopo = 35;
        public const int Efeitos = 40;
        public const int TextoFlutuante = 50;
    }

    /// <summary>Nomes das cenas (precisam estar em File &gt; Build Profiles &gt; Scene List).</summary>
    public static class Cenas
    {
        public const string Menu = "Menu";
        public const string Fase = "Suecia1958";
        public const string FimDeJogo = "FimDeJogo";
        /// <summary>Arquivo do mapa em Resources/Fases.</summary>
        public const string MapaDaFase = "Suecia1958";
    }
}
