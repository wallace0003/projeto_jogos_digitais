using UnityEngine;
using UnityEngine.UI;

namespace CincoEstrelas
{
    /// <summary>
    /// Cena "Menu" (tela inicial): título, as cinco estrelas da jornada e as
    /// orientações de como jogar (controles da pág. 4 do GDD, objetivo, como
    /// ganhar e como perder), com os botões JOGAR e SAIR. Ao fundo, Estocolmo
    /// passa devagar e o Camisa 10 faz embaixadinhas no cais.
    /// </summary>
    public class MenuPrincipal : MonoBehaviour
    {
        [Header("Materiais do URP 2D")]
        public Material materialAceso;
        public Material materialBrilho;

        [Header("Créditos (edite com os nomes do grupo)")]
        [TextArea] public string creditos = Textos.CreditosPadrao;

        public static MenuPrincipal Atual { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Atual = null;
        }

        Button botaoJogar;
        Canvas tela;
        Image[] estrelas = new Image[5];

        void Awake()
        {
            Atual = this;
        }

        void OnDestroy()
        {
            if (Atual == this)
                Atual = null;
        }

        public Button BotaoJogar
        {
            get { return botaoJogar; }
        }

        /// <summary>O Canvas da tela inicial (com o título e as instruções).</summary>
        public Canvas Tela
        {
            get { return tela; }
        }

        void Start()
        {
            Materiais2D.Definir(materialAceso, materialBrilho);
            Camadas.ConfigurarColisoes();
            Time.timeScale = 1f;

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = CameraSeguir.TamanhoOrtografico;
                cam.transform.position = new Vector3(0f, 0f, -10f);
                Fundo.Criar(cam.transform, 1.2f);
                Fundo.Cais(-3f, 40f);
                CriarPersonagens();
            }

            FabricaUI.GarantirEventSystem();
            Canvas canvas = FabricaUI.CriarCanvas("Menu", 10);
            tela = canvas;
            Transform t = canvas.transform;
            Vector2 c = new Vector2(0.5f, 0.5f);

            TextoPixel titulo = FabricaUI.Texto(t, Textos.Titulo, c, new Vector2(0f, 222f), 6f, FabricaUI.CorDestaque,
                TextAnchor.MiddleCenter, new Vector2(900f, 72f));
            titulo.contorno = true;
            titulo.quebrarLinhas = false;
            titulo.corContorno = new Color(0.04f, 0.3f, 0.14f, 1f);
            FabricaUI.Texto(t, Textos.Subtitulo, c, new Vector2(0f, 178f), 2.5f, FabricaUI.CorTexto).contorno = true;
            for (int i = 0; i < 5; i++)
                estrelas[i] = FabricaUI.Imagem(t, Arte.Sprite(i == 0 ? "estrela_cheia" : "estrela_vazia"), c,
                    new Vector2(-80f + i * 40f, 148f), new Vector2(30f, 30f));
            FabricaUI.Texto(t, Textos.FaseDoMenu, c, new Vector2(0f, 120f), 2f, FabricaUI.CorTexto).contorno = true;

            // orientações de como jogar, direto na tela inicial
            FabricaUI.Painel(t, c, new Vector2(0f, -30f), new Vector2(770f, 286f));
            MontarComoJogar(t, c);
            MontarObjetivo(t, c);

            botaoJogar = FabricaUI.Botao(t, Textos.Jogar, c, new Vector2(-120f, -204f), new Vector2(210f, 42f), Jogar);
            Button sair = FabricaUI.Botao(t, Textos.Sair, c, new Vector2(120f, -204f), new Vector2(210f, 42f), Sair);
            FabricaUI.NavegacaoHorizontal(botaoJogar, sair);
            FabricaUI.Selecionar(botaoJogar);

            if (Sessao.MelhorTempo > 0f)
                FabricaUI.Texto(t, string.Format(Textos.FormatoMelhorTempo, Textos.FormatarTempo(Sessao.MelhorTempo)), c, new Vector2(0f, -238f), 1.5f,
                    FabricaUI.CorDestaque).contorno = true;
            TextoPixel cred = FabricaUI.Texto(t, creditos, new Vector2(0.5f, 0f), new Vector2(0f, 10f), 1f, FabricaUI.CorTexto,
                TextAnchor.MiddleCenter, new Vector2(900f, 14f));
            cred.contorno = true;

            SomJogo.Musica(Trilha.Menu);
            Transicao.AoEntrarNaCena();
        }

