using System.Collections.Generic;

namespace CincoEstrelas
{
    public enum ResultadoJogo
    {
        EmAndamento,
        Vitoria,
        Derrota,
    }

    /// <summary>O que um dano (ou uma queda na água) causou.</summary>
    public enum ResultadoDano
    {
        /// <summary>Nada aconteceu (jogo já acabou).</summary>
        Ignorado,
        /// <summary>A Caneleira de Ouro segurou o golpe.</summary>
        Bloqueado,
        /// <summary>Perdeu um coração e segue jogando.</summary>
        PerdeuCoracao,
        /// <summary>Os corações acabaram: perdeu uma vida e volta ao último checkpoint.</summary>
        PerdeuVida,
        /// <summary>As vidas acabaram.</summary>
        FimDeJogo,
    }

    /// <summary>
    /// Regras da partida (GDD, pág. 4): 3 vidas, cada vida com 3 corações;
    /// checkpoints preservam os quizzes respondidos; os quizzes nunca tiram vida.
    /// Também guarda a carta de poder, a Caneleira de Ouro, a torcida, os gols
    /// na final e as estatísticas da tela de vitória.
    /// </summary>
    public class EstadoPartida
    {
        public const int VidasIniciais = 3;
        public const int CoracoesPorVida = 3;
        public const int GolsParaVencer = 3;

        readonly Dictionary<string, bool> quizzes = new Dictionary<string, bool>();
        float tempoUltimoAcerto = -999f;

        public int Vidas { get; private set; }
        public int Coracoes { get; private set; }
        /// <summary>Caneleira de Ouro (P05): anula o próximo dano.</summary>
        public bool Escudo { get; private set; }
        /// <summary>Carta guardada no espaço de poder (ativada com Espaço).</summary>
        public CartaPoder Carta { get; private set; }
        public int Checkpoint { get; private set; }
        public Torcida Torcida { get; private set; }
        public int Gols { get; private set; }
        public float TempoDeJogo { get; private set; }
        public int AdversariosSuperados { get; private set; }
        public int ComboAtual { get; private set; }
        public int MaiorCombo { get; private set; }
        public int ChutesNoGol { get; private set; }
        public ResultadoJogo Resultado { get; private set; }

        public EstadoPartida()
        {
            Torcida = new Torcida();
            NovoJogo();
        }

        public void NovoJogo()
        {
            Vidas = VidasIniciais;
            Coracoes = CoracoesPorVida;
            Escudo = false;
            Carta = CartaPoder.Nenhuma;
            Checkpoint = 0;
            Torcida.Zerar();
            Gols = 0;
            TempoDeJogo = 0f;
            AdversariosSuperados = 0;
            ComboAtual = 0;
            MaiorCombo = 0;
            ChutesNoGol = 0;
            tempoUltimoAcerto = -999f;
            quizzes.Clear();
            Resultado = ResultadoJogo.EmAndamento;
        }

        public bool EmAndamento
        {
            get { return Resultado == ResultadoJogo.EmAndamento; }
        }

        public void AvancarTempo(float dt)
        {
            if (EmAndamento)
                TempoDeJogo += dt;
        }

        // ------------------------------------------------------------ dano e vidas
        /// <summary>Golpe de adversário: tira 1 coração (a Caneleira segura um golpe).</summary>
        public ResultadoDano TomarDano()
        {
            if (!EmAndamento)
                return ResultadoDano.Ignorado;
            ComboAtual = 0;
            if (Escudo)
            {
                Escudo = false;
                return ResultadoDano.Bloqueado;
            }
            return PerderCoracao();
        }

        /// <summary>
        /// Caiu no canal: "armadilhas graves reiniciam o trecho". Perde 1 coração
        /// (a Caneleira não protege da água) e volta ao último checkpoint.
        /// </summary>
        public ResultadoDano CairNaAgua()
        {
            if (!EmAndamento)
                return ResultadoDano.Ignorado;
            ComboAtual = 0;
            return PerderCoracao();
        }

