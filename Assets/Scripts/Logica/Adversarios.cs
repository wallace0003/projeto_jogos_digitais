using System;

namespace CincoEstrelas
{
    /// <summary>Resultado de uma bola num adversário.</summary>
    public enum ResultadoGolpe
    {
        /// <summary>Ele defendeu (a bola ricocheteia sem dano).</summary>
        Defendeu,
        /// <summary>Perdeu vida, mas segue em campo.</summary>
        Acertou,
        /// <summary>Foi superado (sai de campo).</summary>
        Superou,
    }

    // ==================================================================== I01
    public enum EstadoCarrinho
    {
        Patrulha,
        /// <summary>Viu o jogador: se agacha por um instante (aviso "!").</summary>
        Preparando,
        /// <summary>Carrinho rasteiro: golpe baixo e rápido (pule por cima!).</summary>
        Deslizando,
        /// <summary>Sentado no chão depois do carrinho: não machuca.</summary>
        Recuperando,
        /// <summary>Levou uma bolada e cambaleou.</summary>
        Atingido,
        /// <summary>Olhando o companheiro de equipe (não ataca).</summary>
        Distraido,
    }

    /// <summary>
    /// Zagueiro carrinho (GDD, I01): "ataque baixo e telegráfico. Resposta:
    /// pular, usar chapéu ou chutar antes do contato". Só decide o estado;
    /// o componente na cena faz o movimento.
    /// </summary>
    public class CerebroCarrinho
    {
        public const int VidaMaxima = 2;
        public const float AlcanceVisao = 6.5f;
        public const float VelocidadePatrulha = 1.6f;
        public const float VelocidadeDeslize = 9f;
        public const float TempoPreparo = 0.45f;
        public const float TempoDeslize = 0.5f;
        public const float TempoRecuperacao = 0.9f;
        public const float TempoAtingido = 0.35f;
        public const float Descanso = 0.7f;

        public EstadoCarrinho Estado { get; private set; }
        public int Vida { get; private set; }
        float timer;
        float descanso;

        public CerebroCarrinho()
        {
            Estado = EstadoCarrinho.Patrulha;
            Vida = VidaMaxima;
        }

        public bool Superado
        {
            get { return Vida <= 0; }
        }

        public bool PodeMachucar
        {
            get
            {
                return !Superado && (Estado == EstadoCarrinho.Patrulha || Estado == EstadoCarrinho.Preparando ||
                                     Estado == EstadoCarrinho.Deslizando);
            }
        }

        /// <summary>No carrinho e logo depois, ele está rente ao chão.</summary>
        public bool Rasteiro
        {
            get { return Estado == EstadoCarrinho.Deslizando || Estado == EstadoCarrinho.Recuperando; }
        }

        public bool Andando
        {
            get { return Estado == EstadoCarrinho.Patrulha; }
        }

        public void Atualizar(float dt, bool vendoJogador)
        {
            if (Superado)
                return;
            descanso -= dt;
            switch (Estado)
            {
                case EstadoCarrinho.Patrulha:
                    if (vendoJogador && descanso <= 0f)
                        Mudar(EstadoCarrinho.Preparando, TempoPreparo);
                    return;
                case EstadoCarrinho.Preparando:
                    if (Contar(dt))
                        Mudar(EstadoCarrinho.Deslizando, TempoDeslize);
                    return;
                case EstadoCarrinho.Deslizando:
                    if (Contar(dt))
                        Mudar(EstadoCarrinho.Recuperando, TempoRecuperacao);
                    return;
                default:
                    if (Contar(dt))
                    {
                        Mudar(EstadoCarrinho.Patrulha, 0f);
                        descanso = Descanso;
                    }
                    return;
            }
        }

