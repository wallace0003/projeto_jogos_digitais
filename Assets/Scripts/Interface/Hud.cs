using UnityEngine;
using UnityEngine.UI;

namespace CincoEstrelas
{
    /// <summary>
    /// HUD da fase (GDD, pág. 4: "vidas e corações no canto esquerdo; barra da
    /// torcida no topo; poder ativo à direita; carga próxima ao personagem").
    /// Também mostra o companheiro, o placar da final, mensagens do narrador,
    /// as faixas de cada ato e a dica das placas.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        const int MaxVidas = EstadoPartida.VidasIniciais;
        const int MaxCoracoes = EstadoPartida.CoracoesPorVida;

        Image[] iconesVida = new Image[MaxVidas];
        Image[] coracoes = new Image[MaxCoracoes];
        Image escudo;
        Image barraTorcida;
        TextoPixel avisoEspecial;
        Image cartaSlot;
        TextoPixel rotuloEspaco;
        Image barraCanhao;
        Image companheiroIcone;
        Image companheiroRecarga;
        TextoPixel combo;
        TextoPixel gols;
        TextoPixel mensagem;
        TextoPixel bannerTitulo;
        TextoPixel bannerSubtitulo;
        GameObject caixaDica;
        TextoPixel textoDica;
        GameObject cargaRaiz;
        Image[] segmentosCarga = new Image[3];
        TextoPixel textoCarga;

        float tempoMensagem;
        float duracaoMensagem;
        Color corMensagem;
        float tempoBanner = 99f;
        float tempoCombo;
        float tempoPiscar;
        int coracoesAtuais;

        Sprite coracaoCheio;
        Sprite coracaoVazio;

        public static Hud Criar()
        {
            Canvas canvas = FabricaUI.CriarCanvas("HUD", 10);
            Hud h = canvas.gameObject.AddComponent<Hud>();
            h.Montar(canvas.transform);
            return h;
        }

