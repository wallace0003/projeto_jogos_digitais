using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CincoEstrelas.Testes
{
    /// <summary>
    /// Garante que o projeto está configurado como o jogo espera: Unity
    /// 6000.5.10f1, 2D com URP e 2D Renderer, Input System, cenas na Build na
    /// ordem certa, camadas de física e sprites em pixel art.
    /// </summary>
    [TestFixture]
    public class TestesProjeto
    {
        const string GuidRenderer2D = "424799608f7334c24bf367e4bbfa7f9a";

        [Test]
        public void ProjetoEhDaVersao6000_5_10f1()
        {
            string versao = File.ReadAllText("ProjectSettings/ProjectVersion.txt");
            Assert.IsTrue(versao.Contains("m_EditorVersion: 6000.5.10f1"), versao);
        }

        [Test]
        public void Editor_EstaEmModo2D()
        {
            Assert.AreEqual(EditorBehaviorMode.Mode2D, EditorSettings.defaultBehaviorMode);
        }

        [Test]
        public void InputSystemEstaAtivo()
        {
            string ajustes = File.ReadAllText("ProjectSettings/ProjectSettings.asset");
            Assert.IsTrue(ajustes.Contains("activeInputHandler: 1"),
                "Project Settings > Player > Active Input Handling precisa ser 'Input System Package'");
        }

        [Test]
        public void Renderizacao_UsaURPComRenderer2D()
        {
            RenderPipelineAsset padrao = GraphicsSettings.defaultRenderPipeline;
            Assert.IsNotNull(padrao, "Project Settings > Graphics precisa do URP");
            Assert.AreEqual("UniversalRenderPipelineAsset", padrao.GetType().Name);
            Object renderer = AssetDatabase.LoadMainAssetAtPath("Assets/Settings/Renderer2D.asset");
            Assert.IsNotNull(renderer);
            Assert.AreEqual("Renderer2DData", renderer.GetType().Name, "o renderer precisa ser o 2D Renderer");
            string urp = File.ReadAllText(AssetDatabase.GetAssetPath(padrao));
            Assert.IsTrue(urp.Contains(GuidRenderer2D), "o URP precisa usar o Renderer2D");
        }

        [Test]
        public void CenasEstaoNaBuildNaOrdemCerta()
        {
            EditorBuildSettingsScene[] cenas = EditorBuildSettings.scenes;
            Assert.AreEqual(3, cenas.Length, "File > Build Profiles > Scene List");
            Assert.AreEqual("Assets/Scenes/Menu.unity", cenas[0].path, "o jogo começa pela tela inicial");
            Assert.AreEqual("Assets/Scenes/" + Cenas.Fase + ".unity", cenas[1].path);
            Assert.AreEqual("Assets/Scenes/" + Cenas.FimDeJogo + ".unity", cenas[2].path);
            foreach (EditorBuildSettingsScene c in cenas)
            {
                Assert.IsTrue(c.enabled, c.path);
                Assert.IsTrue(File.Exists(c.path), c.path);
            }
        }

        [TestCase("Chao", Camadas.Chao)]
        [TestCase("Jogador", Camadas.Jogador)]
        [TestCase("Inimigo", Camadas.Inimigo)]
        [TestCase("Item", Camadas.Item)]
        [TestCase("Perigo", Camadas.Perigo)]
        [TestCase("Plataforma", Camadas.Plataforma)]
        [TestCase("Bola", Camadas.Bola)]
        [TestCase("Projetil", Camadas.Projetil)]
        public void CamadaDeFisicaExiste(string nome, int numero)
        {
            Assert.AreEqual(numero, LayerMask.NameToLayer(nome), "Project Settings > Tags and Layers: " + nome);
        }

        [TestCase("Menu")]
        [TestCase("Suecia1958")]
        [TestCase("FimDeJogo")]
        public void Cena_TemCameraOrtografica_ELuzGlobal2D(string cena)
        {
            string texto = File.ReadAllText("Assets/Scenes/" + cena + ".unity");
            Assert.IsTrue(texto.Contains("orthographic: 1"), "a câmera precisa ser ortográfica (2D)");
            Assert.IsTrue(texto.Contains("m_SkyboxMaterial: {fileID: 0}"), "sem skybox 3D");
            Assert.IsTrue(texto.Contains("m_LightType: 4"), "a cena precisa da Global Light 2D do URP");
            // IDs de classe do Unity para componentes 3D (Light, Rigidbody, BoxCollider, MeshRenderer...)
            foreach (string id in new[] { "108", "54", "65", "135", "64", "136", "23", "33", "218" })
                Assert.IsFalse(texto.Contains("--- !u!" + id + " "), "a cena " + cena + " tem um componente 3D (!u!" + id + ")");
        }

        [Test]
        public void Codigo_NaoUsaFisica3D()
        {
            var proibidos = new Regex(@"\b(Rigidbody|BoxCollider|SphereCollider|CapsuleCollider|MeshCollider|CharacterController|Collision|Collider)\b(?!2D)|\bPhysics\.");
            foreach (string arquivo in Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories))
            {
                string[] linhas = File.ReadAllLines(arquivo);
                for (int i = 0; i < linhas.Length; i++)
                {
                    string l = linhas[i].Trim();
                    if (l.StartsWith("//"))
                        continue;
                    Assert.IsFalse(proibidos.IsMatch(l), arquivo + ":" + (i + 1) + " usa física 3D: " + l);
                }
            }
        }

        [Test]
        public void Sprites_SaoImportadosComoSpritePixelArt()
        {
            string[] pngs = Directory.GetFiles("Assets/Resources/Sprites", "*.png");
            Assert.Greater(pngs.Length, 5);
            foreach (string png in pngs)
            {
                var imp = AssetImporter.GetAtPath(png.Replace('\\', '/')) as TextureImporter;
                Assert.IsNotNull(imp, png);
                Assert.AreEqual(TextureImporterType.Sprite, imp.textureType, png);
                Assert.AreEqual(FilterMode.Point, imp.filterMode, png);
                Assert.AreEqual(16f, imp.spritePixelsPerUnit, 0.001f, png);
            }
        }

        [Test]
        public void TodoArquivoDoProjetoTemMeta()
        {
            foreach (string arquivo in Directory.GetFiles("Assets", "*", SearchOption.AllDirectories))
            {
                if (arquivo.EndsWith(".meta"))
                    continue;
                Assert.IsTrue(File.Exists(arquivo + ".meta"), "sem .meta: " + arquivo);
            }
        }
    }
}
