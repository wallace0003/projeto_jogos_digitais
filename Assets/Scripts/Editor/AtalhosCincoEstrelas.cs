using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace CincoEstrelas.EditorTools
{
    /// <summary>
    /// Ao abrir o projeto: garante o modo 2D do editor, deixa a aba Scene em 2D
    /// e abre a cena do Menu. Também cria o menu "Cinco Estrelas" na barra do
    /// Unity com atalhos para as cenas e para testar a fase direto.
    /// </summary>
    [InitializeOnLoad]
    static class AtalhosCincoEstrelas
    {
        const string CenaMenu = "Assets/Scenes/Menu.unity";
        const string CenaFase = "Assets/Scenes/Suecia1958.unity";
        const string CenaFim = "Assets/Scenes/FimDeJogo.unity";
        const string Chave = "CincoEstrelas.MenuAbertoNaSessao";

        static AtalhosCincoEstrelas()
        {
            EditorApplication.delayCall += AoAbrirOProjeto;
            EditorApplication.playModeStateChanged += AoMudarModo;
        }

        /// <summary>Ao sair do Play, o jogo volta a começar sempre pela tela inicial.</summary>
        static void AoMudarModo(PlayModeStateChange modo)
        {
            if (modo == PlayModeStateChange.EnteredEditMode)
                SessionState.SetBool(Partida.ChavePularTelaInicial, false);
        }

        static void AoAbrirOProjeto()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(Chave, false))
                return;
            SessionState.SetBool(Chave, true);

            // projeto 2D: sprites importados como Sprite e cenas novas em 2D
            if (EditorSettings.defaultBehaviorMode != EditorBehaviorMode.Mode2D)
                EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

            Scene ativa = SceneManager.GetActiveScene();
            bool vazia = string.IsNullOrEmpty(ativa.path);
            if (vazia && !ativa.isDirty && File.Exists(CenaMenu))
                EditorSceneManager.OpenScene(CenaMenu);
            AbaSceneEm2D();
        }

        /// <summary>Coloca todas as abas Scene abertas no modo 2D.</summary>
        [MenuItem("Cinco Estrelas/Aba Scene em 2D", false, 30)]
        static void AbaSceneEm2D()
        {
            foreach (object item in SceneView.sceneViews)
            {
                SceneView sv = item as SceneView;
                if (sv == null || sv.in2DMode)
                    continue;
                sv.in2DMode = true;
                sv.Repaint();
            }
        }

        [MenuItem("Cinco Estrelas/Abrir cena Menu", false, 1)]
        static void AbrirMenu()
        {
            Abrir(CenaMenu);
        }

        [MenuItem("Cinco Estrelas/Abrir cena da fase (Suécia 1958)", false, 2)]
        static void AbrirFase()
        {
            Abrir(CenaFase);
        }

        [MenuItem("Cinco Estrelas/Abrir cena Fim de Jogo", false, 3)]
        static void AbrirFim()
        {
            Abrir(CenaFim);
        }

        /// <summary>
        /// Dar Play na fase normalmente leva para a tela inicial; este atalho abre
        /// a fase e dá Play direto nela (só para testes no Editor).
        /// </summary>
        [MenuItem("Cinco Estrelas/Testar a fase direto (pula a tela inicial)", false, 10)]
        static void TestarFase()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EditorSceneManager.OpenScene(CenaFase);
            SessionState.SetBool(Partida.ChavePularTelaInicial, true);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Cinco Estrelas/Editar o mapa da fase (texto)", false, 20)]
        static void EditarMapa()
        {
            EditorUtility.OpenWithDefaultApp("Assets/Resources/Fases/Suecia1958.txt");
        }

        [MenuItem("Cinco Estrelas/Rodar os testes (Test Runner)", false, 40)]
        static void Testes()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
        }

        static void Abrir(string caminho)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EditorSceneManager.OpenScene(caminho);
            AbaSceneEm2D();
        }
    }
}