        void Montar(Transform raiz)
        {
            coracaoCheio = Arte.Sprite("ui_coracao_cheio");
            coracaoVazio = Arte.Sprite("ui_coracao_vazio");
            Vector2 cantoSupEsq = new Vector2(0f, 1f);
            Vector2 topo = new Vector2(0.5f, 1f);
            Vector2 cantoSupDir = new Vector2(1f, 1f);
            Vector2 centro = new Vector2(0.5f, 0.5f);
            Vector2 base_ = new Vector2(0.5f, 0f);

            // ---- vidas e corações (canto esquerdo)
            FabricaUI.Texto(raiz, Textos.Vidas, cantoSupEsq, new Vector2(90f, -20f), 2f, FabricaUI.CorTexto,
                TextAnchor.MiddleLeft, new Vector2(150f, 24f));
            for (int i = 0; i < MaxVidas; i++)
                iconesVida[i] = FabricaUI.Imagem(raiz, Arte.Sprite("icone_vida"), cantoSupEsq, new Vector2(34f + i * 40f, -52f), new Vector2(36f, 36f));
            for (int i = 0; i < MaxCoracoes; i++)
                coracoes[i] = FabricaUI.Imagem(raiz, coracaoCheio, cantoSupEsq, new Vector2(36f + i * 42f, -94f), new Vector2(36f, 32f));
            escudo = FabricaUI.Imagem(raiz, Arte.Sprite("carta_caneleira"), cantoSupEsq, new Vector2(170f, -94f), new Vector2(24f, 32f));
            escudo.enabled = false;

            // ---- barra da torcida (topo)
            FabricaUI.Texto(raiz, Textos.Torcida, topo, new Vector2(0f, -20f), 2f, FabricaUI.CorTexto, TextAnchor.MiddleCenter,
                new Vector2(300f, 24f));
            FabricaUI.Bloco(raiz, topo, new Vector2(0f, -46f), new Vector2(308f, 24f), new Color(0.02f, 0.05f, 0.12f, 0.9f));
            FabricaUI.Bloco(raiz, topo, new Vector2(0f, -46f), new Vector2(300f, 16f), new Color(0.2f, 0.25f, 0.35f, 1f));
            barraTorcida = FabricaUI.Bloco(raiz, topo, new Vector2(0f, -46f), new Vector2(300f, 16f), FabricaUI.CorDestaque);
            barraTorcida.type = Image.Type.Filled;
            barraTorcida.fillMethod = Image.FillMethod.Horizontal;
            barraTorcida.fillOrigin = 0;
            barraTorcida.fillAmount = 0f;
            avisoEspecial = FabricaUI.Texto(raiz, Textos.EspecialPronto, topo, new Vector2(0f, -74f), 2f, FabricaUI.CorDestaque,
                TextAnchor.MiddleCenter, new Vector2(600f, 24f));
            avisoEspecial.enabled = false;
            gols = FabricaUI.Texto(raiz, "", topo, new Vector2(0f, -104f), 3f, FabricaUI.CorTexto, TextAnchor.MiddleCenter,
                new Vector2(400f, 36f));
            combo = FabricaUI.Texto(raiz, "", topo, new Vector2(0f, -138f), 2f, FabricaUI.CorVerde, TextAnchor.MiddleCenter,
                new Vector2(400f, 28f));

            // ---- poder e companheiro (canto direito)
            FabricaUI.Texto(raiz, Textos.Poder, cantoSupDir, new Vector2(-60f, -20f), 2f, FabricaUI.CorTexto,
                TextAnchor.MiddleCenter, new Vector2(120f, 24f));
            FabricaUI.Bloco(raiz, cantoSupDir, new Vector2(-60f, -66f), new Vector2(56f, 68f), new Color(0.02f, 0.05f, 0.12f, 0.85f));
            cartaSlot = FabricaUI.Imagem(raiz, Arte.Sprite("carta_canhao"), cantoSupDir, new Vector2(-60f, -66f), new Vector2(48f, 64f));
            cartaSlot.enabled = false;
            rotuloEspaco = FabricaUI.Texto(raiz, Textos.Espaco, cantoSupDir, new Vector2(-60f, -112f), 1.5f, FabricaUI.CorDestaque,
                TextAnchor.MiddleCenter, new Vector2(120f, 20f));
            rotuloEspaco.enabled = false;
            barraCanhao = FabricaUI.Bloco(raiz, cantoSupDir, new Vector2(-60f, -104f), new Vector2(56f, 6f), new Color(1f, 0.6f, 0.2f));
            barraCanhao.type = Image.Type.Filled;
            barraCanhao.fillMethod = Image.FillMethod.Horizontal;
            barraCanhao.enabled = false;

            companheiroIcone = FabricaUI.Imagem(raiz, Arte.Sprite("icone_companheiro"), cantoSupDir, new Vector2(-138f, -60f), new Vector2(42f, 42f));
            companheiroRecarga = FabricaUI.Bloco(raiz, cantoSupDir, new Vector2(-138f, -60f), new Vector2(42f, 42f), new Color(0.02f, 0.05f, 0.12f, 0.7f));
            companheiroRecarga.type = Image.Type.Filled;
            companheiroRecarga.fillMethod = Image.FillMethod.Vertical;
            companheiroRecarga.fillOrigin = 1;
            FabricaUI.Texto(raiz, Textos.Companheiro, cantoSupDir, new Vector2(-138f, -100f), 2f, FabricaUI.CorTexto,
                TextAnchor.MiddleCenter, new Vector2(60f, 24f));

            // ---- carga do chute (embaixo, perto do jogador)
            cargaRaiz = new GameObject("Carga", typeof(RectTransform));
            cargaRaiz.transform.SetParent(raiz, false);
            textoCarga = FabricaUI.Texto(cargaRaiz.transform, Textos.Carga, base_, new Vector2(0f, 150f), 2f, FabricaUI.CorTexto,
                TextAnchor.MiddleCenter, new Vector2(400f, 24f));
            for (int i = 0; i < 3; i++)
            {
                FabricaUI.Bloco(cargaRaiz.transform, base_, new Vector2(-48f + i * 48f, 124f), new Vector2(44f, 18f), new Color(0.02f, 0.05f, 0.12f, 0.9f));
                segmentosCarga[i] = FabricaUI.Bloco(cargaRaiz.transform, base_, new Vector2(-48f + i * 48f, 124f), new Vector2(38f, 12f), Color.white);
            }
            cargaRaiz.SetActive(false);

            // ---- mensagens, faixa de ato e dica
            mensagem = FabricaUI.Texto(raiz, "", centro, new Vector2(0f, 110f), 3f, FabricaUI.CorTexto, TextAnchor.MiddleCenter,
                new Vector2(900f, 80f));
            mensagem.contorno = true;
            bannerTitulo = FabricaUI.Texto(raiz, "", centro, new Vector2(0f, 30f), 5f, FabricaUI.CorDestaque, TextAnchor.MiddleCenter,
                new Vector2(940f, 70f));
            bannerTitulo.contorno = true;
            bannerTitulo.quebrarLinhas = false;
            bannerSubtitulo = FabricaUI.Texto(raiz, "", centro, new Vector2(0f, -22f), 2.5f, FabricaUI.CorTexto, TextAnchor.MiddleCenter,
                new Vector2(900f, 36f));
            bannerSubtitulo.contorno = true;

            Image caixa = FabricaUI.Painel(raiz, base_, new Vector2(0f, 46f), new Vector2(820f, 72f));
            caixa.raycastTarget = false;
            caixaDica = caixa.gameObject;
            textoDica = FabricaUI.Texto(caixa.transform, "", centro, Vector2.zero, 2f, FabricaUI.CorTexto, TextAnchor.MiddleCenter,
                new Vector2(780f, 60f));
            caixaDica.SetActive(false);
        }

