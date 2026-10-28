using System.Collections;
using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Objetos sólidos do caminho: caixote (qualquer chute quebra), pilha de
    /// barris (só cai com chute pesado: carga máxima, superchute, canhão ou
    /// especial) e portão (abre quando o capitão é superado).
    /// </summary>
    public class Obstaculo : MonoBehaviour
    {
        public enum Tipo
        {
            Caixote,
            Barris,
            Portao,
        }

        public Tipo tipo;
        public bool comCoracao;

        BoxCollider2D col;
        SpriteRenderer sr;
        bool destruido;
        float timerAviso;

        public bool Destruido
        {
            get { return destruido; }
        }

        /// <summary>Cria o obstáculo com o pé (canto de baixo, centro) em <paramref name="pe"/>.</summary>
        public static Obstaculo Criar(Tipo tipo, Vector3 pe, bool comCoracao)
        {
            var go = new GameObject(tipo == Tipo.Caixote ? "Caixote" : (tipo == Tipo.Barris ? "Pilha de barris" : "Portão"));
            go.layer = Camadas.Chao;
            go.transform.position = pe;
            Obstaculo o = go.AddComponent<Obstaculo>();
            o.tipo = tipo;
            o.comCoracao = comCoracao;
            Vector2 tamanho;
            string sprite;
            switch (tipo)
            {
                case Tipo.Barris:
                    tamanho = new Vector2(Legenda.LarguraBarris, Legenda.AlturaBarris);
                    sprite = "barris";
                    break;
                case Tipo.Portao:
                    tamanho = new Vector2(1f, Legenda.AlturaPortao);
                    sprite = "portao";
                    break;
                default:
                    tamanho = new Vector2(1f, 1f);
                    sprite = "caixote";
                    break;
            }
            o.col = go.AddComponent<BoxCollider2D>();
            o.col.size = new Vector2(tamanho.x - 0.04f, tamanho.y);
            o.col.offset = new Vector2(0f, tamanho.y * 0.5f);
            o.sr = Efeitos.NovoSprite("Sprite", go.transform, Arte.Sprite(sprite), Ordem.Objetos);
            return o;
        }

        void Update()
        {
            timerAviso -= Time.deltaTime;
        }

        /// <summary>A bola bateu. Devolve true se o obstáculo caiu (a bola segue em frente).</summary>
        public bool ReceberBola(Bola bola)
        {
            if (destruido)
                return false;
            switch (tipo)
            {
                case Tipo.Caixote:
                    Quebrar();
                    return true;
                case Tipo.Barris:
                    if (bola.Parametros.Pesado)
                    {
                        Quebrar();
                        return true;
                    }
                    Avisar(Textos.BarrisPrecisamCarga);
                    StartCoroutine(Tremer());
                    return false;
                default:
                    Avisar(Textos.PortaoFechado);
                    return false;
            }
        }

        void Avisar(string texto)
        {
            if (timerAviso > 0f || GerenciadorFase.Instancia == null)
                return;
            timerAviso = 2.5f;
            GerenciadorFase.Instancia.Mensagem(texto, FabricaUI.CorDestaque, 1.8f);
        }

        void OnCollisionEnter2D(Collision2D c)
        {
            if (tipo == Tipo.Portao && c.collider.GetComponent<Jogador>() != null)
                Avisar(Textos.PortaoFechado);
        }

        /// <summary>Quebra o caixote ou derruba a pilha de barris.</summary>
        public void Quebrar()
        {
            if (destruido || tipo == Tipo.Portao)
                return;
            destruido = true;
            col.enabled = false;
            Vector3 centro = transform.position + Vector3.up * col.size.y * 0.5f;
            if (tipo == Tipo.Barris)
            {
                SomJogo.Tocar(Som.Barris);
                CameraSeguir.Tremer(0.3f, 0.3f);
                for (int i = 0; i < Legenda.AlturaBarris; i++)
                    Efeitos.Particulas("barril_pedaco", transform.position + Vector3.up * (i + 0.5f), 3, Color.white, 3f, 7f, 0.9f, 14f, false, 20f, 160f);
                Efeitos.Animacao("fumaca", centro, 10f, 3f);
            }
            else
            {
                SomJogo.Tocar(Som.Quebra);
                Efeitos.Particulas("lasca", centro, 8, Color.white, 2f, 5f, 0.7f, 14f, false, 30f, 150f);
                Efeitos.Animacao("poeira", centro, 16f, 1f);
            }
            if (comCoracao)
                Coracao.Criar(centro);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.ObstaculoQuebrado(this);
            Destroy(gameObject);
        }

        /// <summary>Abre o portão (ele sobe e some).</summary>
        public void Abrir()
        {
            if (tipo != Tipo.Portao || destruido)
                return;
            destruido = true;
            col.enabled = false;
            SomJogo.Tocar(Som.Portao);
            StartCoroutine(Subir());
        }

        IEnumerator Subir()
        {
            Vector3 inicio = transform.position;
            float t = 0f;
            while (t < 1.6f)
            {
                t += Time.deltaTime;
                transform.position = inicio + Vector3.up * (Legenda.AlturaPortao * Mathf.SmoothStep(0f, 1f, t / 1.6f));
                yield return null;
            }
            gameObject.SetActive(false);
        }

        IEnumerator Tremer()
        {
            Transform v = sr.transform;
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                v.localPosition = new Vector3(Mathf.Sin(t * 90f) * 0.06f, 0f, 0f);
                yield return null;
            }
            v.localPosition = Vector3.zero;
        }
    }
}
