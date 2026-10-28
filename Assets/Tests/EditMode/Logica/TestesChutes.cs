using NUnit.Framework;

namespace CincoEstrelas.Testes
{
    /// <summary>Os chutes da pág. 4 do GDD: carga em 3 níveis, superchute, cavadinha, bicicleta e modificadores.</summary>
    [TestFixture]
    public class TestesChutes
    {
        [Test]
        public void ToqueRapidoEhCargaUm()
        {
            var c = new ControleChute();
            c.Apertar();
            c.Atualizar(0.05f);
            Assert.AreEqual(1, c.Nivel);
            Assert.AreEqual(TipoChute.Fraco, c.Soltar());
            Assert.IsFalse(c.Segurando);
        }

        [Test]
        public void SegurarUmPoucoEhCargaDois()
        {
            var c = new ControleChute();
            c.Apertar();
            c.Atualizar(0.4f);
            Assert.AreEqual(2, c.Nivel);
            Assert.AreEqual(TipoChute.Medio, c.Soltar());
        }

        [Test]
        public void SegurarAteOFimEhCargaMaxima()
        {
            var c = new ControleChute();
            c.Apertar();
            for (int i = 0; i < 40; i++)
                c.Atualizar(0.02f);
            Assert.AreEqual(3, c.Nivel);
            Assert.AreEqual(1f, c.Carga01, 0.0001f);
            Assert.AreEqual(TipoChute.Forte, c.Soltar());
        }

        [Test]
        public void CargaSobeAosPoucos()
        {
            var c = new ControleChute();
            Assert.AreEqual(0f, c.Carga01, 0.0001f);
            c.Apertar();
            c.Atualizar(ControleChute.TempoNivel3 * 0.5f);
            Assert.AreEqual(0.5f, c.Carga01, 0.01f);
        }

        [Test]
        public void SoChuteForteESuperchuteDerrubamBarris()
        {
            Assert.IsFalse(Chutes.Parametros(TipoChute.Fraco).Pesado);
            Assert.IsFalse(Chutes.Parametros(TipoChute.Medio).Pesado);
            Assert.IsFalse(Chutes.Parametros(TipoChute.Cavadinha).Pesado);
            Assert.IsFalse(Chutes.Parametros(TipoChute.Bicicleta).Pesado);
            Assert.IsTrue(Chutes.Parametros(TipoChute.Forte).Pesado);
            Assert.IsTrue(Chutes.Parametros(TipoChute.Superchute).Pesado);
        }

        [Test]
        public void ForcaCresceComACarga()
        {
            Assert.Less(Chutes.Parametros(TipoChute.Fraco).VelocidadeX, Chutes.Parametros(TipoChute.Medio).VelocidadeX);
            Assert.Less(Chutes.Parametros(TipoChute.Medio).VelocidadeX, Chutes.Parametros(TipoChute.Forte).VelocidadeX);
            Assert.Less(Chutes.Parametros(TipoChute.Forte).VelocidadeX, Chutes.Parametros(TipoChute.Superchute).VelocidadeX);
            Assert.Less(Chutes.Parametros(TipoChute.Fraco).Dano, Chutes.Parametros(TipoChute.Forte).Dano);
        }

        [Test]
        public void CavadinhaEhAltaECurta()
        {
            ParametrosChute cav = Chutes.Parametros(TipoChute.Cavadinha);
            ParametrosChute medio = Chutes.Parametros(TipoChute.Medio);
            Assert.Greater(cav.AlturaMaxima, 4f, "a cavadinha precisa passar por cima do goleiro e alcançar o sino");
            Assert.Less(medio.AlturaMaxima, 1f, "o chute rasteiro fica baixo");
            Assert.Less(cav.VelocidadeX, medio.VelocidadeX);
        }

        [Test]
        public void BicicletaAtravessaDoisEDesceNaDiagonal()
        {
            ParametrosChute b = Chutes.Parametros(TipoChute.Bicicleta);
            Assert.AreEqual(2, b.Atravessa);
            Assert.Less(b.VelocidadeY, 0f);
        }

        [Test]
        public void ChuteDoCanhaoAtravessaTodos()
        {
            ParametrosChute p = Chutes.Parametros(TipoChute.Fraco, ModificadorChute.Canhao);
            Assert.AreEqual(Chutes.AtravessaTudo, p.Atravessa);
            Assert.IsTrue(p.Pesado);
            Assert.IsFalse(p.Imparavel);
        }

        [Test]
        public void EspecialEhImparavel()
        {
            ParametrosChute p = Chutes.Parametros(TipoChute.Medio, ModificadorChute.Especial);
            Assert.IsTrue(p.Imparavel);
            Assert.AreEqual(Chutes.AtravessaTudo, p.Atravessa);
            Assert.GreaterOrEqual(p.Dano, 5);
        }

        [Test]
        public void CavadinhaNaoUsaCartaNemEspecial()
        {
            ParametrosChute normal = Chutes.Parametros(TipoChute.Cavadinha);
            ParametrosChute especial = Chutes.Parametros(TipoChute.Cavadinha, ModificadorChute.Especial);
            Assert.AreEqual(normal.VelocidadeY, especial.VelocidadeY, 0.0001f);
            Assert.IsFalse(especial.Imparavel);
            Assert.IsFalse(Chutes.AceitaModificador(TipoChute.Cavadinha));
        }

        [Test]
        public void CorrendoOChuteSaiMaisForte()
        {
            ParametrosChute p = Chutes.Parametros(TipoChute.Medio);
            float vx, vy, parado;
            Chutes.VelocidadeInicial(p, 1, 0f, out parado, out vy);
            Chutes.VelocidadeInicial(p, 1, 8f, out vx, out vy);
            Assert.Greater(vx, parado);
            // correndo para trás não "puxa" a bola
            Chutes.VelocidadeInicial(p, 1, -8f, out vx, out vy);
            Assert.AreEqual(parado, vx, 0.0001f);
            Chutes.VelocidadeInicial(p, -1, 0f, out vx, out vy);
            Assert.AreEqual(-parado, vx, 0.0001f);
        }

        [Test]
        public void PedaladaAbreJanelaDoSuperchute()
        {
            var j = new JanelaSuperchute();
            Assert.IsFalse(j.Consumir());
            j.Abrir();
            j.Atualizar(JanelaSuperchute.Duracao * 0.5f);
            Assert.IsTrue(j.Ativa);
            Assert.IsTrue(j.Consumir());
            Assert.IsFalse(j.Ativa, "a janela vale para um chute só");
        }

        [Test]
        public void JanelaDoSuperchuteFecha()
        {
            var j = new JanelaSuperchute();
            j.Abrir();
            j.Atualizar(JanelaSuperchute.Duracao + 0.01f);
            Assert.IsFalse(j.Consumir());
        }

        [Test]
        public void CadaChuteTemUmaAcaoDiferenteNaTorcida()
        {
            var vistos = new System.Collections.Generic.HashSet<string>();
            foreach (TipoChute t in System.Enum.GetValues(typeof(TipoChute)))
                Assert.IsTrue(vistos.Add(Chutes.Acao(t, ModificadorChute.Nenhum)), "ação repetida: " + t);
            Assert.AreEqual(AcoesTorcida.Canhao, Chutes.Acao(TipoChute.Forte, ModificadorChute.Canhao));
            Assert.AreEqual(AcoesTorcida.Cavadinha, Chutes.Acao(TipoChute.Cavadinha, ModificadorChute.Canhao));
        }
    }
}
