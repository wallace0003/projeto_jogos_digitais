using NUnit.Framework;

namespace CincoEstrelas.Testes
{
    /// <summary>Zagueiro carrinho, capitão e goleiro final (GDD, pág. 8).</summary>
    [TestFixture]
    public class TestesAdversarios
    {
        static void Rodar(CerebroCarrinho c, float segundos, bool vendo)
        {
            for (float t = 0f; t < segundos; t += 0.02f)
                c.Atualizar(0.02f, vendo);
        }

        static void Rodar(CerebroCapitao c, float segundos)
        {
            for (float t = 0f; t < segundos; t += 0.02f)
                c.Atualizar(0.02f);
        }

        // ------------------------------------------------------------ carrinho
        [Test]
        public void CarrinhoAvisaAntesDeDeslizar()
        {
            var c = new CerebroCarrinho();
            Rodar(c, 0.1f, false);
            Assert.AreEqual(EstadoCarrinho.Patrulha, c.Estado);
            c.Atualizar(0.02f, true);
            Assert.AreEqual(EstadoCarrinho.Preparando, c.Estado, "o ataque é telegráfico");
            Rodar(c, CerebroCarrinho.TempoPreparo + 0.05f, true);
            Assert.AreEqual(EstadoCarrinho.Deslizando, c.Estado);
            Assert.IsTrue(c.Rasteiro);
            Assert.IsTrue(c.PodeMachucar);
        }

        [Test]
        public void DepoisDoCarrinhoEleFicaNoChaoSemMachucar()
        {
            var c = new CerebroCarrinho();
            c.Atualizar(0.02f, true);
            Rodar(c, CerebroCarrinho.TempoPreparo + CerebroCarrinho.TempoDeslize + 0.05f, false);
            Assert.AreEqual(EstadoCarrinho.Recuperando, c.Estado);
            Assert.IsFalse(c.PodeMachucar);
            Rodar(c, CerebroCarrinho.TempoRecuperacao + 0.05f, false);
            Assert.AreEqual(EstadoCarrinho.Patrulha, c.Estado);
        }

        [Test]
        public void CarrinhoDescansaAntesDoProximoAtaque()
        {
            var c = new CerebroCarrinho();
            c.Atualizar(0.02f, true);
            Rodar(c, CerebroCarrinho.TempoPreparo + CerebroCarrinho.TempoDeslize + CerebroCarrinho.TempoRecuperacao + 0.1f, false);
            c.Atualizar(0.02f, true);
            Assert.AreEqual(EstadoCarrinho.Patrulha, c.Estado);
            Rodar(c, CerebroCarrinho.Descanso, true);
            Assert.AreEqual(EstadoCarrinho.Preparando, c.Estado);
        }

        [Test]
        public void ChutarAntesDoContatoCancelaOCarrinho()
        {
            var c = new CerebroCarrinho();
            c.Atualizar(0.02f, true);
            Assert.AreEqual(ResultadoGolpe.Acertou, c.ReceberChute(1));
            Assert.AreEqual(EstadoCarrinho.Atingido, c.Estado);
            Assert.AreEqual(ResultadoGolpe.Superou, c.ReceberChute(1));
            Assert.IsTrue(c.Superado);
            Assert.IsFalse(c.PodeMachucar);
        }

        [Test]
        public void ChuteForteSuperaOCarrinhoDeUmaVez()
        {
            var c = new CerebroCarrinho();
            Assert.AreEqual(ResultadoGolpe.Superou, c.ReceberChute(Chutes.Parametros(TipoChute.Forte).Dano));
        }

        [Test]
        public void CarrinhoDistraidoNaoAtaca()
        {
            var c = new CerebroCarrinho();
            c.Distrair(2f);
            Rodar(c, 1f, true);
            Assert.AreEqual(EstadoCarrinho.Distraido, c.Estado);
            Assert.IsFalse(c.PodeMachucar);
        }

        // ------------------------------------------------------------ capitão
        [Test]
        public void CapitaoEsperaOJogadorChegar()
        {
            var c = new CerebroCapitao();
            Rodar(c, 3f);
            Assert.AreEqual(EstadoCapitao.Aguardando, c.Estado);
            c.Despertar();
            Assert.AreEqual(EstadoCapitao.Avancando, c.Estado);
        }

