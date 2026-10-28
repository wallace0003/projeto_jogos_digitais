using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>Coração que sai do caixote: recupera 1 coração (ou enche a torcida, se já estiver cheio).</summary>
    public class Coracao : MonoBehaviour
    {
        float idade;
        Vector3 basePos;

        public static Coracao Criar(Vector3 posicao)
        {
            var go = new GameObject("Coração");
            go.layer = Camadas.Item;
            go.transform.position = posicao;
            Coracao c = go.AddComponent<Coracao>();
            c.basePos = posicao;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
            SpriteRenderer sr = Efeitos.NovoSprite("Sprite", go.transform, Arte.Sprite("coracao_0"), Ordem.Itens);
            var an = sr.gameObject.AddComponent<AnimadorSprite>();
            an.alvo = sr;
            an.Tocar("coracao", 4f);
            return c;
        }

        void Update()
        {
            idade += Time.deltaTime;
            // sai do caixote pulando e depois flutua
            float subida = Mathf.Min(1f, idade * 3f) * 0.6f;
            transform.position = basePos + Vector3.up * (subida + Mathf.Sin(idade * 4f) * 0.08f);
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            if (outro.GetComponent<Jogador>() == null || idade < 0.2f)
                return;
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.CoracaoColetado(transform.position);
            Destroy(gameObject);
        }
    }
}
