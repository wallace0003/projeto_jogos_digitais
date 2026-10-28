using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CincoEstrelas.Testes
{
    /// <summary>
    /// Testes jogando de verdade (Play Mode): carregam as cenas e conferem a
    /// montagem da fase, o chão, o sino e a ponte, os barris, os quizzes, o
    /// capitão e o portão, o goleiro, os gols, a vitória, a derrota e a tela
    /// inicial. Rodam em Window &gt; General &gt; Test Runner &gt; PlayMode.
    /// </summary>
    public class TestesJogabilidade
    {
        [UnitySetUp]
        public IEnumerator Preparar()
        {
            Time.timeScale = 1f;
            // espera uma troca de cena que tenha sobrado de outro teste
            float limite = 5f;
            while (Transicao.Carregando && limite > 0f)
            {
                limite -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        [TearDown]
        public void Finalizar()
        {
            Time.timeScale = 1f;
        }

        /// <summary>Carrega a fase já sem a abertura do narrador.</summary>
        static IEnumerator CarregarFase()
        {
            Sessao.NovoJogo();
            Sessao.IntroVista = true;
            yield return SceneManager.LoadSceneAsync(Cenas.Fase);
            yield return null;
            yield return new WaitForFixedUpdate();
            Assert.IsNotNull(GerenciadorFase.Instancia, "a fase não montou o GerenciadorFase");
            Assert.IsNotNull(Jogador.Atual, "a fase não criou o jogador");
        }

        static IEnumerator EsperarAte(Func<bool> condicao, float segundos)
        {
            float t = 0f;
            while (!condicao() && t < segundos)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        /// <summary>Põe o jogador em pé no chão, em x, olhando para a direita.</summary>
        static IEnumerator Teleportar(float x)
        {
            MapaFase m = GerenciadorFase.Instancia.Fase.Mapa;
            float y = m.TopoSolido(Mathf.FloorToInt(x));
            Jogador.Atual.Renascer(new Vector3(x, y + 0.02f, 0f));
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return EsperarAte(delegate { return Jogador.Atual.NoChao; }, 1f);
        }

        static EstadoPartida E
        {
            get { return Sessao.Estado; }
        }

        [UnityTest]
        public IEnumerator Fase_MontaJogadorBolaAdversariosEObjetos()
        {
            yield return CarregarFase();
            ConstrutorFase f = GerenciadorFase.Instancia.Fase;
            Assert.IsNotNull(Jogador.Atual.Bola);
            Assert.IsTrue(Jogador.Atual.Bola.NoPe, "a bola começa no pé");
            Assert.GreaterOrEqual(Inimigo.Todos.Count, 4, "2 carrinhos, 1 saltador e o capitão");
            Assert.IsNotNull(Goleiro.Atual);
            Assert.IsNotNull(f.Sino);
            Assert.IsNotNull(f.Ponte);
            Assert.IsFalse(f.Ponte.Baixada, "a ponte começa levantada");
            Assert.IsNotNull(f.Barris);
            Assert.IsNotNull(f.Portao);
            Assert.IsNotNull(f.Capitao);
            Assert.IsNotNull(f.Gol);
            Assert.GreaterOrEqual(f.Checkpoints.Count, 3);
            Assert.AreEqual(2, f.Atos.Count);
        }

        [UnityTest]
        public IEnumerator Jogador_FicaEmCimaDoChao()
        {
            yield return CarregarFase();
            float y = Jogador.Atual.transform.position.y;
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(Jogador.Atual.NoChao, "o Camisa 10 precisa estar apoiado no chão");
            Assert.AreEqual(y, Jogador.Atual.transform.position.y, 0.1f, "o Camisa 10 não pode atravessar o chão");
        }

        [UnityTest]
        public IEnumerator Cavadinha_TocaOSino_EAPonteBaixa()
        {
            yield return CarregarFase();
            ConstrutorFase f = GerenciadorFase.Instancia.Fase;
            Sino sino = f.Sino;
            // a cavadinha passa pela altura do sino ~2,3 tiles depois do pé (subindo)
            float x = sino.transform.position.x - 2.26f - ParametrosJogador.DistanciaBolaNoPe;
            yield return Teleportar(x);
            Assert.IsTrue(Jogador.Atual.Chutar(TipoChute.Cavadinha));
            yield return EsperarAte(delegate { return sino.Tocado; }, 3f);
            Assert.IsTrue(sino.Tocado, "a cavadinha precisa alcançar o sino");
            yield return EsperarAte(delegate { return f.Ponte.Baixada; }, 4f);
            Assert.IsTrue(f.Ponte.Baixada, "o sino baixa a ponte levadiça");
        }

        [UnityTest]
        public IEnumerator ChuteRasteiro_NaoAlcancaOSino()
        {
            yield return CarregarFase();
            Sino sino = GerenciadorFase.Instancia.Fase.Sino;
            yield return Teleportar(sino.transform.position.x - 4f);
            Assert.IsTrue(Jogador.Atual.Chutar(TipoChute.Forte));
            yield return new WaitForSeconds(1.5f);
            Assert.IsFalse(sino.Tocado, "só a bola alta toca o sino");
        }

        [UnityTest]
        public IEnumerator Barris_SoCaemComCargaMaxima()
        {
            yield return CarregarFase();
            Obstaculo barris = GerenciadorFase.Instancia.Fase.Barris;
            float x = barris.transform.position.x - 3.5f;
            yield return Teleportar(x);
            Bola bola = Jogador.Atual.Bola;
            Assert.IsTrue(Jogador.Atual.Chutar(TipoChute.Medio));
            yield return EsperarAte(delegate { return bola.NoPe; }, 4f);
            Assert.IsFalse(barris.Destruido, "chute médio não derruba a pilha");
            yield return Teleportar(x);
            Assert.IsTrue(Jogador.Atual.Chutar(TipoChute.Forte));
            yield return EsperarAte(delegate { return barris == null || barris.Destruido; }, 2f);
            Assert.IsTrue(barris == null || barris.Destruido, "o chute com carga máxima derruba a pilha");
        }

        [UnityTest]
        public IEnumerator PrimeiroCarrinhoSuperado_AbreOQuizSurpresa()
        {
            yield return CarregarFase();
            ZagueiroCarrinho carrinho = null;
            foreach (Inimigo i in Inimigo.Todos)
            {
                ZagueiroCarrinho c = i as ZagueiroCarrinho;
                if (c != null && (carrinho == null || c.transform.position.x < carrinho.transform.position.x))
                    carrinho = c;
            }
            Assert.IsNotNull(carrinho);
            GerenciadorFase.Instancia.AdversarioAtingido(carrinho, Jogador.Atual.Bola, ResultadoGolpe.Superou);
            PainelQuiz quiz = GerenciadorFase.Instancia.Quiz;
            yield return EsperarAte(delegate { return quiz.Aberto; }, 3f);
            Assert.IsTrue(quiz.Aberto, "o quiz surpresa abre depois do primeiro carrinho");
            Assert.AreEqual(BancoQuiz.IdSurpresa1958, quiz.Pergunta.Id);
            Assert.AreEqual(0f, Time.timeScale, 0.0001f, "o jogo para durante o quiz");
        }

        [UnityTest]
        public IEnumerator QuizCerto_DaAChuteDoCanhao()
        {
            yield return CarregarFase();
            GerenciadorFase g = GerenciadorFase.Instancia;
            Assert.IsTrue(g.AbrirQuiz(BancoQuiz.Surpresa1958()));
            g.Quiz.Responder(BancoQuiz.Surpresa1958().Correta);
            Assert.IsTrue(g.Quiz.Respondido);
            Assert.IsTrue(g.Quiz.UltimaResposta.Acertou);
            Assert.AreEqual(CartaPoder.ChuteDoCanhao, E.Carta);
            g.Quiz.Continuar();
            yield return null;
            Assert.IsFalse(g.Quiz.Aberto);
            Assert.AreEqual(1f, Time.timeScale, 0.0001f, "o jogo volta depois do quiz");
            g.UsarPoder();
            Assert.Greater(Jogador.Atual.CanhaoRestante, 0f, "Espaço ativa o Chute do Canhão");
            Assert.AreEqual(CartaPoder.Nenhuma, E.Carta);
        }

        [UnityTest]
        public IEnumerator QuizErrado_NaoTiraVida()
        {
            yield return CarregarFase();
            GerenciadorFase g = GerenciadorFase.Instancia;
            g.AbrirQuiz(BancoQuiz.Estadio1958());
            g.Quiz.Responder(0);
            Assert.IsFalse(g.Quiz.UltimaResposta.Acertou);
            Assert.AreEqual(EstadoPartida.VidasIniciais, E.Vidas);
            Assert.AreEqual(EstadoPartida.CoracoesPorVida, E.Coracoes);
            Assert.IsFalse(E.Escudo);
            g.Quiz.Continuar();
            yield return null;
            Assert.IsFalse(g.AbrirQuiz(BancoQuiz.Estadio1958()), "cada pergunta aparece uma vez só");
        }

        [UnityTest]
        public IEnumerator CapitaoSuperado_AbreOPortao()
        {
            yield return CarregarFase();
            ConstrutorFase f = GerenciadorFase.Instancia.Fase;
            Assert.IsFalse(f.Portao.Destruido, "o portão começa fechado");
            GerenciadorFase.Instancia.AdversarioAtingido(f.Capitao, Jogador.Atual.Bola, ResultadoGolpe.Superou);
            Assert.IsTrue(f.Portao.Destruido, "superar o capitão abre o portão");
            yield return new WaitForSeconds(2f);
            Assert.IsFalse(f.Portao.gameObject.activeSelf, "o portão sobe e sai do caminho");
        }

        [UnityTest]
        public IEnumerator CairNoCanal_TiraCoracaoEVoltaAoCheckpoint()
        {
            yield return CarregarFase();
            Vector3 inicio = GerenciadorFase.Instancia.PontoRenascer;
            // o primeiro canal do ato 1 (entre as docas)
            Jogador.Atual.transform.position = new Vector3(25.5f, 4.6f, 0f);
            yield return EsperarAte(delegate { return E.Coracoes < EstadoPartida.CoracoesPorVida; }, 3f);
            Assert.AreEqual(EstadoPartida.CoracoesPorVida - 1, E.Coracoes);
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(inicio.x, Jogador.Atual.transform.position.x, 0.5f, "volta para o último checkpoint");
        }

        [UnityTest]
        public IEnumerator Goleiro_ChuteFracoDerruba_EOSegundoChuteEhGol()
        {
            yield return CarregarFase();
            GerenciadorFase g = GerenciadorFase.Instancia;
            ConstrutorFase f = g.Fase;
            g.EntrarNaArena();
            Goleiro goleiro = Goleiro.Atual;
            yield return Teleportar(f.Gol.LinhaDoGol - 6.5f);
            Bola bola = Jogador.Atual.Bola;
            Assert.IsTrue(Jogador.Atual.Chutar(TipoChute.Fraco));
            yield return EsperarAte(delegate { return goleiro.Cerebro.Postura == PosturaGoleiro.Caido; }, 2f);
            Assert.AreEqual(PosturaGoleiro.Caido, goleiro.Cerebro.Postura, "o chute fraco faz o goleiro mergulhar e cair");
            yield return EsperarAte(delegate { return bola.NoPe; }, 3f);
            Assert.IsTrue(bola.NoPe);
            Assert.IsTrue(Jogador.Atual.Chutar(TipoChute.Medio));
            yield return EsperarAte(delegate { return E.Gols > 0; }, 2f);
            Assert.AreEqual(1, E.Gols, "com o goleiro caído, o chute entra");
        }

        [UnityTest]
        public IEnumerator TresGols_LevamAVitoria()
        {
            yield return CarregarFase();
            GerenciadorFase g = GerenciadorFase.Instancia;
            g.EntrarNaArena();
            for (int i = 0; i < EstadoPartida.GolsParaVencer; i++)
                g.Gol(Jogador.Atual.Bola);
            Assert.AreEqual(ResultadoJogo.Vitoria, E.Resultado);
            yield return EsperarAte(delegate { return SceneManager.GetActiveScene().name == Cenas.FimDeJogo; }, 12f);
            Assert.AreEqual(Cenas.FimDeJogo, SceneManager.GetActiveScene().name);
            yield return null;
            Assert.IsNotNull(TelaFinal.Atual);
            Assert.IsTrue(TelaFinal.Atual.Vitoria, "a tela final mostra a vitória (a estrela acesa)");
            Assert.IsNotNull(TelaFinal.Atual.BotaoJogarDeNovo);
        }

        [UnityTest]
        public IEnumerator SemVidas_FimDeJogo()
        {
            yield return CarregarFase();
            GerenciadorFase g = GerenciadorFase.Instancia;
            for (int i = 0; i < EstadoPartida.VidasIniciais * EstadoPartida.CoracoesPorVida; i++)
                g.JogadorFerido();
            Assert.AreEqual(ResultadoJogo.Derrota, E.Resultado);
            yield return EsperarAte(delegate { return SceneManager.GetActiveScene().name == Cenas.FimDeJogo; }, 10f);
            yield return null;
            Assert.IsNotNull(TelaFinal.Atual);
            Assert.IsFalse(TelaFinal.Atual.Vitoria);
        }

        [UnityTest]
        public IEnumerator Companheiro_FazTabelaEDevolveABola()
        {
            yield return CarregarFase();
            Bola bola = Jogador.Atual.Bola;
            float torcidaAntes = E.Torcida.Valor;
            Assert.IsTrue(GerenciadorFase.Instancia.ChamarCompanheiro());
            yield return EsperarAte(delegate { return bola.NaTabela; }, 4f);
            Assert.IsTrue(bola.NaTabela, "o companheiro recebe o passe");
            yield return EsperarAte(delegate { return bola.NoPe; }, 4f);
            Assert.IsTrue(bola.NoPe, "o companheiro devolve a bola");
            Assert.Greater(E.Torcida.Valor, torcidaAntes, "a tabela anima a torcida");
            Assert.IsFalse(GerenciadorFase.Instancia.ChamarCompanheiro(), "ele precisa de um tempo para voltar");
        }

        [UnityTest]
        public IEnumerator TelaInicial_TemComoJogarEBotaoJogar()
        {
            Sessao.Encerrar();
            yield return SceneManager.LoadSceneAsync(Cenas.Menu);
            yield return null;
            MenuPrincipal menu = MenuPrincipal.Atual;
            Assert.IsNotNull(menu);
            Assert.IsNotNull(menu.BotaoJogar);
            var textos = new System.Collections.Generic.List<string>();
            foreach (TextoPixel t in menu.Tela.GetComponentsInChildren<TextoPixel>(true))
                textos.Add(t.Texto);
            foreach (string necessario in new[] { Textos.ComoJogar, Textos.Objetivo, Textos.ComoGanhar, Textos.ComoPerder })
                Assert.IsTrue(textos.Contains(necessario), "a tela inicial precisa mostrar " + necessario);
            menu.BotaoJogar.onClick.Invoke();
            yield return EsperarAte(delegate { return SceneManager.GetActiveScene().name == Cenas.Fase; }, 6f);
            Assert.AreEqual(Cenas.Fase, SceneManager.GetActiveScene().name, "JOGAR abre a fase");
        }

        [UnityTest]
        public IEnumerator DarPlayNaFaseSemOMenu_AbreATelaInicial()
        {
            Sessao.Encerrar();
            yield return SceneManager.LoadSceneAsync(Cenas.Fase);
            yield return null;
            yield return null;
            Assert.AreEqual(Cenas.Menu, SceneManager.GetActiveScene().name, "o jogo sempre começa pela tela inicial");
        }
    }
}
