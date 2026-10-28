using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CincoEstrelas
{
    /// <summary>
    /// Regras da fase durante o jogo: abertura do narrador, atos, dano e
    /// vidas, canal, checkpoints, placas, quizzes, barra da torcida, combos,
    /// companheiro, poderes, sino, capitão e portão, arena final, gols,
    /// pausa, vitória e derrota. As regras puras ficam em <see cref="EstadoPartida"/>.
    /// </summary>
    public class GerenciadorFase : MonoBehaviour
    {
        public static GerenciadorFase Instancia { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Instancia = null;
        }

        /// <summary>Segundos até poder chamar o companheiro de novo.</summary>
        public const float RecargaCompanheiro = 9f;

        ConstrutorFase fase;
        Jogador jogador;
        Hud hud;
        PainelQuiz quiz;
        PainelNarrador narrador;
        MenuPausa pausa;
        Vector3 pontoRenascer;
        int atoAtual;
        bool arenaAtiva;
        float recargaCompanheiro;
        Placa dicaAtual;
        bool terminou;
        bool primeiroCarrinhoSuperado;
        int sorteioNarrador;

        static EstadoPartida E
        {
            get { return Sessao.Estado; }
        }

        /// <summary>Abaixo disso o jogador (ou a bola) caiu do mapa.</summary>
        public float LimiteQueda
        {
            get { return -3f; }
        }

        public bool Terminou
        {
            get { return terminou; }
        }

        public bool ArenaAtiva
        {
            get { return arenaAtiva; }
        }

        /// <summary>Algum painel parou o jogo (abertura, quiz ou pausa).</summary>
        public bool Bloqueado
        {
            get { return (narrador != null && narrador.Aberto) || (quiz != null && quiz.Aberto) || (pausa != null && pausa.Aberto); }
        }

        public PainelQuiz Quiz
        {
            get { return quiz; }
        }

        public PainelNarrador Narrador
        {
            get { return narrador; }
        }

        public ConstrutorFase Fase
        {
            get { return fase; }
        }

        public Jogador Jogador
        {
            get { return jogador; }
        }

        public Vector3 PontoRenascer
        {
            get { return pontoRenascer; }
        }

        public void Iniciar(ConstrutorFase construtor, Jogador j)
        {
            Instancia = this;
            fase = construtor;
            jogador = j;
            pontoRenascer = j.transform.position;
            atoAtual = 0;

            FabricaUI.GarantirEventSystem();
            hud = Hud.Criar();
            quiz = PainelQuiz.Criar();
            narrador = PainelNarrador.Criar();
            pausa = MenuPausa.Criar(this);

            SomJogo.Musica(Trilha.Ato1);
            Transicao.AoEntrarNaCena();
            hud.AtualizarTudo(E, jogador);

            if (!Sessao.IntroVista)
            {
                Sessao.IntroVista = true;
                AbrirIntro();
            }
            else
            {
                MostrarAto(0);
            }
        }

        void OnDestroy()
        {
            if (Instancia == this)
                Instancia = null;
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (Entrada.MudoApertado)
                SomJogo.AlternarMudo();
            if (narrador.Aberto || quiz.Aberto)
                return; // os painéis cuidam das próprias teclas
            if (!terminou && !Transicao.Carregando && Entrada.PausaApertada)
                AlternarPausa();
            if (pausa.Aberto || Time.timeScale <= 0f)
                return;

            float dt = Time.deltaTime;
            E.AvancarTempo(dt);
            recargaCompanheiro -= dt;
            ChecarAtos();
            if (jogador != null)
                hud.Atualizar(E, jogador, 1f - Mathf.Clamp01(recargaCompanheiro / RecargaCompanheiro), arenaAtiva);
        }

        // ------------------------------------------------------------ abertura e atos
        void AbrirIntro()
        {
            Time.timeScale = 0f;
            SomJogo.AbafarMusica(true);
            narrador.Mostrar(Textos.IntroAno, Textos.IntroTitulo, Textos.IntroTexto, Textos.IntroContinuar, FecharIntro);
        }

        void FecharIntro()
        {
            Time.timeScale = 1f;
            SomJogo.AbafarMusica(false);
            SomJogo.Tocar(Som.Apito);
            MostrarAto(0);
        }

        /// <summary>Usado pelos testes: fecha a abertura do narrador na hora.</summary>
        public void PularIntro()
        {
            if (narrador.Aberto)
                narrador.Fechar();
        }

        void ChecarAtos()
        {
            if (jogador == null || fase == null)
                return;
            float x = jogador.transform.position.x;
            while (atoAtual < fase.Atos.Count && x >= fase.Atos[atoAtual])
            {
                atoAtual++;
                MostrarAto(atoAtual);
                if (atoAtual == 1)
                    SomJogo.Musica(Trilha.Ato2);
                else if (atoAtual == 2)
                    SomJogo.Tocar(Som.Torcida, 0.8f);
            }
        }

        void MostrarAto(int indice)
        {
            if (indice >= 0 && indice < Textos.Atos.Length)
                hud.Banner(Textos.Atos[indice], Textos.SubtitulosAtos[indice]);
        }

        // ------------------------------------------------------------ dano, canal e checkpoints
        /// <summary>Um adversário (ou a bola do goleiro) acertou o jogador.</summary>
        public ResultadoDano JogadorFerido()
        {
            if (terminou)
                return ResultadoDano.Ignorado;
            ResultadoDano r = E.TomarDano();
            switch (r)
            {
                case ResultadoDano.Bloqueado:
                    Mensagem(Textos.CaneleiraSegurou, FabricaUI.CorDestaque, 1.6f);
                    break;
                case ResultadoDano.PerdeuCoracao:
                    hud.PiscarCoracoes();
                    break;
                case ResultadoDano.PerdeuVida:
                    SomJogo.Tocar(Som.Dano);
                    Mensagem(Textos.PerdeuVida, FabricaUI.CorVermelha, 1.8f);
                    Renascer();
                    break;
                case ResultadoDano.FimDeJogo:
                    StartCoroutine(Derrota());
                    break;
            }
            hud.AtualizarTudo(E, jogador);
            return r;
        }

        /// <summary>Caiu no canal: perde 1 coração e volta ao último checkpoint.</summary>
        public void JogadorCaiuNaAgua()
        {
            if (terminou || jogador == null || !jogador.Vivo)
                return;
            ResultadoDano r = E.CairNaAgua();
            hud.PiscarCoracoes();
            if (r == ResultadoDano.FimDeJogo)
            {
                StartCoroutine(Derrota());
            }
            else
            {
                Mensagem(r == ResultadoDano.PerdeuVida ? Textos.PerdeuVida : Textos.CaiuNoCanal, FabricaUI.CorVermelha, 1.4f);
                Renascer();
            }
            hud.AtualizarTudo(E, jogador);
        }

        void Renascer()
        {
            jogador.Renascer(pontoRenascer);
            foreach (Inimigo ini in Inimigo.Todos.ToArray())
            {
                if (ini != null)
                    ini.Reiniciar();
            }
            if (arenaAtiva && Goleiro.Atual != null)
                Goleiro.Atual.Reiniciar();
        }

        public void CheckpointAtivado(Bandeira b)
        {
            if (arenaAtiva)
                return;
            pontoRenascer = b.PontoRenascer;
            if (E.AtivarCheckpoint(b.indice))
                Mensagem(Textos.CheckpointAtivado, FabricaUI.CorVerde, 1.2f);
        }

        public void CoracaoColetado(Vector3 pos)
        {
            if (E.RecuperarCoracao())
            {
                SomJogo.Tocar(Som.Coracao);
                Efeitos.TextoSubindo(Textos.Coracao, pos + Vector3.up * 0.5f, FabricaUI.CorVermelha);
            }
            else
            {
                SomJogo.Tocar(Som.Coracao, 0.6f);
                MostrarGanho(E.Torcida.Somar(5f), pos + Vector3.up * 0.5f);
            }
            hud.AtualizarTudo(E, jogador);
        }

        // ------------------------------------------------------------ placas
        public void MostrarDica(Placa p)
        {
            dicaAtual = p;
            hud.Dica(p.texto);
        }

        public void EsconderDica(Placa p)
        {
            if (dicaAtual != p)
                return;
            dicaAtual = null;
            hud.Dica(null);
        }

        // ------------------------------------------------------------ jogadas e torcida
        public void ChuteFeito(TipoChute tipo, ModificadorChute mod)
        {
            if (tipo == TipoChute.Superchute || tipo == TipoChute.Bicicleta ||
                (Chutes.AceitaModificador(tipo) && mod != ModificadorChute.Nenhum))
                Mensagem(Narradores.Chute(tipo, mod), FabricaUI.CorDestaque, 0.9f);
        }

        public void Pedalada()
        {
            MostrarGanho(E.Torcida.Registrar(AcoesTorcida.Pedalada, ValoresTorcida.Pedalada), jogador.Centro + Vector3.up * 1.2f);
            hud.AtualizarTudo(E, jogador);
        }

        public void Embaixadinha(int toques, Vector3 pos)
        {
            GanhoTorcida g = E.Torcida.Embaixadinha();
            if (g.Ganho > 0f)
                Efeitos.TextoSubindo(toques.ToString(), pos, FabricaUI.CorTexto);
            Avisar(g);
        }

        public void FimDasEmbaixadinhas()
        {
            E.Torcida.EncerrarSequenciaEmbaixadinhas();
        }

        /// <summary>A bola chegou num adversário (zagueiros ou capitão).</summary>
        public void AdversarioAtingido(Inimigo ini, Bola bola, ResultadoGolpe r)
        {
            if (r == ResultadoGolpe.Defendeu)
            {
                if (ini is Capitao)
                    Mensagem(Textos.CapitaoDefendeu, FabricaUI.CorVermelha, 1.4f);
                return;
            }
            SomJogo.Tocar(Som.Acerto, 0.8f);
            string acao = Chutes.Acao(bola.UltimoChute, bola.UltimoModificador);
            float valor = r == ResultadoGolpe.Superou ? ValoresTorcida.AdversarioSuperado : ValoresTorcida.AcertoAdversario;
            MostrarGanho(E.Torcida.Registrar(acao, valor), (Vector3)ini.Centro + Vector3.up * (ini.Altura * 0.5f + 1f));
            int combo = E.RegistrarAcerto();
            if (combo >= 2)
                hud.Combo(combo);
            if (combo == 3)
                Avisar(E.Torcida.Somar(ValoresTorcida.ComboTres));
            if (r == ResultadoGolpe.Superou)
            {
                E.RegistrarAdversarioSuperado();
                Mensagem(Narradores.Superou(sorteioNarrador++), FabricaUI.CorVerde, 1.2f);
                if (ini is ZagueiroCarrinho && !primeiroCarrinhoSuperado)
                {
                    primeiroCarrinhoSuperado = true;
                    PerguntaQuiz q = BancoQuiz.Surpresa1958();
                    if (!E.QuizRespondido(q.Id))
                        StartCoroutine(AbrirQuizDepois(q, 0.9f));
                }
                if (ini is Capitao)
                    CapitaoSuperado();
            }
            hud.AtualizarTudo(E, jogador);
        }

        public void CapitaoDespertou(Capitao c)
        {
            SomJogo.Tocar(Som.Apito, 0.6f);
            hud.Banner(Textos.CapitaoChegou, Textos.SubtitulosAtos[1]);
        }

        void CapitaoSuperado()
        {
            if (fase.Portao != null)
                fase.Portao.Abrir();
            SomJogo.Tocar(Som.Torcida, 0.6f);
            Mensagem(Textos.PortaoAberto, FabricaUI.CorVerde, 2.5f);
        }

        public void SinoTocado(Sino s)
        {
            Mensagem(Textos.SinoTocou, FabricaUI.CorDestaque, 2f);
            MostrarGanho(E.Torcida.Registrar(AcoesTorcida.Sino, ValoresTorcida.Sino), s.transform.position + Vector3.up);
            hud.AtualizarTudo(E, jogador);
        }

        public void ObstaculoQuebrado(Obstaculo o)
        {
            Vector3 pos = o.transform.position + Vector3.up * 1.5f;
            if (o.tipo == Obstaculo.Tipo.Barris)
            {
                Mensagem(Textos.BarrisDerrubados, FabricaUI.CorVerde, 1.5f);
                MostrarGanho(E.Torcida.Registrar(AcoesTorcida.Barris, ValoresTorcida.Barris), pos);
            }
            else
            {
                MostrarGanho(E.Torcida.Registrar(AcoesTorcida.Caixote, ValoresTorcida.Caixote), pos);
            }
            hud.AtualizarTudo(E, jogador);
        }

        void MostrarGanho(GanhoTorcida g, Vector3 pos)
        {
            if (g.Ganho > 0.01f)
            {
                string t = "+" + Mathf.RoundToInt(g.Ganho);
                Color cor = FabricaUI.CorTexto;
                if (g.Multiplicador > 1.01f)
                {
                    t += " " + Textos.Variedade;
                    cor = FabricaUI.CorDestaque;
                }
                else if (g.Multiplicador < 0.99f)
                {
                    t += " " + Textos.Repetido;
                    cor = FabricaUI.CorApagada;
                }
                Efeitos.TextoSubindo(t, pos, cor);
            }
            Avisar(g);
        }

        /// <summary>Avisa quando a barra da torcida acabou de encher.</summary>
        void Avisar(GanhoTorcida g)
        {
            if (!g.FicouPronta)
                return;
            SomJogo.Tocar(Som.Torcida);
            Mensagem(Textos.EspecialPronto, FabricaUI.CorDestaque, 2.5f);
        }

        // ------------------------------------------------------------ companheiro
        /// <summary>Q: chama o companheiro (se ele já voltou). Devolve true se ele veio.</summary>
        public bool ChamarCompanheiro()
        {
            if (terminou || jogador == null || !jogador.Vivo)
                return false;
            if (recargaCompanheiro > 0f || Companheiro.Atual != null)
            {
                Mensagem(Textos.CompanheiroDescansando, FabricaUI.CorApagada, 1f);
                return false;
            }
            CameraSeguir cam = CameraSeguir.Atual;
            float x;
            if (cam != null)
                x = jogador.Direcao > 0 ? cam.BordaEsquerda - 1f : cam.BordaDireita + 1f;
            else
                x = jogador.transform.position.x - jogador.Direcao * 12f;
            Companheiro.Chamar(jogador, x);
            recargaCompanheiro = RecargaCompanheiro;
            SomJogo.Tocar(Som.Apito, 0.4f);
            return true;
        }

        public void CompanheiroChegou(int distraidos, bool fezTabela, Vector3 pos)
        {
            if (distraidos > 0)
                Mensagem(Textos.Distraidos, FabricaUI.CorAzul, 1.4f);
        }

        public void TabelaConcluida(Vector3 pos)
        {
            Efeitos.TextoSubindo(Textos.Tabela, pos + Vector3.up * 1.2f, FabricaUI.CorVerde);
            MostrarGanho(E.Torcida.Registrar(AcoesTorcida.Tabela, ValoresTorcida.Tabela), pos + Vector3.up * 0.6f);
            hud.AtualizarTudo(E, jogador);
        }

        // ------------------------------------------------------------ poderes
        /// <summary>Espaço: a bola de fogo da torcida (se a barra estiver cheia) ou a carta guardada.</summary>
        public void UsarPoder()
        {
            if (terminou || jogador == null || !jogador.Vivo || jogador.EspecialArmado)
                return;
            if (E.Torcida.UsarEspecial())
            {
                jogador.ArmarEspecial();
                SomJogo.Tocar(Som.Especial);
                CameraSeguir.Tremer(0.15f, 0.3f);
                Mensagem(Textos.EspecialArmado, new Color(1f, 0.55f, 0.2f), 2f);
            }
            else if (E.Carta == CartaPoder.ChuteDoCanhao)
            {
                E.UsarCarta();
                jogador.AtivarCanhao(Poderes.DuracaoCanhao);
                SomJogo.Tocar(Som.Carta);
                Mensagem(Textos.CanhaoAtivo, FabricaUI.CorDestaque, 1.8f);
            }
            else
            {
                SomJogo.Tocar(Som.Defesa, 0.3f);
            }
            hud.AtualizarTudo(E, jogador);
        }

        // ------------------------------------------------------------ quizzes
        IEnumerator AbrirQuizDepois(PerguntaQuiz q, float espera)
        {
            yield return new WaitForSeconds(espera);
            while (Bloqueado || terminou)
            {
                if (terminou)
                    yield break;
                yield return null;
            }
            AbrirQuiz(q);
        }

        /// <summary>Para o jogo e mostra a pergunta (cada pergunta só aparece uma vez).</summary>
        public bool AbrirQuiz(PerguntaQuiz q)
        {
            if (terminou || q == null || E.QuizRespondido(q.Id) || quiz.Aberto)
                return false;
            Time.timeScale = 0f;
            SomJogo.AbafarMusica(true);
            SomJogo.Tocar(Som.QuizAbre);
            quiz.Mostrar(q, ResponderQuiz, FecharQuiz);
            return true;
        }

        RespostaQuiz ResponderQuiz(int alternativa)
        {
            RespostaQuiz r = E.Responder(quiz.Pergunta, alternativa);
            SomJogo.Tocar(r.Acertou ? Som.QuizCerto : Som.QuizErrado);
            if (r.Ganhou != Recompensa.Nenhuma)
                SomJogo.Tocar(Som.Carta, 0.8f);
            hud.AtualizarTudo(E, jogador);
            return r;
        }

        void FecharQuiz()
        {
            Time.timeScale = 1f;
            SomJogo.AbafarMusica(false);
            if (E.Torcida.EspecialPronto)
                Mensagem(Textos.EspecialPronto, FabricaUI.CorDestaque, 2.5f);
            hud.AtualizarTudo(E, jogador);
        }

        // ------------------------------------------------------------ gatilhos e arena
        public bool GatilhoAtingido(Gatilho g)
        {
            if (terminou)
                return false;
            switch (g.tipo)
            {
                case Gatilho.Tipo.QuizEstadio:
                    PerguntaQuiz q = BancoQuiz.Estadio1958();
                    if (!E.QuizRespondido(q.Id))
                        AbrirQuiz(q);
                    return true;
                case Gatilho.Tipo.Arena:
                    EntrarNaArena();
                    return true;
            }
            return false;
        }

        /// <summary>A final: trava a câmera e a parede atrás do jogador, e o goleiro começa a jogar.</summary>
        public void EntrarNaArena()
        {
            if (arenaAtiva)
                return;
            arenaAtiva = true;
            pontoRenascer = new Vector3(fase.XArena + 2f, fase.ChaoArena, 0f);
            fase.FecharArena();
            if (CameraSeguir.Atual != null)
                CameraSeguir.Atual.limites = new Rect(fase.XArena, 0f, fase.Largura - fase.XArena, fase.Altura + 4f);
            hud.Banner(Textos.ArenaFinal, Textos.SubtitulosAtos[2]);
            SomJogo.Musica(Trilha.Final);
            SomJogo.Tocar(Som.Apito);
            if (Goleiro.Atual != null)
                Goleiro.Atual.Ativar();
            hud.AtualizarTudo(E, jogador);
        }

        // ------------------------------------------------------------ gols e fim
        public void ChuteNoGol()
        {
            E.RegistrarChuteNoGol();
        }

        /// <summary>A bola entrou no gol.</summary>
        public void Gol(Bola bola)
        {
            if (terminou)
                return;
            bool venceu = E.MarcarGol();
            SomJogo.Tocar(Som.Gol);
            CameraSeguir.Tremer(0.3f, 0.4f);
            Vector3 centro = fase.Gol != null ? fase.Gol.Centro : bola.transform.position;
            Efeitos.Confete(centro + Vector3.up * 2f, 30);
            AnelExpansivo.Criar(centro, 3f, new Color(1f, 0.9f, 0.4f, 0.9f), 0.6f);
            Mensagem(Textos.Gol + "  " + string.Format(Textos.FormatoGols, E.Gols, EstadoPartida.GolsParaVencer),
                FabricaUI.CorDestaque, 2f);
            if (Goleiro.Atual != null)
                Goleiro.Atual.SofreuGol();
            hud.AtualizarTudo(E, jogador);
            if (venceu)
                StartCoroutine(Vitoria());
            else
                StartCoroutine(DevolverBola(bola));
        }

        IEnumerator DevolverBola(Bola bola)
        {
            yield return new WaitForSeconds(0.7f);
            if (bola != null)
                bola.Chamar();
        }

        IEnumerator Vitoria()
        {
            terminou = true;
            jogador.Comemorar();
            SomJogo.Musica(Trilha.Vitoria, false);
            SomJogo.Tocar(Som.Torcida);
            for (int i = 0; i < 6; i++)
            {
                Efeitos.Confete(jogador.transform.position + new Vector3(Random.Range(-6f, 6f), 7f, 0f), 18);
                yield return new WaitForSeconds(0.45f);
            }
            yield return new WaitForSeconds(0.8f);
            Sessao.Finalizar();
            Transicao.Carregar(Cenas.FimDeJogo);
        }

        IEnumerator Derrota()
        {
            terminou = true;
            jogador.Morrer();
            SomJogo.Musica(Trilha.Nenhuma);
            Mensagem(Textos.DerrotaTitulo, FabricaUI.CorVermelha, 3f);
            yield return new WaitForSeconds(2f);
            Sessao.Finalizar();
            Transicao.Carregar(Cenas.FimDeJogo);
        }

        // ------------------------------------------------------------ pausa
        public void AlternarPausa()
        {
            bool pausar = !pausa.Aberto;
            pausa.Mostrar(pausar);
            Time.timeScale = pausar ? 0f : 1f;
            SomJogo.AbafarMusica(pausar);
        }

        /// <summary>Recomeça a fase do zero (sem a abertura do narrador).</summary>
        public void ReiniciarFase()
        {
            Time.timeScale = 1f;
            Sessao.NovoJogo();
            Sessao.IntroVista = true;
            Transicao.Carregar(SceneManager.GetActiveScene().name);
        }

        public void IrParaMenu()
        {
            Time.timeScale = 1f;
            Sessao.Encerrar();
            Transicao.Carregar(Cenas.Menu);
        }

        public void Mensagem(string texto, Color cor, float duracao)
        {
            if (hud != null)
                hud.Mensagem(texto, cor, duracao);
        }
    }
}
