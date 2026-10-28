using System;
using UnityEngine;
using UnityEngine.UI;

namespace CincoEstrelas
{
    /// <summary>
    /// Cena curta do narrador (GDD, pág. 9: "panorama do país, ano da Copa na
    /// tela, fala do narrador, controle devolvido ao jogador"). O texto aparece
    /// letra por letra; Enter/E mostra tudo e depois fecha.
    /// </summary>
    public class PainelNarrador : MonoBehaviour
    {
        const float LetrasPorSegundo = 55f;

        GameObject raiz;
        TextoPixel ano;
        TextoPixel titulo;
        TextoPixel texto;
        TextoPixel rodape;
        Image[] estrelas = new Image[5];
        string textoCompleto = "";
        float letras;
        float tempoAberto;
        Action aoFechar;

        public bool Aberto
        {
            get { return raiz != null && raiz.activeSelf; }
        }

        public static PainelNarrador Criar()
        {
            Canvas canvas = FabricaUI.CriarCanvas("Narrador", 70);
            PainelNarrador p = canvas.gameObject.AddComponent<PainelNarrador>();
            p.Montar(canvas.transform);
            return p;
        }

        void Montar(Transform canvas)
        {
            Vector2 c = new Vector2(0.5f, 0.5f);
            raiz = new GameObject("Narrador (raiz)", typeof(RectTransform));
            raiz.transform.SetParent(canvas, false);
            FabricaUI.Esticar((RectTransform)raiz.transform);
            FabricaUI.Cortina(raiz.transform, new Color(0.01f, 0.03f, 0.08f, 0.8f));
            Image painel = FabricaUI.Painel(raiz.transform, c, Vector2.zero, new Vector2(720f, 380f));
            Transform p = painel.transform;

            // as cinco estrelas da jornada: só a primeira está em jogo
            for (int i = 0; i < 5; i++)
            {
                estrelas[i] = FabricaUI.Imagem(p, Arte.Sprite(i == 0 ? "estrela_cheia" : "estrela_vazia"), c,
                    new Vector2(-96f + i * 48f, 148f), new Vector2(40f, 40f));
            }
            ano = FabricaUI.Texto(p, "", c, new Vector2(0f, 96f), 4f, FabricaUI.CorDestaque, TextAnchor.MiddleCenter,
                new Vector2(680f, 50f));
            ano.contorno = true;
            ano.quebrarLinhas = false;
            titulo = FabricaUI.Texto(p, "", c, new Vector2(0f, 50f), 3f, FabricaUI.CorTexto, TextAnchor.MiddleCenter,
                new Vector2(680f, 40f));
            FabricaUI.Texto(p, Textos.Narrador + ":", c, new Vector2(-300f, 10f), 2f, FabricaUI.CorApagada, TextAnchor.MiddleLeft,
                new Vector2(200f, 24f));
            texto = FabricaUI.Texto(p, "", c, new Vector2(0f, -60f), 2.5f, FabricaUI.CorTexto, TextAnchor.UpperLeft,
                new Vector2(620f, 120f));
            rodape = FabricaUI.Texto(p, "", c, new Vector2(0f, -160f), 2f, FabricaUI.CorDestaque, TextAnchor.MiddleCenter,
                new Vector2(680f, 24f));
            raiz.SetActive(false);
        }

        public void Mostrar(string anoCopa, string tituloCena, string fala, string textoRodape, Action fechar)
        {
            raiz.SetActive(true);
            ano.Texto = anoCopa;
            titulo.Texto = tituloCena;
            textoCompleto = fala ?? "";
            texto.Texto = "";
            rodape.Texto = textoRodape;
            letras = 0f;
            tempoAberto = 0f;
            aoFechar = fechar;
        }

        void Update()
        {
            if (!Aberto)
                return;
            float dt = Time.unscaledDeltaTime;
            tempoAberto += dt;
            if (letras < textoCompleto.Length)
            {
                letras = Mathf.Min(textoCompleto.Length, letras + dt * LetrasPorSegundo);
                texto.Texto = textoCompleto.Substring(0, (int)letras);
            }
            Color cr = rodape.color;
            cr.a = letras >= textoCompleto.Length ? 0.55f + Mathf.PingPong(Time.unscaledTime * 1.5f, 0.45f) : 0.25f;
            rodape.color = cr;
            estrelas[0].transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 4f) * 0.08f);

            if (tempoAberto > 0.3f && Entrada.ConfirmarApertado)
            {
                if (letras < textoCompleto.Length)
                {
                    letras = textoCompleto.Length;
                    texto.Texto = textoCompleto;
                }
                else
                {
                    Fechar();
                }
            }
        }

        /// <summary>Fecha a cena e devolve o controle ao jogador.</summary>
        public void Fechar()
        {
            if (!Aberto)
                return;
            raiz.SetActive(false);
            Action a = aoFechar;
            aoFechar = null;
            if (a != null)
                a();
        }
    }
}
