using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>Onda de choque: um anel que cresce e some (superchute, gol, sino).</summary>
    public class AnelExpansivo : MonoBehaviour
    {
        /// <summary>O sprite "anel" tem 32 px = 2 unidades de diâmetro.</summary>
        const float DiametroSprite = 2f;

        float raio;
        float idade;
        float duracao = 0.4f;
        Color cor;
        SpriteRenderer sr;

        public static void Criar(Vector3 centro, float raio, Color cor, float duracao = 0.4f)
        {
            Sprite s = Arte.Sprite("anel");
            if (s == null)
                return;
            SpriteRenderer sr = Efeitos.NovoSprite("Anel", null, s, Ordem.Efeitos, true);
            sr.transform.position = centro;
            sr.color = cor;
            var a = sr.gameObject.AddComponent<AnelExpansivo>();
            a.raio = raio;
            a.cor = cor;
            a.duracao = duracao;
            a.sr = sr;
            a.Update();
        }

        void Update()
        {
            idade += Time.deltaTime;
            float t = Mathf.Clamp01(idade / duracao);
            float diametro = Mathf.Lerp(0.4f, raio * 2f, 1f - (1f - t) * (1f - t));
            transform.localScale = Vector3.one * (diametro / DiametroSprite);
            Color c = cor;
            c.a = cor.a * (1f - t);
            sr.color = c;
            if (idade >= duracao)
                Destroy(gameObject);
        }
    }
}
