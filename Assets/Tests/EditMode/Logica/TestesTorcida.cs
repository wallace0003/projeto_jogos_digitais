using NUnit.Framework;

namespace CincoEstrelas.Testes
{
    /// <summary>Barra da torcida (GDD, M10): premia variedade, pune repetição e libera o especial.</summary>
    [TestFixture]
    public class TestesTorcida
    {
        [Test]
        public void PrimeiraAcaoValeONormal()
        {
            var t = new Torcida();
            GanhoTorcida g = t.Registrar(AcoesTorcida.ChuteFraco, 10f);
            Assert.AreEqual(1f, g.Multiplicador, 0.0001f);
            Assert.AreEqual(10f, t.Valor, 0.0001f);
        }

        [Test]
        public void VariarDaBonus()
        {
            var t = new Torcida();
            t.Registrar(AcoesTorcida.ChuteFraco, 10f);
            GanhoTorcida g = t.Registrar(AcoesTorcida.Cavadinha, 10f);
            Assert.AreEqual(Torcida.BonusVariedade, g.Multiplicador, 0.0001f);
            Assert.AreEqual(15f, g.Ganho, 0.0001f);
        }

        [Test]
        public void RepetirRendeMenos()
        {
            var t = new Torcida();
            t.Registrar(AcoesTorcida.ChuteFraco, 10f);
            GanhoTorcida g = t.Registrar(AcoesTorcida.ChuteFraco, 10f);
            Assert.AreEqual(Torcida.PenalidadeRepeticao, g.Multiplicador, 0.0001f);
            Assert.AreEqual(5f, g.Ganho, 0.0001f);
        }

        [Test]
        public void AcaoRecenteMasNaoUltimaValeONormal()
        {
            var t = new Torcida();
            t.Registrar(AcoesTorcida.ChuteFraco, 1f);
            t.Registrar(AcoesTorcida.Cavadinha, 1f);
            Assert.AreEqual(1f, t.MultiplicadorPara(AcoesTorcida.ChuteFraco), 0.0001f);
        }

        [Test]
        public void VariarSempreEncheMaisRapidoQueRepetir()
        {
            var variando = new Torcida();
            var repetindo = new Torcida();
            string[] acoes = { AcoesTorcida.ChuteFraco, AcoesTorcida.Cavadinha, AcoesTorcida.Bicicleta, AcoesTorcida.Superchute };
            for (int i = 0; i < 8; i++)
            {
                variando.Registrar(acoes[i % acoes.Length], 8f);
                repetindo.Registrar(AcoesTorcida.ChuteFraco, 8f);
            }
            Assert.Greater(variando.Valor, repetindo.Valor * 2f);
        }

        [Test]
        public void BarraCheiaLiberaOEspecialUmaVez()
        {
            var t = new Torcida();
            GanhoTorcida g = t.Somar(99f);
            Assert.IsFalse(g.FicouPronta);
            g = t.Somar(5f);
            Assert.IsTrue(g.FicouPronta);
            Assert.AreEqual(Torcida.Maximo, t.Valor, 0.0001f, "a barra não passa do máximo");
            g = t.Somar(5f);
            Assert.IsFalse(g.FicouPronta, "só avisa na hora em que enche");
            Assert.IsTrue(t.EspecialPronto);
        }

        [Test]
        public void UsarOEspecialZeraABarra()
        {
            var t = new Torcida();
            Assert.IsFalse(t.UsarEspecial());
            t.Somar(Torcida.Maximo);
            Assert.IsTrue(t.UsarEspecial());
            Assert.AreEqual(0f, t.Valor, 0.0001f);
            Assert.IsFalse(t.EspecialPronto);
        }

        [Test]
        public void EmbaixadinhasTemLimitePorSequencia()
        {
            var t = new Torcida();
            for (int i = 0; i < 30; i++)
                t.Embaixadinha();
            Assert.AreEqual(Torcida.MaximoEmbaixadinhasPorSequencia * Torcida.PontosPorEmbaixadinha, t.Valor, 0.0001f);
            t.EncerrarSequenciaEmbaixadinhas();
            GanhoTorcida g = t.Embaixadinha();
            Assert.Greater(g.Ganho, 0f, "uma nova sequência volta a render");
        }

        [Test]
        public void QuizEGolNaoContamComoRepeticao()
        {
            var t = new Torcida();
            t.Registrar(AcoesTorcida.ChuteFraco, 4f);
            t.Somar(ValoresTorcida.QuizCerto);
            Assert.AreEqual(Torcida.PenalidadeRepeticao, t.MultiplicadorPara(AcoesTorcida.ChuteFraco), 0.0001f);
        }
    }
}