        /// <summary>Bolada: tira vida. Antes do carrinho, ele cambaleia e desiste do ataque.</summary>
        public ResultadoGolpe ReceberChute(int dano)
        {
            if (Superado)
                return ResultadoGolpe.Superou;
            Vida = Math.Max(0, Vida - Math.Max(1, dano));
            if (Superado)
                return ResultadoGolpe.Superou;
            if (Estado == EstadoCarrinho.Patrulha || Estado == EstadoCarrinho.Preparando)
                Mudar(EstadoCarrinho.Atingido, TempoAtingido);
            return ResultadoGolpe.Acertou;
        }

        public void Distrair(float duracao)
        {
            if (Superado || Estado == EstadoCarrinho.Deslizando)
                return;
            Mudar(EstadoCarrinho.Distraido, duracao);
        }

        /// <summary>Volta a patrulhar (o jogador perdeu uma vida e voltou ao checkpoint). A vida dele não muda.</summary>
        public void VoltarAPatrulha()
        {
            if (Superado)
                return;
            Mudar(EstadoCarrinho.Patrulha, 0f);
            descanso = Descanso;
        }

        bool Contar(float dt)
        {
            timer -= dt;
            return timer <= 0f;
        }

        void Mudar(EstadoCarrinho novo, float duracao)
        {
            Estado = novo;
            timer = duracao;
        }
    }

    // ==================================================================== I07
    public enum EstadoCapitao
    {
        Aguardando,
        Avancando,
        PreparandoCarrinho,
        Carrinho,
        RecuperandoCarrinho,
        PreparandoSalto,
        Saltando,
        RecuperandoSalto,
        Distraido,
        Superado,
    }

    /// <summary>
    /// Capitão adversário (GDD, I07): minichefe que combina dois padrões
    /// (carrinho e salto). "Resposta: reconhecer o ciclo e atacar durante a
    /// recuperação." Fora da recuperação ele defende as boladas.
    /// </summary>
    public class CerebroCapitao
    {
        public const int VidaMaxima = 6;
        public const float TempoAvancar = 1.2f;
        public const float TempoPreparoCarrinho = 0.45f;
        public const float TempoCarrinho = 0.55f;
        public const float TempoRecuperacaoCarrinho = 1.2f;
        public const float TempoPreparoSalto = 0.4f;
        public const float TempoRecuperacaoSalto = 1.1f;
        public const float VelocidadeAvanco = 2.2f;
        public const float VelocidadeCarrinho = 10f;
        public const float ImpulsoSalto = 13f;

        public EstadoCapitao Estado { get; private set; }
        public int Vida { get; private set; }
        float timer;
        bool proximoEhSalto;

        public CerebroCapitao()
        {
            Estado = EstadoCapitao.Aguardando;
            Vida = VidaMaxima;
        }

        public bool Superado
        {
            get { return Estado == EstadoCapitao.Superado; }
        }

        /// <summary>Só na recuperação (ou distraído pelo companheiro) a bola entra.</summary>
        public bool Vulneravel
        {
            get
            {
                return Estado == EstadoCapitao.RecuperandoCarrinho || Estado == EstadoCapitao.RecuperandoSalto ||
                       Estado == EstadoCapitao.Distraido;
            }
        }

        public bool PodeMachucar
        {
            get
            {
                return Estado == EstadoCapitao.Avancando || Estado == EstadoCapitao.PreparandoCarrinho ||
                       Estado == EstadoCapitao.Carrinho || Estado == EstadoCapitao.Saltando;
            }
        }

        public bool Rasteiro
        {
            get { return Estado == EstadoCapitao.Carrinho || Estado == EstadoCapitao.RecuperandoCarrinho; }
        }

        /// <summary>O jogador entrou na área dele: a luta começa.</summary>
        public void Despertar()
        {
            if (Estado == EstadoCapitao.Aguardando)
                Mudar(EstadoCapitao.Avancando, TempoAvancar);
        }

