using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Barco que vai e volta no canal (entre '&lt;' e '&gt;' no mapa); quem está em
    /// cima é levado junto. O convés fica no topo da célula do barco.
    /// </summary>
    public class Barco : MonoBehaviour
    {
        public Vector2 pontoA;
        public Vector2 pontoB;
        public float velocidade = 2.2f;

        /// <summary>Velocidade atual (o jogador soma isso ao próprio movimento).</summary>
        public Vector2 Velocidade { get; private set; }

        Rigidbody2D rb;
        float t;
        float duracao;
        Transform visual;

        /// <param name="a">Centro do convés no limite esquerdo.</param>
        /// <param name="b">Centro do convés no limite direito.</param>
        /// <param name="inicio">Centro do convés onde ele começa.</param>
        public static Barco Criar(Vector2 a, Vector2 b, Vector2 inicio)
        {
            var go = new GameObject("Barco");
            go.layer = Camadas.Chao;
            go.transform.position = inicio;
            Barco barco = go.AddComponent<Barco>();
            barco.pontoA = a;
            barco.pontoB = b;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(Legenda.LarguraBarco - 0.2f, 0.5f);
            col.offset = new Vector2(0f, -0.25f);
            barco.visual = Efeitos.NovoSprite("Sprite", go.transform, Arte.Sprite("barco"), Ordem.Objetos).transform;
            // começa no ponto certo do vaivém
            float total = Vector2.Distance(a, b);
            float s = total > 0.01f ? Vector2.Distance(a, inicio) / total : 0f;
            barco.t = Mathf.Acos(Mathf.Clamp(1f - 2f * s, -1f, 1f)) / Mathf.PI;
            return barco;
        }

        void Awake()
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        void Start()
        {
            duracao = Mathf.Max(0.5f, Vector2.Distance(pontoA, pontoB) / velocidade);
            t *= duracao;
        }

        void FixedUpdate()
        {
            t += Time.fixedDeltaTime;
            // vai e volta suavemente (movimento senoidal)
            float s = 0.5f - 0.5f * Mathf.Cos(t / duracao * Mathf.PI);
            Vector2 alvo = Vector2.Lerp(pontoA, pontoB, s);
            Velocidade = (alvo - rb.position) / Time.fixedDeltaTime;
            rb.MovePosition(alvo);
            // balança de leve na água
            if (visual != null)
                visual.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 2.2f) * 0.04f, 0f);
        }
    }
}
