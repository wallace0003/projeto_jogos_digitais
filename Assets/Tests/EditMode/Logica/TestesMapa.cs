using System.Collections.Generic;
using NUnit.Framework;

namespace CincoEstrelas.Testes
{
    /// <summary>Leitura do mapa em texto.</summary>
    [TestFixture]
    public class TestesMapa
    {
        const string Pequeno =
            "// comentário\n" +
            "@nome Teste\n" +
            "@placa 3 Olá, mundo!\n" +
            ".........\n" +
            "..3....x.\n" +
            "####~~###\n" +
            "####%%###\n";

        [Test]
        public void LeDimensoesEDados()
        {
            MapaFase m = MapaFase.Ler(Pequeno);
            Assert.AreEqual(9, m.Largura);
            Assert.AreEqual(4, m.Altura);
            Assert.AreEqual("Teste", m.Nome);
            Assert.AreEqual("Olá, mundo!", m.TextoPlaca(3));
            Assert.IsNull(m.TextoPlaca(4));
        }

        [Test]
        public void YCresceParaCima()
        {
            MapaFase m = MapaFase.Ler(Pequeno);
            Assert.AreEqual('3', m.Em(2, 2));
            Assert.AreEqual('#', m.Em(0, 0));
            Assert.AreEqual('~', m.Em(4, 1));
            Assert.IsTrue(m.Solido(0, 1));
            Assert.IsFalse(m.Solido(4, 1));
            Assert.AreEqual('.', m.Em(-1, 0), "fora do mapa é vazio");
        }

        [Test]
        public void TopoDoChao()
        {
            MapaFase m = MapaFase.Ler(Pequeno);
            Assert.AreEqual(2, m.TopoSolido(0));
            Assert.AreEqual(-1, m.TopoSolido(4), "só água");
        }

        [Test]
        public void LinhasCurtasSaoCompletadasComVazio()
        {
            MapaFase m = MapaFase.Ler("....\n..\n####\n");
            Assert.AreEqual(4, m.Largura);
            Assert.AreEqual('.', m.Em(3, 1));
        }

        [Test]
        public void EncontraDaEsquerdaParaADireita()
        {
            MapaFase m = MapaFase.Ler("x...x\n..x..\n#####\n");
            List<Celula> cs = m.Encontrar('x');
            Assert.AreEqual(3, cs.Count);
            Assert.AreEqual(0, cs[0].X);
            Assert.AreEqual(2, cs[1].X);
            Assert.AreEqual(4, cs[2].X);
        }

        [Test]
        public void LimitesDoBarco()
        {
            MapaFase m = MapaFase.Ler("#<B........>#\n#~~~~~~~~~~~#\n");
            int esq, dir;
            Assert.IsTrue(m.LimitesBarco(m.Primeira('B'), out esq, out dir));
            Assert.AreEqual(1, esq);
            Assert.AreEqual(11, dir);
        }

        [Test]
        public void BarcoSemLimitesEhProblema()
        {
            MapaFase m = MapaFase.Ler("#.B.....#\n#~~~~~~~#\n");
            int esq, dir;
            Assert.IsFalse(m.LimitesBarco(m.Primeira('B'), out esq, out dir));
        }

        [Test]
        public void TrechoDaPonte()
        {
            MapaFase m = MapaFase.Ler("#..LLLL..#\n##########\n");
            int ini, fim;
            m.Trecho(4, 1, out ini, out fim);
            Assert.AreEqual(3, ini);
            Assert.AreEqual(6, fim);
        }

        [Test]
        public void MapaIncompletoListaOQueFalta()
        {
            List<string> p = MapaFase.Ler(Pequeno).Validar();
            Assert.Greater(p.Count, 5);
            bool falaDoGoleiro = false;
            foreach (string s in p)
                falaDoGoleiro |= s.Contains("goleiro");
            Assert.IsTrue(falaDoGoleiro);
        }

        [Test]
        public void HashEhFixo()
        {
            Assert.AreEqual(MapaFase.Hash(10, 3), MapaFase.Hash(10, 3));
            Assert.GreaterOrEqual(MapaFase.Hash(-5, 7), 0);
        }
    }
}
