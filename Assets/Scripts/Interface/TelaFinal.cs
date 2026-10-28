using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CincoEstrelas
{
    /// <summary>
    /// Cena "FimDeJogo": vitória (a primeira estrela acende) ou derrota, com as
    /// estatísticas da partida e os botões JOGAR DE NOVO e MENU.
    /// </summary>
    public class TelaFinal : MonoBehaviour
    {
        [Header("Materiais do URP 2D")]
        public Material materialAceso;
        public Material materialBrilho;

        public static TelaFinal Atual { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Atual = null;
        }

        Button botaoJogarDeNovo;
        Image estrelaGrande;
        bool vitoria;

        void Awake()
        {
            Atual = this;
        }

        void OnDestroy()
        {
            if (Atual == this)
                Atual = null;
        }

        public Button BotaoJogarDeNovo
        {
            get { return botaoJogarDeNovo; }
        }

        public bool Vitoria
        {
            get { return vitoria; }
        }

        void Start()
        {
            EstadoPartida e = Sessao.Estado;
            // sem partida terminada (Play direto nesta cena): vai para o menu
            if (e.Resultado == ResultadoJogo.EmAndamento && !Partida.PodePularTelaInicial())
            {
                SceneManager.LoadScene(Cenas.Menu);
                return;
            }
            vitoria = e.Resultado == ResultadoJogo.Vitoria;
            Materiais2D.Definir(materialAceso, materialBrilho);
            Time.timeScale = 1f;

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = CameraSeguir.TamanhoOrtografico;
                cam.transform.position = new Vector3(0f, 0f, -10f);
                Fundo.Criar(cam.transform, 0.6f);
                Fundo.Cais(-3f, 40f);
                SpriteRenderer craque = Efeitos.NovoSprite("Camisa 10", null, null, Ordem.Jogador);
                craque.transform.position = new Vector3(-10.4f, -3f, 0f);
                craque.transform.localScale = Vector3.one * 1.5f;
                var an = craque.gameObject.AddComponent<AnimadorSprite>();
                an.alvo = craque;
                an.usarTempoReal = true;
                an.Tocar(vitoria ? "camisa10_vitoria" : "camisa10_dano", 3f);
            }

            FabricaUI.GarantirEventSystem();
            Canvas canvas = FabricaUI.CriarCanvas("Tela final", 10);
            Transform t = canvas.transform;
            Vector2 c = new Vector2(0.5f, 0.5f);

            TextoPixel titulo = FabricaUI.Texto(t, vitoria ? Textos.VitoriaTitulo : Textos.DerrotaTitulo, c, new Vector2(0f, 222f),
                vitoria ? 4f : 5f, vitoria ? FabricaUI.CorDestaque : FabricaUI.CorVermelha, TextAnchor.MiddleCenter,
                new Vector2(940f, 60f));
            titulo.contorno = true;
            titulo.quebrarLinhas = false;

            if (vitoria)
            {
                estrelaGrande = FabricaUI.Imagem(t, Arte.Sprite("estrela_grande_vazia"), c, new Vector2(0f, 140f), new Vector2(96f, 96f));
                for (int i = 0; i < 5; i++)
                    FabricaUI.Imagem(t, Arte.Sprite(i == 0 ? "estrela_cheia" : "estrela_vazia"), c, new Vector2(-80f + i * 40f, 76f),
                        new Vector2(30f, 30f));
            }
            FabricaUI.Texto(t, vitoria ? Textos.VitoriaTexto : Textos.DerrotaTexto, c, new Vector2(0f, vitoria ? 40f : 150f), 2f,
                FabricaUI.CorTexto, TextAnchor.MiddleCenter, new Vector2(760f, 50f)).contorno = true;

            // estatísticas
            FabricaUI.Painel(t, c, new Vector2(0f, -64f), new Vector2(520f, 170f));
            string[,] linhas =
            {
                { Textos.Tempo, Textos.FormatarTempo(e.TempoDeJogo) },
                { Textos.GolsNaFinal, e.Gols + " / " + EstadoPartida.GolsParaVencer },
                { Textos.QuizzesCertos, e.QuizzesAcertados + " / " + BancoQuiz.Todas().Length },
                { Textos.MaiorCombo, e.MaiorCombo.ToString() },
                { Textos.Superados, e.AdversariosSuperados.ToString() },
                { Textos.VidasRestantes, e.Vidas.ToString() },
            };
            for (int i = 0; i < linhas.GetLength(0); i++)
            {
                float y = -6f - i * 23f;
                FabricaUI.Texto(t, linhas[i, 0], c, new Vector2(-70f, y), 2f, FabricaUI.CorApagada, TextAnchor.MiddleRight,
                    new Vector2(320f, 22f)).quebrarLinhas = false;
                FabricaUI.Texto(t, linhas[i, 1], c, new Vector2(150f, y), 2f, FabricaUI.CorTexto, TextAnchor.MiddleLeft,
                    new Vector2(200f, 22f)).quebrarLinhas = false;
            }
            if (vitoria && Sessao.NovoRecorde)
                FabricaUI.Texto(t, Textos.NovoRecorde, c, new Vector2(0f, -166f), 2f, FabricaUI.CorVerde).contorno = true;

            botaoJogarDeNovo = FabricaUI.Botao(t, Textos.JogarDeNovo, c, new Vector2(-140f, -208f), new Vector2(250f, 44f), JogarDeNovo);
            Button menu = FabricaUI.Botao(t, Textos.Menu, c, new Vector2(140f, -208f), new Vector2(250f, 44f), IrParaMenu);
            FabricaUI.NavegacaoHorizontal(botaoJogarDeNovo, menu);
            FabricaUI.Selecionar(botaoJogarDeNovo);

            SomJogo.Musica(vitoria ? Trilha.Vitoria : Trilha.Derrota, false);
            Transicao.AoEntrarNaCena();
            if (vitoria)
                StartCoroutine(AcenderEstrela());
        }

        /// <summary>A estrela grande acende: a primeira das cinco estrelas da camisa.</summary>
        IEnumerator AcenderEstrela()
        {
            yield return new WaitForSecondsRealtime(0.8f);
            estrelaGrande.sprite = Arte.Sprite("estrela_grande_cheia");
            SomJogo.Tocar(Som.Gol);
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.unscaledDeltaTime;
                float s = 1f + Mathf.Sin(Mathf.Clamp01(t / 0.5f) * Mathf.PI) * 0.35f;
                estrelaGrande.transform.localScale = Vector3.one * s;
                yield return null;
            }
            estrelaGrande.transform.localScale = Vector3.one;
            for (int i = 0; i < 6; i++)
            {
                Efeitos.Confete(new Vector3(Random.Range(-9f, 9f), 7f, 0f), 16);
                yield return new WaitForSecondsRealtime(0.35f);
            }
        }

        void Update()
        {
            if (Entrada.MudoApertado)
                SomJogo.AlternarMudo();
            if (estrelaGrande != null)
                estrelaGrande.transform.Rotate(0f, 0f, Mathf.Sin(Time.unscaledTime * 2f) * 6f * Time.unscaledDeltaTime);
        }

        void JogarDeNovo()
        {
            if (Transicao.Carregando)
                return;
            Sessao.NovoJogo();
            Sessao.IntroVista = true;
            Transicao.Carregar(Cenas.Fase);
        }

        void IrParaMenu()
        {
            if (Transicao.Carregando)
                return;
            Sessao.Encerrar();
            Transicao.Carregar(Cenas.Menu);
        }
    }
}
