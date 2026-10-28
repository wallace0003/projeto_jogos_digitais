using System;

namespace CincoEstrelas
{
    /// <summary>Os chutes do Camisa 10 (GDD, págs. 4 e 7).</summary>
    public enum TipoChute
    {
        /// <summary>X tocado rápido: carga 1, fraco e curto (bom para enganar o goleiro).</summary>
        Fraco,
        /// <summary>X segurado um pouco: carga 2.</summary>
        Medio,
        /// <summary>X segurado até o fim: carga máxima (3). Derruba a pilha de barris.</summary>
        Forte,
        /// <summary>X logo depois da pedalada (Z): força máxima na hora.</summary>
        Superchute,
        /// <summary>C: bola alta e curta, por cima da marcação.</summary>
        Cavadinha,
        /// <summary>X no ar: finalização aérea forte que atravessa dois adversários.</summary>
        Bicicleta,
    }

    /// <summary>O que muda o chute: a carta Chute do Canhão (P01) ou o especial da torcida.</summary>
    public enum ModificadorChute
    {
        Nenhum,
        /// <summary>P01 - Chute do Canhão: o chute atravessa vários adversários.</summary>
        Canhao,
        /// <summary>Especial da torcida: bola de fogo que ninguém segura.</summary>
        Especial,
    }

    /// <summary>Como a bola sai do pé em cada chute.</summary>
    public struct ParametrosChute
    {
        public float VelocidadeX;
        public float VelocidadeY;
        /// <summary>gravityScale da bola durante o voo.</summary>
        public float EscalaGravidade;
        public int Dano;
        /// <summary>Quantos adversários a bola atravessa antes de voltar (0 = volta no primeiro).</summary>
        public int Atravessa;
        /// <summary>Chute pesado: derruba a pilha de barris.</summary>
        public bool Pesado;
        /// <summary>Ninguém segura (o especial da torcida).</summary>
        public bool Imparavel;
        /// <summary>Segundos de voo antes de a bola voltar sozinha para o pé.</summary>
        public float TempoMaximo;

        /// <summary>Altura da bola (em relação ao ponto do chute) depois de andar <paramref name="distancia"/> na horizontal.</summary>
        public float AlturaNaDistancia(float distancia)
        {
            float vx = Math.Abs(VelocidadeX);
            if (vx < 0.001f)
                return 0f;
            float t = Math.Abs(distancia) / vx;
            float g = ParametrosJogador.GravidadeMundo * EscalaGravidade;
            return VelocidadeY * t - 0.5f * g * t * t;
        }

        /// <summary>Altura máxima que a bola alcança acima do ponto do chute.</summary>
        public float AlturaMaxima
        {
            get
            {
                if (VelocidadeY <= 0f)
                    return 0f;
                float g = ParametrosJogador.GravidadeMundo * EscalaGravidade;
                if (g <= 0.001f)
                    return 0f;
                return VelocidadeY * VelocidadeY / (2f * g);
            }
        }
    }

    public static class Chutes
    {
        /// <summary>Quanto da velocidade do jogador passa para a bola.</summary>
        public const float HerancaVelocidade = 0.3f;
        public const int AtravessaTudo = 99;

        public static ParametrosChute Parametros(TipoChute tipo)
        {
            var p = new ParametrosChute();
            switch (tipo)
            {
                case TipoChute.Fraco:
                    p.VelocidadeX = 9f;
                    p.VelocidadeY = 3f;
                    p.EscalaGravidade = 2.5f;
                    p.Dano = 1;
                    p.TempoMaximo = 1.2f;
                    break;
                case TipoChute.Medio:
                    p.VelocidadeX = 14f;
                    p.VelocidadeY = 4f;
                    p.EscalaGravidade = 2.3f;
                    p.Dano = 1;
                    p.TempoMaximo = 1.5f;
                    break;
                case TipoChute.Forte:
                    p.VelocidadeX = 19f;
                    p.VelocidadeY = 4.5f;
                    p.EscalaGravidade = 1.6f;
                    p.Dano = 2;
                    p.Pesado = true;
                    p.TempoMaximo = 1.6f;
                    break;
                case TipoChute.Superchute:
                    p.VelocidadeX = 22f;
                    p.VelocidadeY = 3.5f;
                    p.EscalaGravidade = 1.2f;
                    p.Dano = 3;
                    p.Atravessa = 1;
                    p.Pesado = true;
                    p.TempoMaximo = 1.6f;
                    break;
                case TipoChute.Cavadinha:
                    p.VelocidadeX = 7.5f;
                    p.VelocidadeY = 14f;
                    p.EscalaGravidade = 2.2f;
                    p.Dano = 1;
                    p.TempoMaximo = 2.4f;
                    break;
                case TipoChute.Bicicleta:
                    p.VelocidadeX = 16f;
                    p.VelocidadeY = -5f;
                    p.EscalaGravidade = 2f;
                    p.Dano = 2;
                    p.Atravessa = 2;
                    p.TempoMaximo = 1.3f;
                    break;
            }
            return p;
        }

        /// <summary>
        /// Parâmetros com a carta ou o especial aplicados. A cavadinha nunca muda:
        /// ela continua sendo a bola alta (e o especial fica guardado para o próximo chute).
        /// </summary>
        public static ParametrosChute Parametros(TipoChute tipo, ModificadorChute mod)
        {
            ParametrosChute p = Parametros(tipo);
            if (tipo == TipoChute.Cavadinha)
                return p;
            switch (mod)
            {
                case ModificadorChute.Canhao:
                    p.VelocidadeX = 24f;
                    p.VelocidadeY = 1.5f;
                    p.EscalaGravidade = 0.4f;
                    p.Dano = Math.Max(p.Dano, 3);
                    p.Atravessa = AtravessaTudo;
                    p.Pesado = true;
                    p.TempoMaximo = 1.2f;
                    break;
                case ModificadorChute.Especial:
                    p.VelocidadeX = 24f;
                    p.VelocidadeY = 2f;
                    p.EscalaGravidade = 0.3f;
                    p.Dano = 5;
                    p.Atravessa = AtravessaTudo;
                    p.Pesado = true;
                    p.Imparavel = true;
                    p.TempoMaximo = 1.4f;
                    break;
            }
            return p;
        }

