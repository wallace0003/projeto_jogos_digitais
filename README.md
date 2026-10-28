# Cinco Estrelas: A Jornada do Brasil · Fase 1 (Suécia 1958)

Protótipo jogável da primeira fase do **Cinco Estrelas**, o jogo de plataforma 2D em pixel art da disciplina de
Jogos Digitais. O Camisa 10 conduz a bola pelas docas de Estocolmo, supera a marcação nas ruas de Gamla Stan e
decide contra o goleiro sueco no estádio Råsunda para acender a **primeira estrela**.

- **Unity:** `6000.5.10f1` (Unity 6.5), projeto 2D com URP (2D Renderer) e o Input System novo.
- Arte, música, efeitos sonoros e código são originais e foram feitos para este protótipo.

---

## Como abrir e jogar

1. Instale o **Unity Hub** e, por ele, o editor **6000.5.10f1** (aba *Installs › Install Editor › Archive*, se
   ele não aparecer na lista).
2. Clone o repositório:

   ```bash
   git clone https://github.com/wallace0003/projeto_jogos_digitais.git
   ```

3. No Unity Hub: **Projects › Add › Add project from disk** e escolha a pasta `projeto_jogos_digitais`.
4. Abra o projeto. A primeira abertura demora alguns minutos, porque o Unity importa tudo e cria a pasta
   `Library/`. Ele já abre na cena **Menu**, com a aba Scene em 2D.
5. Aperte **Play** (▶). Na tela inicial, **JOGAR** (Enter) começa a fase.

> Atalhos no menu **Cinco Estrelas**, na barra do Unity:
> - **Abrir cena Menu / da fase / Fim de Jogo**;
> - **Testar a fase direto**, que dá Play pulando a tela inicial (sem isso, dar Play na cena da fase leva ao Menu, de propósito);
> - **Editar o mapa da fase**;
> - **Rodar os testes**.

Não é preciso configurar nada: a lista de cenas da Build (Menu, Suecia1958, FimDeJogo), as camadas de física,
o Input System e o URP 2D já vêm configurados no projeto.

## Controles

Os controles seguem a pág. 4 do GDD. Também funciona com controle (gamepad).

| Teclado | Controle | Ação |
|---|---|---|
| A / D ou ← / → | analógico ou direcional | andar |
| W ou ↑ | A (Sul) | pular |
| SHIFT | gatilho direito | correr |
| X (segure) | X (Oeste) | chutar, com 3 níveis de carga (fraco, médio, forte) |
| X no ar | X (Oeste) no ar | bicicleta |
| C | Y (Norte) | cavadinha (bola alta) |
| Z | RB | pedalada: o próximo chute sai como **superchute** |
| S ou ↓, parado | direcional ↓ | embaixadinhas (enchem a torcida) |
| S ou ↓, em cima de uma tábua | direcional ↓ | descer da tábua |
| Q | LB | chamar o companheiro (tabela) |
| ESPAÇO | B (Leste) | usar a carta de poder ou o especial da torcida |
| E / ENTER | A / Start | confirmar e responder |
| 1 a 4 | direcional + A | escolher a alternativa do quiz |
| P / ESC | Start | pausa (continuar, reiniciar a fase, menu) |
| M | — | liga e desliga o som |

## A fase

A fase tem três atos, montados a partir de um mapa em texto (`Assets/Resources/Fases/Suecia1958.txt`). Placas
pelo caminho ensinam cada controle no momento em que ele é necessário.

### Ato 1 · Chegada: docas e canais de Estocolmo
- Barcos que vão e voltam pelo canal servem de plataforma. Cair na água tira 1 coração e devolve o jogador à
  última bandeira (checkpoint).
- A **ponte levadiça** só baixa quando o jogador acerta o **sino** com a **cavadinha**. Um chute rasteiro não
  alcança o sino.
- Caixotes quebram com um chute. Alguns guardam um coração.

### Ato 2 · Caminho: as ruas de Gamla Stan
- **Zagueiro carrinho:** patrulha e dá o bote. Pule por cima ou chute antes do contato. Depois do primeiro
  carrinho, o narrador faz o **quiz surpresa** (o do slide 8):
  *"Qual jogador de 17 anos marcou dois gols na final de 1958, contra a Suécia?"*. Quem acerta (Pelé) ganha a
  carta **Chute do Canhão**: ESPAÇO e, por 10 segundos, os chutes atravessam vários adversários.
