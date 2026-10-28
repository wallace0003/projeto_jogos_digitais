using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CincoEstrelas
{
    /// <summary>
    /// Tela do quiz: o jogo para, o narrador faz a pergunta com quatro
    /// alternativas (teclas 1 a 4, setas + Enter/E ou mouse). Depois da resposta
    /// mostra se acertou, a explicação e a carta ganha. Errar nunca tira vida.
    /// </summary>
    public class PainelQuiz : MonoBehaviour
    {
        GameObject raiz;
        TextoPixel etiqueta;
        TextoPixel pergunta;
        readonly Button[] botoes = new Button[4];
        readonly Image[] fundosBotoes = new Image[4];
        readonly TextoPixel[] rotulos = new TextoPixel[4];
        TextoPixel instrucao;
        TextoPixel resultadoTitulo;
        TextoPixel resultadoTexto;
        Image cartaGanha;
        TextoPixel continuar;

        PerguntaQuiz atual;
        Func<int, RespostaQuiz> aoResponder;
        Action aoFechar;
        RespostaQuiz resposta;
        int selecionada;
        float tempoAberto;

        public bool Aberto
        {
            get { return raiz != null && raiz.activeSelf; }
        }

        public PerguntaQuiz Pergunta
        {
            get { return atual; }
        }

        /// <summary>A resposta já foi dada (a tela mostra o resultado).</summary>
        public bool Respondido
        {
            get { return resposta != null; }
        }

        public RespostaQuiz UltimaResposta
        {
            get { return resposta; }
        }

        public static PainelQuiz Criar()
        {
            Canvas canvas = FabricaUI.CriarCanvas("Quiz", 60);
            PainelQuiz p = canvas.gameObject.AddComponent<PainelQuiz>();
            p.Montar(canvas.transform);
            return p;
        }

        void Montar(Transform canvas)
        {
            Vector2 c = new Vector2(0.5f, 0.5f);
            raiz = new GameObject("Quiz (raiz)", typeof(RectTransform));
            raiz.transform.SetParent(canvas, false);
            FabricaUI.Esticar((RectTransform)raiz.transform);
            FabricaUI.Cortina(raiz.transform, new Color(0.01f, 0.03f, 0.08f, 0.72f));

            Image painel = FabricaUI.Painel(raiz.transform, c, new Vector2(0f, 0f), new Vector2(800f, 480f));
            Transform p = painel.transform;
            etiqueta = FabricaUI.Texto(p, "", c, new Vector2(0f, 205f), 2f, FabricaUI.CorDestaque, TextAnchor.MiddleCenter,
                new Vector2(740f, 26f));
            FabricaUI.Texto(p, Textos.Narrador + ":", c, new Vector2(-330f, 172f), 2f, FabricaUI.CorApagada,
                TextAnchor.MiddleLeft, new Vector2(200f, 24f));
            pergunta = FabricaUI.Texto(p, "", c, new Vector2(0f, 120f), 3f, FabricaUI.CorTexto, TextAnchor.MiddleCenter,
                new Vector2(740f, 90f));

            for (int i = 0; i < 4; i++)
            {
                int indice = i;
                float x = i % 2 == 0 ? -185f : 185f;
                float y = i < 2 ? 34f : -26f;
                Button b = FabricaUI.Botao(p, "", c, new Vector2(x, y), new Vector2(350f, 50f), delegate { Responder(indice); });
                botoes[i] = b;
                fundosBotoes[i] = b.GetComponent<Image>();
                rotulos[i] = b.GetComponentInChildren<TextoPixel>();
                rotulos[i].alinhamento = TextAnchor.MiddleLeft;
                RectTransform rt = rotulos[i].rectTransform;
                rt.sizeDelta = new Vector2(320f, 50f);
                // sem o efeito de "setinhas" (o quiz destaca a alternativa de outro jeito)
                EfeitoBotao efeito = b.GetComponent<EfeitoBotao>();
                if (efeito != null)
                    Destroy(efeito);
            }
            FabricaUI.NavegacaoHorizontal(botoes[0], botoes[1]);

            instrucao = FabricaUI.Texto(p, Textos.QuizInstrucao, c, new Vector2(0f, -78f), 2f, FabricaUI.CorApagada,
                TextAnchor.MiddleCenter, new Vector2(740f, 24f));
            resultadoTitulo = FabricaUI.Texto(p, "", c, new Vector2(-40f, -86f), 3f, FabricaUI.CorVerde, TextAnchor.MiddleLeft,
                new Vector2(640f, 34f));
            resultadoTexto = FabricaUI.Texto(p, "", c, new Vector2(-40f, -150f), 2f, FabricaUI.CorTexto, TextAnchor.UpperLeft,
                new Vector2(640f, 96f));
            cartaGanha = FabricaUI.Imagem(p, Arte.Sprite("carta_canhao"), c, new Vector2(330f, -130f), new Vector2(72f, 96f));
            continuar = FabricaUI.Texto(p, Textos.QuizContinuar, c, new Vector2(0f, -218f), 2f, FabricaUI.CorDestaque,
                TextAnchor.MiddleCenter, new Vector2(740f, 24f));
            raiz.SetActive(false);
        }

        /// <summary>Mostra a pergunta. <paramref name="responder"/> aplica a resposta; <paramref name="fechar"/> volta ao jogo.</summary>
        public void Mostrar(PerguntaQuiz q, Func<int, RespostaQuiz> responder, Action fechar)
        {
            atual = q;
            aoResponder = responder;
            aoFechar = fechar;
            resposta = null;
            selecionada = 0;
            tempoAberto = 0f;
            raiz.SetActive(true);
            etiqueta.Texto = q.Codigo + " · " + q.Formato.ToUpperInvariant() + " · " + q.Copa.ToUpperInvariant();
            pergunta.Texto = q.Pergunta;
            for (int i = 0; i < 4; i++)
            {
                bool existe = i < q.Alternativas.Length;
                botoes[i].gameObject.SetActive(existe);
                if (existe)
                    rotulos[i].Texto = "  " + (i + 1) + "  " + q.Alternativas[i];
                botoes[i].interactable = true;
            }
            instrucao.enabled = true;
            resultadoTitulo.Texto = "";
            resultadoTexto.Texto = "";
            cartaGanha.enabled = false;
            continuar.enabled = false;
            Destacar();
        }

        void Update()
        {
            if (!Aberto)
                return;
            tempoAberto += Time.unscaledDeltaTime;
            if (resposta == null)
            {
                int tecla = Entrada.AlternativaApertada;
                if (tecla >= 0 && tecla < atual.Alternativas.Length)
                {
                    Responder(tecla);
                    return;
                }
                int nav = Entrada.NavegacaoVertical;
                float h = Entrada.Horizontal;
                if (nav != 0)
                {
                    selecionada = (selecionada + nav * 2 + 4) % 4;
                    SomJogo.Tocar(Som.Foco, 0.6f);
                    Destacar();
                }
                if (tempoAberto > 0.25f && Entrada.ConfirmarApertado)
                    Responder(selecionada);
                // A/D e setas laterais trocam de coluna
                if (Mathf.Abs(h) > 0.5f && !segurandoLado)
                {
                    segurandoLado = true;
                    int coluna = selecionada % 2;
                    int novaColuna = h > 0f ? 1 : 0;
                    if (novaColuna != coluna)
                    {
                        selecionada += novaColuna - coluna;
                        SomJogo.Tocar(Som.Foco, 0.6f);
                        Destacar();
                    }
                }
                else if (Mathf.Abs(h) <= 0.5f)
                {
                    segurandoLado = false;
                }
            }
            else
            {
                continuar.color = new Color(continuar.color.r, continuar.color.g, continuar.color.b,
                    0.55f + Mathf.PingPong(Time.unscaledTime * 1.5f, 0.45f));
                if (tempoAberto > 0.4f && Entrada.ConfirmarApertado)
                    Continuar();
            }
        }

        bool segurandoLado;

        void Destacar()
        {
            for (int i = 0; i < 4; i++)
            {
                bool sel = i == selecionada && resposta == null;
                fundosBotoes[i].color = sel ? Color.white : new Color(0.75f, 0.78f, 0.86f);
                rotulos[i].color = sel ? FabricaUI.CorDestaque : FabricaUI.CorTexto;
                botoes[i].transform.localScale = sel ? new Vector3(1.04f, 1.04f, 1f) : Vector3.one;
            }
        }

        /// <summary>Responde (público para os testes e para o clique do mouse).</summary>
        public void Responder(int alternativa)
        {
            if (!Aberto || resposta != null || atual == null || aoResponder == null)
                return;
            resposta = aoResponder(alternativa);
            tempoAberto = 0f;
            for (int i = 0; i < 4; i++)
            {
                botoes[i].interactable = false;
                botoes[i].transform.localScale = Vector3.one;
                Color cor = new Color(0.4f, 0.42f, 0.5f);
                if (i == atual.Correta)
                    cor = new Color(0.35f, 0.85f, 0.45f);
                else if (i == alternativa)
                    cor = new Color(0.95f, 0.4f, 0.35f);
                fundosBotoes[i].color = cor;
                ColorBlock cb = botoes[i].colors;
                cb.disabledColor = Color.white;
                botoes[i].colors = cb;
                rotulos[i].color = FabricaUI.CorTexto;
            }
            instrucao.enabled = false;
            List<string> linhas = resposta.LinhasResultado();
            resultadoTitulo.Texto = linhas[0];
            resultadoTitulo.color = resposta.Acertou ? FabricaUI.CorVerde : FabricaUI.CorVermelha;
            var resto = new System.Text.StringBuilder();
            for (int i = 1; i < linhas.Count; i++)
            {
                if (resto.Length > 0)
                    resto.Append('\n');
                resto.Append(linhas[i]);
            }
            resultadoTexto.Texto = resto.ToString();
            CartaPoder carta = Poderes.CartaDa(resposta.Ganhou);
            cartaGanha.enabled = carta != CartaPoder.Nenhuma;
            if (carta != CartaPoder.Nenhuma)
                cartaGanha.sprite = Arte.Sprite(carta == CartaPoder.ChuteDoCanhao ? "carta_canhao" : "carta_caneleira");
            continuar.enabled = true;
        }

        /// <summary>Fecha a tela e volta ao jogo (depois da resposta).</summary>
        public void Continuar()
        {
            if (!Aberto || resposta == null)
                return;
            raiz.SetActive(false);
            SomJogo.Tocar(Som.Clique);
            if (aoFechar != null)
                aoFechar();
        }
    }
}
