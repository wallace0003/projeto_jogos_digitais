using System;
using System.Collections.Generic;

namespace CincoEstrelas
{
    /// <summary>
    /// Caracteres do mapa em texto (Resources/Fases/*.txt). A legenda completa
    /// também está no começo do arquivo da fase.
    /// </summary>
    public static class Legenda
    {
        public const char Vazio = '.';
        public const char Pedra = '#';
        public const char Doca = 'D';
        public const char Rua = 'R';
        public const char Grama = 'G';
        public const char Tabua = '=';
        public const char AguaTopo = '~';
        public const char AguaFundo = '%';
        public const char Inicio = 'P';
        public const char Checkpoint = 'K';
        public const char Barco = 'B';
        public const char LimiteBarcoEsquerda = '<';
        public const char LimiteBarcoDireita = '>';
        public const char Sino = 'S';
        public const char Ponte = 'L';
        public const char Caixote = 'x';
        public const char CaixoteCoracao = 'h';
        public const char Barris = 'O';
        public const char Carrinho = 'c';
        public const char Saltador = 's';
        public const char Capitao = 'C';
        public const char Goleiro = 'g';
        public const char Portao = '|';
        public const char Quiz = 'Q';
        public const char Arena = 'A';
        public const char Gol = 'Y';
        public const char Ato = '!';
        public const char Lampiao = 'l';
        public const char Arvore = 't';
        public const char Banco = 'b';
        public const char Mastro = 'm';
        public const char BarrilDecorativo = 'k';
        public const char Refletor = 'r';

        /// <summary>Largura e altura (em tiles) da pilha de barris: mais alta que o pulo.</summary>
        public const int LarguraBarris = 2;
        public const int AlturaBarris = 5;
        public const int AlturaPortao = 6;
        public const int LarguraBarco = 4;
        /// <summary>Altura (em tiles) da trave do gol e profundidade da rede.</summary>
        public const int AlturaGol = 3;
        public const int ProfundidadeGol = 2;

        /// <summary>Chão sólido (Tilemap com colisor).</summary>
        public static bool EhSolido(char c)
        {
            return c == Pedra || c == Doca || c == Rua || c == Grama;
        }

        public static bool EhAgua(char c)
        {
            return c == AguaTopo || c == AguaFundo;
        }

        /// <summary>Placas com dicas: dígitos de 0 a 9 (texto definido com "@placa N ...").</summary>
        public static bool EhPlaca(char c)
        {
            return c >= '0' && c <= '9';
        }
    }

    public struct Celula
    {
        public int X;
        public int Y;

        public Celula(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Valida
        {
            get { return X >= 0 && Y >= 0; }
        }

        public static readonly Celula Nenhuma = new Celula(-1, -1);

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }

    /// <summary>
    /// Mapa da fase lido de um texto. A primeira linha do mapa é o topo; aqui
    /// dentro y = 0 é a linha de baixo (igual ao Unity, onde y cresce para cima).
    /// Linhas com "//" são comentários; linhas com "@" são dados (nome, placas).
    /// </summary>
    public class MapaFase
    {
        char[,] grade;
        readonly Dictionary<int, string> placas = new Dictionary<int, string>();

        public int Largura { get; private set; }
        public int Altura { get; private set; }
        public string Nome { get; private set; }

        public static MapaFase Ler(string texto)
        {
            var mapa = new MapaFase();
            mapa.Nome = "";
            var linhas = new List<string>();
            string[] brutas = (texto ?? "").Split('\n');
            foreach (string bruta in brutas)
            {
                string l = bruta.TrimEnd('\r', ' ', '\t');
                if (l.Length == 0 || l.StartsWith("//"))
                    continue;
                if (l.StartsWith("@"))
                {
                    mapa.LerDado(l);
                    continue;
                }
                linhas.Add(l);
            }

            int largura = 0;
            foreach (string l in linhas)
                largura = Math.Max(largura, l.Length);
            mapa.Largura = largura;
            mapa.Altura = linhas.Count;
            mapa.grade = new char[Math.Max(1, largura), Math.Max(1, linhas.Count)];
            for (int linha = 0; linha < linhas.Count; linha++)
            {
                int y = linhas.Count - 1 - linha;
                string l = linhas[linha];
                for (int x = 0; x < largura; x++)
                    mapa.grade[x, y] = x < l.Length ? l[x] : Legenda.Vazio;
            }
            return mapa;
        }

