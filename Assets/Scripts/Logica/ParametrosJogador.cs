using System;

namespace CincoEstrelas
{
    /// <summary>
    /// Números do movimento do Camisa 10. Ficam aqui (sem Unity) porque o
    /// componente <c>Jogador</c> usa os mesmos valores e o
    /// <see cref="AnalisadorMapa"/> precisa deles para conferir se cada pulo
    /// da fase é possível. 1 unidade = 1 tile = 16 pixels.
    /// </summary>
    public static class ParametrosJogador
    {
        /// <summary>Gravidade do Physics2D (Project Settings &gt; Physics 2D).</summary>
        public const float GravidadeMundo = 9.81f;
        public const float VelocidadeAndando = 6f;
        public const float VelocidadeCorrendo = 8.5f;
        /// <summary>Carregando o chute ou fazendo embaixadinhas o jogador anda devagar.</summary>
        public const float FatorCarregando = 0.35f;
        public const float AceleracaoChao = 70f;
        public const float AceleracaoAr = 45f;
        /// <summary>gravityScale do Rigidbody2D subindo.</summary>
        public const float EscalaGravidade = 4.3f;
        /// <summary>Caindo, a gravidade fica mais forte (pulo mais "firme").</summary>
        public const float MultiplicadorQueda = 1.6f;
        public const float ForcaPulo = 17.8f;
        public const float QuedaMaxima = 18f;
        public const float TempoCoyote = 0.1f;
        public const float TempoBuffer = 0.12f;
        public const float LarguraColisor = 0.62f;
        public const float AlturaColisor = 1.3f;
        /// <summary>Altura (a partir do pé) do centro da bola dominada.</summary>
        public const float AlturaBolaNoPe = 0.26f;
        /// <summary>Distância horizontal (a partir do centro do jogador) da bola dominada.</summary>
        public const float DistanciaBolaNoPe = 0.5f;

        public static float GravidadeSubindo
        {
            get { return GravidadeMundo * EscalaGravidade; }
        }

        public static float GravidadeCaindo
        {
            get { return GravidadeSubindo * MultiplicadorQueda; }
        }

        /// <summary>Quanto o pé sobe num pulo completo (botão segurado).</summary>
        public static float AlturaMaximaPulo
        {
            get { return ForcaPulo * ForcaPulo / (2f * GravidadeSubindo); }
        }

        /// <summary>
        /// Tempo no ar de um pulo completo que termina <paramref name="desnivel"/>
        /// unidades acima do ponto de partida (negativo = abaixo).
        /// Devolve -1 se o pulo não alcança essa altura.
        /// </summary>
        public static float TempoNoAr(float desnivel)
        {
            float h = AlturaMaximaPulo;
            if (desnivel > h)
                return -1f;
            float subida = ForcaPulo / GravidadeSubindo;
            float queda = h - desnivel;
            float g = GravidadeCaindo;
            float distanciaAteVelocidadeMaxima = QuedaMaxima * QuedaMaxima / (2f * g);
            float tempoQueda;
            if (queda <= distanciaAteVelocidadeMaxima)
                tempoQueda = (float)Math.Sqrt(2f * queda / g);
            else
                tempoQueda = QuedaMaxima / g + (queda - distanciaAteVelocidadeMaxima) / QuedaMaxima;
            return subida + tempoQueda;
        }

        /// <summary>Distância horizontal de um pulo completo com a velocidade dada.</summary>
        public static float AlcancePulo(float velocidade, float desnivel)
        {
            float t = TempoNoAr(desnivel);
            return t < 0f ? 0f : velocidade * t;
        }
    }
}