        public void Atualizar(float dt)
        {
            switch (Estado)
            {
                case EstadoCapitao.Aguardando:
                case EstadoCapitao.Superado:
                case EstadoCapitao.Saltando:
                    return; // o salto termina quando ele toca o chão (Aterrissou)
                case EstadoCapitao.Avancando:
                    if (Contar(dt))
                    {
                        if (proximoEhSalto)
                            Mudar(EstadoCapitao.PreparandoSalto, TempoPreparoSalto);
                        else
                            Mudar(EstadoCapitao.PreparandoCarrinho, TempoPreparoCarrinho);
                    }
                    return;
                case EstadoCapitao.PreparandoCarrinho:
                    if (Contar(dt))
                        Mudar(EstadoCapitao.Carrinho, TempoCarrinho);
                    return;
                case EstadoCapitao.Carrinho:
                    if (Contar(dt))
                        Mudar(EstadoCapitao.RecuperandoCarrinho, TempoRecuperacaoCarrinho);
                    return;
                case EstadoCapitao.PreparandoSalto:
                    if (Contar(dt))
                        Mudar(EstadoCapitao.Saltando, 0f);
                    return;
                case EstadoCapitao.RecuperandoCarrinho:
                    if (Contar(dt))
                    {
                        proximoEhSalto = true;
                        Mudar(EstadoCapitao.Avancando, TempoAvancar);
                    }
                    return;
                case EstadoCapitao.RecuperandoSalto:
                case EstadoCapitao.Distraido:
                    if (Contar(dt))
                    {
                        proximoEhSalto = false;
                        Mudar(EstadoCapitao.Avancando, TempoAvancar);
                    }
                    return;
            }
        }

        /// <summary>Chamado quando ele toca o chão depois do salto.</summary>
        public void Aterrissou()
        {
            if (Estado == EstadoCapitao.Saltando)
                Mudar(EstadoCapitao.RecuperandoSalto, TempoRecuperacaoSalto);
        }

        /// <summary>
        /// Bolada no capitão. Fora da recuperação ele defende, a não ser que o
        /// chute atravesse tudo (Chute do Canhão ou especial da torcida).
        /// </summary>
        public ResultadoGolpe ReceberChute(int dano, bool atravessaTudo)
        {
            if (Superado)
                return ResultadoGolpe.Superou;
            if (Estado == EstadoCapitao.Aguardando)
                Despertar();
            if (!Vulneravel && !atravessaTudo)
                return ResultadoGolpe.Defendeu;
            Vida = Math.Max(0, Vida - Math.Max(1, dano));
            if (Vida <= 0)
            {
                Mudar(EstadoCapitao.Superado, 0f);
                return ResultadoGolpe.Superou;
            }
            return ResultadoGolpe.Acertou;
        }

        /// <summary>O companheiro chamou a atenção dele: fica parado e vulnerável.</summary>
        public void Distrair(float duracao)
        {
            if (Superado || Estado == EstadoCapitao.Saltando || Estado == EstadoCapitao.Aguardando)
                return;
            Mudar(EstadoCapitao.Distraido, duracao);
        }

        bool Contar(float dt)
        {
            timer -= dt;
            return timer <= 0f;
        }

        void Mudar(EstadoCapitao novo, float duracao)
        {
            Estado = novo;
            timer = duracao;
        }
    }

    // ==================================================================== I08
    public enum PosturaGoleiro
    {
        /// <summary>Embaixo da trave: alcança a bola em toda a altura do gol.</summary>
        NaLinha,
        /// <summary>Espalmou um chute forte: em pé de novo num instante.</summary>
        Espalmando,
        /// <summary>Mergulhou para defender e ficou no chão: o gol está aberto!</summary>
        Caido,
        Levantando,
        /// <summary>Saindo do gol para abafar o jogador.</summary>
        Saindo,
        /// <summary>Fora do gol, na frente do jogador: por cima dele, a cavadinha entra.</summary>
        ForaDoGol,
        /// <summary>Segurou a bola fora do gol e volta para a linha.</summary>
        Segurando,
        Voltando,
    }

