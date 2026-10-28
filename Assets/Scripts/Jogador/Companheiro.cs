using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Companheiro de equipe (GDD, M09): entra correndo por alguns segundos,
    /// recebe o passe, distrai a marcação e devolve a bola (tabela). Depois sai
    /// de cena. Chamado com a tecla Q.
    /// </summary>
    public class Companheiro : MonoBehaviour
    {
        public static Companheiro Atual { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Atual = null;
        }

        enum Fase
        {
            Entrando,
            Tabela,
            Saindo,
        }

        public const float DuracaoDistracao = 2.5f;
        public const float RaioDistracao = 8f;
        const float Velocidade = 11f;

        SpriteRenderer sr;
        AnimadorSprite anim;
        Jogador jogador;
        Fase fase;
        float alvoX;
        float timer;
        int lado;
        bool fezTabela;

        /// <summary>Onde a bola fica quando está com ele.</summary>
        public Vector2 PosicaoPe
        {
            get { return (Vector2)transform.position + new Vector2(-lado * ParametrosJogador.DistanciaBolaNoPe, ParametrosJogador.AlturaBolaNoPe); }
        }

        /// <summary>Entra pela borda da tela, atrás do jogador, e para um pouco à frente dele.</summary>
        public static Companheiro Chamar(Jogador j, float xEntrada)
        {
            var go = new GameObject("Companheiro");
            go.transform.position = new Vector3(xEntrada, j.transform.position.y, 0f);
            Companheiro c = go.AddComponent<Companheiro>();
            c.jogador = j;
            c.lado = j.Direcao;
            c.alvoX = j.transform.position.x + j.Direcao * 3.2f;
            return c;
        }

        void Awake()
        {
            Atual = this;
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = Ordem.Companheiro;
            Materiais2D.Aplicar(sr, false);
            anim = gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
            anim.Tocar("companheiro_correr", 14f);
            fase = Fase.Entrando;
        }

        void OnDestroy()
        {
            if (Atual == this)
                Atual = null;
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;
            float dt = Time.deltaTime;
            if (jogador == null)
            {
                Destroy(gameObject);
                return;
            }
            Vector3 p = transform.position;
            // acompanha a altura do jogador (ele corre "no chão" do jogador)
            p.y = Mathf.MoveTowards(p.y, jogador.NoChao ? jogador.transform.position.y : p.y, 12f * dt);

            switch (fase)
            {
                case Fase.Entrando:
                    alvoX = jogador.transform.position.x + lado * 3.2f;
                    p.x = Mathf.MoveTowards(p.x, alvoX, Velocidade * dt);
                    sr.flipX = alvoX < p.x;
                    transform.position = p;
                    if (Mathf.Abs(p.x - alvoX) < 0.05f)
                        Chegou();
                    break;
                case Fase.Tabela:
                    timer -= dt;
                    sr.flipX = jogador.transform.position.x < p.x;
                    bool comBola = jogador.Bola != null && jogador.Bola.ComOCompanheiro;
                    anim.Tocar(comBola ? "companheiro_passe" : "companheiro_parado", comBola ? 6f : 4f);
                    transform.position = p;
                    if (timer <= 0f && (jogador.Bola == null || !jogador.Bola.NaTabela))
                    {
                        fase = Fase.Saindo;
                        timer = 2f;
                        anim.Tocar("companheiro_correr", 14f);
                    }
                    break;
                case Fase.Saindo:
                    timer -= dt;
                    p.x += lado * Velocidade * dt;
                    sr.flipX = lado < 0;
                    transform.position = p;
                    if (timer <= 0f)
                        Destroy(gameObject);
                    break;
            }
        }

        void Chegou()
        {
            fase = Fase.Tabela;
            timer = 1.3f;
            anim.Tocar("companheiro_comemorar", 6f);
            int distraidos = DistrairMarcacao();
            if (jogador.Bola != null && jogador.Bola.NoPe && jogador.Vivo)
                fezTabela = jogador.Bola.IniciarTabela(this);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.CompanheiroChegou(distraidos, fezTabela, transform.position + Vector3.up * 2f);
        }

        /// <summary>Todos os adversários perto dele ficam olhando para ele por alguns segundos.</summary>
        int DistrairMarcacao()
        {
            int n = 0;
            foreach (Inimigo ini in Inimigo.Todos.ToArray())
            {
                if (ini == null || ini.Superado)
                    continue;
                if (Mathf.Abs(ini.transform.position.x - transform.position.x) <= RaioDistracao)
                {
                    ini.Distrair(DuracaoDistracao);
                    n++;
                }
            }
            return n;
        }
    }
}
