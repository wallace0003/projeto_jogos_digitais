using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Zagueiro saltador (GDD, I04): "intercepta bolas altas. Resposta: chute
    /// rasteiro ou ataque durante a aterrissagem". Quando vê uma bola alta
    /// vindo (cavadinha), pula e cabeceia para longe. Bola rasteira com ele no
    /// chão acerta; logo depois de aterrissar ele leva o dobro de dano.
    /// </summary>
    public class ZagueiroSaltador : Inimigo
    {
        public const int VidaMaxima = 2;
        const float Gravidade = 30f;
        const float ImpulsoBloqueio = 12.5f;
        const float ImpulsoPulinho = 7f;
        const float VelocidadePulinho = 2.2f;
        const float IntervaloPulinho = 2.2f;
        public const float TempoAterrissagem = 0.45f;
        /// <summary>Acima disso (em relação ao pé dele) a bola é "alta".</summary>
        const float AlturaBolaAlta = 1.15f;
        const float RaioMovimento = 4f;

        int vida = VidaMaxima;
        float baseY;
        float vy;
        float vx;
        bool noAr;
        float timerPulinho = 1f;
        float timerAterrissagem;
        float distraido;

        public bool NoAr
        {
            get { return noAr; }
        }

        public int Vida
        {
            get { return vida; }
        }

        protected override string Prefixo
        {
            get { return "saltador"; }
        }

        protected override string AnimInicial
        {
            get { return "saltador_parado"; }
        }

        protected override bool PodeMachucar
        {
            get { return distraido <= 0f; }
        }

        protected override void Awake()
        {
            base.Awake();
            baseY = posicaoInicial.y;
            limiteEsquerda = posicaoInicial.x - RaioMovimento;
            limiteDireita = posicaoInicial.x + RaioMovimento;
        }

        protected override void Mover(float dt)
        {
            if (distraido > 0f)
            {
                distraido -= dt;
                if (!noAr)
                {
                    anim.Tocar("saltador_distraido", 1f);
                    return;
                }
            }
            Vector2 pos = rb.position;
            if (!noAr)
            {
                OlharParaJogador();
                timerAterrissagem -= dt;
                timerPulinho -= dt;
                if (distraido <= 0f && BolaAltaChegando())
                {
                    Pular(ImpulsoBloqueio, 0f);
                    return;
                }
                if (distraido <= 0f && timerPulinho <= 0f && JogadorPerto(7f, 2f) != null)
                {
                    timerPulinho = IntervaloPulinho;
                    Pular(ImpulsoPulinho, direcao * VelocidadePulinho);
                    return;
                }
                anim.Tocar(timerAterrissagem > 0f ? "saltador_aterrissar" : (timerPulinho < 0.3f ? "saltador_agachar" : "saltador_parado"), 3f);
                return;
            }

            vy -= Gravidade * dt;
            pos.y += vy * dt;
            float novoX = Mathf.Clamp(pos.x + vx * dt, limiteEsquerda, limiteDireita);
            pos.x = novoX;
            anim.Tocar("saltador_pulo", 1f);
            if (pos.y <= baseY && vy < 0f)
            {
                pos.y = baseY;
                vy = 0f;
                vx = 0f;
                noAr = false;
                timerAterrissagem = TempoAterrissagem;
                Efeitos.Animacao("poeira", new Vector3(pos.x, pos.y + 0.2f, 0f), 16f, 0.8f);
            }
            rb.MovePosition(pos);
        }

        void Pular(float impulso, float velocidadeX)
        {
            vy = impulso;
            vx = velocidadeX;
            noAr = true;
            anim.Tocar("saltador_pulo", 1f);
        }

        /// <summary>Uma bola alta vem na direção dele e chega logo: hora de pular para cabecear.</summary>
        bool BolaAltaChegando()
        {
            Jogador j = Jogador.Atual;
            if (j == null || j.Bola == null || !j.Bola.Voando)
                return false;
            Bola b = j.Bola;
            Vector2 pb = b.transform.position;
            Vector2 vb = b.Velocidade;
            float dx = rb.position.x - pb.x;
            if (Mathf.Abs(dx) > 6f || Mathf.Abs(vb.x) < 0.5f || Mathf.Sign(vb.x) != Mathf.Sign(dx))
                return false;
            float t = Mathf.Abs(dx) / Mathf.Abs(vb.x);
            if (t > 0.5f)
                return false;
            float g = ParametrosJogador.GravidadeMundo * b.EscalaGravidade;
            float yChegada = pb.y + vb.y * t - 0.5f * g * t * t;
            float alturaRelativa = yChegada - baseY;
            // alta o bastante para passar por cima dele, mas ao alcance do pulo
            return alturaRelativa > AlturaBolaAlta && alturaRelativa < AlturaBolaAlta + 3.4f;
        }

        public override ResultadoGolpe ReceberBola(Bola bola)
        {
            float alturaBola = bola.transform.position.y - baseY;
            if (noAr && alturaBola > AlturaBolaAlta * 0.8f && vy > -4f)
            {
                // cabeceou a bola alta para longe
                Efeitos.TextoSubindo(Textos.Cabecada, (Vector3)Centro + Vector3.up * 1.4f, FabricaUI.CorVermelha);
                return ResultadoGolpe.Defendeu;
            }
            int dano = bola.Parametros.Dano * (timerAterrissagem > 0f ? 2 : 1);
            vida = Mathf.Max(0, vida - Mathf.Max(1, dano));
            Piscar();
            if (vida <= 0)
            {
                SairDeCampo(new Vector2(Mathf.Sign(bola.Velocidade.x == 0f ? -direcao : bola.Velocidade.x), 0f));
                return ResultadoGolpe.Superou;
            }
            return ResultadoGolpe.Acertou;
        }

        public override void Distrair(float duracao)
        {
            base.Distrair(duracao);
            distraido = duracao;
        }

        public override void Reiniciar()
        {
            base.Reiniciar();
            noAr = false;
            vy = 0f;
            vx = 0f;
            distraido = 0f;
            timerPulinho = 1f;
        }
    }
}