- **Zagueiro saltador:** pula para cortar as bolas altas. Chute rasteiro nele.
- **Pilha de barris:** só cede com **carga máxima** (segure X até o 3º nível) ou com o superchute.
- **Capitão adversário (minichefe):** defende tudo, menos quando está se recuperando do próprio ataque. Superá-lo
  abre o portão para o estádio.

### Ato 3 · Decisão: o estádio Råsunda
- **Quiz do estádio:** *"Qual foi o placar da final da Copa de 1958, contra a Suécia?"*. Quem acerta (5 x 2)
  ganha a **Caneleira de Ouro**, que anula o próximo dano.
- **Goleiro final**, numa arena com a câmera travada:
  - na linha do gol, ele defende os chutes rasteiros;
  - um **chute fraco ou médio** engana: ele mergulha e fica caído por uns 2 segundos, e é a hora de chutar de novo;
  - o **chute forte** ele espalma e levanta na hora;
  - de tempos em tempos ele **sai do gol** para fechar o ângulo, e aí a **cavadinha** passa por cima dele;
  - ele também arremessa bolas: pule ou desvie.
- **3 gols** vencem a partida: a tela de vitória mostra a estrela acesa, o tempo e o melhor tempo.

### Sistemas
- **3 vidas com 3 corações cada.** Cada golpe tira 1 coração. Sem corações, perde uma vida e volta ao último
  checkpoint. Sem vidas, é fim de jogo, com a opção de jogar de novo.
- **Barra da torcida:** cada ação enche a barra (chutes, cavadinha, embaixadinhas, tabela, quiz certo, gol...).
  Variar as ações rende mais, e repetir a mesma ação rende menos. Com a barra cheia, ESPAÇO arma o
  **especial da torcida**: o próximo chute sai como uma bola de fogo que atravessa tudo.
- **Pedalada → superchute:** depois da pedalada, o próximo chute sai mais forte, se for dado dentro de um tempo
  curto.
- **Companheiro (Q):** entra em campo, distrai a marcação e devolve a bola na tabela. Depois ele precisa de um
  tempo para poder ser chamado de novo.
- **Quizzes:** o jogo pausa e o narrador pergunta. **Errar nunca tira vida.** Só não ganha a carta.

## Testes

### No Unity
Abra **Window › General › Test Runner** (ou **Cinco Estrelas › Rodar os testes**):
- **EditMode:**
  - as regras do jogo (chutes e carga, torcida, bola, vidas e checkpoints, quizzes, adversários, goleiro);
  - o mapa: um analisador confere se todos os pulos são possíveis com a física do jogador e se a ordem dos atos está certa;
  - a configuração do projeto: versão, Input System, URP 2D, cenas da Build, camadas, importação das sprites;
  - os recursos: sprites, animações, fonte e sons.
- **PlayMode:** monta a fase de verdade e simula as jogadas principais:
  - cavadinha no sino baixa a ponte, e o chute rasteiro não;
  - os barris só caem com carga máxima;
  - o quiz entra depois do primeiro carrinho;
  - o capitão abre o portão;
  - o goleiro: chute fraco e depois gol;
  - 3 gols levam à vitória, e sem vidas é derrota.

### Sem o Unity
As regras ficam no assembly `CincoEstrelas.Logica`, que não usa a UnityEngine. Por isso os testes delas rodam
fora do Unity:

- **Windows, sem instalar nada.** Usa o compilador C# que já vem no Windows:

  ```bash
  powershell -ExecutionPolicy Bypass -File Ferramentas/TestesSemUnity/rodar_testes.ps1
  ```

- **Qualquer sistema com o .NET SDK 8:**

  ```bash
  dotnet test Ferramentas/TestesSemUnity
  ```

- **Compilação de todo o código** (Windows, sem instalar nada). Compila o jogo, o Editor e os testes contra uma
  versão simplificada da API do Unity 6 (`Ferramentas/CompilarSemUnity`), separada nas mesmas DLLs do Unity e
  com as referências de cada `.asmdef`, do jeito do Editor e do jeito de uma build. Isso pega erros de C# e
  referências faltando sem abrir o Unity, mas quem dá a palavra final é o Unity:

  ```bash
  powershell -ExecutionPolicy Bypass -File Ferramentas/CompilarSemUnity/compilar.ps1
  ```

