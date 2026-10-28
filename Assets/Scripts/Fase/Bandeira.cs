using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Checkpoint (bandeira). Ao passar, a flâmula sobe tremulando e o jogador
    /// volta para cá se cair no canal ou perder uma vida.
    /// </summary>
    public class Bandeira : MonoBehaviour
    {
        public int indice;
        AnimadorSprite anim;
        bool ativa;

        public bool Ativa
        {
            get { return ativa; }
        }

        /// <summary>Onde o jogador renasce (um pouco à frente do mastro).</summary>
        public Vector3 PontoRenascer
        {
            get { return transform.position + new Vector3(0.9f, 0.05f, 0f); }
        }

        public static Bandeira Criar(Vector3 pe, int indice)
        {
            var go = new GameObject("Checkpoint " + indice);
            go.layer = Camadas.Item;
            go.transform.position = pe;
            Bandeira b = go.AddComponent<Bandeira>();
            b.indice = indice;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 3f);
            col.offset = new Vector2(0f, 1.5f);
            SpriteRenderer sr = Efeitos.NovoSprite("Sprite", go.transform, Arte.Sprite("bandeira_off"), Ordem.Bandeira);
            b.anim = sr.gameObject.AddComponent<AnimadorSprite>();
            b.anim.alvo = sr;
            b.anim.Tocar("bandeira_off", 1f);
            return b;
        }

        /// <summary>Marca como ativa sem efeitos (o primeiro checkpoint já começa assim).</summary>
        public void JaAtiva()
        {
            ativa = true;
            anim.Tocar("bandeira_on", 8f);
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            if (ativa || outro.GetComponent<Jogador>() == null)
                return;
            ativa = true;
            anim.Tocar("bandeira_on", 8f);
            SomJogo.Tocar(Som.Checkpoint);
            Efeitos.Particulas("brilho", transform.position + Vector3.up * 2.5f, 8, new Color(0.6f, 1f, 0.5f), 2f, 4f, 0.6f, 2f, true);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.CheckpointAtivado(this);
        }
    }
}