        /// <summary>Atualiza tudo o que muda a cada quadro.</summary>
        public void Atualizar(EstadoPartida e, Jogador j, float prontidaoCompanheiro, bool arena)
        {
            AtualizarTudo(e, j);
            companheiroRecarga.fillAmount = 1f - Mathf.Clamp01(prontidaoCompanheiro);
            companheiroIcone.color = prontidaoCompanheiro >= 1f ? Color.white : new Color(0.6f, 0.6f, 0.7f);
            gols.Texto = arena ? string.Format(Textos.FormatoGols, e.Gols, EstadoPartida.GolsParaVencer) : "";

            // carga do chute e superchute
            int nivel = j != null ? j.NivelCarga : 0;
            bool superchute = j != null && j.SuperchutePronto;
            cargaRaiz.SetActive(nivel > 0 || superchute);
            if (cargaRaiz.activeSelf)
            {
                textoCarga.Texto = superchute ? Textos.Superchute : Textos.Carga;
                textoCarga.color = superchute ? FabricaUI.CorDestaque : FabricaUI.CorTexto;
                Color[] cores = { new Color(0.45f, 0.86f, 0.5f), new Color(0.96f, 0.78f, 0.3f), new Color(1f, 0.42f, 0.36f) };
                for (int i = 0; i < 3; i++)
                {
                    bool aceso = superchute || nivel > i;
                    segmentosCarga[i].color = aceso ? cores[i] : new Color(0.25f, 0.3f, 0.4f);
                }
            }
        }

