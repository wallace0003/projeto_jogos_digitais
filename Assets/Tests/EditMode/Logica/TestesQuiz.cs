using System.Collections.Generic;
using NUnit.Framework;

namespace CincoEstrelas.Testes
{
    /// <summary>Os dois quizzes da Suécia 1958 (o Q01 é o exemplo do slide 8 da apresentação).</summary>
    [TestFixture]
    public class TestesQuiz
    {
        [Test]
        public void Q01PerguntaQuemTinha17Anos()
        {
            PerguntaQuiz q = BancoQuiz.Surpresa1958();
            Assert.AreEqual("Q01", q.Codigo);
            Assert.AreEqual(4, q.Alternativas.Length);
            Assert.AreEqual("Pelé", q.Alternativas[q.Correta]);
            Assert.AreEqual("B", PerguntaQuiz.Letra(q.Correta));
            Assert.AreEqual(Recompensa.CartaChuteDoCanhao, q.Recompensa);
            Assert.IsTrue(q.Explicacao.Contains("5 x 2"));
        }

        [Test]
        public void Q02PerguntaOPlacarDaFinal()
        {
            PerguntaQuiz q = BancoQuiz.Estadio1958();
            Assert.AreEqual("Q02", q.Codigo);
            Assert.AreEqual(4, q.Alternativas.Length);
            Assert.AreEqual("5 x 2", q.Alternativas[q.Correta]);
            Assert.AreEqual(Recompensa.CaneleiraDeOuro, q.Recompensa);
            Assert.IsTrue(q.Explicacao.Contains("29 de junho de 1958"));
        }

        [Test]
        public void AlternativasNaoSeRepetem()
        {
            foreach (PerguntaQuiz q in BancoQuiz.Todas())
            {
                var vistas = new HashSet<string>();
                foreach (string a in q.Alternativas)
                    Assert.IsTrue(vistas.Add(a), q.Codigo + ": alternativa repetida " + a);
                Assert.IsTrue(q.Correta >= 0 && q.Correta < q.Alternativas.Length);
            }
        }

        [Test]
        public void IdsSaoUnicosEAchaveis()
        {
            var ids = new HashSet<string>();
            foreach (PerguntaQuiz q in BancoQuiz.Todas())
            {
                Assert.IsTrue(ids.Add(q.Id));
                Assert.AreEqual(q.Pergunta, BancoQuiz.PorId(q.Id).Pergunta);
            }
            Assert.IsNull(BancoQuiz.PorId("nao-existe"));
        }

        [Test]
        public void ResultadoMostraExplicacaoECarta()
        {
            var e = new EstadoPartida();
            PerguntaQuiz q = BancoQuiz.Surpresa1958();
            List<string> linhas = e.Responder(q, q.Correta).LinhasResultado();
            Assert.AreEqual(Textos.QuizCerto, linhas[0]);
            Assert.AreEqual(q.Explicacao, linhas[1]);
            Assert.IsTrue(linhas[2].Contains("P01"));
            Assert.IsTrue(linhas[2].Contains("Chute do Canhão"));
        }

        [Test]
        public void ResultadoDoErroAvisaQueNaoTiraVida()
        {
            var e = new EstadoPartida();
            PerguntaQuiz q = BancoQuiz.Surpresa1958();
            List<string> linhas = e.Responder(q, 3).LinhasResultado();
            Assert.AreEqual(Textos.QuizErrado, linhas[0]);
            Assert.AreEqual(q.Explicacao, linhas[1]);
            Assert.AreEqual(Textos.QuizSemPerda, linhas[2]);
        }
    }
}
