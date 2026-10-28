using System.Collections.Generic;
using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// A bola do Camisa 10. No pé ela acompanha o jogador (e sobe e desce nas
    /// embaixadinhas); chutada vira um corpo físico que quica no chão e nas
    /// paredes. Acerta adversários, o goleiro, o sino, caixotes e barris; entra
    /// no gol. Depois de um tempo (ou apertando X) volta sozinha para o pé.
    /// As decisões ficam em <see cref="EstadoBola"/>.
    /// </summary>
    public class Bola : MonoBehaviour
    {
        public const float Raio = 0.22f;
        const float TempoIdaTabela = 0.35f;
        const float TempoComCompanheiro = 0.35f;
        const float TempoVoltaTabela = 0.45f;

        Jogador dono;
        Rigidbody2D rb;
        CircleCollider2D col;
        Transform visual;
        SpriteRenderer sr;
        SpriteRenderer fogo;
        AnimadorSprite animFogo;
        ParametrosChute parametros;
        float tempoRastro;
        float giro;
        float tempoQuique;
        Vector2 velocidadeAntes;
        bool golMarcado;
        readonly HashSet<Inimigo> acertados = new HashSet<Inimigo>();
        bool goleiroTocou;

        // tabela com o companheiro
        Companheiro companheiro;
        float tempoTabela;
        Vector2 inicioTabela;

        public EstadoBola Estado { get; private set; }

        public bool NoPe
        {
            get { return Estado.NoPe; }
        }

        public bool Voando
        {
            get { return Estado.Situacao == SituacaoBola.Voando; }
        }

        public bool NaTabela
        {
            get { return Estado.Situacao == SituacaoBola.Tabela; }
        }

        public Vector2 Velocidade
        {
            get { return Voando ? rb.linearVelocity : Vector2.zero; }
        }

        /// <summary>gravityScale do voo atual (para quem quiser prever a trajetória).</summary>
        public float EscalaGravidade
        {
            get { return rb.gravityScale; }
        }

        public ParametrosChute Parametros
        {
            get { return parametros; }
        }

        public TipoChute UltimoChute
        {
            get { return Estado.UltimoChute; }
        }

        public ModificadorChute UltimoModificador
        {
            get { return Estado.UltimoModificador; }
        }

        public Jogador Dono
        {
            get { return dono; }
        }

        /// <summary>Este chute já virou gol (o gol só conta uma vez por chute).</summary>
        public bool GolMarcado
        {
            get { return golMarcado; }
        }

        public static Bola Criar(Jogador dono)
        {
            var go = new GameObject("Bola");
            go.layer = Camadas.Bola;
            go.transform.position = dono.PosicaoPe;
            var b = go.AddComponent<Bola>();
            b.dono = dono;
            return b;
        }

        void Awake()
        {
            Estado = new EstadoBola();

            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 0.3f;
            rb.linearDamping = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.simulated = false; // no pé, quem move a bola é o código

            col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = Raio;
            var material = new PhysicsMaterial2D("Bola");
            material.bounciness = 0.55f;
            material.friction = 0.3f;
            col.sharedMaterial = material;

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);
            fogo = Efeitos.NovoSprite("Fogo", visual, Arte.Sprite("bola_fogo_0"), Ordem.Bola - 1, true);
            animFogo = fogo.gameObject.AddComponent<AnimadorSprite>();
            animFogo.alvo = fogo;
            animFogo.Tocar("bola_fogo", 14f);
            fogo.enabled = false;
            sr = Efeitos.NovoSprite("Sprite", visual, Arte.Sprite("bola"), Ordem.Bola, false);
        }

        void Update()
        {
            if (dono == null)
            {
                Destroy(gameObject);
                return;
            }
            if (Time.timeScale <= 0f)
                return;
            float dt = Time.deltaTime;

            switch (Estado.Situacao)
            {
                case SituacaoBola.NoPe:
                    SeguirOPe(dt);
                    break;
                case SituacaoBola.Voltando:
                    Voltar(dt);
                    break;
                case SituacaoBola.Tabela:
                    MoverTabela(dt);
                    break;
            }

            // a bola gira conforme anda
            float vx = Estado.Situacao == SituacaoBola.Voando ? rb.linearVelocity.x :
                (Estado.Situacao == SituacaoBola.NoPe ? dono.VelocidadeX : 0f);
            giro -= vx * dt * (180f / (Mathf.PI * Raio));
            if (Estado.Situacao == SituacaoBola.Voltando || Estado.Situacao == SituacaoBola.Tabela)
                giro += 900f * dt;
            visual.localRotation = Quaternion.Euler(0f, 0f, giro);
            fogo.transform.rotation = Quaternion.identity;

            // a bola de fogo e o canhão aparecem também com a bola no pé (aviso para o jogador)
            bool especial = Voando ? UltimoModificador == ModificadorChute.Especial : dono.EspecialArmado;
            bool canhao = Voando ? UltimoModificador == ModificadorChute.Canhao : dono.CanhaoRestante > 0f;
            fogo.enabled = especial || canhao;
            fogo.color = especial ? Color.white : new Color(1f, 0.8f, 0.45f, 0.75f);

            bool rapida = Estado.Situacao == SituacaoBola.Voltando ||
                          (Estado.Situacao == SituacaoBola.Voando && rb.linearVelocity.sqrMagnitude > 60f);
            if (rapida)
                Rastro(dt);
        }

        void FixedUpdate()
        {
            if (Estado.Situacao != SituacaoBola.Voando)
                return;
            float dt = Time.fixedDeltaTime;
            velocidadeAntes = rb.linearVelocity;
            tempoQuique -= dt;

            GerenciadorFase g = GerenciadorFase.Instancia;
            bool caiu = g != null && rb.position.y < g.LimiteQueda;
            float distancia = Vector2.Distance(rb.position, dono.PosicaoPe);
            Estado.AtualizarVoo(dt, rb.linearVelocity.magnitude, dono.Vivo ? distancia : 999f, caiu);
            if (Estado.Situacao == SituacaoBola.NoPe)
                Dominada();
            else if (Estado.Situacao == SituacaoBola.Voltando)
                ComecarVolta();
        }

        // ------------------------------------------------------------ no pé
        void SeguirOPe(float dt)
        {
            Vector2 alvo = dono.PosicaoPe;
            if (dono.FazendoEmbaixadinha)
            {
                // sobe e desce sincronizada com o pé (o pé sobe no começo de cada ciclo)
                float f = Mathf.Repeat(dono.TempoEmbaixadinha / Jogador.CicloEmbaixadinha, 1f);
                alvo = (Vector2)dono.transform.position +
                       new Vector2(dono.Direcao * 0.32f, 0.55f + Mathf.Sin(f * Mathf.PI) * 1.1f);
                transform.position = alvo;
                return;
            }
            if (Mathf.Abs(dono.VelocidadeX) > 0.6f && dono.NoChao)
            {
                // conduzindo: a bola vai um pouquinho à frente e volta
                alvo.x += dono.Direcao * (0.08f + Mathf.Sin(Time.time * 14f) * 0.08f);
            }
            if (!dono.NoChao)
                alvo.y += 0.05f;
            Vector2 atual = transform.position;
            float suave = 1f - Mathf.Exp(-30f * dt);
            transform.position = Vector2.Lerp(atual, alvo, suave);
        }

        /// <summary>Chamado pelo jogador. Devolve false se a bola não estava no pé.</summary>
        public bool Chutar(TipoChute tipo, ModificadorChute mod, int direcao, float velocidadeJogadorX)
        {
            if (!Estado.Chutar(tipo, mod))
                return false;
            parametros = Estado.ParametrosAtuais;
            float vx, vy;
            Chutes.VelocidadeInicial(parametros, direcao, velocidadeJogadorX, out vx, out vy);

            // se o pé está encostado na parede, a bola sai de dentro do jogador
            Vector2 inicio = dono.PosicaoPe;
            if (dono.FazendoEmbaixadinha || !dono.NoChao)
                inicio = transform.position;
            if (Physics2D.OverlapCircle(inicio, Raio, Camadas.MascaraSolida) != null)
                inicio = new Vector2(dono.transform.position.x, inicio.y + 0.1f);

            acertados.Clear();
            golMarcado = false;
            goleiroTocou = false;
            transform.position = inicio;
            rb.simulated = true;
            rb.position = inicio;
            rb.gravityScale = parametros.EscalaGravidade;
            rb.linearVelocity = new Vector2(vx, vy);
            velocidadeAntes = rb.linearVelocity;
            col.enabled = true;
            sr.color = UltimoModificador == ModificadorChute.Especial ? new Color(1f, 0.85f, 0.6f) : Color.white;

            Som som = Som.Chute;
            if (tipo == TipoChute.Cavadinha) som = Som.Cavadinha;
            else if (tipo == TipoChute.Bicicleta) som = Som.Bicicleta;
            else if (tipo == TipoChute.Superchute) som = Som.Superchute;
            else if (tipo == TipoChute.Forte || UltimoModificador != ModificadorChute.Nenhum) som = Som.ChuteForte;
            SomJogo.Tocar(som);
            if (UltimoModificador == ModificadorChute.Especial)
                SomJogo.Tocar(Som.Especial, 0.7f);
            return true;
        }

        /// <summary>Faz a bola voltar para o pé (apertar X com a bola longe).</summary>
        public void Chamar()
        {
            if (!Voando)
                return;
            Estado.Chamar();
            ComecarVolta();
        }

        /// <summary>Coloca a bola direto no pé (ex.: quando o jogador renasce).</summary>
        public void VoltarParaOPe()
        {
            companheiro = null;
            if (Estado.Situacao == SituacaoBola.Tabela)
                Estado.TerminarTabela();
            Estado.Dominar();
            Dominada();
            transform.position = dono.PosicaoPe;
        }

        void ComecarVolta()
        {
            rb.simulated = false;
            sr.color = Color.white;
            SomJogo.Tocar(Som.BolaVolta, 0.5f);
        }

        void Voltar(float dt)
        {
            Vector2 pos = transform.position;
            Vector2 alvo = dono.PosicaoPe;
            float vel = EstadoBola.VelocidadeVolta(Estado.TempoVoltando);
            pos = Vector2.MoveTowards(pos, alvo, vel * dt);
            transform.position = pos;
            Estado.AtualizarVolta(dt, Vector2.Distance(pos, alvo));
            if (Estado.Situacao == SituacaoBola.NoPe)
                Dominada();
        }

        void Dominada()
        {
            rb.simulated = false;
            sr.color = Color.white;
        }

        // ------------------------------------------------------------ tabela
        /// <summary>Passa a bola para o companheiro, que devolve em seguida. Devolve false se não deu.</summary>
        public bool IniciarTabela(Companheiro c)
        {
            if (c == null || !Estado.IniciarTabela())
                return false;
            companheiro = c;
            tempoTabela = 0f;
            inicioTabela = transform.position;
            rb.simulated = false;
            SomJogo.Tocar(Som.Tabela);
            return true;
        }

        void MoverTabela(float dt)
        {
            if (companheiro == null)
            {
                VoltarParaOPe();
                return;
            }
            tempoTabela += dt;
            Vector2 pe = companheiro.PosicaoPe;
            if (tempoTabela < TempoIdaTabela)
            {
                float t = tempoTabela / TempoIdaTabela;
                transform.position = Arco(inicioTabela, pe, t, 0.8f);
                return;
            }
            if (tempoTabela < TempoIdaTabela + TempoComCompanheiro)
            {
                transform.position = pe;
                return;
            }
            float tv = (tempoTabela - TempoIdaTabela - TempoComCompanheiro) / TempoVoltaTabela;
            if (tv < 1f)
            {
                transform.position = Arco(pe, dono.PosicaoPe, tv, 1.6f);
                return;
            }
            companheiro = null;
            Estado.TerminarTabela();
            Dominada();
            SomJogo.Tocar(Som.Tabela, 0.8f);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.TabelaConcluida(transform.position);
        }

        /// <summary>A bola chegou ao companheiro (para ele tocar a animação do passe).</summary>
        public bool ComOCompanheiro
        {
            get
            {
                return Estado.Situacao == SituacaoBola.Tabela && tempoTabela >= TempoIdaTabela &&
                       tempoTabela < TempoIdaTabela + TempoComCompanheiro;
            }
        }

        static Vector2 Arco(Vector2 de, Vector2 para, float t, float altura)
        {
            Vector2 p = Vector2.Lerp(de, para, t);
            p.y += Mathf.Sin(t * Mathf.PI) * altura;
            return p;
        }

        void Rastro(float dt)
        {
            tempoRastro -= dt;
            if (tempoRastro > 0f)
                return;
            tempoRastro = 0.03f;
            bool forte = Voando && (Parametros.Pesado || UltimoModificador != ModificadorChute.Nenhum);
            Color c = forte ? new Color(1f, 0.72f, 0.25f, 0.9f) : new Color(1f, 1f, 1f, 0.45f);
            Efeitos.Particulas("rastro", transform.position, 1, c, 0f, 0.2f, forte ? 0.35f : 0.2f, 0f, true);
        }

        // ------------------------------------------------------------ colisões
        void OnCollisionEnter2D(Collision2D c)
        {
            if (!Voando)
                return;
            if (tempoQuique <= 0f && velocidadeAntes.sqrMagnitude > 9f)
            {
                tempoQuique = 0.08f;
                SomJogo.Tocar(Som.Quique, Mathf.Clamp01(velocidadeAntes.magnitude / 16f) * 0.6f);
            }
            Obstaculo o = c.collider.GetComponentInParent<Obstaculo>();
            if (o != null && o.ReceberBola(this))
            {
                // derrubou: a bola segue em frente (atravessa o que quebrou)
                rb.linearVelocity = velocidadeAntes * 0.7f;
            }
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            TentarAcertar(outro);
        }

        void OnTriggerStay2D(Collider2D outro)
        {
            TentarAcertar(outro);
        }

        void TentarAcertar(Collider2D outro)
        {
            if (!Estado.PodeAcertar)
                return;

            Goleiro goleiro = outro.GetComponentInParent<Goleiro>();
            if (goleiro != null)
            {
                if (!goleiroTocou)
                    AcertarGoleiro(goleiro);
                return;
            }

            Inimigo ini = outro.GetComponentInParent<Inimigo>();
            if (ini == null || ini.Superado || acertados.Contains(ini))
                return;
            acertados.Add(ini);
            Vector2 v = rb.linearVelocity;
            ResultadoGolpe r = ini.ReceberBola(this);
            if (r == ResultadoGolpe.Defendeu)
            {
                // defendeu: a bola bate e volta, sem dano
                SomJogo.Tocar(Som.Defesa);
                rb.linearVelocity = new Vector2(-v.x * 0.45f, Mathf.Max(Mathf.Abs(v.y), 6f));
            }
            else
            {
                bool continua = Estado.RegistrarAcerto();
                if (!continua)
                {
                    // ricochete: a bola volta e sobe um pouco (dá para pegar outro na volta!)
                    rb.linearVelocity = new Vector2(-v.x * 0.55f, Mathf.Max(v.y, 5.5f));
                }
            }
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.AdversarioAtingido(ini, this, r);
        }

        void AcertarGoleiro(Goleiro goleiro)
        {
            ReacaoGoleiro r = goleiro.ReceberBola(this);
            if (r == ReacaoGoleiro.Nenhuma)
                return;
            goleiroTocou = true;
            Vector2 v = rb.linearVelocity;
            float paraOCampo = v.x > 0f ? -1f : 1f;
            switch (r)
            {
                case ReacaoGoleiro.Defendeu:
                case ReacaoGoleiro.DefesaDificil:
                    rb.linearVelocity = new Vector2(paraOCampo * Mathf.Max(4f, Mathf.Abs(v.x) * 0.35f), 6f);
                    break;
                case ReacaoGoleiro.Espalmou:
                    rb.linearVelocity = new Vector2(paraOCampo * 8f, 10f);
                    break;
                case ReacaoGoleiro.Segurou:
                    Estado.Chamar();
                    ComecarVolta();
                    break;
                case ReacaoGoleiro.Atropelado:
                    // a bola de fogo segue para o gol
                    break;
            }
        }

        /// <summary>A bola entrou no gol (chamado pela rede).</summary>
        public bool RegistrarGol()
        {
            if (!Voando || golMarcado)
                return false;
            golMarcado = true;
            rb.linearVelocity = rb.linearVelocity * 0.2f;
            return true;
        }
    }
}
