using System;
using System.Collections.Generic;

namespace CincoEstrelas
{
    /// <summary>
    /// Confere, com a mesma física do jogo, se a fase é jogável: todo vão do
    /// caminho cabe num pulo, a ponte levadiça é obrigatória (o canal não dá
    /// para pular), a cavadinha alcança o sino, a pilha de barris e o portão
    /// não dão para pular, e os elementos estão na ordem dos três atos.
    /// Os testes rodam isto em cada mapa; lista vazia = fase aprovada.
    /// </summary>
    public static class AnalisadorMapa
    {
        /// <summary>Os pulos exigidos usam no máximo esta fração do alcance teórico (margem de erro).</summary>
        public const float Folga = 0.85f;
        /// <summary>Raio de acerto do sino (o colisor da cena é um pouco maior).</summary>
        public const float RaioSino = 0.55f;
        /// <summary>Degrau mais alto que o caminho principal pode exigir.</summary>
        public static float DegrauMaximo
        {
            get { return ParametrosJogador.AlturaMaximaPulo - 0.6f; }
        }

        public static List<string> Analisar(MapaFase m)
        {
            var problemas = new List<string>();
            List<string> invalido = m.Validar();
            if (invalido.Count > 0)
            {
                foreach (string s in invalido)
                    problemas.Add("mapa inválido: " + s);
                return problemas;
            }
            Caminho(m, problemas);
            PonteObrigatoria(m, problemas);
            SinoAlcancavel(m, problemas);
            Bloqueios(m, problemas);
            Ordem(m, problemas);
            Arena(m, problemas);
            return problemas;
        }

        // ------------------------------------------------------------ superfícies
        /// <summary>
        /// Altura (y do pé) onde se pisa na coluna, ou -1 se a coluna não tem chão.
        /// Considera chão, tábuas, caixotes, a ponte (se abaixada), os barcos
        /// (o trecho inteiro onde eles andam) e, se fechados, barris e portão.
        /// </summary>
        public static float Superficie(MapaFase m, int x, bool ponteAbaixada, bool barreirasAbertas)
        {
            int chao = m.TopoSolido(x);
            float topo = chao;
            for (int y = m.Altura - 1; y >= 0; y--)
            {
                char c = m.Em(x, y);
                if (c == Legenda.Tabua && topo < 0f)
                    topo = y + 1;
                if (c == Legenda.Ponte && ponteAbaixada)
                    topo = Math.Max(topo, y + 1);
                // caixote no chão vira um degrau do caminho (os que ficam em cima de toldos são extras)
                if ((c == Legenda.Caixote || c == Legenda.CaixoteCoracao) && y == chao)
                    topo = Math.Max(topo, y + 1);
            }
            foreach (Celula b in m.Encontrar(Legenda.Barco))
            {
                int esq, dir;
                if (m.LimitesBarco(b, out esq, out dir) && x >= esq && x <= dir)
                    topo = Math.Max(topo, b.Y + 1);
            }
            if (!barreirasAbertas)
            {
                foreach (Celula o in m.Encontrar(Legenda.Barris))
                {
                    if (x >= o.X && x < o.X + Legenda.LarguraBarris)
                        topo = Math.Max(topo, o.Y + Legenda.AlturaBarris);
                }
                foreach (Celula p in m.Encontrar(Legenda.Portao))
                {
                    if (x == p.X)
                        topo = Math.Max(topo, p.Y + Legenda.AlturaPortao);
                }
            }
            return topo;
        }

