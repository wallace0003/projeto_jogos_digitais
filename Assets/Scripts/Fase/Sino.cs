using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>
    /// Sino pendurado num poste alto, antes do canal da ponte levadiça. Só a
    /// cavadinha (a bola alta) alcança: tocado, ele baixa a ponte.
    /// </summary>
    public class Sino : MonoBehaviour
    {
        /// <summary>Um pouco maior que o raio usado pelo AnalisadorMapa (a análise é mais exigente que o jogo).</summary>
        public const float RaioAcerto = 0.8f;
        /// <summary>O gancho do poste fica no topo, centralizado: poste e sino no mesmo x.</summary>
        const float DeslocamentoPoste = 0f;

        PonteLevadica ponte;
        AnimadorSprite anim;
        bool tocado;
        float timerAviso;

        public bool Tocado
        {
            get { return tocado; }
        }

        public PonteLevadica Ponte
        {
            get { return ponte; }
        }

        /// <param name="centro">Centro do sino.</param>
        /// <param name="chaoY">Altura do chão embaixo dele (onde o poste começa).</param>
        public static Sino Criar(Vector3 centro, float chaoY, PonteLevadica ponte)
        {
            var go = new GameObject("Sino");
            go.layer = Camadas.Item;
            go.transform.position = centro;
            Sino s = go.AddComponent<Sino>();
            s.ponte = ponte;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = RaioAcerto;

            // poste de 4 tiles saindo do chão; o sino pendura no braço do topo
            SpriteRenderer poste = Efeitos.NovoSprite("Poste", go.transform, Arte.Sprite("sino_poste"), Ordem.DecoracaoFundo);
            poste.transform.position = new Vector3(centro.x + DeslocamentoPoste, chaoY, 0f);

            SpriteRenderer sr = Efeitos.NovoSprite("Sprite", go.transform, Arte.Sprite("sino_0"), Ordem.Objetos);
            s.anim = sr.gameObject.AddComponent<AnimadorSprite>();
            s.anim.alvo = sr;
            s.anim.Tocar("sino_0", 1f);
            return s;
        }

        void Update()
        {
            timerAviso -= Time.deltaTime;
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            Bola b = outro.GetComponent<Bola>();
            if (b == null || !b.Voando || tocado)
                return;
            if (b.UltimoChute == TipoChute.Cavadinha)
            {
                Tocar();
                return;
            }
            if (timerAviso <= 0f && GerenciadorFase.Instancia != null)
            {
                timerAviso = 2f;
                GerenciadorFase.Instancia.Mensagem(Textos.SinoPrecisaCavadinha, FabricaUI.CorDestaque, 1.6f);
            }
        }

        /// <summary>Toca o sino e baixa a ponte (público para os testes).</summary>
        public void Tocar()
        {
            if (tocado)
                return;
            tocado = true;
            anim.Tocar(new[] { Arte.Sprite("sino_1"), Arte.Sprite("sino_2"), Arte.Sprite("sino_1"), Arte.Sprite("sino_0") }, 8f, true);
            SomJogo.Tocar(Som.Sino);
            AnelExpansivo.Criar(transform.position, 2.2f, new Color(1f, 0.9f, 0.5f, 0.9f), 0.5f);
            Efeitos.Particulas("brilho", transform.position, 10, new Color(1f, 0.9f, 0.5f), 2f, 5f, 0.6f, 2f, true);
            if (ponte != null)
                ponte.Baixar();
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.SinoTocado(this);
            Invoke("PararDeBalancar", 2.5f);
        }

        void PararDeBalancar()
        {
            anim.Tocar("sino_0", 1f, true, true);
        }
    }
}
