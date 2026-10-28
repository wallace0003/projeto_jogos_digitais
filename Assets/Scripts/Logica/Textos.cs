namespace CincoEstrelas
{
    /// <summary>
    /// Todos os textos fixos do jogo (português). Ficam juntos para facilitar a
    /// revisão e para o teste conferir se a fonte pixelada tem todas as letras.
    /// Os textos das placas ficam no próprio mapa (linhas "@placa").
    /// </summary>
    public static class Textos
    {
        // ------------------------------------------------------------ menu
        public const string Titulo = "CINCO ESTRELAS";
        public const string Subtitulo = "A Jornada do Brasil";
        public const string FaseDoMenu = "Fase 1 · Suécia 1958 · O início da lenda";
        public const string ComoJogar = "COMO JOGAR";
        public const string Objetivo = "OBJETIVO";
        public const string ComoGanhar = "COMO GANHAR";
        public const string ComoPerder = "COMO PERDER";
        public const string TextoObjetivo =
            "Leve a bola pelas docas de Estocolmo, supere a marcação e decida no estádio Råsunda.";
        public const string TextoComoGanhar = "Marque 3 gols no goleiro sueco e acenda a primeira estrela.";
        public const string TextoComoPerder =
            "Cada golpe tira 1 coração e cair no canal também. Sem corações, perde uma vida; sem as 3 vidas, fim de jogo.";
        public const string Jogar = "JOGAR";
        public const string Sair = "SAIR";
        public const string CreditosPadrao = "Protótipo da disciplina de Jogos Digitais · arte, música e código originais";

        /// <summary>Tabela de controles (GDD, pág. 4): tecla e ação, em pares.</summary>
        public static readonly string[] Controles =
        {
            "A / D  ou  ← / →", "Andar",
            "W  ou  ↑", "Pular",
            "SHIFT", "Correr",
            "X", "Chutar (segure: 3 níveis de carga)",
            "C", "Cavadinha (bola alta)",
            "Z", "Pedalada: o próximo chute vira superchute",
            "X no ar", "Bicicleta",
            "S  ou  ↓ parado", "Embaixadinhas",
            "Q", "Chamar o companheiro",
            "ESPAÇO", "Carta de poder ou especial da torcida",
            "E / ENTER", "Interagir e responder quizzes",
            "P / ESC   ·   M", "Pausa   ·   Som",
        };

        // ------------------------------------------------------------ abertura e atos
        public const string IntroAno = "1958 · SUÉCIA";
        public const string IntroTitulo = "O início da lenda";
        public const string IntroTexto =
            "Estocolmo, 1958. A primeira estrela está do outro lado dos canais. " +
            "Conduza a bola pelas docas, supere a marcação e decida no estádio.";
        public const string IntroContinuar = "ENTER para começar";
        public const string Narrador = "Narrador";

        public static readonly string[] Atos = { "ATO 1 · CHEGADA", "ATO 2 · CAMINHO", "ATO 3 · DECISÃO" };
        public static readonly string[] SubtitulosAtos =
        {
            "Docas e canais de Estocolmo", "As ruas de Gamla Stan", "O estádio Råsunda",
        };

        // ------------------------------------------------------------ HUD
        public const string Vidas = "VIDAS";
        public const string Torcida = "TORCIDA";
        public const string Poder = "PODER";
        public const string Carga = "CARGA";
        public const string Espaco = "ESPAÇO";
        public const string Companheiro = "Q";
        public const string FormatoCombo = "COMBO x{0}";
        public const string FormatoGols = "GOLS {0}/{1}";
        public const string EspecialPronto = "ESPECIAL PRONTO! Aperte ESPAÇO";
        public const string EspecialArmado = "BOLA DE FOGO! Chute agora!";
        public const string Superchute = "SUPERCHUTE PRONTO!";

        // ------------------------------------------------------------ mensagens da fase
        public const string CheckpointAtivado = "Checkpoint!";
        public const string SinoTocou = "Blém! A ponte está baixando!";
        public const string SinoPrecisaCavadinha = "Só a cavadinha alcança o sino!";
        public const string BarrisPrecisamCarga = "Precisa de carga máxima! Segure X.";
        public const string BarrisDerrubados = "Caminho livre!";
        public const string PortaoFechado = "O portão só abre depois do capitão.";
        public const string PortaoAberto = "Capitão superado! O portão abriu.";
        public const string CapitaoDefendeu = "DEFENDEU! Ataque na recuperação.";
        public const string CaneleiraSegurou = "A Caneleira de Ouro segurou o golpe!";
        public const string PerdeuVida = "Perdeu uma vida!";
        public const string CaiuNoCanal = "Caiu no canal!";
        public const string CompanheiroDescansando = "O companheiro ainda está voltando...";
        public const string Tabela = "TABELA!";
        public const string Distraidos = "Marcação distraída!";
        public const string CanhaoAtivo = "CHUTE DO CANHÃO! 10 segundos";
        public const string Variedade = "VARIEDADE!";
        public const string Repetido = "repetido...";
        public const string AdversarioSuperado = "Superou a marcação!";
        public const string ArenaFinal = "A FINAL! Marque 3 gols.";
        public const string Coracao = "+1 coração";
        public const string Cabecada = "CABEÇADA!";
        public const string Defendeu = "DEFENDEU!";
        public const string CapitaoChegou = "O CAPITÃO!";

        // ------------------------------------------------------------ goleiro
        public const string GoleiroDefendeu = "DEFENDEU! Ele caiu: chute agora!";
        public const string GoleiroEspalmou = "ESPALMOU! Forte demais na linha.";
        public const string GoleiroSegurou = "SEGUROU! Por cima dele, só de cavadinha.";
        public const string GoleiroSaiu = "SAIU DO GOL!";
        public const string GoleiroDefesaDificil = "QUE DEFESA! Ele demorou a levantar.";
        public const string Gol = "GOOOL!";
        public const string BolaPorCima = "Por cima do gol!";

        // ------------------------------------------------------------ quiz
        public const string QuizCerto = "Resposta certa!";
        public const string QuizErrado = "Não foi dessa vez.";
        public const string QuizGanhouCarta = "Você ganhou: {0} · {1}";
        public const string QuizSemPerda = "Errar não tira vida. O jogo continua.";
        public const string QuizInstrucao = "Teclas 1 a 4 (ou setas e ENTER)";
        public const string QuizContinuar = "ENTER para continuar";

        // ------------------------------------------------------------ pausa
        public const string Pausa = "PAUSA";
        public const string Continuar = "CONTINUAR";
        public const string ReiniciarFase = "REINICIAR FASE";
        public const string MenuPrincipal = "MENU PRINCIPAL";
        public const string DicaSom = "M: liga e desliga o som";

        // ------------------------------------------------------------ fim de jogo
        public const string VitoriaTitulo = "PRIMEIRA ESTRELA CONQUISTADA!";
        public const string VitoriaTexto =
            "Brasil campeão do mundo em 1958! A jornada continua no Chile, em 1962.";
        public const string DerrotaTitulo = "FIM DE JOGO";
        public const string DerrotaTexto = "As vidas acabaram. A primeira estrela ainda espera por você.";
        public const string Tempo = "Tempo";
        public const string GolsNaFinal = "Gols na final";
        public const string QuizzesCertos = "Quizzes certos";
        public const string MaiorCombo = "Maior combo";
        public const string Superados = "Adversários superados";
        public const string VidasRestantes = "Vidas restantes";
        public const string JogarDeNovo = "JOGAR DE NOVO";
        public const string Menu = "MENU";
        public const string FormatoMelhorTempo = "Melhor tempo: {0}";
        public const string NovoRecorde = "Novo recorde de tempo!";

        /// <summary>Todos os textos acima (o teste da fonte confere letra por letra).</summary>
        public static string[] Todos()
        {
            var lista = new System.Collections.Generic.List<string>
            {
                Titulo, Subtitulo, FaseDoMenu, ComoJogar, Objetivo, ComoGanhar, ComoPerder, TextoObjetivo,
                TextoComoGanhar, TextoComoPerder, Jogar, Sair, CreditosPadrao, IntroAno, IntroTitulo, IntroTexto,
                IntroContinuar, Narrador, Vidas, Torcida, Poder, Carga, Espaco, Companheiro,
                string.Format(FormatoCombo, 12), string.Format(FormatoGols, 2, EstadoPartida.GolsParaVencer),
                EspecialPronto, EspecialArmado, Superchute, CheckpointAtivado, SinoTocou,
                SinoPrecisaCavadinha, BarrisPrecisamCarga, BarrisDerrubados, PortaoFechado, PortaoAberto,
                CapitaoDefendeu, CaneleiraSegurou, PerdeuVida, CaiuNoCanal, CompanheiroDescansando, Tabela,
                Distraidos, CanhaoAtivo, Variedade, Repetido, AdversarioSuperado, ArenaFinal, Coracao, Cabecada,
                Defendeu, CapitaoChegou,
                GoleiroDefendeu, GoleiroEspalmou, GoleiroSegurou, GoleiroSaiu, GoleiroDefesaDificil, Gol,
                BolaPorCima, QuizCerto, QuizErrado, string.Format(QuizGanhouCarta, "P01", "Chute do Canhão"),
                QuizSemPerda, QuizInstrucao, QuizContinuar,
                Pausa, Continuar, ReiniciarFase, MenuPrincipal, DicaSom, VitoriaTitulo, VitoriaTexto,
                DerrotaTitulo, DerrotaTexto, Tempo, GolsNaFinal, QuizzesCertos, MaiorCombo, Superados,
                VidasRestantes, JogarDeNovo, Menu, string.Format(FormatoMelhorTempo, "3:21"), NovoRecorde,
            };
            lista.AddRange(Controles);
            lista.AddRange(Atos);
            lista.AddRange(SubtitulosAtos);
            lista.AddRange(Narradores.Todas());
            foreach (PerguntaQuiz q in BancoQuiz.Todas())
            {
                lista.Add(q.Codigo);
                lista.Add(q.Formato);
                lista.Add(q.Copa);
                lista.Add(q.Pergunta);
                lista.Add(q.Explicacao);
                lista.AddRange(q.Alternativas);
            }
            foreach (CartaPoder c in new[] { CartaPoder.ChuteDoCanhao, CartaPoder.CaneleiraDeOuro })
            {
                lista.Add(Poderes.Codigo(c));
                lista.Add(Poderes.Nome(c));
                lista.Add(Poderes.Descricao(c));
            }
            return lista.ToArray();
        }

        /// <summary>Formata segundos como m:ss.</summary>
        public static string FormatarTempo(float segundos)
        {
            int s = (int)segundos;
            if (s < 0)
                s = 0;
            return (s / 60) + ":" + (s % 60).ToString("00");
        }
    }

    /// <summary>Frases do narrador para os lances.</summary>
    public static class Narradores
    {
        public static string Chute(TipoChute tipo, ModificadorChute mod)
        {
            if (Chutes.AceitaModificador(tipo))
            {
                if (mod == ModificadorChute.Especial)
                    return "BOLA DE FOGO!";
                if (mod == ModificadorChute.Canhao)
                    return "CHUTE DO CANHÃO!";
            }
            switch (tipo)
            {
                case TipoChute.Fraco: return "Toquinho!";
                case TipoChute.Medio: return "Bom chute!";
                case TipoChute.Forte: return "QUE PANCADA!";
                case TipoChute.Superchute: return "SUPERCHUTE!";
                case TipoChute.Cavadinha: return "DE CAVADINHA!";
                default: return "BICICLETA!";
            }
        }

        static readonly string[] superou = { "Superou a marcação!", "Passou pelo zagueiro!", "Que jogada!" };

        public static string Superou(int sorteio)
        {
            int i = sorteio % superou.Length;
            if (i < 0)
                i += superou.Length;
            return superou[i];
        }

        public static string[] Todas()
        {
            var lista = new System.Collections.Generic.List<string>(superou);
            foreach (TipoChute t in new[]
                     {
                         TipoChute.Fraco, TipoChute.Medio, TipoChute.Forte, TipoChute.Superchute, TipoChute.Cavadinha,
                         TipoChute.Bicicleta,
                     })
            {
                lista.Add(Chute(t, ModificadorChute.Nenhum));
            }
            lista.Add(Chute(TipoChute.Forte, ModificadorChute.Canhao));
            lista.Add(Chute(TipoChute.Forte, ModificadorChute.Especial));
            return lista.ToArray();
        }
    }
}
