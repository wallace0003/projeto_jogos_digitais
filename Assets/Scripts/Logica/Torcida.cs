using System;
using System.Collections.Generic;

namespace CincoEstrelas
{
    /// <summary>Nomes das ações que a barra da torcida reconhece.</summary>
    public static class AcoesTorcida
    {
        public const string ChuteFraco = "chute_fraco";
        public const string ChuteMedio = "chute_medio";
        public const string ChuteForte = "chute_forte";
        public const string Superchute = "superchute";
        public const string Cavadinha = "cavadinha";
        public const string Bicicleta = "bicicleta";
        public const string Canhao = "canhao";
        public const string Especial = "especial";
        public const string Pedalada = "pedalada";
        public const string Tabela = "tabela";
        public const string Sino = "sino";
        public const string Barris = "barris";
        public const string Caixote = "caixote";
    }

    /// <summary>O que aconteceu com a barra depois de uma ação.</summary>
    public struct GanhoTorcida
    {
        public float Ganho;
        /// <summary>1,5 = variou (bônus); 0,5 = repetiu a mesma ação; 1 = normal.</summary>
        public float Multiplicador;
        /// <summary>A barra acabou de encher (o especial ficou pronto agora).</summary>
        public bool FicouPronta;
    }

    /// <summary>
    /// Barra da torcida (GDD, M10): cresce com variedade, precisão e quizzes;
    /// cheia, libera o especial (Espaço). Repetir sempre a mesma ação rende menos.
    /// </summary>
    public class Torcida
    {
        public const float Maximo = 100f;
        public const float BonusVariedade = 1.5f;
        public const float PenalidadeRepeticao = 0.5f;
        /// <summary>Uma ação conta como "variada" se não aparece nas últimas N.</summary>
        public const int JanelaVariedade = 3;
        /// <summary>Cada sequência de embaixadinhas rende no máximo isso.</summary>
        public const int MaximoEmbaixadinhasPorSequencia = 12;
        public const float PontosPorEmbaixadinha = 1f;

        readonly List<string> historico = new List<string>();
        int embaixadinhasNaSequencia;

        public float Valor { get; private set; }

        public float Fracao
        {
            get { return Valor / Maximo; }
        }

        public bool EspecialPronto
        {
            get { return Valor >= Maximo; }
        }

        /// <summary>Últimas ações (a mais recente no fim). Só para consulta.</summary>
        public IList<string> Historico
        {
            get { return historico.AsReadOnly(); }
        }

        /// <summary>Multiplicador que a ação receberia agora (sem registrá-la).</summary>
        public float MultiplicadorPara(string acao)
        {
            if (historico.Count == 0)
                return 1f;
            if (historico[historico.Count - 1] == acao)
                return PenalidadeRepeticao;
            int inicio = Math.Max(0, historico.Count - JanelaVariedade);
            for (int i = inicio; i < historico.Count; i++)
            {
                if (historico[i] == acao)
                    return 1f;
            }
            return BonusVariedade;
        }

        /// <summary>Registra uma ação (com o bônus de variedade) e soma o ganho na barra.</summary>
        public GanhoTorcida Registrar(string acao, float valorBase)
        {
            float m = MultiplicadorPara(acao);
            historico.Add(acao);
            if (historico.Count > 8)
                historico.RemoveAt(0);
            embaixadinhasNaSequencia = 0;
            GanhoTorcida g = Somar(valorBase * m);
            g.Multiplicador = m;
            return g;
        }

        /// <summary>Soma direto (quiz, gol): não conta para a variedade.</summary>
        public GanhoTorcida Somar(float valor)
        {
            bool antes = EspecialPronto;
            float novo = Math.Max(0f, Math.Min(Maximo, Valor + valor));
            var g = new GanhoTorcida();
            g.Ganho = novo - Valor;
            g.Multiplicador = 1f;
            Valor = novo;
            g.FicouPronta = !antes && EspecialPronto;
            return g;
        }

        /// <summary>Uma embaixadinha: rende pouco e tem limite por sequência.</summary>
        public GanhoTorcida Embaixadinha()
        {
            if (embaixadinhasNaSequencia >= MaximoEmbaixadinhasPorSequencia)
                return new GanhoTorcida { Ganho = 0f, Multiplicador = 1f, FicouPronta = false };
            embaixadinhasNaSequencia++;
            return Somar(PontosPorEmbaixadinha);
        }

        /// <summary>O jogador parou de fazer embaixadinhas: a próxima sequência pode render de novo.</summary>
        public void EncerrarSequenciaEmbaixadinhas()
        {
            embaixadinhasNaSequencia = 0;
        }

        public int EmbaixadinhasNaSequencia
        {
            get { return embaixadinhasNaSequencia; }
        }

        /// <summary>Usa o especial (se a barra estiver cheia): a barra volta a zero.</summary>
        public bool UsarEspecial()
        {
            if (!EspecialPronto)
                return false;
            Valor = 0f;
            historico.Clear();
            return true;
        }

        public void Zerar()
        {
            Valor = 0f;
            historico.Clear();
            embaixadinhasNaSequencia = 0;
        }
    }

    /// <summary>Quanto cada ação vale na barra da torcida (antes do multiplicador de variedade).</summary>
    public static class ValoresTorcida
    {
        public const float AcertoAdversario = 8f;
        public const float AdversarioSuperado = 10f;
        public const float Caixote = 3f;
        public const float Barris = 10f;
        public const float Sino = 10f;
        public const float Pedalada = 2f;
        public const float Tabela = 6f;
        public const float QuizCerto = 20f;
        public const float Gol = 15f;
        public const float ComboTres = 5f;
    }
}