        /// <summary>O modificador vale para este chute? (a cavadinha não usa carta nem especial)</summary>
        public static bool AceitaModificador(TipoChute tipo)
        {
            return tipo != TipoChute.Cavadinha;
        }

        /// <summary>
        /// Velocidade inicial da bola. <paramref name="direcao"/> é 1 (direita) ou -1 (esquerda).
        /// Correndo para o mesmo lado, o chute sai mais forte.
        /// </summary>
        public static void VelocidadeInicial(ParametrosChute p, int direcao, float velocidadeJogadorX,
            out float vx, out float vy)
        {
            int d = direcao >= 0 ? 1 : -1;
            float heranca = velocidadeJogadorX * d > 0f ? velocidadeJogadorX * HerancaVelocidade : 0f;
            vx = p.VelocidadeX * d + heranca;
            vy = p.VelocidadeY;
        }

        /// <summary>Identificador da ação para a barra da torcida (que premia variedade).</summary>
        public static string Acao(TipoChute tipo, ModificadorChute mod)
        {
            if (AceitaModificador(tipo))
            {
                if (mod == ModificadorChute.Especial)
                    return AcoesTorcida.Especial;
                if (mod == ModificadorChute.Canhao)
                    return AcoesTorcida.Canhao;
            }
            switch (tipo)
            {
                case TipoChute.Fraco: return AcoesTorcida.ChuteFraco;
                case TipoChute.Medio: return AcoesTorcida.ChuteMedio;
                case TipoChute.Forte: return AcoesTorcida.ChuteForte;
                case TipoChute.Superchute: return AcoesTorcida.Superchute;
                case TipoChute.Cavadinha: return AcoesTorcida.Cavadinha;
                default: return AcoesTorcida.Bicicleta;
            }
        }

        /// <summary>Nível de carga (1, 2 ou 3) de um chute rasteiro, para o HUD e os textos.</summary>
        public static int NivelCarga(TipoChute tipo)
        {
            switch (tipo)
            {
                case TipoChute.Fraco: return 1;
                case TipoChute.Medio: return 2;
                case TipoChute.Forte:
                case TipoChute.Superchute:
                    return 3;
            }
            return 0;
        }
    }

    /// <summary>
    /// Decide a carga do chute pelo tempo que o X ficou apertado (GDD, pág. 4:
    /// "X: chutar; segurar para carregar força e alcance").
    /// </summary>
    public class ControleChute
    {
        /// <summary>Segurando mais que isso o chute vira carga 2.</summary>
        public const float TempoNivel2 = 0.25f;
        /// <summary>Segurando mais que isso o chute vira carga máxima (3).</summary>
        public const float TempoNivel3 = 0.7f;

        public bool Segurando { get; private set; }
        public float TempoSegurado { get; private set; }

        /// <summary>1, 2 ou 3 (0 se não está segurando).</summary>
        public int Nivel
        {
            get
            {
                if (!Segurando)
                    return 0;
                if (TempoSegurado >= TempoNivel3)
                    return 3;
                if (TempoSegurado >= TempoNivel2)
                    return 2;
                return 1;
            }
        }

        /// <summary>0 a 1: quanto da barra de carga está cheia (para o HUD).</summary>
        public float Carga01
        {
            get
            {
                if (!Segurando)
                    return 0f;
                return Math.Max(0f, Math.Min(1f, TempoSegurado / TempoNivel3));
            }
        }

        /// <summary>Segurando há tempo suficiente para o jogador perceber que está carregando.</summary>
        public bool Carregando
        {
            get { return Segurando && TempoSegurado >= TempoNivel2 * 0.5f; }
        }

        public void Apertar()
        {
            Segurando = true;
            TempoSegurado = 0f;
        }

        public void Atualizar(float dt)
        {
            if (Segurando)
                TempoSegurado += dt;
        }

        /// <summary>Soltou o X: devolve o chute que deve sair.</summary>
        public TipoChute Soltar()
        {
            int nivel = Math.Max(1, Nivel);
            Cancelar();
            return TipoPorNivel(nivel);
        }

        public void Cancelar()
        {
            Segurando = false;
            TempoSegurado = 0f;
        }

        public static TipoChute TipoPorNivel(int nivel)
        {
            if (nivel >= 3)
                return TipoChute.Forte;
            if (nivel == 2)
                return TipoChute.Medio;
            return TipoChute.Fraco;
        }
    }

    /// <summary>
    /// A pedalada (Z) abre uma janela curta: o próximo X sai como superchute
    /// (GDD, M02: "uma pedalada bem executada prepara uma janela de força").
    /// </summary>
    public class JanelaSuperchute
    {
        public const float Duracao = 1.6f;

        public float Restante { get; private set; }

        public bool Ativa
        {
            get { return Restante > 0f; }
        }

        public void Abrir()
        {
            Restante = Duracao;
        }

        public void Atualizar(float dt)
        {
            if (Restante > 0f)
                Restante = Math.Max(0f, Restante - dt);
        }

        /// <summary>Usa a janela (se estiver aberta). Devolve true se o chute vira superchute.</summary>
        public bool Consumir()
        {
            if (!Ativa)
                return false;
            Restante = 0f;
            return true;
        }

        public void Fechar()
        {
            Restante = 0f;
        }
    }
}