        ResultadoDano PerderCoracao()
        {
            Coracoes--;
            if (Coracoes > 0)
                return ResultadoDano.PerdeuCoracao;
            Vidas--;
            if (Vidas <= 0)
            {
                Vidas = 0;
                Coracoes = 0;
                Resultado = ResultadoJogo.Derrota;
                return ResultadoDano.FimDeJogo;
            }
            Coracoes = CoracoesPorVida;
            return ResultadoDano.PerdeuVida;
        }

        /// <summary>Coração do caixote: recupera 1 coração (até o máximo). Devolve true se recuperou.</summary>
        public bool RecuperarCoracao()
        {
            if (!EmAndamento || Coracoes >= CoracoesPorVida)
                return false;
            Coracoes++;
            return true;
        }

        // ------------------------------------------------------------ checkpoints
        /// <summary>Ativa um checkpoint (só avança, nunca volta). Devolve true se é novo.</summary>
        public bool AtivarCheckpoint(int indice)
        {
            if (indice <= Checkpoint)
                return false;
            Checkpoint = indice;
            return true;
        }

        // ------------------------------------------------------------ poderes
        public void GanharCarta(CartaPoder carta)
        {
            if (carta == CartaPoder.CaneleiraDeOuro)
                Escudo = true;
            else if (carta != CartaPoder.Nenhuma)
                Carta = carta;
        }

        /// <summary>Usa a carta guardada (Espaço). Devolve a carta usada ou Nenhuma.</summary>
        public CartaPoder UsarCarta()
        {
            CartaPoder c = Carta;
            Carta = CartaPoder.Nenhuma;
            return c;
        }

        // ------------------------------------------------------------ quizzes
        public bool QuizRespondido(string id)
        {
            return quizzes.ContainsKey(id);
        }

        public int QuizzesAcertados
        {
            get
            {
                int n = 0;
                foreach (bool v in quizzes.Values)
                {
                    if (v)
                        n++;
                }
                return n;
            }
        }

        public int QuizzesRespondidos
        {
            get { return quizzes.Count; }
        }

        /// <summary>
        /// Responde um quiz. Acertando: recompensa e bônus na torcida. Errando:
        /// só a explicação (nunca tira vida). Cada pergunta só vale uma vez.
        /// </summary>
        public RespostaQuiz Responder(PerguntaQuiz pergunta, int alternativa)
        {
            var r = new RespostaQuiz();
            r.Pergunta = pergunta;
            r.Escolhida = alternativa;
            r.Acertou = pergunta.Acertou(alternativa);
            r.Ganhou = Recompensa.Nenhuma;
            if (QuizRespondido(pergunta.Id))
                return r;
            quizzes[pergunta.Id] = r.Acertou;
            if (r.Acertou)
            {
                r.Ganhou = pergunta.Recompensa;
                GanharCarta(Poderes.CartaDa(pergunta.Recompensa));
                r.Torcida = Torcida.Somar(pergunta.BonusTorcida);
            }
            return r;
        }

        // ------------------------------------------------------------ combos e estatísticas
        /// <summary>
        /// Registra um acerto num adversário. Acertos com menos de
        /// <see cref="JanelaCombo"/> segundos entre si formam um combo.
        /// Devolve o tamanho do combo atual.
        /// </summary>
        public int RegistrarAcerto()
        {
            if (TempoDeJogo - tempoUltimoAcerto <= JanelaCombo)
                ComboAtual++;
            else
                ComboAtual = 1;
            tempoUltimoAcerto = TempoDeJogo;
            if (ComboAtual > MaiorCombo)
                MaiorCombo = ComboAtual;
            return ComboAtual;
        }

        public const float JanelaCombo = 3f;

        public void RegistrarAdversarioSuperado()
        {
            AdversariosSuperados++;
        }

        public void RegistrarChuteNoGol()
        {
            ChutesNoGol++;
        }

        // ------------------------------------------------------------ final
        /// <summary>Marca um gol na final. Devolve true se foi o gol da vitória.</summary>
        public bool MarcarGol()
        {
            if (!EmAndamento)
                return false;
            Gols++;
            Torcida.Somar(ValoresTorcida.Gol);
            if (Gols >= GolsParaVencer)
            {
                Resultado = ResultadoJogo.Vitoria;
                return true;
            }
            return false;
        }
    }
}
