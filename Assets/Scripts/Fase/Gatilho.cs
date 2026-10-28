using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>Linha invisível que dispara um evento quando o jogador passa (quiz do estádio, arena).</summary>
    public class Gatilho : MonoBehaviour
    {
        public enum Tipo
        {
            QuizEstadio,
            Arena,
        }

        public Tipo tipo;
        bool disparado;

        public static Gatilho Criar(Tipo tipo, float x, float alturaMapa)
        {
            var go = new GameObject("Gatilho " + tipo);
            go.layer = Camadas.Item;
            go.transform.position = new Vector3(x, alturaMapa * 0.5f, 0f);
            Gatilho g = go.AddComponent<Gatilho>();
            g.tipo = tipo;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.5f, alturaMapa + 10f);
            return g;
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            if (disparado || outro.GetComponent<Jogador>() == null || GerenciadorFase.Instancia == null)
                return;
            if (GerenciadorFase.Instancia.GatilhoAtingido(this))
                disparado = true;
        }
    }
}
