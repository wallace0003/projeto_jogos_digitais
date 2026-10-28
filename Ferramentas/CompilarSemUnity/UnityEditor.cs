// API simplificada do Unity 6: assembly UnityEditor (só existe no Editor).
#pragma warning disable 0067, 0649
using System;
using System.Collections;
using UnityEngine;

namespace UnityEditor
{
    public static class SessionState
    {
        public static bool GetBool(string key, bool defaultValue) { return defaultValue; }
        public static void SetBool(string key, bool value) { }
    }

    public enum PlayModeStateChange { EnteredEditMode, ExitingEditMode, EnteredPlayMode, ExitingPlayMode }

    public sealed class EditorApplication
    {
        public delegate void CallbackFunction();
        public static CallbackFunction delayCall;
        public static event Action<PlayModeStateChange> playModeStateChanged;
        public static bool isPlaying { get; set; }
        public static bool isPlayingOrWillChangePlaymode { get { return false; } }
        public static bool ExecuteMenuItem(string menuItemPath) { return true; }
    }

    public enum EditorBehaviorMode { Mode3D, Mode2D }

    public sealed class EditorSettings : UnityEngine.Object
    {
        public static EditorBehaviorMode defaultBehaviorMode { get; set; }
    }

    public class SceneView
    {
        public static ArrayList sceneViews { get { return null; } }
        public bool in2DMode { get; set; }
        public void Repaint() { }
    }

    public sealed class EditorUtility
    {
        public static void OpenWithDefaultApp(string fileName) { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MenuItem : Attribute
    {
        public MenuItem(string itemName) { }
        public MenuItem(string itemName, bool isValidateFunction, int priority) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class InitializeOnLoadAttribute : Attribute { }

    public sealed class AssetDatabase
    {
        public static UnityEngine.Object LoadMainAssetAtPath(string assetPath) { return null; }
        public static string GetAssetPath(UnityEngine.Object assetObject) { return ""; }
    }

    public class AssetImporter : UnityEngine.Object
    {
        public static AssetImporter GetAtPath(string path) { return null; }
    }

    public enum TextureImporterType { Default = 0, NormalMap = 1, GUI = 2, Sprite = 8, Cursor = 7, Cookie = 4, Lightmap = 6, SingleChannel = 10 }

    public sealed class TextureImporter : AssetImporter
    {
        public TextureImporterType textureType { get; set; }
        public FilterMode filterMode { get; set; }
        public float spritePixelsPerUnit { get; set; }
    }

    public class EditorBuildSettingsScene
    {
        public bool enabled { get; set; }
        public string path { get; set; }
    }

    public class EditorBuildSettings
    {
        public static EditorBuildSettingsScene[] scenes { get; set; }
    }
}

namespace UnityEditor.SceneManagement
{
    using UnityEngine.SceneManagement;

    public enum OpenSceneMode { Single, Additive, AdditiveWithoutLoading }

    public sealed class EditorSceneManager : SceneManager
    {
        public static Scene OpenScene(string scenePath, OpenSceneMode mode = OpenSceneMode.Single) { return default(Scene); }
        public static bool SaveCurrentModifiedScenesIfUserWantsTo() { return true; }
    }
}