    public enum ReacaoGoleiro
    {
        Nenhuma,
        /// <summary>Mergulhou e defendeu, mas ficou caído (abertura para o próximo chute).</summary>
        Defendeu,
        /// <summary>Defesa difícil (Chute do Canhão): fica caído por mais tempo.</summary>
        DefesaDificil,
        /// <summary>Chute forte na linha: ele espalma e já se levanta (sem abertura).</summary>
        Espalmou,
        /// <summary>Fora do gol ele agarra a bola rasteira e volta para a linha.</summary>
        Segurou,
        /// <summary>A bola de fogo da torcida passa por cima dele: gol certo.</summary>
        Atropelado,
    }

    public enum EventoGoleiro
    {
        Nenhum,
        IniciarSaida,
        Voltar,
        Arremessar,
        Levantou,
    }

    /// <summary>
    /// Regras do goleiro final da Suécia 1958 (GDD: "defende chutes rasteiros;
    /// habilidade testada: cavadinha e variação de carga"). Na linha ele pega
    /// tudo: chute fraco ou médio faz ele mergulhar e ficar caído (abertura);
    /// chute forte ele espalma e levanta na hora. Quando sai do gol, só a
    /// cavadinha passa por cima dele.
    /// </summary>
    public static class RegrasGoleiro
    {
        public const float TempoCaido = 2f;
        public const float TempoCaidoDefesaDificil = 2.6f;
        public const float TempoLevantar = 0.45f;
        public const float TempoEspalmar = 0.5f;
        public const float TempoSegurando = 0.6f;

        public static bool PodeDefender(PosturaGoleiro postura)
        {
            return postura != PosturaGoleiro.Caido && postura != PosturaGoleiro.Levantando;
        }

        public static bool NaLinha(PosturaGoleiro postura)
        {
            return postura == PosturaGoleiro.NaLinha || postura == PosturaGoleiro.Espalmando;
        }

        /// <summary>O que o goleiro faz quando a bola chega nele.</summary>
        public static ReacaoGoleiro Reagir(PosturaGoleiro postura, TipoChute tipo, ModificadorChute mod)
        {
            if (!PodeDefender(postura))
                return ReacaoGoleiro.Nenhuma;
            bool modificado = Chutes.AceitaModificador(tipo);
            if (modificado && mod == ModificadorChute.Especial)
                return ReacaoGoleiro.Atropelado;
            if (!NaLinha(postura))
                return ReacaoGoleiro.Segurou;
            if (modificado && mod == ModificadorChute.Canhao)
                return ReacaoGoleiro.DefesaDificil;
            if (tipo == TipoChute.Forte || tipo == TipoChute.Superchute)
                return ReacaoGoleiro.Espalmou;
            return ReacaoGoleiro.Defendeu;
        }
    }

    /// <summary>Decide quando o goleiro sai do gol, arremessa bolas e se levanta.</summary>
    public class CerebroGoleiro
    {
        /// <summary>Tempo na linha antes de sair (alterna entre os dois valores).</summary>
        public const float EsperaSaidaA = 4f;
        public const float EsperaSaidaB = 5.5f;
        public const float DuracaoForaDoGol = 1.4f;
        public const float IntervaloArremesso = 2.6f;
        /// <summary>Ele só sai quando o jogador está pelo menos a essa distância dele.</summary>
        public const float DistanciaMinimaParaSair = 4.5f;

        public PosturaGoleiro Postura { get; private set; }
        float timer;
        float timerSaida;
        float timerArremesso;
        int saidas;

        public CerebroGoleiro()
        {
            Reiniciar();
        }

        public void Reiniciar()
        {
            Postura = PosturaGoleiro.NaLinha;
            timer = 0f;
            saidas = 0;
            timerSaida = EsperaSaidaA;
            timerArremesso = IntervaloArremesso;
        }

        public bool PodeDefender
        {
            get { return RegrasGoleiro.PodeDefender(Postura); }
        }

        public bool NaLinha
        {
            get { return RegrasGoleiro.NaLinha(Postura); }
        }

