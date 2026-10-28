using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Capitão adversário (GDD, I07): minichefe do ato 2. Alterna carrinho e
    /// salto em cima do jogador; só pode ser atingido enquanto se recupera
    /// (ou distraído pelo companheiro). Superado, o portão do caminho abre.
    /// As decisões ficam em <see cref="CerebroCapitao"/>.
    /// </summary>
    public class Capitao : Inimigo
    {
        const float Gravidade = 30f;
        const float DistanciaDespertar = 10f;

        readonly CerebroCapitao cerebro = new CerebroCapitao();
        EstadoCapitao anterior = EstadoCapitao.Aguardando;
        float baseY;
        float vy;
        float vx;
        bool noAr;
        float tempoPoeira;

        public CerebroCapitao Cerebro
        {
            get { return cerebro; }
        }

        protected override string Prefixo
        {
            get { return "capitao"; }
        }

        protected override Vector2 TamanhoColisor
        {
            get { return new Vector2(0.9f, 1.75f); }
        }

        protected override bool PodeMachucar
        {
            get { return cerebro.PodeMachucar; }
        }

        protected override void Awake()
        {
            base.Awake();
            baseY = posicaoInicial.y;
            sr.sortingOrder = Ordem.Inimigos + 1;
            anim.Tocar("capitao_andar", 1f);
        }

        protected override void Mover(float dt)
        {
            if (cerebro.Estado == EstadoCapitao.Aguardando)
            {
                OlharParaJogador();
                anim.Tocar("capitao_distraido", 1f);
                if (JogadorPerto(DistanciaDespertar, 3f) != null)
                {
                    cerebro.Despertar();
                    if (GerenciadorFase.Instancia != null)
                        GerenciadorFase.Instancia.CapitaoDespertou(this);
                }
                return;
            }

            cerebro.Atualizar(dt);
            EstadoCapitao e = cerebro.Estado;
            bool mudou = e != anterior;
            anterior = e;
            Vector2 pos = rb.position;
            switch (e)
            {
                case EstadoCapitao.Avancando:
                    OlharParaJogador();
                    Avancar(CerebroCapitao.VelocidadeAvanco, dt);
                    anim.Tocar("capitao_andar", 7f);
                    break;
                case EstadoCapitao.PreparandoCarrinho:
                case EstadoCapitao.PreparandoSalto:
                    OlharParaJogador();
                    anim.Tocar("capitao_preparar", 1f);
                    if (mudou)
                        MostrarAviso("exclamacao", e == EstadoCapitao.PreparandoCarrinho ?
                            CerebroCapitao.TempoPreparoCarrinho : CerebroCapitao.TempoPreparoSalto);
                    break;
                case EstadoCapitao.Carrinho:
                    if (mudou)
                        SomJogo.Tocar(Som.Pedalada);
                    Avancar(CerebroCapitao.VelocidadeCarrinho, dt);
                    anim.Tocar("capitao_deslizar", 10f);
                    tempoPoeira -= dt;
                    if (tempoPoeira <= 0f)
                    {
                        tempoPoeira = 0.07f;
                        Efeitos.Animacao("poeira", transform.position + new Vector3(-direcao * 0.5f, 0.15f, 0f), 18f, 0.7f);
                    }
                    break;
                case EstadoCapitao.Saltando:
                    if (!noAr)
                    {
                        Jogador j = Jogador.Atual;
                        float alvo = j != null ? j.transform.position.x : pos.x;
                        float tempoVoo = 2f * CerebroCapitao.ImpulsoSalto / Gravidade;
                        vx = Mathf.Clamp((alvo - pos.x) / tempoVoo, -7f, 7f);
                        vy = CerebroCapitao.ImpulsoSalto;
                        noAr = true;
                        SomJogo.Tocar(Som.Pulo, 0.8f);
                    }
                    vy -= Gravidade * dt;
                    pos.y += vy * dt;
                    pos.x = Mathf.Clamp(pos.x + vx * dt, limiteEsquerda, limiteDireita);
                    anim.Tocar(vy > 0f ? "capitao_salto_0" : "capitao_salto_1", 1f);
                    if (pos.y <= baseY && vy < 0f)
                    {
                        pos.y = baseY;
                        noAr = false;
                        vy = 0f;
                        vx = 0f;
                        cerebro.Aterrissou();
                        CameraSeguir.Tremer(0.2f, 0.2f);
                        SomJogo.Tocar(Som.Defesa);
                        Efeitos.Animacao("poeira", new Vector3(pos.x - 0.5f, pos.y + 0.2f, 0f), 16f, 1f);
                        Efeitos.Animacao("poeira", new Vector3(pos.x + 0.5f, pos.y + 0.2f, 0f), 16f, 1f, false, Ordem.Efeitos, true);
                    }
                    rb.MovePosition(pos);
                    break;
                case EstadoCapitao.RecuperandoCarrinho:
                    anim.Tocar("capitao_sentado", 1f);
                    break;
                case EstadoCapitao.RecuperandoSalto:
                    anim.Tocar("capitao_tonto", 4f);
                    break;
                case EstadoCapitao.Distraido:
                    anim.Tocar("capitao_distraido", 1f);
                    break;
            }
            Tonto(cerebro.Vulneravel && e != EstadoCapitao.Distraido);
            AjustarColisor(cerebro.Rasteiro);
        }

        public override ResultadoGolpe ReceberBola(Bola bola)
        {
            bool atravessaTudo = bola.Parametros.Atravessa >= Chutes.AtravessaTudo;
            ResultadoGolpe r = cerebro.ReceberChute(bola.Parametros.Dano, atravessaTudo);
            if (r == ResultadoGolpe.Defendeu)
            {
                Efeitos.TextoSubindo(Textos.Defendeu, (Vector3)Centro + Vector3.up * 1.5f, FabricaUI.CorVermelha);
                return r;
            }
            Piscar();
            if (r == ResultadoGolpe.Superou)
                SairDeCampo(new Vector2(Mathf.Sign(bola.Velocidade.x == 0f ? -direcao : bola.Velocidade.x), 0f));
            return r;
        }

        public override void Distrair(float duracao)
        {
            base.Distrair(duracao);
            cerebro.Distrair(duracao);
        }

        public override void Reiniciar()
        {
            // o capitão não volta ao lugar: a luta continua de onde parou
        }
    }
}