        [Test]
        public void CapitaoAlternaCarrinhoESalto()
        {
            var c = new CerebroCapitao();
            c.Despertar();
            Rodar(c, CerebroCapitao.TempoAvancar + 0.05f);
            Assert.AreEqual(EstadoCapitao.PreparandoCarrinho, c.Estado);
            Rodar(c, CerebroCapitao.TempoPreparoCarrinho + CerebroCapitao.TempoCarrinho + 0.05f);
            Assert.AreEqual(EstadoCapitao.RecuperandoCarrinho, c.Estado);
            Rodar(c, CerebroCapitao.TempoRecuperacaoCarrinho + CerebroCapitao.TempoAvancar + 0.05f);
            Assert.AreEqual(EstadoCapitao.PreparandoSalto, c.Estado);
            Rodar(c, CerebroCapitao.TempoPreparoSalto + 0.05f);
            Assert.AreEqual(EstadoCapitao.Saltando, c.Estado);
            Rodar(c, 2f);
            Assert.AreEqual(EstadoCapitao.Saltando, c.Estado, "o salto só termina quando ele toca o chão");
            c.Aterrissou();
            Assert.AreEqual(EstadoCapitao.RecuperandoSalto, c.Estado);
            Rodar(c, CerebroCapitao.TempoRecuperacaoSalto + CerebroCapitao.TempoAvancar + 0.05f);
            Assert.AreEqual(EstadoCapitao.PreparandoCarrinho, c.Estado, "depois do salto volta o carrinho");
        }

        [Test]
        public void CapitaoDefendeForaDaRecuperacao()
        {
            var c = new CerebroCapitao();
            c.Despertar();
            Assert.IsFalse(c.Vulneravel);
            Assert.AreEqual(ResultadoGolpe.Defendeu, c.ReceberChute(3, false));
            Assert.AreEqual(CerebroCapitao.VidaMaxima, c.Vida);
        }

        [Test]
        public void CapitaoLevaGolNaRecuperacao()
        {
            var c = new CerebroCapitao();
            c.Despertar();
            Rodar(c, CerebroCapitao.TempoAvancar + CerebroCapitao.TempoPreparoCarrinho + CerebroCapitao.TempoCarrinho + 0.1f);
            Assert.IsTrue(c.Vulneravel);
            Assert.AreEqual(ResultadoGolpe.Acertou, c.ReceberChute(2, false));
            Assert.AreEqual(CerebroCapitao.VidaMaxima - 2, c.Vida);
        }

        [Test]
        public void CanhaoEEspecialPassamPelaDefesaDoCapitao()
        {
            var c = new CerebroCapitao();
            c.Despertar();
            Assert.AreEqual(ResultadoGolpe.Acertou, c.ReceberChute(3, true));
        }

        [Test]
        public void CapitaoDistraidoFicaVulneravel()
        {
            var c = new CerebroCapitao();
            c.Despertar();
            c.Distrair(2.5f);
            Assert.IsTrue(c.Vulneravel);
            Assert.IsFalse(c.PodeMachucar);
        }

        [Test]
        public void SeisDeDanoSuperamOCapitao()
        {
            var c = new CerebroCapitao();
            c.Despertar();
            c.Distrair(10f);
            Assert.AreEqual(ResultadoGolpe.Acertou, c.ReceberChute(3, false));
            Assert.AreEqual(ResultadoGolpe.Superou, c.ReceberChute(3, false));
            Assert.IsTrue(c.Superado);
            Assert.IsFalse(c.PodeMachucar);
        }

        // ------------------------------------------------------------ goleiro
        [Test]
        public void NaLinhaChuteFracoFazOGoleiroCair()
        {
            Assert.AreEqual(ReacaoGoleiro.Defendeu,
                RegrasGoleiro.Reagir(PosturaGoleiro.NaLinha, TipoChute.Fraco, ModificadorChute.Nenhum));
            Assert.AreEqual(ReacaoGoleiro.Defendeu,
                RegrasGoleiro.Reagir(PosturaGoleiro.NaLinha, TipoChute.Medio, ModificadorChute.Nenhum));
        }

        [Test]
        public void NaLinhaChuteForteEleEspalma()
        {
            Assert.AreEqual(ReacaoGoleiro.Espalmou,
                RegrasGoleiro.Reagir(PosturaGoleiro.NaLinha, TipoChute.Forte, ModificadorChute.Nenhum));
            Assert.AreEqual(ReacaoGoleiro.Espalmou,
                RegrasGoleiro.Reagir(PosturaGoleiro.NaLinha, TipoChute.Superchute, ModificadorChute.Nenhum));
        }

        [Test]
        public void ForaDoGolEleSeguraTudoQueChegaNele()
        {
            foreach (TipoChute t in System.Enum.GetValues(typeof(TipoChute)))
            {
                Assert.AreEqual(ReacaoGoleiro.Segurou,
                    RegrasGoleiro.Reagir(PosturaGoleiro.ForaDoGol, t, ModificadorChute.Nenhum), t.ToString());
                Assert.AreEqual(ReacaoGoleiro.Segurou,
                    RegrasGoleiro.Reagir(PosturaGoleiro.Saindo, t, ModificadorChute.Canhao), t.ToString());
            }
        }

        [Test]
        public void CaidoEleNaoDefendeNada()
        {
            Assert.AreEqual(ReacaoGoleiro.Nenhuma,
                RegrasGoleiro.Reagir(PosturaGoleiro.Caido, TipoChute.Fraco, ModificadorChute.Nenhum));
            Assert.AreEqual(ReacaoGoleiro.Nenhuma,
                RegrasGoleiro.Reagir(PosturaGoleiro.Levantando, TipoChute.Forte, ModificadorChute.Nenhum));
        }

