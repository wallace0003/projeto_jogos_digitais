using NUnit.Framework;

namespace CincoEstrelas.Testes
{
    /// <summary>A bola: só chuta do pé, volta sozinha, atravessa adversários e faz tabela.</summary>
    [TestFixture]
    public class TestesEstadoBola
    {
        [Test]
        public void ComecaNoPe()
        {
            var b = new EstadoBola();
            Assert.IsTrue(b.NoPe);
            Assert.IsTrue(b.PodeChutar);
            Assert.IsFalse(b.PodeAcertar);
        }

        [Test]
        public void SoChutaComABolaNoPe()
        {
            var b = new EstadoBola();
            Assert.IsTrue(b.Chutar(TipoChute.Medio, ModificadorChute.Nenhum));
            Assert.AreEqual(SituacaoBola.Voando, b.Situacao);
            Assert.IsFalse(b.Chutar(TipoChute.Forte, ModificadorChute.Nenhum));
            Assert.AreEqual(TipoChute.Medio, b.UltimoChute);
        }

        [Test]
        public void VoltaSozinhaDepoisDeUmTempo()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Fraco, ModificadorChute.Nenhum);
            float t = 0f;
            while (b.Situacao == SituacaoBola.Voando && t < 5f)
            {
                b.AtualizarVoo(0.02f, 6f, 10f, false);
                t += 0.02f;
            }
            Assert.AreEqual(SituacaoBola.Voltando, b.Situacao);
            Assert.LessOrEqual(t, Chutes.Parametros(TipoChute.Fraco).TempoMaximo + 0.05f);
        }

        [Test]
        public void BolaParadaVoltaLogo()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Medio, ModificadorChute.Nenhum);
            for (int i = 0; i < 25; i++)
                b.AtualizarVoo(0.02f, 0.1f, 10f, false);
            Assert.AreEqual(SituacaoBola.Voltando, b.Situacao);
        }

        [Test]
        public void CaiuNaAguaVolta()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Medio, ModificadorChute.Nenhum);
            b.AtualizarVoo(0.02f, 10f, 5f, true);
            Assert.AreEqual(SituacaoBola.Voltando, b.Situacao);
        }

        [Test]
        public void BolaLentaPertoDoPeEhDominada()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Fraco, ModificadorChute.Nenhum);
            b.AtualizarVoo(0.1f, 2f, 0.5f, false);
            Assert.AreEqual(SituacaoBola.Voando, b.Situacao, "logo depois do chute não domina sem querer");
            b.AtualizarVoo(0.3f, 2f, 0.5f, false);
            Assert.IsTrue(b.NoPe);
        }

        [Test]
        public void VoltandoChegaNoPe()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Fraco, ModificadorChute.Nenhum);
            b.Chamar();
            b.AtualizarVolta(0.1f, 3f);
            Assert.AreEqual(SituacaoBola.Voltando, b.Situacao);
            b.AtualizarVolta(0.1f, 0.2f);
            Assert.IsTrue(b.NoPe);
        }

        [Test]
        public void VoltaCadaVezMaisRapido()
        {
            Assert.Less(EstadoBola.VelocidadeVolta(0f), EstadoBola.VelocidadeVolta(0.5f));
            Assert.AreEqual(34f, EstadoBola.VelocidadeVolta(10f), 0.0001f);
        }

        [Test]
        public void ChuteNormalRicocheteiaNoPrimeiro()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Medio, ModificadorChute.Nenhum);
            Assert.IsFalse(b.RegistrarAcerto());
            Assert.AreEqual(1, b.AcertosNoChute);
        }

        [Test]
        public void BicicletaAtravessaDois()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Bicicleta, ModificadorChute.Nenhum);
            Assert.IsTrue(b.RegistrarAcerto());
            Assert.IsTrue(b.RegistrarAcerto());
            Assert.IsFalse(b.RegistrarAcerto());
        }

        [Test]
        public void CanhaoAtravessaTodos()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Fraco, ModificadorChute.Canhao);
            for (int i = 0; i < 10; i++)
                Assert.IsTrue(b.RegistrarAcerto());
            Assert.AreEqual(ModificadorChute.Canhao, b.UltimoModificador);
        }

        [Test]
        public void CavadinhaIgnoraOModificador()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Cavadinha, ModificadorChute.Especial);
            Assert.AreEqual(ModificadorChute.Nenhum, b.UltimoModificador);
        }

        [Test]
        public void TabelaSoComABolaNoPe()
        {
            var b = new EstadoBola();
            Assert.IsTrue(b.IniciarTabela());
            Assert.IsFalse(b.PodeChutar);
            Assert.IsFalse(b.PodeAcertar);
            Assert.IsFalse(b.IniciarTabela());
            b.TerminarTabela();
            Assert.IsTrue(b.NoPe);
            b.Chutar(TipoChute.Fraco, ModificadorChute.Nenhum);
            Assert.IsFalse(b.IniciarTabela());
        }
    }
}
