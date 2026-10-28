using System.Collections;
using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Ponte levadiça sobre o canal (trecho 'L' do mapa). Começa levantada (em
    /// pé, presa na margem esquerda) e só baixa quando o sino é tocado com a
    /// cavadinha. Abaixada, vira chão (colisor ligado).
    /// </summary>
    public class PonteLevadica : MonoBehaviour
    {
        const float DuracaoDescida = 1.4f;

        Transform pivo;
        BoxCollider2D col;
        bool baixada;
        bool descendo;

        public bool Baixada
        {
            get { return baixada; }
        }

        /// <param name="xInicio">Primeira coluna do tablado.</param>
        /// <param name="y">Linha (y) do tablado: o pé pisa em y + 1.</param>
        public static PonteLevadica Criar(int xInicio, int y, int largura)
        {
            var go = new GameObject("Ponte levadiça");
            go.layer = Camadas.Chao;
            go.transform.position = new Vector3(xInicio + largura * 0.5f, y + 0.5f, 0f);
            PonteLevadica p = go.AddComponent<PonteLevadica>();

            p.col = go.AddComponent<BoxCollider2D>();
            p.col.size = new Vector2(largura, 1f);
            p.col.enabled = false;

            // o tablado gira em volta da dobradiça (canto de cima, na margem esquerda)
            p.pivo = new GameObject("Dobradiça").transform;
            p.pivo.SetParent(go.transform, false);
            p.pivo.position = new Vector3(xInicio, y + 1f, 0f);
            for (int i = 0; i < largura; i++)
            {
                string nome = i == 0 ? "ponte_ponta" : "ponte";
                SpriteRenderer sr = Efeitos.NovoSprite("Tábua", p.pivo, Arte.Sprite(nome), Ordem.Objetos);
                sr.transform.localPosition = new Vector3(i + 0.5f, -0.5f, 0f);
            }
            p.pivo.localRotation = Quaternion.Euler(0f, 0f, 90f);
            return p;
        }

        /// <summary>Abaixa a ponte (uma vez só).</summary>
        public void Baixar()
        {
            if (baixada || descendo)
                return;
            descendo = true;
            SomJogo.Tocar(Som.Ponte);
            StartCoroutine(Descer());
        }

        IEnumerator Descer()
        {
            float t = 0f;
            while (t < DuracaoDescida)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / DuracaoDescida);
                // começa devagar e acelera como algo pesado caindo
                float angulo = 90f * (1f - k * k);
                pivo.localRotation = Quaternion.Euler(0f, 0f, angulo);
                yield return null;
            }
            pivo.localRotation = Quaternion.identity;
            col.enabled = true;
            baixada = true;
            descendo = false;
            CameraSeguir.Tremer(0.25f, 0.25f);
            Vector3 c = transform.position;
            for (int i = -2; i <= 2; i++)
                Efeitos.Animacao("poeira", c + new Vector3(i * col.size.x * 0.22f, 0.5f, 0f), 16f, 1f);
        }

        /// <summary>Usado pelos testes: ponte já abaixada, sem animação.</summary>
        public void BaixarNaHora()
        {
            StopAllCoroutines();
            pivo.localRotation = Quaternion.identity;
            col.enabled = true;
            baixada = true;
            descendo = false;
        }
    }
}
