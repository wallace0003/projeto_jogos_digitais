using NUnit.Framework;

namespace CincoEstrelas.Testes
{
    /// <summary>Vidas, corações, checkpoints, quizzes, cartas e gols (GDD, pág. 4).</summary>
    [TestFixture]
    public class TestesEstadoPartida
    {
        EstadoPartida e;

        [SetUp]
        public void Preparar()
        {
            e = new EstadoPartida();
        }

        [Test]
        public void ComecaCom3VidasE3Coracoes()
        {
            Assert.AreEqual(3, e.Vidas);
            Assert.AreEqual(3, e.Coracoes);
            Assert.AreEqual(ResultadoJogo.EmAndamento, e.Resultado);
            Assert.AreEqual(CartaPoder.Nenhuma, e.Carta);
            Assert.IsFalse(e.Escudo);
        }

        [Test]
        public void GolpeTiraUmCoracao()
        {
            Assert.AreEqual(ResultadoDano.PerdeuCoracao, e.TomarDano());
            Assert.AreEqual(2, e.Coracoes);
            Assert.AreEqual(3, e.Vidas);
        }

        [Test]
        public void SemCoracoesPerdeUmaVidaEVoltaCom3()
        {
            e.TomarDano();
            e.TomarDano();
            Assert.AreEqual(ResultadoDano.PerdeuVida, e.TomarDano());
            Assert.AreEqual(2, e.Vidas);
            Assert.AreEqual(3, e.Coracoes);
        }

        [Test]
        public void NoveGolpesAcabamOJogo()
        {
            ResultadoDano r = ResultadoDano.Ignorado;
            for (int i = 0; i < 9; i++)
                r = e.TomarDano();
            Assert.AreEqual(ResultadoDano.FimDeJogo, r);
            Assert.AreEqual(0, e.Vidas);
            Assert.AreEqual(ResultadoJogo.Derrota, e.Resultado);
            Assert.AreEqual(ResultadoDano.Ignorado, e.TomarDano(), "depois do fim nada mais acontece");
        }

        [Test]
        public void CaneleiraDeOuroSeguraUmGolpe()
        {
            e.GanharCarta(CartaPoder.CaneleiraDeOuro);
            Assert.IsTrue(e.Escudo);
            Assert.AreEqual(ResultadoDano.Bloqueado, e.TomarDano());
            Assert.AreEqual(3, e.Coracoes);
            Assert.IsFalse(e.Escudo, "só o próximo dano");
            Assert.AreEqual(ResultadoDano.PerdeuCoracao, e.TomarDano());
        }

        [Test]
        public void CairNoCanalTiraCoracaoMesmoComCaneleira()
        {
            e.GanharCarta(CartaPoder.CaneleiraDeOuro);
            Assert.AreEqual(ResultadoDano.PerdeuCoracao, e.CairNaAgua());
            Assert.AreEqual(2, e.Coracoes);
            Assert.IsTrue(e.Escudo);
        }

        [Test]
        public void CoracaoDoCaixoteRecupera()
        {
            Assert.IsFalse(e.RecuperarCoracao(), "já está cheio");
            e.TomarDano();
            Assert.IsTrue(e.RecuperarCoracao());
            Assert.AreEqual(3, e.Coracoes);
        }

        [Test]
        public void CheckpointSoAvanca()
        {
            Assert.IsTrue(e.AtivarCheckpoint(2));
            Assert.IsFalse(e.AtivarCheckpoint(1));
            Assert.IsFalse(e.AtivarCheckpoint(2));
            Assert.AreEqual(2, e.Checkpoint);
        }

        [Test]
        public void AcertarOQuizDaACartaEEncheATorcida()
        {
            PerguntaQuiz q = BancoQuiz.Surpresa1958();
            RespostaQuiz r = e.Responder(q, q.Correta);
            Assert.IsTrue(r.Acertou);
            Assert.AreEqual(Recompensa.CartaChuteDoCanhao, r.Ganhou);
            Assert.AreEqual(CartaPoder.ChuteDoCanhao, e.Carta);
            Assert.AreEqual(ValoresTorcida.QuizCerto, e.Torcida.Valor, 0.0001f);
            Assert.AreEqual(1, e.QuizzesAcertados);
        }

