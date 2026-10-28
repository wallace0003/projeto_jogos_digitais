using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>Monta as camadas de fundo (parallax): céu, nuvens, silhueta de Estocolmo e Gamla Stan.</summary>
    public static class Fundo
    {
        /// <param name="rolagemAutomatica">No menu o fundo rola sozinho (efeito de viagem).</param>
        public static void Criar(Transform cam, float rolagemAutomatica = 0f)
        {
            float a = rolagemAutomatica;
            Camada("fundo_ceu", cam, Ordem.Ceu, 1f, 1f, 0f, 0f, a);
            Camada("fundo_nuvens", cam, Ordem.Nuvens, 0.96f, 0.95f, 3.4f, -0.25f - a * 0.05f, a);
            Camada("fundo_cidade", cam, Ordem.Cidade, 0.9f, 0.9f, 0.9f, -a * 0.1f, a);
            Camada("fundo_gamla_stan", cam, Ordem.GamlaStan, 0.72f, 0.85f, 0.3f, -a * 0.28f, a);
        }

        /// <summary>
        /// Cais de madeira com o canal embaixo, para as telas de menu (que não têm
        /// fase): o topo das tábuas fica em <paramref name="yTopo"/>.
        /// </summary>
        public static void Cais(float yTopo, float largura)
        {
            var pai = new GameObject("Cais (menu)").transform;
            Faixa(pai, "doca_m", yTopo - 0.5f, largura, 1f, Ordem.Chao);
            Faixa(pai, "agua_topo_0", yTopo - 1.5f, largura, 1f, Ordem.AguaTopo);
            Faixa(pai, "agua_fundo_0", yTopo - 4.5f, largura, 5f, Ordem.AguaTopo);
        }

        static void Faixa(Transform pai, string sprite, float yCentro, float largura, float altura, int ordem)
        {
            Sprite s = Arte.Sprite(sprite);
            if (s == null)
                return;
            SpriteRenderer sr = Efeitos.NovoSprite(sprite, pai, s, ordem);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(largura, altura);
            sr.transform.position = new Vector3(0f, yCentro, 0f);
        }

        static CamadaParallax Camada(string sprite, Transform cam, int ordem, float fx, float fy, float dy, float vel, float auto)
        {
            CamadaParallax c = CamadaParallax.Criar(sprite, cam, ordem, fx, fy, dy, vel);
            if (c != null && auto != 0f)
                c.usarTempoReal = true;
            return c;
        }

        /// <summary>
        /// Arquibancada do estádio Råsunda no ato 3: fica parada no mundo, atrás do
        /// gramado, de <paramref name="xInicio"/> até <paramref name="xFim"/>. A torcida pula.
        /// </summary>
        public static void Estadio(float xInicio, float xFim, float chaoY)
        {
            Sprite[] quadros = Arte.Anim("estadio_arquibancada");
            if (quadros.Length == 0)
                return;
            var pai = new GameObject("Estádio (arquibancada)").transform;
            float w = quadros[0].bounds.size.x;
            SpriteRenderer sr = Efeitos.NovoSprite("Arquibancada", pai, quadros[0], Ordem.Estadio);
            sr.drawMode = SpriteDrawMode.Tiled;
            float largura = xFim - xInicio;
            int copias = Mathf.Max(1, Mathf.CeilToInt(largura / w));
            sr.size = new Vector2(w * copias, quadros[0].bounds.size.y);
            sr.transform.position = new Vector3(xInicio + w * copias * 0.5f, chaoY - 0.05f, 0f);
            var anim = sr.gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
            anim.Tocar("estadio_arquibancada", 2.5f);
        }
    }
}