        // ------------------------------------------------------------ caminho principal
        static void Caminho(MapaFase m, List<string> problemas)
        {
            Celula inicio = m.Primeira(Legenda.Inicio);
            Celula gol = m.Primeira(Legenda.Gol);
            float atual = Superficie(m, inicio.X, true, true);
            if (atual < 0f)
            {
                problemas.Add("o início (P) não tem chão");
                return;
            }
            int x = inicio.X + 1;
            while (x <= gol.X)
            {
                float s = Superficie(m, x, true, true);
                if (s >= 0f)
                {
                    if (s - atual > DegrauMaximo)
                        problemas.Add("degrau alto demais em x=" + x + " (" + (s - atual) + " tiles)");
                    atual = s;
                    x++;
                    continue;
                }
                int comeco = x;
                while (x <= gol.X && Superficie(m, x, true, true) < 0f)
                    x++;
                if (x > gol.X)
                {
                    problemas.Add("o caminho termina na água em x=" + comeco + " (sem chão até o gol)");
                    return;
                }
                float destino = Superficie(m, x, true, true);
                int vao = x - comeco;
                float necessario = vao + ParametrosJogador.LarguraColisor;
                float alcance = ParametrosJogador.AlcancePulo(ParametrosJogador.VelocidadeCorrendo, destino - atual);
                if (necessario > alcance * Folga)
                    problemas.Add("vão de " + vao + " tiles em x=" + comeco + " não dá para pular (precisa " +
                                  necessario.ToString("0.0") + ", alcance com folga " + (alcance * Folga).ToString("0.0") + ")");
                atual = destino;
            }
        }

        /// <summary>Com a ponte levantada o canal não pode ser pulado: o sino é obrigatório.</summary>
        static void PonteObrigatoria(MapaFase m, List<string> problemas)
        {
            Celula p = m.Primeira(Legenda.Ponte);
            int ini, fim;
            m.Trecho(p.X, p.Y, out ini, out fim);
            float antes = Superficie(m, ini - 1, false, true);
            float depois = Superficie(m, fim + 1, false, true);
            for (int x = ini; x <= fim; x++)
            {
                if (Superficie(m, x, false, true) >= 0f)
                {
                    problemas.Add("há chão embaixo da ponte levadiça em x=" + x + " (ela não seria necessária)");
                    return;
                }
            }
            int vao = fim - ini + 1;
            float alcance = ParametrosJogador.AlcancePulo(ParametrosJogador.VelocidadeCorrendo, depois - antes);
            // aqui a margem é ao contrário: mesmo com um pulo perfeito o canal não pode ser vencido
            if (vao + ParametrosJogador.LarguraColisor <= alcance * 1.15f)
                problemas.Add("o canal da ponte (" + vao + " tiles) dá para pular sem baixar a ponte");
            if (antes < 0f || depois < 0f)
                problemas.Add("a ponte levadiça precisa de chão nas duas pontas");
        }

        /// <summary>A cavadinha, chutada do chão antes do canal, precisa acertar o sino.</summary>
        static void SinoAlcancavel(MapaFase m, List<string> problemas)
        {
            Celula s = m.Primeira(Legenda.Sino);
            float cx = s.X + 0.5f;
            float cy = s.Y + 0.5f;
            float chao = Superficie(m, s.X, false, true);
            if (chao >= 0f && cy - chao < 3f)
                problemas.Add("o sino está baixo demais (" + (cy - chao) + " tiles): precisa da bola alta");
            ParametrosChute cav = Chutes.Parametros(TipoChute.Cavadinha);
            int posicoes = 0;
            for (int x = s.X - 12; x <= s.X; x++)
            {
                float y = Superficie(m, x, false, true);
                if (y < 0f || !m.Solido(x, (int)y - 1))
                    continue;
                float bx = x + 0.5f + ParametrosJogador.DistanciaBolaNoPe;
                float by = y + ParametrosJogador.AlturaBolaNoPe;
                for (float d = 0f; d <= 12f; d += 0.05f)
                {
                    float px = bx + d;
                    float py = by + cav.AlturaNaDistancia(d);
                    float dx = px - cx;
                    float dy = py - cy;
                    if (dx * dx + dy * dy <= (RaioSino - 0.1f) * (RaioSino - 0.1f))
                    {
                        posicoes++;
                        break;
                    }
                }
            }
            if (posicoes < 2)
                problemas.Add("a cavadinha só acerta o sino de " + posicoes + " posição(ões) do chão (precisa de pelo menos 2)");
        }

