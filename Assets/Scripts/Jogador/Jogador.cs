using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// O Camisa 10: movimento (andar, correr, pulo com "coyote time" e buffer),
    /// chutes (3 níveis de carga, cavadinha, bicicleta no ar, superchute depois
    /// da pedalada), embaixadinhas, chamada do companheiro, poderes e dano.
    /// Os números do movimento ficam em <see cref="ParametrosJogador"/>.
    /// </summary>
    public class Jogador : MonoBehaviour
    {
        public static Jogador Atual { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Atual = null;
        }

        const float TempoAnimChute = 0.3f;
        const float TempoAnimBicicleta = 0.45f;
        const float DuracaoPedalada = 0.36f;
        const float RecargaPedalada = 0.5f;
        const float VelocidadePedalada = 7f;
        /// <summary>Duração de um ciclo da embaixadinha (a bola sobe e volta ao pé).</summary>
        public const float CicloEmbaixadinha = 0.45f;
        const float ForcaQuique = 13f;
        const float TempoInvulneravel = 1.5f;

        Rigidbody2D rb;
        BoxCollider2D col;
        SpriteRenderer sr;
        SpriteRenderer aura;
        AnimadorSprite anim;
        Transform visual;
        readonly ControleChute controle = new ControleChute();
        readonly JanelaSuperchute janela = new JanelaSuperchute();

        float entradaH;
        bool correndo;
        float timerCoyote;
        float timerBuffer;
        bool noChao;
        bool cortouPulo;
        bool olhandoDireita = true;
        float timerInvulneravel;
        float timerSemControle;
        float timerAnimChute;
        float timerPedalada;
        float recargaPedalada;
        float timerDescer;
        float timerChamar;
        int ultimoNivelCarga;
        Collider2D plataformaIgnorada;
        Barco barco;
        bool comemorando;
        float yAntes;
        bool fazendoEmbaixadinha;
        float tempoEmbaixadinha;
        int toquesEmbaixadinha;
        float canhaoRestante;
        bool especialArmado;

        public Bola Bola { get; private set; }
        public bool Vivo { get; private set; }

        public bool NoChao
        {
            get { return noChao; }
        }

        public float VelocidadeY
        {
            get { return rb != null ? rb.linearVelocity.y : 0f; }
        }

        public float VelocidadeX
        {
            get { return rb != null ? rb.linearVelocity.x : 0f; }
        }

        /// <summary>1 olhando para a direita, -1 para a esquerda.</summary>
        public int Direcao
        {
            get { return olhandoDireita ? 1 : -1; }
        }

        public bool Invulneravel
        {
            get { return timerInvulneravel > 0f; }
        }

        public bool Comemorando
        {
            get { return comemorando; }
        }

        /// <summary>0 a 1: barra de carga do chute (para o HUD).</summary>
        public float CargaChute
        {
            get { return controle.Carga01; }
        }

        /// <summary>1, 2 ou 3 enquanto o X está segurado; 0 se não.</summary>
        public int NivelCarga
        {
            get { return controle.Nivel; }
        }

        public bool SuperchutePronto
        {
            get { return janela.Ativa; }
        }

        /// <summary>Segundos restantes do Chute do Canhão (0 = desligado).</summary>
        public float CanhaoRestante
        {
            get { return Mathf.Max(0f, canhaoRestante); }
        }

        public bool EspecialArmado
        {
            get { return especialArmado; }
        }

        public bool FazendoEmbaixadinha
        {
            get { return fazendoEmbaixadinha; }
        }

        /// <summary>Tempo desde o começo da embaixadinha (a bola usa para acompanhar o pé).</summary>
        public float TempoEmbaixadinha
        {
            get { return tempoEmbaixadinha; }
        }

        /// <summary>Onde a bola fica quando está dominada.</summary>
        public Vector2 PosicaoPe
        {
            get
            {
                return (Vector2)transform.position +
                       new Vector2(Direcao * ParametrosJogador.DistanciaBolaNoPe, ParametrosJogador.AlturaBolaNoPe);
            }
        }

        /// <summary>Centro do corpo (para efeitos e textos).</summary>
        public Vector3 Centro
        {
            get { return transform.position + Vector3.up * (ParametrosJogador.AlturaColisor * 0.5f); }
        }

        public static Jogador Criar(Vector3 posicao)
        {
            var go = new GameObject("Camisa 10 (Jogador)");
            go.layer = Camadas.Jogador;
            go.transform.position = posicao;
            Jogador j = go.AddComponent<Jogador>();
            j.Bola = Bola.Criar(j);
            return j;
        }

        void Awake()
        {
            Atual = this;
            Vivo = true;

            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = ParametrosJogador.EscalaGravidade;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

            col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(ParametrosJogador.LarguraColisor, ParametrosJogador.AlturaColisor);
            col.offset = new Vector2(0f, ParametrosJogador.AlturaColisor * 0.5f + 0.01f);
            col.edgeRadius = 0.02f;
            var semAtrito = new PhysicsMaterial2D("SemAtrito");
            semAtrito.friction = 0f;
            semAtrito.bounciness = 0f;
            col.sharedMaterial = semAtrito;

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);
            aura = Efeitos.NovoSprite("Aura", visual, Arte.Sprite("luz_suave"), Ordem.Jogador - 1, true);
            aura.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            aura.transform.localScale = Vector3.one * 1.6f;
            aura.enabled = false;
            sr = visual.gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = Ordem.Jogador;
            Materiais2D.Aplicar(sr, false);
            anim = visual.gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
            anim.Tocar("camisa10_parado", 5f);
        }

        void OnDestroy()
        {
            if (Atual == this)
                Atual = null;
            if (Bola != null)
                Destroy(Bola.gameObject);
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;
            float dt = Time.deltaTime;

            bool controlavel = Vivo && timerSemControle <= 0f && !comemorando;
            entradaH = controlavel && timerPedalada <= 0f ? Entrada.Horizontal : 0f;
            correndo = controlavel && Entrada.CorrerSegurado;

            if (controlavel && Entrada.PuloApertado)
                timerBuffer = ParametrosJogador.TempoBuffer;
            if (controlavel && Entrada.BaixoApertado && noChao)
                TentarDescerPlataforma();

            if (controlavel)
                LerAcoes(dt);
            else
            {
                controle.Cancelar();
                PararEmbaixadinha();
            }

            timerBuffer -= dt;
            timerInvulneravel -= dt;
            timerSemControle -= dt;
            timerAnimChute -= dt;
            timerPedalada -= dt;
            recargaPedalada -= dt;
            timerChamar -= dt;
            janela.Atualizar(dt);
            if (canhaoRestante > 0f)
                canhaoRestante -= dt;

            if (timerDescer > 0f)
            {
                timerDescer -= dt;
                if (timerDescer <= 0f && plataformaIgnorada != null)
                {
                    Physics2D.IgnoreCollision(col, plataformaIgnorada, false);
                    plataformaIgnorada = null;
                }
            }

            AtualizarEmbaixadinha(dt);

            // pisca enquanto está invulnerável
            sr.enabled = !(timerInvulneravel > 0f && Vivo && Mathf.Repeat(Time.time * 14f, 1f) < 0.35f);

            // brilho dourado: superchute pronto, Chute do Canhão ativo ou bola de fogo armada
            bool brilhar = janela.Ativa || canhaoRestante > 0f || especialArmado;
            aura.enabled = brilhar && Vivo;
            if (brilhar)
            {
                Color c = especialArmado ? new Color(1f, 0.45f, 0.15f) : (canhaoRestante > 0f ? new Color(1f, 0.7f, 0.25f) : new Color(1f, 0.92f, 0.45f));
                c.a = 0.45f + Mathf.Sin(Time.time * 12f) * 0.15f;
                aura.color = c;
            }

            // volta a escala do "squash & stretch" (treme com a carga máxima)
            Vector3 alvoEscala = Vector3.one;
            if (controle.Nivel >= 3)
                alvoEscala = new Vector3(1f + Mathf.Sin(Time.time * 60f) * 0.04f, 1f, 1f);
            visual.localScale = Vector3.Lerp(visual.localScale, alvoEscala, 1f - Mathf.Exp(-14f * dt));

            if (entradaH > 0.01f) olhandoDireita = true;
            else if (entradaH < -0.01f) olhandoDireita = false;
            sr.flipX = !olhandoDireita;

            Animar();

            GerenciadorFase g = GerenciadorFase.Instancia;
            if (Vivo && g != null && transform.position.y < g.LimiteQueda)
                g.JogadorCaiuNaAgua();
        }

        // ------------------------------------------------------------ ações
        void LerAcoes(float dt)
        {
            if (Bola == null)
                return;
            GerenciadorFase g = GerenciadorFase.Instancia;

            if (Entrada.DribleApertado)
                TentarPedalada();

            if (Entrada.CavadinhaApertada)
            {
                if (Bola.NoPe)
                {
                    controle.Cancelar();
                    Chutar(TipoChute.Cavadinha);
                }
                else
                {
                    Bola.Chamar();
                }
            }

            if (Entrada.ChuteApertado)
            {
                if (!Bola.NoPe)
                    Bola.Chamar(); // X com a bola longe faz ela voltar
                else if (!noChao)
                    Chutar(TipoChute.Bicicleta);
                else if (janela.Consumir())
                    Chutar(TipoChute.Superchute);
                else
                    controle.Apertar();
            }

            if (controle.Segurando)
            {
                if (!Bola.NoPe)
                {
                    controle.Cancelar();
                }
                else
                {
                    controle.Atualizar(dt);
                    int nivel = controle.Nivel;
                    if (nivel != ultimoNivelCarga)
                    {
                        ultimoNivelCarga = nivel;
                        if (nivel == 2)
                            SomJogo.Tocar(Som.CargaNivel, 0.6f);
                        if (nivel == 3)
                        {
                            SomJogo.Tocar(Som.CargaCheia, 0.8f);
                            Efeitos.Particulas("brilho", (Vector3)PosicaoPe, 6, new Color(1f, 0.85f, 0.3f), 1.5f, 3f, 0.5f, 0f, true);
                        }
                    }
                    if (!Entrada.ChuteSegurado)
                        Chutar(controle.Soltar());
                }
            }
            else
            {
                ultimoNivelCarga = 0;
            }

            if (Entrada.CompanheiroApertado && g != null && g.ChamarCompanheiro())
                timerChamar = 0.4f;
            if (Entrada.PoderApertado && g != null)
                g.UsarPoder();

            bool querEmbaixadinha = Entrada.BaixoSegurado && noChao && Bola.NoPe && Mathf.Abs(entradaH) < 0.1f &&
                                    !controle.Segurando && timerAnimChute <= 0f && timerPedalada <= 0f &&
                                    plataformaIgnorada == null;
            if (querEmbaixadinha)
            {
                if (!fazendoEmbaixadinha)
                {
                    fazendoEmbaixadinha = true;
                    tempoEmbaixadinha = 0f;
                    toquesEmbaixadinha = 0;
                }
            }
            else
            {
                PararEmbaixadinha();
            }
        }

        void AtualizarEmbaixadinha(float dt)
        {
            if (!fazendoEmbaixadinha)
                return;
            tempoEmbaixadinha += dt;
            int toques = Mathf.FloorToInt(tempoEmbaixadinha / CicloEmbaixadinha);
            while (toquesEmbaixadinha < toques)
            {
                toquesEmbaixadinha++;
                SomJogo.Tocar(Som.Quique, 0.4f);
                if (GerenciadorFase.Instancia != null)
                    GerenciadorFase.Instancia.Embaixadinha(toquesEmbaixadinha, (Vector3)PosicaoPe + Vector3.up * 1.6f);
            }
        }

        void PararEmbaixadinha()
        {
            if (!fazendoEmbaixadinha)
                return;
            fazendoEmbaixadinha = false;
            toquesEmbaixadinha = 0;
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.FimDasEmbaixadinhas();
        }

        /// <summary>Modificador do próximo chute: bola de fogo armada ou Chute do Canhão ativo.</summary>
        public ModificadorChute ModificadorAtual
        {
            get
            {
                if (especialArmado)
                    return ModificadorChute.Especial;
                if (canhaoRestante > 0f)
                    return ModificadorChute.Canhao;
                return ModificadorChute.Nenhum;
            }
        }

        /// <summary>Chuta a bola (se ela estiver no pé). Público para os testes.</summary>
        public bool Chutar(TipoChute tipo)
        {
            if (Bola == null || !Bola.NoPe || !Vivo)
                return false;
            ModificadorChute mod = ModificadorAtual;
            if (!Bola.Chutar(tipo, mod, Direcao, rb.linearVelocity.x))
                return false;
            if (mod == ModificadorChute.Especial && Chutes.AceitaModificador(tipo))
                especialArmado = false;
            PararEmbaixadinha();
            controle.Cancelar();
            switch (tipo)
            {
                case TipoChute.Cavadinha:
                    timerAnimChute = TempoAnimChute;
                    anim.Tocar("camisa10_cavadinha", 2f / TempoAnimChute, false, true);
                    break;
                case TipoChute.Bicicleta:
                    timerAnimChute = TempoAnimBicicleta;
                    anim.Tocar("camisa10_bicicleta", 4f / TempoAnimBicicleta, false, true);
                    // a bicicleta segura o jogador no ar por um instante
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.4f, Mathf.Max(rb.linearVelocity.y, 4f));
                    break;
                default:
                    timerAnimChute = TempoAnimChute;
                    anim.Tocar("camisa10_chute", 3f / TempoAnimChute, false, true);
                    break;
            }
            if (tipo == TipoChute.Forte || tipo == TipoChute.Superchute || mod != ModificadorChute.Nenhum)
            {
                visual.localScale = new Vector3(1.25f, 0.85f, 1f);
                CameraSeguir.Tremer(0.15f, 0.2f);
                AnelExpansivo.Criar((Vector3)PosicaoPe, 1.6f, new Color(1f, 0.85f, 0.3f, 0.9f));
            }
            Efeitos.Animacao("poeira", transform.position + new Vector3(Direcao * 0.3f, 0.2f, 0f), 18f, 0.7f);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.ChuteFeito(tipo, mod);
            return true;
        }

        /// <summary>Z: pedalada. Dá um passinho com a bola e abre a janela do superchute.</summary>
        public bool TentarPedalada()
        {
            if (!noChao || Bola == null || !Bola.NoPe || timerPedalada > 0f || recargaPedalada > 0f ||
                controle.Segurando || !Vivo)
                return false;
            PararEmbaixadinha();
            timerPedalada = DuracaoPedalada;
            recargaPedalada = RecargaPedalada;
            janela.Abrir();
            anim.Tocar("camisa10_pedalada", 4f / DuracaoPedalada, false, true);
            SomJogo.Tocar(Som.Pedalada);
            rb.linearVelocity = new Vector2(Direcao * VelocidadePedalada, rb.linearVelocity.y);
            Efeitos.Animacao("poeira", transform.position + new Vector3(-Direcao * 0.3f, 0.2f, 0f), 18f, 0.6f);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.Pedalada();
            return true;
        }

        /// <summary>Liga o Chute do Canhão (carta P01).</summary>
        public void AtivarCanhao(float duracao)
        {
            canhaoRestante = duracao;
        }

        /// <summary>Arma a bola de fogo da torcida para o próximo chute.</summary>
        public void ArmarEspecial()
        {
            especialArmado = true;
        }

        // ------------------------------------------------------------ física
        void FixedUpdate()
        {
            bool estavaNoChao = noChao;
            Vector2 pos = rb.position;
            Collider2D chao = null;
            if (rb.linearVelocity.y <= 0.1f)
                chao = Physics2D.OverlapBox(pos + new Vector2(0f, -0.03f), new Vector2(0.56f, 0.08f), 0f, Camadas.MascaraPisavel);
            if (chao != null && chao == plataformaIgnorada)
                chao = null;
            noChao = chao != null;
            barco = noChao ? chao.GetComponentInParent<Barco>() : null;

            if (noChao)
            {
                timerCoyote = ParametrosJogador.TempoCoyote;
                cortouPulo = false;
                if (!estavaNoChao && yAntes < -6f)
                {
                    visual.localScale = new Vector3(1.25f, 0.78f, 1f);
                    Efeitos.Animacao("poeira", transform.position + new Vector3(0f, 0.3f, 0f), 16f);
                }
            }
            else
            {
                timerCoyote -= Time.fixedDeltaTime;
            }

            Vector2 v = rb.linearVelocity;
            float velocidade = correndo ? ParametrosJogador.VelocidadeCorrendo : ParametrosJogador.VelocidadeAndando;
            if (controle.Carregando)
                velocidade *= ParametrosJogador.FatorCarregando;
            if (fazendoEmbaixadinha)
                velocidade = 0f;
            float alvo = entradaH * velocidade + VelocidadeBarco();
            float acel = noChao ? ParametrosJogador.AceleracaoChao : ParametrosJogador.AceleracaoAr;
            if (timerSemControle <= 0f && timerPedalada <= 0f)
                v.x = Mathf.MoveTowards(v.x, alvo, acel * Time.fixedDeltaTime);
            else if (timerPedalada > 0f)
                v.x = Mathf.MoveTowards(v.x, VelocidadeBarco(), 18f * Time.fixedDeltaTime);

            if (timerBuffer > 0f && timerCoyote > 0f && Vivo && !fazendoEmbaixadinha)
            {
                v.y = ParametrosJogador.ForcaPulo;
                timerBuffer = 0f;
                timerCoyote = 0f;
                noChao = false;
                visual.localScale = new Vector3(0.8f, 1.2f, 1f);
                SomJogo.Tocar(Som.Pulo, 0.7f);
                Efeitos.Animacao("poeira", transform.position + new Vector3(0f, 0.3f, 0f), 18f, 0.8f);
            }

            // pulo variável: soltar o botão cedo faz um pulo mais baixo
            if (!cortouPulo && v.y > 0f && !Entrada.PuloSegurado && timerSemControle <= 0f)
            {
                v.y *= 0.5f;
                cortouPulo = true;
            }

            float g = ParametrosJogador.EscalaGravidade;
            rb.gravityScale = v.y < 0f ? g * ParametrosJogador.MultiplicadorQueda : g;
            if (v.y < -ParametrosJogador.QuedaMaxima)
                v.y = -ParametrosJogador.QuedaMaxima;
            yAntes = v.y;
            rb.linearVelocity = v;
        }

        float VelocidadeBarco()
        {
            return barco != null ? barco.Velocidade.x : 0f;
        }

        bool TentarDescerPlataforma()
        {
            Collider2D c = Physics2D.OverlapBox(rb.position + new Vector2(0f, -0.03f), new Vector2(0.56f, 0.08f), 0f,
                1 << Camadas.Plataforma);
            if (c == null || c.GetComponent<PlatformEffector2D>() == null)
                return false;
            PararEmbaixadinha();
            Physics2D.IgnoreCollision(col, c, true);
            plataformaIgnorada = c;
            timerDescer = 0.35f;
            return true;
        }

        // ------------------------------------------------------------ animação
        void Animar()
        {
            if (!Vivo || timerSemControle > 0f)
            {
                anim.Tocar("camisa10_dano", 1f);
                return;
            }
            if (comemorando)
            {
                anim.Tocar("camisa10_vitoria", 4f);
                return;
            }
            if (timerAnimChute > 0f || timerPedalada > 0f)
                return; // chute e pedalada tocam até o fim
            if (timerChamar > 0f)
            {
                anim.Tocar("camisa10_chamar", 1f);
                return;
            }
            if (controle.Carregando)
            {
                anim.Tocar("camisa10_carregar", controle.Nivel >= 3 ? 16f : 8f);
                return;
            }
            if (fazendoEmbaixadinha)
            {
                // 4 quadros por ciclo: o pé sobe no quadro 0, quando a bola volta
                anim.Tocar("camisa10_embaixadinha", 4f / CicloEmbaixadinha, true, anim.Atual != "camisa10_embaixadinha");
                return;
            }
            if (!noChao)
            {
                anim.Tocar(rb.linearVelocity.y > 0f ? "camisa10_pulo" : "camisa10_queda", 8f);
                return;
            }
            float vx = Mathf.Abs(rb.linearVelocity.x - VelocidadeBarco());
            if (vx > 0.6f)
            {
                anim.Tocar("camisa10_correr", correndo ? 15f : 11f);
                return;
            }
            anim.Tocar("camisa10_parado", 5f);
        }

        // ------------------------------------------------------------ dano e vida
        /// <summary>Quica depois de cair em cima de um adversário (sem dano para ninguém).</summary>
        public void Quicar()
        {
            Vector2 v = rb.linearVelocity;
            v.y = Entrada.PuloSegurado ? ForcaQuique * 1.15f : ForcaQuique;
            rb.linearVelocity = v;
            cortouPulo = !Entrada.PuloSegurado;
            timerCoyote = 0f;
            visual.localScale = new Vector3(0.8f, 1.2f, 1f);
            SomJogo.Tocar(Som.Pulo, 0.5f);
        }

        /// <summary>Golpe de um adversário. O gerenciador decide se tira coração (ou se a caneleira segura).</summary>
        public void TomarDano(Vector2 origem)
        {
            if (!Vivo || timerInvulneravel > 0f || comemorando)
                return;
            GerenciadorFase g = GerenciadorFase.Instancia;
            ResultadoDano r = g != null ? g.JogadorFerido() : ResultadoDano.PerdeuCoracao;
            if (r == ResultadoDano.Ignorado || r == ResultadoDano.FimDeJogo || r == ResultadoDano.PerdeuVida)
                return; // o gerenciador cuida do resto (morte ou volta ao checkpoint)
            controle.Cancelar();
            PararEmbaixadinha();
            float lado = transform.position.x < origem.x ? -1f : 1f;
            if (r == ResultadoDano.Bloqueado)
            {
                timerInvulneravel = 1f;
                rb.linearVelocity = new Vector2(lado * 4f, 6f);
                SomJogo.Tocar(Som.Escudo);
                AnelExpansivo.Criar(Centro, 1.4f, new Color(1f, 0.85f, 0.3f, 0.9f));
                return;
            }
            SomJogo.Tocar(Som.Dano);
            timerInvulneravel = TempoInvulneravel;
            timerSemControle = 0.35f;
            rb.linearVelocity = new Vector2(lado * 6f, 9f);
            CameraSeguir.Tremer(0.25f, 0.25f);
            Efeitos.Particulas("brilho", Centro, 6, FabricaUI.CorVermelha, 2f, 4f, 0.4f, 4f, true);
        }

        /// <summary>Chamado quando as vidas acabam.</summary>
        public void Morrer()
        {
            Vivo = false;
            controle.Cancelar();
            PararEmbaixadinha();
            sr.enabled = true;
            col.enabled = false;
            rb.linearVelocity = new Vector2(0f, 12f);
            rb.gravityScale = ParametrosJogador.EscalaGravidade;
            sr.sortingOrder = Ordem.TextoFlutuante;
            SomJogo.Tocar(Som.Queda);
        }

        /// <summary>Volta para o último checkpoint (perdeu uma vida ou caiu no canal).</summary>
        public void Renascer(Vector3 posicao)
        {
            transform.position = posicao;
            rb.position = posicao;
            rb.linearVelocity = Vector2.zero;
            timerInvulneravel = TempoInvulneravel;
            timerSemControle = 0f;
            timerPedalada = 0f;
            janela.Fechar();
            controle.Cancelar();
            PararEmbaixadinha();
            olhandoDireita = true;
            if (Bola != null)
                Bola.VoltarParaOPe();
            Efeitos.Animacao("brilho", posicao + Vector3.up * 0.8f, 10f, 2f, true);
            CameraSeguir.Teleportar();
        }

        public void Comemorar()
        {
            comemorando = true;
            timerInvulneravel = 0f;
            controle.Cancelar();
            PararEmbaixadinha();
        }
    }
}
