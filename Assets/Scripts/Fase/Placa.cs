using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>Placa de dica: mostra o texto no rodapé enquanto o jogador está perto.</summary>
    public class Placa : MonoBehaviour
    {
        public string texto;

        public static Placa Criar(Vector3 pe, string texto)
        {
            var go = new GameObject("Placa");
            go.layer = Camadas.Item;
            go.transform.position = pe;
            Placa p = go.AddComponent<Placa>();
            p.texto = texto;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(4f, 3f);
            col.offset = new Vector2(0.5f, 1.5f);
            Efeitos.NovoSprite("Sprite", go.transform, Arte.Sprite("placa"), Ordem.DecoracaoFundo + 5);
            return p;
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            if (outro.GetComponent<Jogador>() != null && GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.MostrarDica(this);
        }

        void OnTriggerExit2D(Collider2D outro)
        {
            if (outro.GetComponent<Jogador>() != null && GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.EsconderDica(this);
        }
    }
}