        /// <summary>A pilha de barris e o portão precisam bloquear de verdade (ninguém pula por cima).</summary>
        static void Bloqueios(MapaFase m, List<string> problemas)
        {
            Celula o = m.Primeira(Legenda.Barris);
            Bloqueio(m, problemas, o.X, o.Y + Legenda.AlturaBarris, "pilha de barris");
            Celula p = m.Primeira(Legenda.Portao);
            Bloqueio(m, problemas, p.X, p.Y + Legenda.AlturaPortao, "portão");
        }

        static void Bloqueio(MapaFase m, List<string> problemas, int x, float topo, string nome)
        {
            float maisAlto = -1f;
            for (int i = x - 7; i < x; i++)
                maisAlto = Math.Max(maisAlto, Superficie(m, i, true, true));
            if (maisAlto + ParametrosJogador.AlturaMaximaPulo + 0.2f >= topo)
                problemas.Add("dá para pular por cima do " + nome + " em x=" + x);
        }

        static void Ordem(MapaFase m, List<string> problemas)
        {
            List<Celula> atos = m.Encontrar(Legenda.Ato);
            List<Celula> carrinhos = m.Encontrar(Legenda.Carrinho);
            int[] sequencia =
            {
                m.Primeira(Legenda.Inicio).X,
                m.Primeira(Legenda.Sino).X,
                m.Primeira(Legenda.Ponte).X,
                atos[0].X,
                carrinhos[0].X,
                m.Primeira(Legenda.Barris).X,
                m.Primeira(Legenda.Capitao).X,
                m.Primeira(Legenda.Portao).X,
                atos[1].X,
                m.Primeira(Legenda.Quiz).X,
                m.Primeira(Legenda.Arena).X,
                m.Primeira(Legenda.Goleiro).X,
                m.Primeira(Legenda.Gol).X,
            };
            string[] nomes =
            {
                "início", "sino", "ponte", "ato 2", "1º carrinho", "barris", "capitão", "portão", "ato 3",
                "quiz do estádio", "arena", "goleiro", "gol",
            };
            for (int i = 1; i < sequencia.Length; i++)
            {
                if (sequencia[i] <= sequencia[i - 1])
                    problemas.Add("ordem errada: " + nomes[i] + " (x=" + sequencia[i] + ") deveria vir depois de " +
                                  nomes[i - 1] + " (x=" + sequencia[i - 1] + ")");
            }
            int saltadores = 0;
            foreach (Celula s in m.Encontrar(Legenda.Saltador))
            {
                if (s.X > atos[0].X && s.X < m.Primeira(Legenda.Capitao).X)
                    saltadores++;
            }
            if (saltadores == 0)
                problemas.Add("o zagueiro saltador precisa aparecer no ato 2, antes do capitão");
        }

        static void Arena(MapaFase m, List<string> problemas)
        {
            Celula a = m.Primeira(Legenda.Arena);
            Celula y = m.Primeira(Legenda.Gol);
            int largura = y.X - a.X;
            if (largura < 20 || largura > 40)
                problemas.Add("a arena final tem " + largura + " tiles entre o início (A) e o gol (Y): use de 20 a 40");
            if (y.X + Legenda.ProfundidadeGol >= m.Largura)
                problemas.Add("o gol precisa de " + Legenda.ProfundidadeGol + " tiles de rede antes do fim do mapa");
            for (int x = a.X; x <= y.X + Legenda.ProfundidadeGol; x++)
            {
                if (Superficie(m, x, true, true) != m.TopoSolido(y.X))
                {
                    problemas.Add("o gramado da arena precisa ser plano (x=" + x + ")");
                    break;
                }
            }
        }
    }
}
