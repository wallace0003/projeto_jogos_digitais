using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>Canal: quem cai perde 1 coração e volta ao checkpoint; a bola volta para o pé.</summary>
    public class ZonaAgua : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D outro)
        {
            Bola bola = outro.GetComponent<Bola>();
            if (bola != null && bola.Voando)
            {
                Efeitos.Particulas("gota", bola.transform.position, 8, Color.white, 2f, 4f, 0.5f, 10f, false, 50f, 130f);
                SomJogo.Tocar(Som.Splash, 0.5f);
                bola.Chamar();
                return;
            }
            Jogador j = outro.GetComponent<Jogador>();
            if (j == null || !j.Vivo)
                return;
            SomJogo.Tocar(Som.Splash);
            Efeitos.Particulas("gota", j.transform.position + Vector3.up * 0.3f, 14, Color.white, 3f, 6f, 0.6f, 12f, false, 45f, 135f);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.JogadorCaiuNaAgua();
        }
    }
}