        void LerDado(string linha)
        {
            string resto = linha.Substring(1).Trim();
            int espaco = resto.IndexOf(' ');
            string chave = espaco < 0 ? resto : resto.Substring(0, espaco);
            string valor = espaco < 0 ? "" : resto.Substring(espaco + 1).Trim();
            switch (chave.ToLowerInvariant())
            {
                case "nome":
                    Nome = valor;
                    break;
                case "placa":
                    int esp = valor.IndexOf(' ');
                    int numero;
                    if (esp > 0 && int.TryParse(valor.Substring(0, esp), out numero))
                        placas[numero] = valor.Substring(esp + 1).Trim();
                    break;
            }
        }

        // ------------------------------------------------------------ consultas
        public bool Dentro(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Largura && y < Altura;
        }

        /// <summary>Caractere na célula (fora do mapa = vazio).</summary>
        public char Em(int x, int y)
        {
            return Dentro(x, y) ? grade[x, y] : Legenda.Vazio;
        }

        public bool Solido(int x, int y)
        {
            return Legenda.EhSolido(Em(x, y));
        }

        /// <summary>Todas as células com o caractere, da esquerda para a direita (e de baixo para cima).</summary>
        public List<Celula> Encontrar(char c)
        {
            var lista = new List<Celula>();
            for (int x = 0; x < Largura; x++)
            {
                for (int y = 0; y < Altura; y++)
                {
                    if (grade[x, y] == c)
                        lista.Add(new Celula(x, y));
                }
            }
            return lista;
        }

        public bool Existe(char c)
        {
            return Encontrar(c).Count > 0;
        }

        public Celula Primeira(char c)
        {
            List<Celula> l = Encontrar(c);
            return l.Count > 0 ? l[0] : Celula.Nenhuma;
        }

        public string TextoPlaca(int numero)
        {
            string t;
            return placas.TryGetValue(numero, out t) ? t : null;
        }

        public IEnumerable<int> NumerosComTexto
        {
            get { return placas.Keys; }
        }

        /// <summary>Número da placa (0-9) de um caractere de placa.</summary>
        public static int NumeroPlaca(char c)
        {
            return c - '0';
        }

        /// <summary>
        /// Limites do barco: as células '&lt;' e '&gt;' na mesma linha. O canto esquerdo
        /// do barco anda de <paramref name="esquerda"/> até <paramref name="direita"/> - 3
        /// (o barco tem 4 tiles).
        /// </summary>
        public bool LimitesBarco(Celula barco, out int esquerda, out int direita)
        {
            esquerda = -1;
            direita = -1;
            for (int x = barco.X; x >= 0; x--)
            {
                if (Em(x, barco.Y) == Legenda.LimiteBarcoEsquerda)
                {
                    esquerda = x;
                    break;
                }
            }
            for (int x = barco.X; x < Largura; x++)
            {
                if (Em(x, barco.Y) == Legenda.LimiteBarcoDireita)
                {
                    direita = x;
                    break;
                }
            }
            return esquerda >= 0 && direita >= 0 && direita - esquerda + 1 >= Legenda.LarguraBarco;
        }

        /// <summary>Trecho contínuo do caractere na linha y, a partir de x (para a ponte).</summary>
        public void Trecho(int x, int y, out int inicio, out int fim)
        {
            char c = Em(x, y);
            inicio = x;
            fim = x;
            while (Em(inicio - 1, y) == c)
                inicio--;
            while (Em(fim + 1, y) == c)
                fim++;
        }

        /// <summary>Y do topo do chão sólido mais alto da coluna (onde o pé pisa), ou -1 se só tem água/vazio.</summary>
        public int TopoSolido(int x)
        {
            for (int y = Altura - 1; y >= 0; y--)
            {
                if (Solido(x, y))
                    return y + 1;
            }
            return -1;
        }

        /// <summary>Número "aleatório" fixo por célula (para variar a arte sem mudar a cada Play).</summary>
        public static int Hash(int x, int y)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263;
                h = (h ^ (h >> 13)) * 1274126177;
                return (h ^ (h >> 16)) & 0x7fffffff;
            }
        }

