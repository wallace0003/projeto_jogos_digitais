using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// O gol da final (trave, travessão e rede). A bola que cruza a linha por
    /// baixo do travessão é gol; o travessão e o fundo da rede são sólidos.
    /// </summary>
    public class Gol : MonoBehaviour
    {
        float linhaDoGol;
        float chao;

        public float LinhaDoGol
        {
            get { return linhaDoGol; }
        }

        /// <summary>Centro da boca do gol (para efeitos).</summary>
        public Vector3 Centro
        {
            get { return new Vector3(linhaDoGol + 0.8f, chao + Legenda.AlturaGol * 0.5f, 0f); }
        }

        /// <param name="linhaGol">x da linha do gol (poste da frente).</param>
        /// <param name="chaoY">Altura do gramado.</param>
        public static Gol Criar(float linhaGol, float chaoY)
        {
            float altura = Legenda.AlturaGol;
            float fundo = Legenda.ProfundidadeGol;

            // a rede: gatilho que conta o gol
            var go = new GameObject("Gol");
            go.layer = Camadas.Item;
            go.transform.position = new Vector3(linhaGol, chaoY, 0f);
            Gol g = go.AddComponent<Gol>();
            g.linhaDoGol = linhaGol;
            g.chao = chaoY;
            var rede = go.AddComponent<BoxCollider2D>();
            rede.isTrigger = true;
            rede.size = new Vector2(fundo - 0.45f, altura - 0.15f);
            rede.offset = new Vector2(0.35f + rede.size.x * 0.5f, rede.size.y * 0.5f);

            Solido(go.transform, "Travessão", new Vector2(fundo + 0.1f, 0.25f), new Vector2(fundo * 0.5f, altura + 0.125f));
            Solido(go.transform, "Fundo da rede", new Vector2(0.3f, altura + 0.4f), new Vector2(fundo + 0.15f, (altura + 0.4f) * 0.5f));

            Efeitos.NovoSprite("Sprite", go.transform, Arte.Sprite("gol"), Ordem.Gol);
            return g;
        }

        static void Solido(Transform pai, string nome, Vector2 tamanho, Vector2 centroLocal)
        {
            var go = new GameObject(nome);
            go.layer = Camadas.Chao;
            go.transform.SetParent(pai, false);
            go.transform.localPosition = centroLocal;
            go.AddComponent<BoxCollider2D>().size = tamanho;
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            Bola b = outro.GetComponent<Bola>();
            if (b == null || !b.RegistrarGol())
                return;
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.Gol(b);
        }
    }
}