        [Test]
        public void BolaDeFogoAtropelaOGoleiro()
        {
            Assert.AreEqual(ReacaoGoleiro.Atropelado,
                RegrasGoleiro.Reagir(PosturaGoleiro.NaLinha, TipoChute.Forte, ModificadorChute.Especial));
            Assert.AreEqual(ReacaoGoleiro.Atropelado,
                RegrasGoleiro.Reagir(PosturaGoleiro.ForaDoGol, TipoChute.Fraco, ModificadorChute.Especial));
        }

        [Test]
        public void ChuteFracoAbreOGolPorDoisSegundos()
        {
            var g = new CerebroGoleiro();
            Assert.AreEqual(ReacaoGoleiro.Defendeu, g.ReceberChute(TipoChute.Fraco, ModificadorChute.Nenhum));
            Assert.IsFalse(g.PodeDefender);
            for (float t = 0f; t < RegrasGoleiro.TempoCaido - 0.1f; t += 0.02f)
                g.Atualizar(0.02f, 10f);
            Assert.IsFalse(g.PodeDefender, "ainda no chão");
            EventoGoleiro ev = EventoGoleiro.Nenhum;
            for (float t = 0f; t < 1f && ev != EventoGoleiro.Levantou; t += 0.02f)
                ev = g.Atualizar(0.02f, 10f);
            Assert.AreEqual(EventoGoleiro.Levantou, ev);
            Assert.IsTrue(g.NaLinha);
        }

        [Test]
        public void ChuteForteNaLinhaNaoAbreOGol()
        {
            var g = new CerebroGoleiro();
            g.ReceberChute(TipoChute.Forte, ModificadorChute.Nenhum);
            Assert.IsTrue(g.PodeDefender, "espalmando ele continua de pé");
            for (int i = 0; i < 30; i++)
                g.Atualizar(0.02f, 10f);
            Assert.AreEqual(PosturaGoleiro.NaLinha, g.Postura);
        }

        [Test]
        public void GoleiroSaiDoGolQuandoOJogadorEstaLonge()
        {
            var g = new CerebroGoleiro();
            EventoGoleiro ev = EventoGoleiro.Nenhum;
            float t = 0f;
            while (ev != EventoGoleiro.IniciarSaida && t < 10f)
            {
                ev = g.Atualizar(0.02f, 8f);
                t += 0.02f;
            }
            Assert.AreEqual(EventoGoleiro.IniciarSaida, ev);
            Assert.AreEqual(PosturaGoleiro.Saindo, g.Postura);
            Assert.AreEqual(CerebroGoleiro.EsperaSaidaA, t, 0.1f);
            g.ChegouForaDoGol();
            Assert.AreEqual(PosturaGoleiro.ForaDoGol, g.Postura);
            ev = EventoGoleiro.Nenhum;
            for (float s = 0f; s < 3f && ev != EventoGoleiro.Voltar; s += 0.02f)
                ev = g.Atualizar(0.02f, 8f);
            Assert.AreEqual(EventoGoleiro.Voltar, ev);
            g.ChegouNaLinha();
            Assert.AreEqual(PosturaGoleiro.NaLinha, g.Postura);
        }

        [Test]
        public void GoleiroNaoSaiComOJogadorColado()
        {
            var g = new CerebroGoleiro();
            for (float t = 0f; t < 12f; t += 0.02f)
                Assert.AreNotEqual(EventoGoleiro.IniciarSaida, g.Atualizar(0.02f, 2f));
            Assert.IsTrue(g.NaLinha);
        }

        [Test]
        public void GoleiroArremessaBolasNaLinha()
        {
            var g = new CerebroGoleiro();
            int arremessos = 0;
            for (float t = 0f; t < 3.9f; t += 0.02f)
            {
                if (g.Atualizar(0.02f, 2f) == EventoGoleiro.Arremessar)
                    arremessos++;
            }
            Assert.AreEqual(1, arremessos);
        }

        [Test]
        public void CavadinhaPorCimaEntraQuandoEleEstaFora()
        {
            // a cavadinha passa por cima do goleiro fora do gol: a regra é física (ele não alcança),
            // mas se a bola chegar até ele, fora do gol ele segura
            var g = new CerebroGoleiro();
            for (float t = 0f; t < CerebroGoleiro.EsperaSaidaA + 0.1f; t += 0.02f)
                g.Atualizar(0.02f, 8f);
            g.ChegouForaDoGol();
            Assert.IsFalse(g.NaLinha);
            Assert.IsTrue(Chutes.Parametros(TipoChute.Cavadinha).AlturaNaDistancia(2.5f) > 2.5f,
                "a 2,5 tiles do pé a cavadinha já está acima da cabeça do goleiro");
        }
    }
}
