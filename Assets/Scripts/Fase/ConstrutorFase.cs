using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace CincoEstrelas
{
    /// <summary>
    /// Monta a fase inteira a partir do mapa em texto (Resources/Fases/Suecia1958.txt):
    /// Grid + Tilemaps (chão com Tilemap Collider 2D + Composite Collider 2D +
    /// Rigidbody 2D Static), água, tábuas, barcos, sino e ponte levadiça,
    /// caixotes, barris, portão, adversários, goleiro, gol, placas, checkpoints,
    /// decoração, fundo com parallax e câmera. Veja a legenda no começo do mapa.
    /// </summary>
    public class ConstrutorFase : MonoBehaviour
    {
        [Header("Fase")]
        [Tooltip("Arquivo do mapa em Assets/Resources/Fases (sem a extensão).")]
        public string arquivoFase = Cenas.MapaDaFase;

        [Header("Materiais do URP 2D")]
        public Material materialAceso;
        public Material materialBrilho;

        MapaFase mapa;
        Tilemap tmChao;
        Tilemap tmTabuas;
        Tilemap tmAgua;
        Vector3 inicio;
        BoxCollider2D paredeArena;
        readonly List<Bandeira> checkpoints = new List<Bandeira>();
        readonly List<float> atos = new List<float>();
        readonly List<Inimigo> inimigos = new List<Inimigo>();

        public MapaFase Mapa
        {
            get { return mapa; }
        }

        public int Largura
        {
            get { return mapa != null ? mapa.Largura : 0; }
        }

        public int Altura
        {
            get { return mapa != null ? mapa.Altura : 0; }
        }

        public Vector3 Inicio
        {
            get { return inicio; }
        }

        /// <summary>x onde começa cada ato (2 e 3).</summary>
        public IList<float> Atos
        {
            get { return atos; }
        }

        public IList<Bandeira> Checkpoints
        {
            get { return checkpoints; }
        }

        public IList<Inimigo> Inimigos
        {
            get { return inimigos; }
        }

        public Sino Sino { get; private set; }
        public PonteLevadica Ponte { get; private set; }
        public Obstaculo Portao { get; private set; }
        public Obstaculo Barris { get; private set; }
        public Capitao Capitao { get; private set; }
        public Goleiro Goleiro { get; private set; }
        public Gol Gol { get; private set; }
        public float XArena { get; private set; }
        public float ChaoArena { get; private set; }

        void Awake()
        {
            // o jogo sempre começa pela tela inicial (com as orientações de como jogar):
            // dar Play direto na fase leva primeiro para o Menu
            if (!Sessao.Iniciada && !Partida.PodePularTelaInicial())
            {
                enabled = false;
                SceneManager.LoadScene(Cenas.Menu);
                return;
            }

            Materiais2D.Definir(materialAceso, materialBrilho);
            Camadas.ConfigurarColisoes();
            Sessao.GarantirIniciada();

            if (!LerMapa())
                return;

            CriarTilemaps();
            CriarEntidades();
            Jogador jogador = Jogador.Criar(inicio);

            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
            }
            CameraSeguir seguir = cam.GetComponent<CameraSeguir>();
            if (seguir == null)
                seguir = cam.gameObject.AddComponent<CameraSeguir>();
            seguir.alvo = jogador.transform;
            seguir.limites = new Rect(0f, 0f, mapa.Largura, mapa.Altura + 4f);
            // posiciona a câmera antes de criar o fundo (o parallax usa a posição inicial)
            cam.transform.position = new Vector3(inicio.x, Mathf.Max(inicio.y + 1.6f, CameraSeguir.TamanhoOrtografico), -10f);
            CameraSeguir.Teleportar();
            Fundo.Criar(cam.transform);

            var g = gameObject.AddComponent<GerenciadorFase>();
            g.Iniciar(this, jogador);
        }

        // ------------------------------------------------------------ mapa
        bool LerMapa()
        {
            TextAsset arquivo = Resources.Load<TextAsset>("Fases/" + arquivoFase);
            if (arquivo == null)
            {
                Debug.LogError("[CincoEstrelas] Mapa não encontrado: Resources/Fases/" + arquivoFase + ".txt");
                return false;
            }
            mapa = MapaFase.Ler(arquivo.text);
            foreach (string problema in mapa.Validar())
                Debug.LogWarning("[CincoEstrelas] " + arquivoFase + ": " + problema);
            return mapa.Largura > 0;
        }

        /// <summary>Máscara de vizinhos sólidos (cima=1, direita=2, baixo=4, esquerda=8) para o auto-tile.</summary>
        int Mascara(int x, int y)
        {
            int m = 0;
            if (SolidoParaBorda(x, y + 1)) m |= 1;
            if (SolidoParaBorda(x + 1, y)) m |= 2;
            if (SolidoParaBorda(x, y - 1) || y == 0) m |= 4;
            if (SolidoParaBorda(x - 1, y) || x == 0 || x == mapa.Largura - 1) m |= 8;
            return m;
        }

        bool SolidoParaBorda(int x, int y)
        {
            if (x < 0 || x >= mapa.Largura)
                return true;
            return mapa.Solido(x, y);
        }

        static string PrefixoChao(char c)
        {
            switch (c)
            {
                case Legenda.Rua: return "rua_";
                case Legenda.Grama: return "grama_";
                default: return "pedra_";
            }
        }

        static string Ponta(bool esquerda, bool direita, string prefixo)
        {
            if (esquerda && direita) return prefixo + "m";
            if (esquerda) return prefixo + "d";
            if (direita) return prefixo + "e";
            return prefixo + "u";
        }

        // ------------------------------------------------------------ tilemaps
        Tilemap NovoTilemap(Transform grade, string nome, int ordem, int camada)
        {
            var go = new GameObject(nome);
            go.layer = camada;
            go.transform.SetParent(grade, false);
            Tilemap tm = go.AddComponent<Tilemap>();
            TilemapRenderer tr = go.AddComponent<TilemapRenderer>();
            tr.sortingOrder = ordem;
            Materiais2D.Aplicar(tr, false);
            return tm;
        }

        void CriarTilemaps()
        {
            var gridGo = new GameObject("Grid (Suécia 1958)");
            Grid grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            // chão (com colisão, igual ao passo a passo da disciplina)
            tmChao = NovoTilemap(gridGo.transform, "Chao (Tilemap)", Ordem.Chao, Camadas.Chao);
            Rigidbody2D rb = tmChao.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            TilemapCollider2D tc = tmChao.gameObject.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
            CompositeCollider2D cc = tmChao.gameObject.AddComponent<CompositeCollider2D>();
            cc.geometryType = CompositeCollider2D.GeometryType.Polygons;

            tmTabuas = NovoTilemap(gridGo.transform, "Tabuas (Tilemap)", Ordem.Plataformas, Camadas.Plataforma);
            tmAgua = NovoTilemap(gridGo.transform, "Canal (Tilemap)", Ordem.AguaTopo, 0);

            var colisores = new GameObject("Colisores extras").transform;
            colisores.SetParent(gridGo.transform, false);

            TileBase aguaTopo = Arte.TileAnimado("agua_topo", 5f);
            TileBase aguaFundo = Arte.TileAnimado("agua_fundo", 2f);

            for (int y = 0; y < mapa.Altura; y++)
            {
                for (int x = 0; x < mapa.Largura; x++)
                {
                    var cel = new Vector3Int(x, y, 0);
                    char c = mapa.Em(x, y);
                    switch (c)
                    {
                        case Legenda.Pedra:
                        case Legenda.Rua:
                        case Legenda.Grama:
                            tmChao.SetTile(cel, Arte.Tile(PrefixoChao(c) + Mascara(x, y), true));
                            break;
                        case Legenda.Doca:
                            tmChao.SetTile(cel, Arte.Tile(Ponta(mapa.Em(x - 1, y) == Legenda.Doca, mapa.Em(x + 1, y) == Legenda.Doca, "doca_"), true));
                            break;
                        case Legenda.Tabua:
                            tmTabuas.SetTile(cel, Arte.Tile(Ponta(mapa.Em(x - 1, y) == Legenda.Tabua, mapa.Em(x + 1, y) == Legenda.Tabua, "tabua_"), false));
                            break;
                        case Legenda.AguaTopo:
                            tmAgua.SetTile(cel, aguaTopo);
                            break;
                        case Legenda.AguaFundo:
                            tmAgua.SetTile(cel, aguaFundo);
                            break;
                    }
                }
            }

            // colisores juntando células vizinhas iguais (uma caixa por trecho)
            for (int y = 0; y < mapa.Altura; y++)
            {
                int x = 0;
                while (x < mapa.Largura)
                {
                    char c = mapa.Em(x, y);
                    if (c != Legenda.Tabua && c != Legenda.AguaTopo)
                    {
                        x++;
                        continue;
                    }
                    int ini = x;
                    while (x < mapa.Largura && mapa.Em(x, y) == c)
                        x++;
                    if (c == Legenda.Tabua)
                        ColisorTabua(colisores, ini, y, x - ini);
                    else
                        ColisorAgua(colisores, ini, y, x - ini);
                }
            }

            // paredes invisíveis nas laterais
            Parede(colisores, -0.5f);
            Parede(colisores, mapa.Largura + 0.5f);
        }

        void ColisorTabua(Transform pai, int x, int y, int n)
        {
            var go = new GameObject("Tábua atravessável");
            go.layer = Camadas.Plataforma;
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(x + n * 0.5f, y + 1f, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(n, 0.25f);
            col.offset = new Vector2(0f, -0.125f);
            col.usedByEffector = true;
            var ef = go.AddComponent<PlatformEffector2D>();
            ef.useOneWay = true;
            ef.surfaceArc = 160f;
        }

        void ColisorAgua(Transform pai, int x, int y, int n)
        {
            var go = new GameObject("Canal");
            go.layer = Camadas.Perigo;
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(x + n * 0.5f, y, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(n, 0.7f);
            col.offset = new Vector2(0f, 0.35f);
            go.AddComponent<ZonaAgua>();
        }

        BoxCollider2D Parede(Transform pai, float x)
        {
            var go = new GameObject("Parede invisível");
            go.layer = Camadas.Chao;
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(x, mapa.Altura * 0.5f + 5f, 0f);
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, mapa.Altura + 20f);
            return col;
        }

        // ------------------------------------------------------------ entidades
        static Transform Pai(Dictionary<string, Transform> pais, string nome)
        {
            Transform t;
            if (!pais.TryGetValue(nome, out t))
            {
                t = new GameObject(nome).transform;
                pais[nome] = t;
            }
            return t;
        }

        static Vector3 Pe(Celula c)
        {
            return new Vector3(c.X + 0.5f, c.Y, 0f);
        }

        void CriarEntidades()
        {
            var pais = new Dictionary<string, Transform>();
            Celula p = mapa.Primeira(Legenda.Inicio);
            inicio = p.Valida ? Pe(p) : new Vector3(2.5f, mapa.Altura * 0.5f, 0f);

            // ponte levadiça e sino (o sino precisa da ponte)
            Celula l = mapa.Primeira(Legenda.Ponte);
            if (l.Valida)
            {
                int ini, fim;
                mapa.Trecho(l.X, l.Y, out ini, out fim);
                Ponte = PonteLevadica.Criar(ini, l.Y, fim - ini + 1);
            }
            Celula s = mapa.Primeira(Legenda.Sino);
            if (s.Valida)
                Sino = Sino.Criar(new Vector3(s.X + 0.5f, s.Y + 0.5f, 0f), mapa.TopoSolido(s.X), Ponte);

            int indiceCheckpoint = 1;
            foreach (Celula k in mapa.Encontrar(Legenda.Checkpoint))
            {
                Bandeira b = Bandeira.Criar(Pe(k), indiceCheckpoint++);
                b.transform.SetParent(Pai(pais, "Checkpoints"), true);
                checkpoints.Add(b);
            }

            foreach (Celula a in mapa.Encontrar(Legenda.Ato))
                atos.Add(a.X);

            foreach (Celula b in mapa.Encontrar(Legenda.Barco))
            {
                int esq, dir;
                if (!mapa.LimitesBarco(b, out esq, out dir))
                    continue;
                float meio = Legenda.LarguraBarco * 0.5f;
                Barco barco = Barco.Criar(new Vector2(esq + meio, b.Y + 1f), new Vector2(dir + 1 - meio, b.Y + 1f),
                    new Vector2(b.X + meio, b.Y + 1f));
                barco.transform.SetParent(Pai(pais, "Barcos"), true);
            }

            Celula gol = mapa.Primeira(Legenda.Gol);
            Celula arena = mapa.Primeira(Legenda.Arena);
            if (gol.Valida)
            {
                Gol = Gol.Criar(gol.X + 0.5f, gol.Y);
                ChaoArena = gol.Y;
            }
            if (arena.Valida)
            {
                XArena = arena.X;
                Gatilho.Criar(Gatilho.Tipo.Arena, arena.X + 1.5f, mapa.Altura);
                paredeArena = Parede(transform, arena.X - 0.5f);
                paredeArena.gameObject.name = "Parede da arena";
                paredeArena.enabled = false;
            }

            for (int y = 0; y < mapa.Altura; y++)
            {
                for (int x = 0; x < mapa.Largura; x++)
                {
                    char c = mapa.Em(x, y);
                    var cel = new Celula(x, y);
                    Vector3 pe = Pe(cel);
                    switch (c)
                    {
                        case Legenda.Caixote:
                        case Legenda.CaixoteCoracao:
                            Obstaculo.Criar(Obstaculo.Tipo.Caixote, pe, c == Legenda.CaixoteCoracao).transform
                                .SetParent(Pai(pais, "Caixotes"), true);
                            break;
                        case Legenda.Barris:
                            Barris = Obstaculo.Criar(Obstaculo.Tipo.Barris, new Vector3(x + Legenda.LarguraBarris * 0.5f, y, 0f), false);
                            break;
                        case Legenda.Portao:
                            Portao = Obstaculo.Criar(Obstaculo.Tipo.Portao, pe, false);
                            break;
                        case Legenda.Carrinho:
                            Adversario(Inimigo.Criar<ZagueiroCarrinho>(pe), pais);
                            break;
                        case Legenda.Saltador:
                            Adversario(Inimigo.Criar<ZagueiroSaltador>(pe), pais);
                            break;
                        case Legenda.Capitao:
                            Capitao = Inimigo.Criar<Capitao>(pe);
                            Adversario(Capitao, pais);
                            break;
                        case Legenda.Goleiro:
                            Goleiro = Goleiro.Criar(pe, gol.Valida ? gol.X + 0.5f : x + 2.5f, XArena, Legenda.AlturaGol);
                            break;
                        case Legenda.Quiz:
                            Gatilho.Criar(Gatilho.Tipo.QuizEstadio, x + 0.5f, mapa.Altura);
                            break;
                        default:
                            if (Legenda.EhPlaca(c))
                            {
                                string texto = mapa.TextoPlaca(MapaFase.NumeroPlaca(c));
                                if (texto != null)
                                    Placa.Criar(pe, texto).transform.SetParent(Pai(pais, "Placas"), true);
                            }
                            else
                            {
                                Decoracao(c, pe, pais);
                            }
                            break;
                    }
                }
            }

            // o capitão luta entre a placa dele e o portão
            if (Capitao != null)
            {
                Capitao.limiteDireita = Portao != null ? Portao.transform.position.x - 1.2f : Capitao.transform.position.x + 10f;
                Capitao.limiteEsquerda = Capitao.transform.position.x - 9f;
            }

            if (atos.Count >= 2)
                Fundo.Estadio(atos[1] - 2f, mapa.Largura + 2f, gol.Valida ? gol.Y : mapa.TopoSolido(mapa.Largura - 1));
        }

        void Adversario(Inimigo ini, Dictionary<string, Transform> pais)
        {
            ini.transform.SetParent(Pai(pais, "Adversários"), true);
            inimigos.Add(ini);
        }

        /// <summary>Sprite da decoração de cada letra do mapa (l t b m k r).</summary>
        public static string NomeDecoracao(char c)
        {
            switch (c)
            {
                case Legenda.Lampiao: return "lampiao";
                case Legenda.Arvore: return "arvore";
                case Legenda.Banco: return "banco";
                case Legenda.Mastro: return "mastro";
                case Legenda.BarrilDecorativo: return "barril";
                case Legenda.Refletor: return "refletor";
            }
            return null;
        }

        void Decoracao(char c, Vector3 pe, Dictionary<string, Transform> pais)
        {
            string nome = NomeDecoracao(c);
            if (nome == null)
                return;
            Sprite[] quadros = Arte.Anim(nome);
            if (quadros.Length == 0)
                return;
            SpriteRenderer sr = Efeitos.NovoSprite(nome, Pai(pais, "Decoração"), quadros[0],
                c == Legenda.Refletor ? Ordem.Estadio + 1 : Ordem.DecoracaoFundo);
            sr.transform.position = pe;
            if (quadros.Length > 1)
            {
                var anim = sr.gameObject.AddComponent<AnimadorSprite>();
                anim.alvo = sr;
                anim.Tocar(nome, 6f);
                anim.Dessincronizar();
            }
        }

        /// <summary>Fecha a arena final (parede invisível atrás do jogador).</summary>
        public void FecharArena()
        {
            if (paredeArena != null)
                paredeArena.enabled = true;
        }

#if UNITY_EDITOR
        /// <summary>Mostra o formato da fase na aba Scene, mesmo sem dar Play.</summary>
        void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;
            TextAsset arquivo = Resources.Load<TextAsset>("Fases/" + arquivoFase);
            if (arquivo == null)
                return;
            MapaFase m = MapaFase.Ler(arquivo.text);
            for (int y = 0; y < m.Altura; y++)
            {
                for (int x = 0; x < m.Largura; x++)
                {
                    char c = m.Em(x, y);
                    Color cor;
                    switch (c)
                    {
                        case Legenda.Pedra: cor = new Color(0.55f, 0.6f, 0.7f, 0.8f); break;
                        case Legenda.Doca: cor = new Color(0.6f, 0.42f, 0.25f, 0.8f); break;
                        case Legenda.Rua: cor = new Color(0.65f, 0.55f, 0.45f, 0.8f); break;
                        case Legenda.Grama: cor = new Color(0.3f, 0.65f, 0.3f, 0.8f); break;
                        case Legenda.Tabua: cor = new Color(0.9f, 0.75f, 0.5f, 0.8f); break;
                        case Legenda.AguaTopo:
                        case Legenda.AguaFundo: cor = new Color(0.2f, 0.45f, 0.9f, 0.6f); break;
                        case Legenda.Ponte: cor = new Color(0.8f, 0.6f, 0.3f, 0.9f); break;
                        case Legenda.Inicio: cor = new Color(0.2f, 1f, 0.3f, 1f); break;
                        case Legenda.Sino: cor = new Color(1f, 0.85f, 0.2f, 1f); break;
                        case Legenda.Carrinho:
                        case Legenda.Saltador:
                        case Legenda.Capitao:
                        case Legenda.Goleiro: cor = new Color(1f, 0.3f, 0.3f, 1f); break;
                        case Legenda.Gol: cor = Color.white; break;
                        default: continue;
                    }
                    Gizmos.color = cor;
                    bool cheio = Legenda.EhSolido(c) || Legenda.EhAgua(c) || c == Legenda.Tabua || c == Legenda.Ponte;
                    float tam = cheio ? 1f : 0.6f;
                    Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0f), new Vector3(tam, tam, 0.1f));
                }
            }
        }
#endif
    }
}