        public EventoGoleiro Atualizar(float dt, float distanciaJogador)
        {
            switch (Postura)
            {
                case PosturaGoleiro.NaLinha:
                    timerSaida -= dt;
                    timerArremesso -= dt;
                    if (timerSaida <= 0f)
                    {
                        if (distanciaJogador >= DistanciaMinimaParaSair)
                        {
                            Postura = PosturaGoleiro.Saindo;
                            saidas++;
                            return EventoGoleiro.IniciarSaida;
                        }
                        timerSaida = 0.5f;
                    }
                    if (timerArremesso <= 0f)
                    {
                        timerArremesso = IntervaloArremesso;
                        return EventoGoleiro.Arremessar;
                    }
                    return EventoGoleiro.Nenhum;
                case PosturaGoleiro.Espalmando:
                    if (Contar(dt))
                        Postura = PosturaGoleiro.NaLinha;
                    return EventoGoleiro.Nenhum;
                case PosturaGoleiro.Caido:
                    if (Contar(dt))
                    {
                        Postura = PosturaGoleiro.Levantando;
                        timer = RegrasGoleiro.TempoLevantar;
                    }
                    return EventoGoleiro.Nenhum;
                case PosturaGoleiro.Levantando:
                    if (Contar(dt))
                    {
                        Postura = PosturaGoleiro.NaLinha;
                        timerArremesso = IntervaloArremesso;
                        return EventoGoleiro.Levantou;
                    }
                    return EventoGoleiro.Nenhum;
                case PosturaGoleiro.ForaDoGol:
                case PosturaGoleiro.Segurando:
                    if (Contar(dt))
                    {
                        Postura = PosturaGoleiro.Voltando;
                        return EventoGoleiro.Voltar;
                    }
                    return EventoGoleiro.Nenhum;
            }
            return EventoGoleiro.Nenhum;
        }

        /// <summary>A bola chegou nele: aplica a reação (e muda a postura).</summary>
        public ReacaoGoleiro ReceberChute(TipoChute tipo, ModificadorChute mod)
        {
            ReacaoGoleiro r = RegrasGoleiro.Reagir(Postura, tipo, mod);
            switch (r)
            {
                case ReacaoGoleiro.Defendeu:
                    Postura = PosturaGoleiro.Caido;
                    timer = RegrasGoleiro.TempoCaido;
                    break;
                case ReacaoGoleiro.DefesaDificil:
                case ReacaoGoleiro.Atropelado:
                    Postura = PosturaGoleiro.Caido;
                    timer = r == ReacaoGoleiro.DefesaDificil ? RegrasGoleiro.TempoCaidoDefesaDificil : RegrasGoleiro.TempoCaido;
                    break;
                case ReacaoGoleiro.Espalmou:
                    Postura = PosturaGoleiro.Espalmando;
                    timer = RegrasGoleiro.TempoEspalmar;
                    break;
                case ReacaoGoleiro.Segurou:
                    Postura = PosturaGoleiro.Segurando;
                    timer = RegrasGoleiro.TempoSegurando;
                    break;
            }
            return r;
        }

        /// <summary>Chegou ao ponto de abafar o jogador.</summary>
        public void ChegouForaDoGol()
        {
            if (Postura != PosturaGoleiro.Saindo)
                return;
            Postura = PosturaGoleiro.ForaDoGol;
            timer = DuracaoForaDoGol;
        }

        /// <summary>Voltou para debaixo da trave.</summary>
        public void ChegouNaLinha()
        {
            if (Postura != PosturaGoleiro.Voltando)
                return;
            Postura = PosturaGoleiro.NaLinha;
            timerSaida = saidas % 2 == 0 ? EsperaSaidaA : EsperaSaidaB;
            timerArremesso = IntervaloArremesso;
        }

        /// <summary>Depois de um gol, ele volta para a linha e recomeça.</summary>
        public void SofreuGol()
        {
            Postura = PosturaGoleiro.Voltando;
            timer = 0f;
        }

        bool Contar(float dt)
        {
            timer -= dt;
            return timer <= 0f;
        }
    }
}
