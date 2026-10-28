using NUnit.Framework;

namespace CincoEstrelas.Testes
{
    /// <summary>Números do pulo e a fonte pixelada (todo texto do jogo precisa ter letra).</summary>
    [TestFixture]
    public class TestesFisicaETextos
    {
        [Test]
        public void PuloAlcancaQuaseQuatroTiles()
        {
            float h = ParametrosJogador.AlturaMaximaPulo;
            Assert.Greater(h, 3.5f);
            Assert.Less(h, 4f);
        }

        [Test]
        public void CorrendoPulaMaisLonge()
        {
            float andando = ParametrosJogador.AlcancePulo(ParametrosJogador.VelocidadeAndando, 0f);
            float correndo = ParametrosJogador.AlcancePulo(ParametrosJogador.VelocidadeCorrendo, 0f);
            Assert.Greater(correndo, andando);
            Assert.Greater(andando, 4f, "andando ainda dá para pular um canal de 3 tiles");
        }

        [Test]
        public void PularParaBaixoVaiMaisLongeQueParaCima()
        {
            float v = ParametrosJogador.VelocidadeAndando;
            Assert.Greater(ParametrosJogador.AlcancePulo(v, -3f), ParametrosJogador.AlcancePulo(v, 0f));
            Assert.Greater(ParametrosJogador.AlcancePulo(v, 0f), ParametrosJogador.AlcancePulo(v, 2f));
        }

        [Test]
        public void NaoAlcancaAlturaMaiorQueOPulo()
        {
            Assert.AreEqual(-1f, ParametrosJogador.TempoNoAr(ParametrosJogador.AlturaMaximaPulo + 0.5f), 0.0001f);
            Assert.AreEqual(0f, ParametrosJogador.AlcancePulo(6f, 10f), 0.0001f);
        }

        [Test]
        public void TodosOsTextosTemLetraNaFonte()
        {
            foreach (string t in Textos.Todos())
            {
                char faltando;
                Assert.IsTrue(GlifosFonte.TextoSuportado(t, out faltando),
                    "falta a letra '" + faltando + "' (U+" + ((int)faltando).ToString("X4") + ") em: " + t);
            }
        }

        [Test]
        public void GlifosTemAlturaELarguraCertas()
        {
            foreach (var par in GlifosFonte.Todos())
            {
                Assert.AreEqual(GlifosFonte.AlturaGlifo, par.Value.Length, "altura do glifo " + par.Key);
                int largura = par.Value[0].Length;
                Assert.Greater(largura, 0);
                Assert.LessOrEqual(largura, 5, "largura do glifo " + par.Key);
                foreach (string linha in par.Value)
                    Assert.AreEqual(largura, linha.Length, "linhas com larguras diferentes no glifo " + par.Key);
            }
        }

        [Test]
        public void AcentosFicamAcimaDaLetra()
        {
            string[] a = GlifosFonte.Todos()['a'];
            string[] aAgudo = GlifosFonte.Todos()['á'];
            string[] A = GlifosFonte.Todos()['A'];
            string[] AAgudo = GlifosFonte.Todos()['Á'];
            Assert.AreEqual(a[GlifosFonte.LinhaTopo + 4], aAgudo[GlifosFonte.LinhaTopo + 4], "o corpo da letra não muda");
            Assert.IsTrue(aAgudo[GlifosFonte.LinhaTopo].Contains("#"), "acento da minúscula acima do corpo");
            Assert.IsFalse(A[0].Contains("#"));
            Assert.IsTrue(AAgudo[0].Contains("#") || AAgudo[1].Contains("#"), "acento da maiúscula acima da letra");
        }

        [Test]
        public void NormalizaCaracteresParecidos()
        {
            Assert.AreEqual('é', GlifosFonte.Normalizar('è'));
            Assert.AreEqual('-', GlifosFonte.Normalizar('—'));
            Assert.AreEqual('?', GlifosFonte.Normalizar('€'));
            Assert.AreEqual(' ', GlifosFonte.Normalizar(' '));
        }

        [Test]
        public void TempoFormatado()
        {
            Assert.AreEqual("0:00", Textos.FormatarTempo(0f));
            Assert.AreEqual("1:05", Textos.FormatarTempo(65.9f));
            Assert.AreEqual("12:00", Textos.FormatarTempo(720f));
        }
    }
}
