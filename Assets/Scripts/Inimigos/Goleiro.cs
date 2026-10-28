using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Goleiro final da Suécia 1958 (GDD, I08 e "confrontos finais": defende
    /// chutes rasteiros; habilidade testada: cavadinha e variação de carga).
    /// Na linha ele alcança a altura toda do gol: chute fraco ou médio faz ele
    /// mergulhar e ficar caído (é a hora de chutar de novo); chute forte ele
    /// espalma e levanta na hora. De vez em quando sai do gol para abafar:
    /// aí só a cavadinha passa por cima dele. Também arremessa bolas no jogador.
    /// As decisões ficam em <see cref="CerebroGoleiro"/>.
    /// </summary>
    public class Goleiro : MonoBehaviour
    {
        public static Goleiro Atual { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Atual = null;
        }

        const float VelocidadeSaida = 7f;
        const float VelocidadeVolta = 6f;
        /// <summary>Distância que ele para à frente do jogador quando sai do gol.</summary>
        const float DistanciaAbafar = 2.4f;

        readonly CerebroGoleiro cerebro = new CerebroGoleiro();
        Rigidbody2D rb;
        BoxCollider2D luvas;
        SpriteRenderer sr;
        AnimadorSprite anim;
        Transform visual;
        float xCasa;
        float xLinhaGol;
        float xArena;
        float alturaGol;
        float alvoSaida;
        bool ativo;
        float timerFeedback;

        public CerebroGoleiro Cerebro
        {
            get { return cerebro; }
        }

        public bool Ativo
        {
            get { return ativo; }
        }

        public Vector3 Centro
        {
            get { return transform.position + Vector3.up * 0.8f; }
        }

        /// <param name="pe">Posição de casa (pé), na frente da linha do gol.</param>
        /// <param name="linhaGol">x da linha do gol (ele fica à esquerda dela, olhando para o campo).</param>
        /// <param name="inicioArena">x do começo da arena (ele nunca sai além disso).</param>
        /// <param name="altura">Altura do travessão em relação ao chão.</param>
        public static Goleiro Criar(Vector3 pe, float linhaGol, float inicioArena, float altura)
        {
            var go = new GameObject("Goleiro");
            go.layer = Camadas.Inimigo;
            go.transform.position = pe;
            Goleiro g = go.AddComponent<Goleiro>();
            g.xCasa = pe.x;
            g.xLinhaGol = linhaGol;
            g.xArena = inicioArena;
            g.alturaGol = altura;
            g.AjustarLuvas();
            return g;
        }

        void Awake()
        {
            Atual = this;
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            luvas = gameObject.AddComponent<BoxCollider2D>();
            luvas.isTrigger = true;
            luvas.size = new Vector2(1.2f, 3f);

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);
            sr = visual.gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = Ordem.Goleiro;
            sr.flipX = true; // olha para o campo (para a esquerda)
            Materiais2D.Aplicar(sr, false);
            anim = visual.gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
            anim.Tocar("goleiro_parado", 3f);
        }

        void OnDestroy()
        {
            if (Atual == this)
                Atual = null;
        }

        /// <summary>O jogador entrou na arena: a final começa.</summary>
        public void Ativar()
        {
            ativo = true;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            timerFeedback -= dt;
            GerenciadorFase g = GerenciadorFase.Instancia;
            Jogador j = Jogador.Atual;
            if (!ativo || j == null || (g != null && g.Terminou))
            {
                anim.Tocar("goleiro_parado", 3f);
                return;
            }

            float distancia = Mathf.Abs(j.transform.position.x - rb.position.x);
            EventoGoleiro ev = cerebro.Atualizar(dt, distancia);
            switch (ev)
            {
                case EventoGoleiro.IniciarSaida:
                    alvoSaida = Mathf.Clamp(j.transform.position.x + DistanciaAbafar, xArena + 1.5f, xCasa - 1f);
                    if (g != null)
                        g.Mensagem(Textos.GoleiroSaiu, FabricaUI.CorAzul, 1.2f);
                    SomJogo.Tocar(Som.Apito, 0.35f);
                    break;
                case EventoGoleiro.Arremessar:
                    Arremessar(j);
                    break;
            }

            Vector2 pos = rb.position;
            switch (cerebro.Postura)
            {
                case PosturaGoleiro.NaLinha:
                    pos.x = Mathf.MoveTowards(pos.x, xCasa, VelocidadeVolta * dt);
                    bool bolaVindo = j.Bola != null && j.Bola.Voando && j.Bola.Velocidade.x > 0f;
                    anim.Tocar(bolaVindo ? "goleiro_pronto" : "goleiro_parado", 4f);
                    break;
                case PosturaGoleiro.Espalmando:
                    anim.Tocar("goleiro_espalmar", 1f);
                    break;
                case PosturaGoleiro.Caido:
                    anim.Tocar("goleiro_caido", 1f);
                    break;
                case PosturaGoleiro.Levantando:
                    anim.Tocar("goleiro_levantar", 1f);
                    break;
                case PosturaGoleiro.Saindo:
                    pos.x = Mathf.MoveTowards(pos.x, alvoSaida, VelocidadeSaida * dt);
                    anim.Tocar("goleiro_correr", 12f);
                    if (Mathf.Abs(pos.x - alvoSaida) < 0.05f)
                        cerebro.ChegouForaDoGol();
                    break;
                case PosturaGoleiro.ForaDoGol:
                    anim.Tocar("goleiro_pronto", 1f);
                    break;
                case PosturaGoleiro.Segurando:
                    anim.Tocar("goleiro_segurar", 1f);
                    break;
                case PosturaGoleiro.Voltando:
                    pos.x = Mathf.MoveTowards(pos.x, xCasa, VelocidadeVolta * dt);
                    anim.Tocar("goleiro_correr", 12f);
                    if (Mathf.Abs(pos.x - xCasa) < 0.05f)
                        cerebro.ChegouNaLinha();
                    break;
            }
            rb.MovePosition(pos);
            AjustarLuvas();
        }

        /// <summary>
        /// Na linha, as luvas cobrem a boca do gol inteira (do chão ao travessão).
        /// Fora do gol, só o corpo (cabeça a ~2 tiles): a cavadinha passa por cima.
        /// Caído, não alcança nada.
        /// </summary>
        void AjustarLuvas()
        {
            if (luvas == null)
                return;
            PosturaGoleiro p = cerebro.Postura;
            luvas.enabled = RegrasGoleiro.PodeDefender(p);
            if (RegrasGoleiro.NaLinha(p))
            {
                // da frente do corpo até a linha do gol
                float largura = Mathf.Max(1.2f, xLinhaGol - rb.position.x + 0.6f);
                luvas.size = new Vector2(largura, alturaGol + 0.1f);
                luvas.offset = new Vector2(largura * 0.5f - 0.6f, (alturaGol + 0.1f) * 0.5f);
            }
            else
            {
                luvas.size = new Vector2(1f, 1.9f);
                luvas.offset = new Vector2(0f, 0.95f);
            }
        }

        /// <summary>A bola chegou nas luvas: o cérebro decide; aqui ficam o som e o texto.</summary>
        public ReacaoGoleiro ReceberBola(Bola bola)
        {
            ReacaoGoleiro r = cerebro.ReceberChute(bola.UltimoChute, bola.UltimoModificador);
            GerenciadorFase g = GerenciadorFase.Instancia;
            switch (r)
            {
                case ReacaoGoleiro.Defendeu:
                case ReacaoGoleiro.DefesaDificil:
                    SomJogo.Tocar(Som.Defesa);
                    Efeitos.Animacao("poeira", transform.position + new Vector3(-0.6f, 0.2f, 0f), 16f, 1.2f);
                    anim.Tocar("goleiro_mergulho", 1f);
                    if (g != null)
                        g.Mensagem(r == ReacaoGoleiro.Defendeu ? Textos.GoleiroDefendeu : Textos.GoleiroDefesaDificil,
                            FabricaUI.CorVerde, 1.6f);
                    break;
                case ReacaoGoleiro.Espalmou:
                    SomJogo.Tocar(Som.Defesa);
                    anim.Tocar("goleiro_espalmar", 1f);
                    if (g != null)
                        g.Mensagem(Textos.GoleiroEspalmou, FabricaUI.CorVermelha, 1.4f);
                    break;
                case ReacaoGoleiro.Segurou:
                    SomJogo.Tocar(Som.Defesa, 0.7f);
                    anim.Tocar("goleiro_segurar", 1f);
                    if (g != null)
                        g.Mensagem(Textos.GoleiroSegurou, FabricaUI.CorVermelha, 1.6f);
                    break;
                case ReacaoGoleiro.Atropelado:
                    CameraSeguir.Tremer(0.3f, 0.3f);
                    Efeitos.Particulas("brilho", Centro, 10, new Color(1f, 0.6f, 0.2f), 3f, 6f, 0.5f, 2f, true);
                    break;
            }
            if (r != ReacaoGoleiro.Nenhuma && g != null)
                g.ChuteNoGol();
            return r;
        }

        void Arremessar(Jogador j)
        {
            anim.Tocar("goleiro_arremesso", 6f, false, true);
            Vector3 maos = transform.position + new Vector3(-0.4f, 1.3f, 0f);
            BolaGoleiro.Criar(maos, j.transform.position + Vector3.up * 0.6f);
            SomJogo.Tocar(Som.Cavadinha, 0.6f);
        }

        /// <summary>Depois do gol ele volta para debaixo da trave.</summary>
        public void SofreuGol()
        {
            cerebro.SofreuGol();
        }

        /// <summary>O jogador perdeu uma vida: o goleiro volta para a linha.</summary>
        public void Reiniciar()
        {
            cerebro.Reiniciar();
            transform.position = new Vector3(xCasa, transform.position.y, 0f);
            rb.position = transform.position;
            AjustarLuvas();
        }

        void OnTriggerStay2D(Collider2D outro)
        {
            // saindo do gol ele chega "duro" no jogador
            PosturaGoleiro p = cerebro.Postura;
            if (p != PosturaGoleiro.Saindo && p != PosturaGoleiro.ForaDoGol)
                return;
            Jogador j = outro.GetComponent<Jogador>();
            if (j == null || !j.Vivo)
                return;
            if (j.transform.position.y > transform.position.y + 1.5f && j.VelocidadeY < 1f)
            {
                j.Quicar();
                return;
            }
            j.TomarDano(Centro);
        }
    }
}
