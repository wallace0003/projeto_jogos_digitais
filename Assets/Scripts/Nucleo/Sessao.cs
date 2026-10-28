using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Dados da partida que atravessam as cenas (vidas, quizzes, torcida,
    /// estatísticas e resultado). As regras ficam em <see cref="EstadoPartida"/>;
    /// aqui só se guarda a instância e o melhor tempo (PlayerPrefs).
    /// </summary>
    public static class Sessao
    {
        const string ChaveMelhorTempo = "cincoestrelas_melhor_tempo_1958";

        static EstadoPartida estado;

        /// <summary>O estado da partida (nunca é nulo).</summary>
        public static EstadoPartida Estado
        {
            get
            {
                if (estado == null)
                    estado = new EstadoPartida();
                return estado;
            }
        }

        public static bool Iniciada { get; private set; }

        /// <summary>A abertura do narrador já foi mostrada nesta partida.</summary>
        public static bool IntroVista { get; set; }

        /// <summary>
        /// O projeto usa "Enter Play Mode Options" sem recarregar o domínio, então
        /// campos estáticos precisam ser zerados manualmente a cada Play.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            estado = new EstadoPartida();
            Iniciada = false;
            IntroVista = false;
            NovoRecorde = false;
        }

        public static void NovoJogo()
        {
            Estado.NovoJogo();
            Iniciada = true;
            IntroVista = false;
        }

        /// <summary>Volta ao menu: a partida acaba (para jogar de novo é preciso apertar JOGAR).</summary>
        public static void Encerrar()
        {
            Iniciada = false;
        }

        /// <summary>Permite dar Play direto na fase pelo Editor.</summary>
        public static void GarantirIniciada()
        {
            if (!Iniciada)
                NovoJogo();
        }

        /// <summary>Melhor tempo de vitória (segundos), ou 0 se ainda não venceu.</summary>
        public static float MelhorTempo
        {
            get { return PlayerPrefs.GetFloat(ChaveMelhorTempo, 0f); }
        }

        /// <summary>A última vitória bateu o melhor tempo (a tela final comemora).</summary>
        public static bool NovoRecorde { get; private set; }

        /// <summary>Encerra a partida. Devolve true se foi um novo melhor tempo.</summary>
        public static bool Finalizar()
        {
            Iniciada = false;
            NovoRecorde = false;
            if (Estado.Resultado != ResultadoJogo.Vitoria)
                return false;
            float melhor = MelhorTempo;
            if (melhor > 0f && Estado.TempoDeJogo >= melhor)
                return false;
            PlayerPrefs.SetFloat(ChaveMelhorTempo, Estado.TempoDeJogo);
            PlayerPrefs.Save();
            NovoRecorde = true;
            return true;
        }
    }
}