        /// <summary>Vidas, corações, escudo, carta e barra da torcida.</summary>
        public void AtualizarTudo(EstadoPartida e, Jogador j)
        {
            for (int i = 0; i < MaxVidas; i++)
                iconesVida[i].color = i < e.Vidas ? Color.white : new Color(0.3f, 0.3f, 0.35f, 0.6f);
            if (e.Coracoes < coracoesAtuais)
                tempoPiscar = 0.6f;
            coracoesAtuais = e.Coracoes;
            for (int i = 0; i < MaxCoracoes; i++)
                coracoes[i].sprite = i < e.Coracoes ? coracaoCheio : coracaoVazio;
            escudo.enabled = e.Escudo;

            float alvo = e.Torcida.Fracao;
            barraTorcida.fillAmount = Mathf.MoveTowards(barraTorcida.fillAmount, alvo, Time.unscaledDeltaTime * 1.5f);
            if (Mathf.Abs(barraTorcida.fillAmount - alvo) > 0.5f)
                barraTorcida.fillAmount = alvo;
            bool pronto = e.Torcida.EspecialPronto;
            avisoEspecial.enabled = pronto;
            barraTorcida.color = pronto ? Color.Lerp(FabricaUI.CorDestaque, Color.white, Mathf.PingPong(Time.unscaledTime * 3f, 1f)) : FabricaUI.CorDestaque;

            bool canhaoAtivo = j != null && j.CanhaoRestante > 0f;
            cartaSlot.enabled = e.Carta == CartaPoder.ChuteDoCanhao || canhaoAtivo;
            cartaSlot.color = canhaoAtivo ? new Color(1f, 1f, 1f, 0.55f) : Color.white;
            rotuloEspaco.enabled = e.Carta != CartaPoder.Nenhuma || pronto;
            barraCanhao.enabled = canhaoAtivo;
            if (canhaoAtivo)
                barraCanhao.fillAmount = j.CanhaoRestante / Poderes.DuracaoCanhao;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;

            // mensagem central: aparece e some
            if (tempoMensagem > 0f)
            {
                tempoMensagem -= dt;
                float a = Mathf.Clamp01(tempoMensagem / 0.35f) * Mathf.Clamp01((duracaoMensagem - tempoMensagem) / 0.1f + 0.3f);
                Color c = corMensagem;
                c.a = a;
                mensagem.color = c;
                if (tempoMensagem <= 0f)
                    mensagem.Texto = "";
            }

            // faixa do ato: entra, fica e sai
            tempoBanner += dt;
            const float duracaoBanner = 2.8f;
            float ab = 0f;
            if (tempoBanner < duracaoBanner)
                ab = Mathf.Clamp01(tempoBanner / 0.3f) * Mathf.Clamp01((duracaoBanner - tempoBanner) / 0.5f);
            Color ct = bannerTitulo.color;
            ct.a = ab;
            bannerTitulo.color = ct;
            Color cs = bannerSubtitulo.color;
            cs.a = ab;
            bannerSubtitulo.color = cs;

            if (tempoCombo > 0f)
            {
                tempoCombo -= dt;
                if (tempoCombo <= 0f)
                    combo.Texto = "";
            }

            // corações piscam quando perde um
            if (tempoPiscar > 0f)
            {
                tempoPiscar -= dt;
                bool apagado = tempoPiscar > 0f && Mathf.Repeat(tempoPiscar * 10f, 1f) < 0.5f;
                for (int i = 0; i < MaxCoracoes; i++)
                    coracoes[i].enabled = !apagado;
            }

            if (avisoEspecial.enabled)
            {
                Color c = avisoEspecial.color;
                c.a = 0.6f + Mathf.PingPong(Time.unscaledTime * 2f, 0.4f);
                avisoEspecial.color = c;
            }
        }

        public void Mensagem(string texto, Color cor, float duracao)
        {
            mensagem.Texto = texto;
            corMensagem = cor;
            mensagem.color = cor;
            duracaoMensagem = duracao;
            tempoMensagem = duracao;
        }

        public void Banner(string titulo, string subtitulo)
        {
            bannerTitulo.Texto = titulo;
            bannerSubtitulo.Texto = subtitulo ?? "";
            tempoBanner = 0f;
        }

        /// <summary>Mostra a dica de uma placa (ou esconde, com null).</summary>
        public void Dica(string texto)
        {
            caixaDica.SetActive(!string.IsNullOrEmpty(texto));
            textoDica.Texto = texto ?? "";
        }

        public void PiscarCoracoes()
        {
            tempoPiscar = 0.6f;
        }

        public void Combo(int n)
        {
            combo.Texto = string.Format(Textos.FormatoCombo, n);
            tempoCombo = 1.6f;
        }

        public string TextoMensagem
        {
            get { return mensagem.Texto; }
        }

        public bool DicaVisivel
        {
            get { return caixaDica.activeSelf; }
        }
    }
}
