using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Zagueiro carrinho (GDD, I01): patrulha; quando vê o Camisa 10 na mesma
    /// altura, avisa ("!") e dá um carrinho rasteiro. Resposta: pular por cima
    /// ou chutar antes do contato. Depois do carrinho fica sentado no chão.
    /// As decisões ficam em <see cref="CerebroCarrinho"/>.
    /// </summary>
    public class ZagueiroCarrinho : Inimigo
    {
        /// <summary>Até onde ele patrulha a partir de onde nasce.</summary>
        public const float RaioPatrulha = 3.5f;

        readonly CerebroCarrinho cerebro = new CerebroCarrinho();
        EstadoCarrinho anterior = EstadoCarrinho.Patrulha;
        float tempoPoeira;

        public CerebroCarrinho Cerebro
        {
            get { return cerebro; }
        }

        protected override string Prefixo
        {
            get { return "carrinho"; }
        }

        protected override bool PodeMachucar
        {
            get { return cerebro.PodeMachucar; }
        }

        protected override void Awake()
        {
            base.Awake();
            limiteEsquerda = posicaoInicial.x - RaioPatrulha;
            limiteDireita = posicaoInicial.x + RaioPatrulha;
        }

        protected override void Mover(float dt)
        {
            cerebro.Atualizar(dt, VendoJogador());
            EstadoCarrinho e = cerebro.Estado;
            bool mudou = e != anterior;
            anterior = e;
            switch (e)
            {
                case EstadoCarrinho.Patrulha:
                    Patrulhar(CerebroCarrinho.VelocidadePatrulha, dt);
                    anim.Tocar("carrinho_andar", 7f);
                    break;
                case EstadoCarrinho.Preparando:
                    OlharParaJogador();
                    anim.Tocar("carrinho_preparar", 1f);
                    if (mudou)
                        MostrarAviso("exclamacao", CerebroCarrinho.TempoPreparo);
                    break;
                case EstadoCarrinho.Deslizando:
                    if (mudou)
                        SomJogo.Tocar(Som.Pedalada, 0.8f);
                    Avancar(CerebroCarrinho.VelocidadeDeslize, dt);
                    anim.Tocar("carrinho_deslizar", 10f);
                    tempoPoeira -= dt;
                    if (tempoPoeira <= 0f)
                    {
                        tempoPoeira = 0.08f;
                        Efeitos.Animacao("poeira", transform.position + new Vector3(-direcao * 0.4f, 0.15f, 0f), 18f, 0.6f);
                    }
                    break;
                case EstadoCarrinho.Recuperando:
                    anim.Tocar("carrinho_sentado", 1f);
                    break;
                case EstadoCarrinho.Atingido:
                    anim.Tocar("carrinho_atingido", 1f);
                    break;
                case EstadoCarrinho.Distraido:
                    anim.Tocar("carrinho_distraido", 1f);
                    break;
            }
            AjustarColisor(cerebro.Rasteiro);
        }

        bool VendoJogador()
        {
            Jogador j = JogadorPerto(CerebroCarrinho.AlcanceVisao, 1.2f);
            if (j == null)
                return false;
            float dx = j.transform.position.x - rb.position.x;
            // só "vê" quem está na frente (ou bem perto)
            return Mathf.Sign(dx) == direcao || Mathf.Abs(dx) < 1.5f;
        }

        public override ResultadoGolpe ReceberBola(Bola bola)
        {
            ResultadoGolpe r = cerebro.ReceberChute(bola.Parametros.Dano);
            Piscar();
            float lado = Mathf.Sign(bola.Velocidade.x == 0f ? -direcao : bola.Velocidade.x);
            if (r == ResultadoGolpe.Superou)
                SairDeCampo(new Vector2(lado, 0f));
            else if (PodeAndar(0.3f))
                rb.MovePosition(rb.position + new Vector2(lado * 0.3f, 0f));
            return r;
        }

        public override void Distrair(float duracao)
        {
            base.Distrair(duracao);
            cerebro.Distrair(duracao);
        }

        public override void Reiniciar()
        {
            base.Reiniciar();
            cerebro.VoltarAPatrulha();
        }
    }
}
