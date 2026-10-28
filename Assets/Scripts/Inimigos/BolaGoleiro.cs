using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Bola arremessada pelo goleiro em arco (ele também é "arremessador", GDD I03).
    /// Desvie pulando ou recuando: se acertar, tira 1 coração.
    /// </summary>
    public class BolaGoleiro : MonoBehaviour
    {
        const float TempoDeVoo = 1.05f;
        const float EscalaGravidade = 1.6f;

        Rigidbody2D rb;
        float vida = 3f;

        public static BolaGoleiro Criar(Vector3 origem, Vector3 alvo)
        {
            var go = new GameObject("Bola do goleiro");
            go.layer = Camadas.Projetil;
            go.transform.position = origem;
            BolaGoleiro b = go.AddComponent<BolaGoleiro>();
            float g = ParametrosJogador.GravidadeMundo * EscalaGravidade;
            float vx = (alvo.x - origem.x) / TempoDeVoo;
            float vy = (alvo.y - origem.y + 0.5f * g * TempoDeVoo * TempoDeVoo) / TempoDeVoo;
            b.rb.linearVelocity = new Vector2(vx, vy);
            return b;
        }

        void Awake()
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = EscalaGravidade;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var c = gameObject.AddComponent<CircleCollider2D>();
            c.radius = 0.22f;
            c.isTrigger = true;
            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = Arte.Sprite("bola_goleiro");
            sr.sortingOrder = Ordem.Bola;
            Materiais2D.Aplicar(sr, false);
        }

        void Update()
        {
            transform.Rotate(0f, 0f, 540f * Time.deltaTime);
            vida -= Time.deltaTime;
            GerenciadorFase g = GerenciadorFase.Instancia;
            if (vida <= 0f || (g != null && (g.Terminou || transform.position.y < g.LimiteQueda)))
                Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            Jogador j = outro.GetComponent<Jogador>();
            if (j != null)
            {
                if (j.Vivo && !j.Invulneravel)
                    j.TomarDano(transform.position);
                Estourar();
                return;
            }
            if (outro.gameObject.layer == Camadas.Chao)
                Estourar();
        }

        void Estourar()
        {
            Efeitos.Animacao("poeira", transform.position, 18f, 0.8f);
            SomJogo.Tocar(Som.Quique, 0.6f);
            Destroy(gameObject);
        }
    }
}
