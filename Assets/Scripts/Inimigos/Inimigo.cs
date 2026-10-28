using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Base dos adversários (zagueiro carrinho, saltador e capitão). Encostar
    /// neles machuca (menos quando o jogador cai por cima: ele só quica). A
    /// bola tira vida deles; sem vida, saem de campo. São corpos Kinematic com
    /// colisor Trigger, movidos por código.
    /// </summary>
    public abstract class Inimigo : MonoBehaviour
    {
        public static readonly List<Inimigo> Todos = new List<Inimigo>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Todos.Clear();
        }

        protected SpriteRenderer sr;
        protected AnimadorSprite anim;
        protected Rigidbody2D rb;
        protected BoxCollider2D col;
        protected Transform visual;
        protected int direcao = -1;
        protected Vector3 posicaoInicial;
        protected bool superado;
        SpriteRenderer aviso;
        float timerAviso;
        float timerPisca;
        Transform estrelinhas;

        /// <summary>Limites (x) por onde ele anda. Definidos pelo construtor da fase.</summary>
        public float limiteEsquerda = float.MinValue;
        public float limiteDireita = float.MaxValue;

        public bool Superado
        {
            get { return superado; }
        }

        public Vector2 Centro
        {
            get { return col != null ? (Vector2)col.bounds.center : (Vector2)transform.position; }
        }

        public float Altura
        {
            get { return TamanhoColisor.y; }
        }

        /// <summary>Prefixo dos sprites (carrinho, saltador, capitao).</summary>
        protected abstract string Prefixo { get; }

        protected virtual Vector2 TamanhoColisor
        {
            get { return new Vector2(0.7f, 1.4f); }
        }

        /// <summary>Animação tocada ao nascer.</summary>
        protected virtual string AnimInicial
        {
            get { return Prefixo + "_andar"; }
        }

        /// <summary>Encostar nele agora machuca o jogador?</summary>
        protected abstract bool PodeMachucar { get; }

        protected abstract void Mover(float dt);

        /// <summary>A bola chegou nele: ele decide se defende ou perde vida.</summary>
        public abstract ResultadoGolpe ReceberBola(Bola bola);

        public static T Criar<T>(Vector3 posicao) where T : Inimigo
        {
            var go = new GameObject(typeof(T).Name);
            go.layer = Camadas.Inimigo;
            go.transform.position = posicao;
            return go.AddComponent<T>();
        }

        protected virtual void Awake()
        {
            posicaoInicial = transform.position;
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            AjustarColisor(false);

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);
            sr = visual.gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = Ordem.Inimigos;
            Materiais2D.Aplicar(sr, false);
            anim = visual.gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
            anim.Tocar(AnimInicial, 8f);
            anim.Dessincronizar();

            aviso = Efeitos.NovoSprite("Aviso", transform, Arte.Sprite("exclamacao"), Ordem.Efeitos, true);
            aviso.enabled = false;
        }

        protected virtual void OnEnable()
        {
            Todos.Add(this);
        }

        protected virtual void OnDisable()
        {
            Todos.Remove(this);
        }

        void FixedUpdate()
        {
            if (superado)
                return;
            Mover(Time.fixedDeltaTime);
            // os sprites olham para a direita; direção -1 = espelhado
            sr.flipX = direcao < 0;
        }

        protected virtual void Update()
        {
            float dt = Time.deltaTime;
            if (timerPisca > 0f)
            {
                timerPisca -= dt;
                sr.color = timerPisca > 0f ? new Color(1f, 0.55f, 0.5f) : Color.white;
            }
            if (timerAviso > 0f)
            {
                timerAviso -= dt;
                aviso.enabled = timerAviso > 0f && !superado;
                aviso.transform.localPosition = new Vector3(0f, Altura + 0.55f + Mathf.Abs(Mathf.Sin(Time.time * 10f)) * 0.12f, 0f);
            }
            if (estrelinhas != null)
            {
                estrelinhas.Rotate(0f, 0f, 240f * dt);
                for (int i = 0; i < estrelinhas.childCount; i++)
                    estrelinhas.GetChild(i).rotation = Quaternion.identity;
            }
        }

        /// <summary>Mostra o "!" (ataque vindo) ou o "?" (distraído) por um tempo.</summary>
        protected void MostrarAviso(string sprite, float duracao)
        {
            Sprite s = Arte.Sprite(sprite);
            if (s != null)
                aviso.sprite = s;
            timerAviso = duracao;
        }

        /// <summary>Estrelinhas girando em volta da cabeça (tonto/recuperando).</summary>
        protected void Tonto(bool ligado)
        {
            if (ligado && estrelinhas == null)
            {
                estrelinhas = new GameObject("Estrelinhas").transform;
                estrelinhas.SetParent(transform, false);
                estrelinhas.localPosition = new Vector3(0f, Altura + 0.25f, 0f);
                for (int i = 0; i < 3; i++)
                {
                    SpriteRenderer e = Efeitos.NovoSprite("estrelinha", estrelinhas, Arte.Sprite("estrelinha_" + (i % 2)), Ordem.Efeitos, true);
                    float a = i * Mathf.PI * 2f / 3f;
                    e.transform.localPosition = new Vector3(Mathf.Cos(a) * 0.45f, Mathf.Sin(a) * 0.18f, 0f);
                }
            }
            else if (!ligado && estrelinhas != null)
            {
                Destroy(estrelinhas.gameObject);
                estrelinhas = null;
            }
        }

        /// <summary>O companheiro de equipe chamou a atenção dele.</summary>
        public virtual void Distrair(float duracao)
        {
            if (superado)
                return;
            MostrarAviso("interrogacao", duracao);
        }

        /// <summary>O jogador perdeu uma vida: quem ainda está em campo volta ao lugar.</summary>
        public virtual void Reiniciar()
        {
            if (superado)
                return;
            transform.position = posicaoInicial;
            rb.position = posicaoInicial;
            timerAviso = 0f;
            aviso.enabled = false;
            Tonto(false);
        }

        protected void AjustarColisor(bool rasteiro)
        {
            Vector2 t = TamanhoColisor;
            if (rasteiro)
            {
                col.size = new Vector2(t.x * 1.9f, 0.55f);
                col.offset = new Vector2(direcao * t.x * 0.35f, 0.28f);
            }
            else
            {
                col.size = t;
                col.offset = new Vector2(0f, t.y * 0.5f);
            }
        }

        protected void Piscar()
        {
            timerPisca = 0.15f;
        }

        void OnTriggerStay2D(Collider2D outro)
        {
            if (superado)
                return;
            Jogador j = outro.GetComponent<Jogador>();
            if (j == null || !j.Vivo)
                return;
            bool vindoDeCima = j.transform.position.y > Centro.y + col.size.y * 0.25f && j.VelocidadeY < 1f;
            if (vindoDeCima)
            {
                // "chapéu": o jogador quica por cima, sem dano para ninguém
                j.Quicar();
                return;
            }
            if (PodeMachucar)
                j.TomarDano(Centro);
        }

        /// <summary>Sai de campo (superado pela bola).</summary>
        protected void SairDeCampo(Vector2 empurrao)
        {
            if (superado)
                return;
            superado = true;
            col.enabled = false;
            aviso.enabled = false;
            Tonto(false);
            sr.color = Color.white;
            anim.Tocar(Prefixo + "_atingido", 1f);
            SomJogo.Tocar(Som.Superado);
            CameraSeguir.Tremer(0.12f, 0.15f);
            AnelExpansivo.Criar(Centro, 1.3f, new Color(1f, 0.85f, 0.35f, 0.8f), 0.3f);
            Efeitos.Particulas("brilho", Centro, 6, Color.white, 2f, 4.5f, 0.45f, 3f, true);
            StartCoroutine(Sumir(empurrao));
        }

        IEnumerator Sumir(Vector2 empurrao)
        {
            // empurrado pela bolada, cai, fica um instante e some numa nuvem
            float t = 0f;
            Vector3 inicio = transform.position;
            while (t < 0.55f)
            {
                t += Time.deltaTime;
                float k = 1f - (1f - t / 0.55f) * (1f - t / 0.55f);
                transform.position = inicio + new Vector3(empurrao.x * 0.8f * k, 0.35f * Mathf.Sin(k * Mathf.PI), 0f);
                visual.localRotation = Quaternion.Euler(0f, 0f, -empurrao.x * 70f * k);
                Color c = sr.color;
                c.a = 1f - Mathf.Max(0f, t - 0.3f) / 0.25f;
                sr.color = c;
                yield return null;
            }
            Efeitos.Animacao("fumaca", (Vector3)Centro + Vector3.up * 0.2f, 12f, 1.4f);
            gameObject.SetActive(false);
        }

        // ---- utilidades para as subclasses
        protected Jogador JogadorPerto(float distanciaX, float distanciaY)
        {
            Jogador j = Jogador.Atual;
            if (j == null || !j.Vivo)
                return null;
            Vector2 d = (Vector2)j.transform.position - rb.position;
            if (Mathf.Abs(d.x) > distanciaX || Mathf.Abs(d.y) > distanciaY)
                return null;
            return j;
        }

        protected void OlharParaJogador()
        {
            Jogador j = Jogador.Atual;
            if (j != null)
                direcao = j.transform.position.x < rb.position.x ? -1 : 1;
        }

        protected bool TemChaoEm(Vector2 ponto, float distancia)
        {
            return Physics2D.Raycast(ponto, Vector2.down, distancia, Camadas.MascaraPisavel).collider != null;
        }

        protected bool TemParedeEm(Vector2 ponto, Vector2 tamanho)
        {
            return Physics2D.OverlapBox(ponto, tamanho, 0f, Camadas.MascaraSolida) != null;
        }

        /// <summary>Pode andar na direção atual? (sem parede, sem buraco e dentro dos limites)</summary>
        protected bool PodeAndar(float passo)
        {
            Vector2 pos = rb.position;
            float novoX = pos.x + direcao * passo;
            if (novoX < limiteEsquerda || novoX > limiteDireita)
                return false;
            Vector2 frente = pos + new Vector2(direcao * (TamanhoColisor.x * 0.5f + 0.1f), 0f);
            bool parede = TemParedeEm(frente + new Vector2(0f, 0.7f), new Vector2(0.1f, 0.6f));
            bool beirada = !TemChaoEm(frente + new Vector2(0f, 0.1f), 0.4f);
            return !parede && !beirada;
        }

        /// <summary>Anda na direção atual e vira na parede, na beirada ou no limite. Devolve true se virou.</summary>
        protected bool Patrulhar(float velocidade, float dt)
        {
            if (!PodeAndar(velocidade * dt))
            {
                direcao = -direcao;
                return true;
            }
            rb.MovePosition(rb.position + new Vector2(direcao * velocidade * dt, 0f));
            return false;
        }

        /// <summary>Avança na direção atual se puder. Devolve false se bateu.</summary>
        protected bool Avancar(float velocidade, float dt)
        {
            if (!PodeAndar(velocidade * dt))
                return false;
            rb.MovePosition(rb.position + new Vector2(direcao * velocidade * dt, 0f));
            return true;
        }
    }
}