        [Test]
        public void ErrarOQuizNaoTiraVidaNemDaRecompensa()
        {
            PerguntaQuiz q = BancoQuiz.Surpresa1958();
            RespostaQuiz r = e.Responder(q, 0);
            Assert.IsFalse(r.Acertou);
            Assert.AreEqual(Recompensa.Nenhuma, r.Ganhou);
            Assert.AreEqual(3, e.Vidas);
            Assert.AreEqual(3, e.Coracoes);
            Assert.AreEqual(CartaPoder.Nenhuma, e.Carta);
            Assert.IsTrue(e.QuizRespondido(q.Id));
            Assert.AreEqual(0, e.QuizzesAcertados);
        }

        [Test]
        public void CadaQuizSoValeUmaVez()
        {
            PerguntaQuiz q = BancoQuiz.Estadio1958();
            e.Responder(q, 0);
            RespostaQuiz segunda = e.Responder(q, q.Correta);
            Assert.AreEqual(Recompensa.Nenhuma, segunda.Ganhou);
            Assert.IsFalse(e.Escudo);
        }

        [Test]
        public void QuizDoEstadioDaACaneleira()
        {
            PerguntaQuiz q = BancoQuiz.Estadio1958();
            e.Responder(q, q.Correta);
            Assert.IsTrue(e.Escudo);
            Assert.AreEqual(CartaPoder.Nenhuma, e.Carta, "a caneleira já vale na hora (não ocupa o espaço da carta)");
        }

        [Test]
        public void UsarACartaEsvaziaOEspaco()
        {
            e.GanharCarta(CartaPoder.ChuteDoCanhao);
            Assert.AreEqual(CartaPoder.ChuteDoCanhao, e.UsarCarta());
            Assert.AreEqual(CartaPoder.Nenhuma, e.Carta);
            Assert.AreEqual(CartaPoder.Nenhuma, e.UsarCarta());
        }

        [Test]
        public void TresGolsVencemAFinal()
        {
            Assert.IsFalse(e.MarcarGol());
            Assert.IsFalse(e.MarcarGol());
            Assert.IsTrue(e.MarcarGol());
            Assert.AreEqual(ResultadoJogo.Vitoria, e.Resultado);
            Assert.AreEqual(EstadoPartida.GolsParaVencer, e.Gols);
            Assert.IsFalse(e.MarcarGol(), "depois da vitória não conta mais");
        }

        [Test]
        public void AcertosSeguidosFormamCombo()
        {
            Assert.AreEqual(1, e.RegistrarAcerto());
            e.AvancarTempo(1f);
            Assert.AreEqual(2, e.RegistrarAcerto());
            e.AvancarTempo(EstadoPartida.JanelaCombo + 0.5f);
            Assert.AreEqual(1, e.RegistrarAcerto(), "demorou demais: o combo recomeça");
            Assert.AreEqual(2, e.MaiorCombo);
        }

        [Test]
        public void TomarDanoQuebraOCombo()
        {
            e.RegistrarAcerto();
            e.RegistrarAcerto();
            e.TomarDano();
            Assert.AreEqual(0, e.ComboAtual);
        }

        [Test]
        public void NovoJogoZeraTudo()
        {
            e.TomarDano();
            e.GanharCarta(CartaPoder.ChuteDoCanhao);
            e.AtivarCheckpoint(3);
            e.MarcarGol();
            e.Responder(BancoQuiz.Surpresa1958(), 1);
            e.NovoJogo();
            Assert.AreEqual(3, e.Coracoes);
            Assert.AreEqual(0, e.Checkpoint);
            Assert.AreEqual(0, e.Gols);
            Assert.AreEqual(CartaPoder.Nenhuma, e.Carta);
            Assert.AreEqual(0, e.QuizzesRespondidos);
            Assert.AreEqual(0f, e.Torcida.Valor, 0.0001f);
        }
    }
}