        // ------------------------------------------------------------ validação
        /// <summary>Confere se o mapa tem tudo o que a fase precisa. Lista vazia = tudo certo.</summary>
        public List<string> Validar()
        {
            var problemas = new List<string>();
            if (Largura < 30 || Altura < 10)
                problemas.Add("mapa pequeno demais: " + Largura + " x " + Altura);

            Exatamente(problemas, Legenda.Inicio, 1, "início do jogador (P)");
            Exatamente(problemas, Legenda.Sino, 1, "sino (S)");
            Exatamente(problemas, Legenda.Capitao, 1, "capitão (C)");
            Exatamente(problemas, Legenda.Goleiro, 1, "goleiro (g)");
            Exatamente(problemas, Legenda.Gol, 1, "gol (Y)");
            Exatamente(problemas, Legenda.Arena, 1, "início da arena (A)");
            Exatamente(problemas, Legenda.Quiz, 1, "quiz do estádio (Q)");
            Exatamente(problemas, Legenda.Barris, 1, "pilha de barris (O)");
            Exatamente(problemas, Legenda.Portao, 1, "portão (|)");
            Exatamente(problemas, Legenda.Ato, 2, "início de ato (!)");
            Pelomenos(problemas, Legenda.Checkpoint, 3, "checkpoint (K)");
            Pelomenos(problemas, Legenda.Carrinho, 2, "zagueiro carrinho (c)");
            Pelomenos(problemas, Legenda.Saltador, 1, "zagueiro saltador (s)");
            Pelomenos(problemas, Legenda.Ponte, 3, "ponte levadiça (L)");
            Pelomenos(problemas, Legenda.Barco, 1, "barco (B)");

            foreach (Celula b in Encontrar(Legenda.Barco))
            {
                int esq, dir;
                if (!LimitesBarco(b, out esq, out dir))
                    problemas.Add("barco em " + b + " sem '<' e '>' na mesma linha (ou trecho menor que o barco)");
                else if (b.X < esq || b.X + Legenda.LarguraBarco - 1 > dir)
                    problemas.Add("barco em " + b + " começa fora dos limites < >");
            }

            List<Celula> pontes = Encontrar(Legenda.Ponte);
            if (pontes.Count > 0)
            {
                int ini, fim;
                Trecho(pontes[0].X, pontes[0].Y, out ini, out fim);
                if (fim - ini + 1 != pontes.Count)
                    problemas.Add("a ponte levadiça (L) precisa ser um trecho só, numa linha");
            }

            for (int x = 0; x < Largura; x++)
            {
                for (int y = 0; y < Altura; y++)
                {
                    char c = grade[x, y];
                    if (Legenda.EhPlaca(c) && TextoPlaca(NumeroPlaca(c)) == null)
                        problemas.Add("placa " + c + " em " + new Celula(x, y) + " sem texto (@placa " + c + " ...)");
                }
            }

            // personagens e objetos apoiados no chão
            char[] noChao =
            {
                Legenda.Inicio, Legenda.Checkpoint, Legenda.Carrinho, Legenda.Saltador, Legenda.Capitao,
                Legenda.Goleiro, Legenda.Barris, Legenda.Portao, Legenda.Gol, Legenda.Caixote, Legenda.CaixoteCoracao,
            };
            foreach (char c in noChao)
            {
                foreach (Celula cel in Encontrar(c))
                {
                    if (!Solido(cel.X, cel.Y - 1) && Em(cel.X, cel.Y - 1) != Legenda.Tabua)
                        problemas.Add("'" + c + "' em " + cel + " não está apoiado no chão");
                }
            }
            return problemas;
        }

        void Exatamente(List<string> problemas, char c, int n, string nome)
        {
            int q = Encontrar(c).Count;
            if (q != n)
                problemas.Add("o mapa precisa de " + n + " " + nome + " (tem " + q + ")");
        }

        void Pelomenos(List<string> problemas, char c, int n, string nome)
        {
            int q = Encontrar(c).Count;
            if (q < n)
                problemas.Add("o mapa precisa de pelo menos " + n + " " + nome + " (tem " + q + ")");
        }
    }
}