- **Consistência do projeto** (Python 3, sem dependências). Confere os `.meta` e GUIDs, se as cenas apontam para
  scripts que existem, a lista de cenas da Build, os `.asmdef` e as sprites:

  ```bash
  py Ferramentas/verificar_projeto.py
  ```

  No Linux ou no macOS, troque `py` por `python3`.

## Editando a fase

O mapa é um arquivo de texto: `Assets/Resources/Fases/Suecia1958.txt`, também aberto pelo menu
**Cinco Estrelas › Editar o mapa da fase**. A legenda está no começo do arquivo:
- `#` cais, `D` doca, `B` barco, `S` sino, `L` ponte;
- `c` carrinho, `s` saltador, `C` capitão, `O` barris;
- `K` checkpoint, `Q` quiz, `g` goleiro, `Y` gol;
- `0`–`9` placas, com os textos nas linhas `@placa`.

Depois de editar, rode os testes EditMode: o teste do mapa avisa se algum pulo ficou impossível.

## Ferramentas (Python 3, só biblioteca padrão)

| Comando | O que faz |
|---|---|
| `py Ferramentas/gerar_arte.py [--previas PASTA]` | Regenera as folhas de sprites (`Assets/Resources/Sprites/*.png` + `manifest.json`). O resultado é sempre o mesmo. |
| `py Ferramentas/gerar_metas.py` | Cria os `.meta` que faltarem, com GUIDs fixos (derivados do caminho). |
| `py Ferramentas/gerar_cenas.py` | Regenera as três cenas e a lista de cenas da Build. |
| `py Ferramentas/verificar_projeto.py` | Confere a consistência do projeto (veja acima). |

## Estrutura

| Pasta | Conteúdo |
|---|---|
| `Assets/Scenes/` | `Menu`, `Suecia1958` (a fase) e `FimDeJogo` (vitória ou derrota). |
| `Assets/Scripts/Logica/` | Regras puras, sem UnityEngine: parâmetros do jogador, chutes, torcida, bola, partida, quizzes, cérebros dos adversários, leitura e análise do mapa, textos. |
| `Assets/Scripts/Nucleo/` | Camadas e colisões, entrada (teclado e controle), carregamento da arte, sessão e transições. |
| `Assets/Scripts/Jogador/` | Camisa 10, bola e companheiro. |
| `Assets/Scripts/Inimigos/` | Zagueiro carrinho, zagueiro saltador, capitão e goleiro. |
| `Assets/Scripts/Fase/` | Construtor da fase (lê o mapa e monta os tilemaps, os objetos e os inimigos), gerenciador da fase, barco, sino, ponte, obstáculos, checkpoint, gol, câmera e fundo. |
| `Assets/Scripts/Interface/` | HUD, quiz, narrador, pausa, menu e tela final, com uma fonte pixel própria. |
| `Assets/Scripts/Som/` | Efeitos e músicas 8-bit sintetizados em tempo real. |
| `Assets/Scripts/Efeitos/` | Animação de sprites, partículas, textos que sobem e confete. |
| `Assets/Scripts/Editor/` | Menu **Cinco Estrelas** no editor. |
| `Assets/Resources/` | Mapa da fase e folhas de sprites (recortadas em tempo real pelo `manifest.json`). |
| `Assets/Tests/` | Testes EditMode e PlayMode. |
| `Ferramentas/` | Geradores de arte, metas e cenas, verificador do projeto, testes e compilação sem o Unity. |

## Problemas comuns

- **Abriu uma cena vazia:** use **Cinco Estrelas › Abrir cena Menu** (ou abra `Assets/Scenes/Menu.unity`).
- **Dei Play na cena da fase e fui para o Menu:** é de propósito, porque o jogo sempre começa pela tela inicial.
  Para ir direto, use **Cinco Estrelas › Testar a fase direto**.
- **O Unity pediu para reiniciar por causa do Input System:** aceite. O projeto usa só o Input System novo.
- **Versão diferente do Unity:** use a `6000.5.10f1`. Outras versões podem pedir para atualizar os pacotes
  (URP 17.5.0, Input System 1.20.0, Test Framework 1.7.0).

## Créditos

Protótipo da disciplina de Jogos Digitais. Arte, música e código originais. O texto dos créditos da tela inicial
pode ser trocado na cena `Menu`, no componente **Menu Principal** (campo *Creditos*).
