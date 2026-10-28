using System.Collections.Generic;

namespace CincoEstrelas
{
    /// <summary>Uma pergunta de quiz com quatro alternativas.</summary>
    public class PerguntaQuiz
    {
        public string Id;
        /// <summary>Código do formato no GDD (Q01 surpresa, Q02 estádio...).</summary>
        public string Codigo;
        public string Formato;
        public string Copa;
        public string Pergunta;
        public string[] Alternativas;
        /// <summary>Índice (0 a 3) da alternativa certa.</summary>
        public int Correta;
        /// <summary>O narrador explica a resposta (acertando ou errando).</summary>
        public string Explicacao;
        public Recompensa Recompensa;
        public float BonusTorcida;

        public bool Acertou(int alternativa)
        {
            return alternativa == Correta;
        }

        /// <summary>Rótulo da alternativa: "A", "B", "C" ou "D".</summary>
        public static string Letra(int indice)
        {
            return indice >= 0 && indice < 4 ? "ABCD".Substring(indice, 1) : "?";
        }
    }

    /// <summary>
    /// Perguntas da fase da Suécia 1958. Os fatos foram conferidos: final em
    /// 29/06/1958 no estádio Råsunda (Solna), Brasil 5 x 2 Suécia, gols do
    /// Brasil de Vavá (2), Pelé (2) e Zagallo; Pelé tinha 17 anos.
    /// </summary>
    public static class BancoQuiz
    {
        public const string IdSurpresa1958 = "Q01_1958";
        public const string IdEstadio1958 = "Q02_1958";

        /// <summary>Q01 - quiz surpresa, logo depois do primeiro zagueiro carrinho (slide 8).</summary>
        public static PerguntaQuiz Surpresa1958()
        {
            var q = new PerguntaQuiz();
            q.Id = IdSurpresa1958;
            q.Codigo = "Q01";
            q.Formato = "Quiz surpresa";
            q.Copa = "Suécia 1958";
            q.Pergunta = "Qual jogador de 17 anos marcou dois gols na final de 1958, contra a Suécia?";
            q.Alternativas = new[] { "Garrincha", "Pelé", "Didi", "Zagallo" };
            q.Correta = 1;
            q.Explicacao = "Pelé tinha 17 anos. A final terminou Brasil 5 x 2 Suécia: dois gols de Pelé, " +
                           "dois de Vavá e um de Zagallo.";
            q.Recompensa = Recompensa.CartaChuteDoCanhao;
            q.BonusTorcida = ValoresTorcida.QuizCerto;
            return q;
        }

        /// <summary>Q02 - quiz do estádio, antes do confronto final.</summary>
        public static PerguntaQuiz Estadio1958()
        {
            var q = new PerguntaQuiz();
            q.Id = IdEstadio1958;
            q.Codigo = "Q02";
            q.Formato = "Quiz do estádio";
            q.Copa = "Suécia 1958";
            q.Pergunta = "Qual foi o placar da final da Copa de 1958, contra a Suécia?";
            q.Alternativas = new[] { "3 x 1", "5 x 2", "2 x 0", "4 x 2" };
            q.Correta = 1;
            q.Explicacao = "Brasil 5 x 2 Suécia, em 29 de junho de 1958, no estádio Råsunda, em Solna. " +
                           "Foi o primeiro título mundial do Brasil.";
            q.Recompensa = Recompensa.CaneleiraDeOuro;
            q.BonusTorcida = ValoresTorcida.QuizCerto;
            return q;
        }

        public static PerguntaQuiz[] Todas()
        {
            return new[] { Surpresa1958(), Estadio1958() };
        }

        public static PerguntaQuiz PorId(string id)
        {
            foreach (PerguntaQuiz q in Todas())
            {
                if (q.Id == id)
                    return q;
            }
            return null;
        }
    }

    /// <summary>Resultado de uma resposta (para a tela do quiz e para os testes).</summary>
    public class RespostaQuiz
    {
        public PerguntaQuiz Pergunta;
        public int Escolhida;
        public bool Acertou;
        /// <summary>O que o jogador ganhou (Nenhuma se errou ou se a pergunta já tinha sido respondida).</summary>
        public Recompensa Ganhou;
        public GanhoTorcida Torcida;

        /// <summary>Linhas que a tela do quiz mostra depois da resposta.</summary>
        public List<string> LinhasResultado()
        {
            var linhas = new List<string>();
            linhas.Add(Acertou ? Textos.QuizCerto : Textos.QuizErrado);
            linhas.Add(Pergunta.Explicacao);
            if (Ganhou != Recompensa.Nenhuma)
            {
                CartaPoder c = Poderes.CartaDa(Ganhou);
                linhas.Add(string.Format(Textos.QuizGanhouCarta, Poderes.Codigo(c), Poderes.Nome(c)));
                linhas.Add(Poderes.Descricao(c));
            }
            else if (!Acertou)
            {
                linhas.Add(Textos.QuizSemPerda);
            }
            return linhas;
        }
    }
}
