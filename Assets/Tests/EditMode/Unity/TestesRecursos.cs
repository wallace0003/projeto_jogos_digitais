using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CincoEstrelas.Testes
{
    /// <summary>
    /// Confere, dentro do Unity, se tudo o que o jogo carrega existe: sprites e
    /// animações usados pelo código, o mapa da fase, a fonte e os sons.
    /// </summary>
    [TestFixture]
    public class TestesRecursos
    {
        /// <summary>Sprites avulsos que o código pede pelo nome.</summary>
        static IEnumerable<string> SpritesUsados()
        {
            var nomes = new List<string>
            {
                "bola", "bola_goleiro", "bola_fogo_0", "caixote", "barris", "sino_0", "sino_1", "sino_2", "sino_poste",
                "ponte", "ponte_ponta", "barco", "bandeira_off", "placa", "portao", "gol", "carta_canhao", "carta_caneleira",
                "estrela_cheia", "estrela_vazia", "estrela_grande_cheia", "estrela_grande_vazia", "luz_suave", "anel",
                "rastro", "exclamacao", "interrogacao", "estrelinha_0", "estrelinha_1", "icone_companheiro", "icone_vida",
                "coracao_0", "ui_painel", "ui_botao", "ui_branco", "ui_coracao_cheio", "ui_coracao_vazio",
                "fundo_ceu", "fundo_nuvens", "fundo_cidade", "fundo_gamla_stan", "doca_m", "agua_topo_0", "agua_fundo_0",
                "capitao_salto_0", "capitao_salto_1",
            };
            foreach (string material in new[] { "pedra_", "rua_", "grama_" })
            {
                for (int m = 0; m < 16; m++)
                    nomes.Add(material + m);
            }
            foreach (string p in new[] { "e", "m", "d", "u" })
            {
                nomes.Add("doca_" + p);
                nomes.Add("tabua_" + p);
            }
            foreach (char c in "ltbmkr")
            {
                string deco = ConstrutorFase.NomeDecoracao(c);
                nomes.Add(Arte.Anim(deco).Length > 0 ? Arte.Anim(deco)[0].name : deco);
            }
            return nomes;
        }

        [Test]
        public void TodosOsSpritesUsadosExistem()
        {
            var faltando = new List<string>();
            foreach (string nome in SpritesUsados())
            {
                if (!Arte.Existe(nome))
                    faltando.Add(nome);
            }
            Assert.AreEqual(0, faltando.Count, "sprites faltando: " + string.Join(", ", faltando.ToArray()));
        }

        [TestCase("camisa10_parado", 4)]
        [TestCase("camisa10_correr", 6)]
        [TestCase("camisa10_chute", 3)]
        [TestCase("camisa10_carregar", 2)]
        [TestCase("camisa10_cavadinha", 2)]
        [TestCase("camisa10_bicicleta", 4)]
        [TestCase("camisa10_pedalada", 4)]
        [TestCase("camisa10_embaixadinha", 4)]
        [TestCase("camisa10_vitoria", 2)]
        [TestCase("camisa10_pulo", 1)]
        [TestCase("camisa10_queda", 1)]
        [TestCase("camisa10_dano", 1)]
        [TestCase("camisa10_chamar", 1)]
        [TestCase("companheiro_correr", 6)]
        [TestCase("companheiro_parado", 2)]
        [TestCase("companheiro_passe", 2)]
        [TestCase("companheiro_comemorar", 2)]
        [TestCase("carrinho_andar", 4)]
        [TestCase("carrinho_deslizar", 2)]
        [TestCase("carrinho_preparar", 1)]
        [TestCase("carrinho_sentado", 1)]
        [TestCase("carrinho_atingido", 1)]
        [TestCase("carrinho_distraido", 1)]
        [TestCase("saltador_parado", 2)]
        [TestCase("saltador_pulo", 1)]
        [TestCase("saltador_agachar", 1)]
        [TestCase("saltador_aterrissar", 1)]
        [TestCase("saltador_atingido", 1)]
        [TestCase("saltador_distraido", 1)]
        [TestCase("capitao_andar", 4)]
        [TestCase("capitao_deslizar", 2)]
        [TestCase("capitao_tonto", 2)]
        [TestCase("capitao_preparar", 1)]
        [TestCase("capitao_sentado", 1)]
        [TestCase("capitao_atingido", 1)]
        [TestCase("capitao_distraido", 1)]
        [TestCase("goleiro_parado", 2)]
        [TestCase("goleiro_correr", 4)]
        [TestCase("goleiro_arremesso", 2)]
        [TestCase("goleiro_pronto", 1)]
        [TestCase("goleiro_espalmar", 1)]
        [TestCase("goleiro_segurar", 1)]
        [TestCase("goleiro_mergulho", 1)]
        [TestCase("goleiro_caido", 1)]
        [TestCase("goleiro_levantar", 1)]
        [TestCase("bola_fogo", 4)]
        [TestCase("coracao", 2)]
        [TestCase("bandeira_on", 4)]
        [TestCase("agua_topo", 4)]
        [TestCase("agua_fundo", 2)]
        [TestCase("mastro", 3)]
        [TestCase("brilho", 4)]
        [TestCase("poeira", 5)]
        [TestCase("fumaca", 5)]
        [TestCase("confete", 3)]
        [TestCase("lasca", 3)]
        [TestCase("barril_pedaco", 2)]
        [TestCase("gota", 2)]
        [TestCase("estadio_arquibancada", 2)]
        public void AnimacaoTemTodosOsQuadros(string nome, int quadros)
        {
            Sprite[] q = Arte.Anim(nome);
            Assert.AreEqual(quadros, q.Length, "animação " + nome);
            foreach (Sprite s in q)
                Assert.IsNotNull(s);
        }

        [Test]
        public void Personagens_Tem32PixelsE16PixelsPorUnidade()
        {
            Sprite s = Arte.Sprite("camisa10_parado_0");
            Assert.AreEqual(32f, s.rect.width, 0.001f);
            Assert.AreEqual(32f, s.rect.height, 0.001f);
            Assert.AreEqual(2f, s.bounds.size.y, 0.001f, "32 px a 16 px por unidade = 2 unidades");
        }

        [Test]
        public void BarcoTemOConvesNoPivo()
        {
            Sprite b = Arte.Sprite("barco");
            Assert.AreEqual(0.5f, b.pivot.y / b.rect.height, 0.001f);
        }

        [Test]
        public void MapaDaFaseCarregaPeloResourcesEPassaNaAnalise()
        {
            TextAsset t = Resources.Load<TextAsset>("Fases/" + Cenas.MapaDaFase);
            Assert.IsNotNull(t, "Resources/Fases/" + Cenas.MapaDaFase + ".txt");
            MapaFase m = MapaFase.Ler(t.text);
            Assert.AreEqual(0, m.Validar().Count, string.Join("; ", m.Validar().ToArray()));
            Assert.AreEqual(0, AnalisadorMapa.Analisar(m).Count, string.Join("; ", AnalisadorMapa.Analisar(m).ToArray()));
        }

        [Test]
        public void FonteCabeNoAtlas()
        {
            Assert.IsNotNull(FontePixel.Atlas);
            foreach (char c in GlifosFonte.Todos().Keys)
            {
                FontePixel.Glifo g;
                Assert.IsTrue(FontePixel.TentarGlifo(c, out g), "letra fora do atlas: " + c);
            }
        }

        [Test]
        public void TodosOsSonsSaoGerados()
        {
            foreach (Som s in System.Enum.GetValues(typeof(Som)))
            {
                AudioClip c = Sintetizador.CriarEfeito(s);
                Assert.IsNotNull(c, s.ToString());
                Assert.Greater(c.samples, 100, s.ToString());
                Object.DestroyImmediate(c);
            }
        }

        [TestCase(Trilha.Menu)]
        [TestCase(Trilha.Ato1)]
        [TestCase(Trilha.Ato2)]
        [TestCase(Trilha.Final)]
        [TestCase(Trilha.Vitoria)]
        [TestCase(Trilha.Derrota)]
        public void MusicaEhGerada(Trilha trilha)
        {
            AudioClip c = Sintetizador.CriarMusica(trilha);
            Assert.IsNotNull(c);
            Assert.Greater(c.length, 3f);
            Object.DestroyImmediate(c);
        }
    }
}