        void Update()
        {
            if (Entrada.MudoApertado)
                SomJogo.AlternarMudo();
            // a primeira estrela (Suécia 1958) pulsa: é a que está em jogo
            if (estrelas[0] != null)
                estrelas[0].transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 4f) * 0.1f);
        }

        /// <summary>Coluna da esquerda: "COMO JOGAR" com as teclas e o que cada uma faz.</summary>
        static void MontarComoJogar(Transform t, Vector2 c)
        {
            FabricaUI.Texto(t, Textos.ComoJogar, c, new Vector2(-160f, 92f), 2.5f, FabricaUI.CorDestaque,
                TextAnchor.MiddleCenter, new Vector2(440f, 30f)).contorno = true;
            int linhas = Textos.Controles.Length / 2;
            for (int i = 0; i < linhas; i++)
            {
                float y = 64f - i * 18.5f;
                TextoPixel tecla = FabricaUI.Texto(t, Textos.Controles[i * 2], c, new Vector2(-280f, y), 1.5f, FabricaUI.CorVerde,
                    TextAnchor.MiddleRight, new Vector2(190f, 18f));
                tecla.quebrarLinhas = false;
                TextoPixel acao = FabricaUI.Texto(t, Textos.Controles[i * 2 + 1], c, new Vector2(-48f, y), 1.5f, FabricaUI.CorTexto,
                    TextAnchor.MiddleLeft, new Vector2(260f, 18f));
                acao.quebrarLinhas = false;
            }
        }

        /// <summary>Coluna da direita: objetivo, como ganhar e como perder.</summary>
        static void MontarObjetivo(Transform t, Vector2 c)
        {
            const float x = 222f;
            var tamanho = new Vector2(300f, 54f);
            FabricaUI.Texto(t, Textos.Objetivo, c, new Vector2(x, 92f), 2.5f, FabricaUI.CorDestaque,
                TextAnchor.MiddleCenter, new Vector2(300f, 30f)).contorno = true;
            FabricaUI.Texto(t, Textos.TextoObjetivo, c, new Vector2(x, 50f), 1.5f, FabricaUI.CorTexto, TextAnchor.MiddleCenter, tamanho);
            FabricaUI.Texto(t, Textos.ComoGanhar, c, new Vector2(x, 8f), 2f, FabricaUI.CorVerde,
                TextAnchor.MiddleCenter, new Vector2(300f, 24f));
            FabricaUI.Texto(t, Textos.TextoComoGanhar, c, new Vector2(x, -24f), 1.5f, FabricaUI.CorTexto, TextAnchor.MiddleCenter, tamanho);
            FabricaUI.Texto(t, Textos.ComoPerder, c, new Vector2(x, -64f), 2f, FabricaUI.CorVermelha,
                TextAnchor.MiddleCenter, new Vector2(300f, 24f));
            FabricaUI.Texto(t, Textos.TextoComoPerder, c, new Vector2(x, -110f), 1.5f, FabricaUI.CorTexto, TextAnchor.MiddleCenter,
                new Vector2(300f, 72f));
        }

        /// <summary>O Camisa 10 faz embaixadinhas no cais e o goleiro espera do outro lado.</summary>
        void CriarPersonagens()
        {
            const float y = -3f;
            const float escala = 1.5f;
            SpriteRenderer craque = Efeitos.NovoSprite("Camisa 10", null, null, Ordem.Jogador);
            craque.transform.position = new Vector3(-10.4f, y, 0f);
            craque.transform.localScale = Vector3.one * escala;
            var an = craque.gameObject.AddComponent<AnimadorSprite>();
            an.alvo = craque;
            an.usarTempoReal = true;
            an.Tocar("camisa10_embaixadinha", 4f / Jogador.CicloEmbaixadinha);

            SpriteRenderer bola = Efeitos.NovoSprite("Bola", null, Arte.Sprite("bola"), Ordem.Bola);
            bola.transform.localScale = Vector3.one * escala;
            var q = bola.gameObject.AddComponent<BolaQuicando>();
            q.pe = new Vector3(-10.4f + 0.32f * escala, y + 0.55f * escala, 0f);
            q.altura = 1.1f * escala;
            q.periodo = Jogador.CicloEmbaixadinha;

            SpriteRenderer goleiro = Efeitos.NovoSprite("Goleiro", null, null, Ordem.Goleiro);
            goleiro.transform.position = new Vector3(10.4f, y, 0f);
            goleiro.transform.localScale = Vector3.one * escala;
            goleiro.flipX = true;
            var an2 = goleiro.gameObject.AddComponent<AnimadorSprite>();
            an2.alvo = goleiro;
            an2.usarTempoReal = true;
            an2.Tocar("goleiro_parado", 3f);
        }

        void Jogar()
        {
            if (Transicao.Carregando)
                return;
            Sessao.NovoJogo();
            Transicao.Carregar(Cenas.Fase);
        }

        void Sair()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
