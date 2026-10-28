using System;

namespace CincoEstrelas
{
    public enum SituacaoBola
    {
        /// <summary>Dominada no pé do Camisa 10 (pronta para chutar).</summary>
        NoPe,
        /// <summary>Chutada, voando ou quicando pelo cenário.</summary>
        Voando,
        /// <summary>Voltando sozinha para o pé (não acerta ninguém).</summary>
        Voltando,
        /// <summary>Com o companheiro de equipe (tabela): ele devolve em seguida.</summary>
        Tabela,
    }

    /// <summary>
    /// Regras da bola: quando pode ser chutada, quando volta para o pé, quantos
    /// adversários ainda atravessa. O componente <c>Bola</c> cuida da física e
    /// do desenho; aqui fica só a decisão (GDD: "a bola retorna após um curto intervalo").
    /// </summary>
    public class EstadoBola
    {
        /// <summary>Abaixo dessa velocidade a bola é considerada parada.</summary>
        public const float VelocidadeParada = 0.8f;
        /// <summary>Parada por esse tempo, a bola volta sozinha.</summary>
        public const float TempoParadaParaVoltar = 0.35f;
        /// <summary>Logo depois do chute o jogador não "pega" a bola de novo sem querer.</summary>
        public const float TempoMinimoParaDominar = 0.3f;
        /// <summary>Bola mais lenta que isso pode ser dominada encostando nela.</summary>
        public const float VelocidadeMaximaParaDominar = 4.5f;
        public const float DistanciaParaDominar = 0.75f;
        /// <summary>Voltando, a bola é dominada quando chega a essa distância do pé.</summary>
        public const float DistanciaChegada = 0.45f;
        /// <summary>Longe demais do jogador a bola volta (não some da tela).</summary>
        public const float DistanciaMaxima = 22f;

        public SituacaoBola Situacao { get; private set; }
        public TipoChute UltimoChute { get; private set; }
        public ModificadorChute UltimoModificador { get; private set; }
        public float TempoVoo { get; private set; }
        public float TempoVoltando { get; private set; }
        /// <summary>Quantos adversários a bola ainda atravessa neste chute.</summary>
        public int AtravessaRestante { get; private set; }
        /// <summary>Quantos adversários este chute já acertou.</summary>
        public int AcertosNoChute { get; private set; }

        float tempoMaximo;
        float tempoParada;

        public EstadoBola()
        {
            Situacao = SituacaoBola.NoPe;
        }

        public bool NoPe
        {
            get { return Situacao == SituacaoBola.NoPe; }
        }

        public bool PodeChutar
        {
            get { return Situacao == SituacaoBola.NoPe; }
        }

        /// <summary>Só a bola em voo acerta adversários, sinos e caixotes.</summary>
        public bool PodeAcertar
        {
            get { return Situacao == SituacaoBola.Voando; }
        }

        public ParametrosChute ParametrosAtuais
        {
            get { return Chutes.Parametros(UltimoChute, UltimoModificador); }
        }

        public bool Chutar(TipoChute tipo, ModificadorChute mod)
        {
            if (!PodeChutar)
                return false;
            ParametrosChute p = Chutes.Parametros(tipo, mod);
            Situacao = SituacaoBola.Voando;
            UltimoChute = tipo;
            UltimoModificador = Chutes.AceitaModificador(tipo) ? mod : ModificadorChute.Nenhum;
            TempoVoo = 0f;
            TempoVoltando = 0f;
            tempoParada = 0f;
            tempoMaximo = p.TempoMaximo;
            AtravessaRestante = p.Atravessa;
            AcertosNoChute = 0;
            return true;
        }

        /// <summary>
        /// Avança o voo. <paramref name="velocidade"/> é o módulo da velocidade da bola e
        /// <paramref name="distanciaAoPe"/> a distância até o pé do jogador.
        /// </summary>
        public void AtualizarVoo(float dt, float velocidade, float distanciaAoPe, bool caiuNaAgua)
        {
            if (Situacao != SituacaoBola.Voando)
                return;
            TempoVoo += dt;
            tempoParada = velocidade < VelocidadeParada ? tempoParada + dt : 0f;

            if (TempoVoo >= TempoMinimoParaDominar && velocidade <= VelocidadeMaximaParaDominar &&
                distanciaAoPe <= DistanciaParaDominar)
            {
                Dominar();
                return;
            }
            if (caiuNaAgua || TempoVoo >= tempoMaximo || tempoParada >= TempoParadaParaVoltar ||
                distanciaAoPe > DistanciaMaxima)
                Chamar();
        }

        /// <summary>
        /// A bola acertou um adversário. Devolve true se ela segue em frente
        /// (atravessa) ou false se deve ricochetear.
        /// </summary>
        public bool RegistrarAcerto()
        {
            AcertosNoChute++;
            if (AtravessaRestante > 0)
            {
                AtravessaRestante--;
                return true;
            }
            return false;
        }

        /// <summary>Faz a bola voltar para o pé (apertar X com a bola longe também chama).</summary>
        public void Chamar()
        {
            if (Situacao != SituacaoBola.Voando)
                return;
            Situacao = SituacaoBola.Voltando;
            TempoVoltando = 0f;
        }

        public void AtualizarVolta(float dt, float distanciaAoPe)
        {
            if (Situacao != SituacaoBola.Voltando)
                return;
            TempoVoltando += dt;
            if (distanciaAoPe <= DistanciaChegada)
                Dominar();
        }

        public void Dominar()
        {
            Situacao = SituacaoBola.NoPe;
            TempoVoo = 0f;
            TempoVoltando = 0f;
            tempoParada = 0f;
            AtravessaRestante = 0;
        }

        /// <summary>Passa a bola para o companheiro (só com a bola no pé).</summary>
        public bool IniciarTabela()
        {
            if (Situacao != SituacaoBola.NoPe)
                return false;
            Situacao = SituacaoBola.Tabela;
            return true;
        }

        /// <summary>O companheiro devolveu: a bola está no pé de novo.</summary>
        public void TerminarTabela()
        {
            if (Situacao == SituacaoBola.Tabela)
                Dominar();
        }

        /// <summary>A bola volta cada vez mais rápido (assim nunca fica para trás).</summary>
        public static float VelocidadeVolta(float tempoVoltando)
        {
            return Math.Min(14f + tempoVoltando * 30f, 34f);
        }
    }
}
