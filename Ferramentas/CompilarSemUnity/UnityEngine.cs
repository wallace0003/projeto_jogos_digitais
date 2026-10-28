// API simplificada do Unity 6: assembly UnityEngine (módulos do motor).
// Só as assinaturas que o jogo usa, para checar a compilação fora do Unity;
// as assinaturas seguem a documentação do Unity 6000.x e os corpos não fazem nada.
#pragma warning disable 0067, 0649, 0660, 0661, 0626, 0824
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public string name { get; set; }
        public static void Destroy(Object obj) { }
        public static void Destroy(Object obj, float t) { }
        public static void DestroyImmediate(Object obj) { }
        public static void DontDestroyOnLoad(Object target) { }
        public static bool operator ==(Object x, Object y) { return true; }
        public static bool operator !=(Object x, Object y) { return false; }
        public static implicit operator bool(Object exists) { return true; }
    }

    public class Component : Object
    {
        public GameObject gameObject { get { return null; } }
        public Transform transform { get { return null; } }
        public string tag { get; set; }
        public T GetComponent<T>() { return default(T); }
        public T GetComponentInParent<T>() { return default(T); }
        public T GetComponentInChildren<T>() { return default(T); }
        public T GetComponentInChildren<T>(bool includeInactive) { return default(T); }
        public T[] GetComponentsInChildren<T>() { return null; }
        public T[] GetComponentsInChildren<T>(bool includeInactive) { return null; }
        public bool CompareTag(string tag) { return false; }
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; }
        public bool isActiveAndEnabled { get { return false; } }
    }

    public class Coroutine : YieldInstruction { }
    public class YieldInstruction { }
    public abstract class CustomYieldInstruction : IEnumerator
    {
        public abstract bool keepWaiting { get; }
        public object Current { get { return null; } }
        public bool MoveNext() { return keepWaiting; }
        public virtual void Reset() { }
    }
    public sealed class WaitForSeconds : YieldInstruction { public WaitForSeconds(float seconds) { } }
    public class WaitForSecondsRealtime : CustomYieldInstruction
    {
        public WaitForSecondsRealtime(float time) { }
        public override bool keepWaiting { get { return false; } }
    }
    public sealed class WaitForFixedUpdate : YieldInstruction { }
    public class AsyncOperation : YieldInstruction
    {
        public bool isDone { get { return true; } }
        public float progress { get { return 1f; } }
    }

    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine) { return null; }
        public void StopAllCoroutines() { }
        public void StopCoroutine(Coroutine routine) { }
        public void Invoke(string methodName, float time) { }
        public void CancelInvoke() { }
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject { return null; }
    }

    public sealed class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string name) { }
        public GameObject(string name, params Type[] components) { }
        public T AddComponent<T>() where T : Component { return null; }
        public T GetComponent<T>() { return default(T); }
        public T GetComponentInChildren<T>() { return default(T); }
        public void SetActive(bool value) { }
        public bool activeSelf { get { return true; } }
        public bool activeInHierarchy { get { return true; } }
        public int layer { get; set; }
        public string tag { get; set; }
        public Transform transform { get { return null; } }
    }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Quaternion rotation { get; set; }
        public Quaternion localRotation { get; set; }
        public Vector3 localScale { get; set; }
        public Vector3 eulerAngles { get; set; }
        public Vector3 right { get { return default(Vector3); } }
        public Vector3 up { get { return default(Vector3); } }
        public Transform parent { get; set; }
        public int childCount { get { return 0; } }
        public Transform GetChild(int index) { return null; }
        public void SetParent(Transform p) { }
        public void SetParent(Transform parent, bool worldPositionStays) { }
        public void Rotate(float xAngle, float yAngle, float zAngle) { }
        public IEnumerator GetEnumerator() { return null; }
    }

    public sealed class RectTransform : Transform
    {
        public Vector2 anchorMin { get; set; }
        public Vector2 anchorMax { get; set; }
        public Vector2 pivot { get; set; }
        public Vector2 anchoredPosition { get; set; }
        public Vector2 sizeDelta { get; set; }
        public Vector2 offsetMin { get; set; }
        public Vector2 offsetMax { get; set; }
        public Rect rect { get { return default(Rect); } }
    }

    public struct Vector2
    {
        public float x;
        public float y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero { get { return default(Vector2); } }
        public static Vector2 one { get { return default(Vector2); } }
        public static Vector2 up { get { return default(Vector2); } }
        public static Vector2 down { get { return default(Vector2); } }
        public static Vector2 left { get { return default(Vector2); } }
        public static Vector2 right { get { return default(Vector2); } }
        public float magnitude { get { return 0f; } }
        public float sqrMagnitude { get { return 0f; } }
        public Vector2 normalized { get { return this; } }
        public static float Distance(Vector2 a, Vector2 b) { return 0f; }
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) { return a; }
        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta) { return current; }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return a; }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return a; }
        public static Vector2 operator -(Vector2 a) { return a; }
        public static Vector2 operator *(Vector2 a, float d) { return a; }
        public static Vector2 operator *(float d, Vector2 a) { return a; }
        public static Vector2 operator /(Vector2 a, float d) { return a; }
        public static bool operator ==(Vector2 lhs, Vector2 rhs) { return true; }
        public static bool operator !=(Vector2 lhs, Vector2 rhs) { return false; }
        public static implicit operator Vector2(Vector3 v) { return default(Vector2); }
        public static implicit operator Vector3(Vector2 v) { return default(Vector3); }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public Vector3(float x, float y) { this.x = x; this.y = y; z = 0f; }
        public static Vector3 zero { get { return default(Vector3); } }
        public static Vector3 one { get { return default(Vector3); } }
        public static Vector3 up { get { return default(Vector3); } }
        public static Vector3 down { get { return default(Vector3); } }
        public static Vector3 left { get { return default(Vector3); } }
        public static Vector3 right { get { return default(Vector3); } }
        public static Vector3 forward { get { return default(Vector3); } }
        public float magnitude { get { return 0f; } }
        public float sqrMagnitude { get { return 0f; } }
        public Vector3 normalized { get { return this; } }
        public static float Distance(Vector3 a, Vector3 b) { return 0f; }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) { return a; }
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta) { return current; }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return a; }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return a; }
        public static Vector3 operator -(Vector3 a) { return a; }
        public static Vector3 operator *(Vector3 a, float d) { return a; }
        public static Vector3 operator *(float d, Vector3 a) { return a; }
        public static Vector3 operator /(Vector3 a, float d) { return a; }
        public static bool operator ==(Vector3 lhs, Vector3 rhs) { return true; }
        public static bool operator !=(Vector3 lhs, Vector3 rhs) { return false; }
    }

    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;
        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Vector4 zero { get { return default(Vector4); } }
        public static implicit operator Vector4(Vector2 v) { return default(Vector4); }
        public static implicit operator Vector4(Vector3 v) { return default(Vector4); }
    }

    public struct Vector3Int
    {
        public Vector3Int(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
        public int x;
        public int y;
        public int z;
    }

    public struct Quaternion
    {
        public static Quaternion identity { get { return default(Quaternion); } }
        public static Quaternion Euler(float x, float y, float z) { return default(Quaternion); }
        public static Quaternion operator *(Quaternion lhs, Quaternion rhs) { return lhs; }
    }

    public struct Matrix4x4
    {
        public static Matrix4x4 identity { get { return default(Matrix4x4); } }
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; a = 1f; }
        public static Color white { get { return default(Color); } }
        public static Color black { get { return default(Color); } }
        public static Color clear { get { return default(Color); } }
        public static Color red { get { return default(Color); } }
        public static Color Lerp(Color a, Color b, float t) { return a; }
        public static Color operator *(Color a, float b) { return a; }
    }

    public struct Color32
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
        public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static implicit operator Color32(Color c) { return default(Color32); }
        public static implicit operator Color(Color32 c) { return default(Color); }
    }

    public struct Rect
    {
        public Rect(float x, float y, float width, float height) : this() { this.x = x; this.y = y; this.width = width; this.height = height; }
        public float x { get; set; }
        public float y { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public float xMin { get { return 0f; } }
        public float xMax { get { return 0f; } }
        public float yMin { get { return 0f; } }
        public float yMax { get { return 0f; } }
        public Vector2 center { get { return default(Vector2); } }
    }

    public struct Bounds
    {
        public Vector3 center { get; set; }
        public Vector3 size { get; set; }
        public Vector3 extents { get; set; }
    }

    public struct LayerMask
    {
        public static int NameToLayer(string layerName) { return 0; }
    }

    public static class Mathf
    {
        public const float PI = 3.14159274f;
        public const float Deg2Rad = 0.0174532924f;
        public const float Rad2Deg = 57.29578f;
        public static float Abs(float f) { return f; }
        public static int Abs(int value) { return value; }
        public static float Max(float a, float b) { return a; }
        public static float Max(params float[] values) { return 0f; }
        public static int Max(int a, int b) { return a; }
        public static int Max(params int[] values) { return 0; }
        public static float Min(float a, float b) { return a; }
        public static float Min(params float[] values) { return 0f; }
        public static int Min(int a, int b) { return a; }
        public static int Min(params int[] values) { return 0; }
        public static float Clamp(float value, float min, float max) { return value; }
        public static int Clamp(int value, int min, int max) { return value; }
        public static float Clamp01(float value) { return value; }
        public static float Lerp(float a, float b, float t) { return a; }
        public static float MoveTowards(float current, float target, float maxDelta) { return current; }
        public static float Sin(float f) { return f; }
        public static float Cos(float f) { return f; }
        public static float Acos(float f) { return f; }
        public static float Sign(float f) { return f; }
        public static float Repeat(float t, float length) { return t; }
        public static float PingPong(float t, float length) { return t; }
        public static float Exp(float power) { return power; }
        public static float Pow(float f, float p) { return f; }
        public static float Round(float f) { return f; }
        public static int RoundToInt(float f) { return 0; }
        public static float Floor(float f) { return f; }
        public static int FloorToInt(float f) { return 0; }
        public static int CeilToInt(float f) { return 0; }
        public static float SmoothStep(float from, float to, float t) { return t; }
        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime) { return current; }
    }

    public static class Random
    {
        public static float Range(float minInclusive, float maxInclusive) { return 0f; }
        public static int Range(int minInclusive, int maxExclusive) { return 0; }
        public static float value { get { return 0f; } }
        public static Vector2 insideUnitCircle { get { return default(Vector2); } }
    }

    public static class Time
    {
        public static float deltaTime { get { return 0f; } }
        public static float fixedDeltaTime { get; set; }
        public static float unscaledDeltaTime { get { return 0f; } }
        public static float unscaledTime { get { return 0f; } }
        public static float time { get { return 0f; } }
        public static float timeScale { get; set; }
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }

    public static class Application
    {
        public static bool isPlaying { get { return false; } }
        public static void Quit() { }
    }

    public static class PlayerPrefs
    {
        public static float GetFloat(string key, float defaultValue) { return defaultValue; }
        public static void SetFloat(string key, float value) { }
        public static int GetInt(string key, int defaultValue) { return defaultValue; }
        public static void SetInt(string key, int value) { }
        public static void Save() { }
    }

    public static class Resources
    {
        public static T Load<T>(string path) where T : Object { return null; }
    }

    public static class JsonUtility
    {
        public static T FromJson<T>(string json) { return default(T); }
    }

    public static class Gizmos
    {
        public static Color color { get; set; }
        public static void DrawCube(Vector3 center, Vector3 size) { }
    }

    public class TextAsset : Object
    {
        public string text { get { return ""; } }
    }

    public class Material : Object { }

    public enum FilterMode { Point, Bilinear, Trilinear }
    public enum TextureWrapMode { Repeat, Clamp, Mirror, MirrorOnce }
    public enum TextureFormat { Alpha8 = 1, RGBA32 = 4, ARGB32 = 5 }

    public class Texture : Object
    {
        public FilterMode filterMode { get; set; }
        public TextureWrapMode wrapMode { get; set; }
        public virtual int width { get { return 0; } set { } }
        public virtual int height { get { return 0; } set { } }
    }

    public sealed class Texture2D : Texture
    {
        public Texture2D(int width, int height, TextureFormat textureFormat, bool mipChain) { }
        public void SetPixels32(Color32[] colors) { }
        public void Apply(bool updateMipmaps, bool makeNoLongerReadable) { }
    }

    public enum SpriteMeshType { FullRect, Tight }
    public enum SpriteDrawMode { Simple, Sliced, Tiled }

    public sealed class Sprite : Object
    {
        public Rect rect { get { return default(Rect); } }
        public Bounds bounds { get { return default(Bounds); } }
        public Vector2 pivot { get { return default(Vector2); } }
        public Texture2D texture { get { return null; } }
        public float pixelsPerUnit { get { return 0f; } }
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit, uint extrude, SpriteMeshType meshType) { return null; }
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit, uint extrude, SpriteMeshType meshType, Vector4 border, bool generateFallbackPhysicsShape) { return null; }
    }

    public class Renderer : Component
    {
        public bool enabled { get; set; }
        public Material sharedMaterial { get; set; }
        public int sortingOrder { get; set; }
        public Bounds bounds { get { return default(Bounds); } }
    }

    public sealed class SpriteRenderer : Renderer
    {
        public Sprite sprite { get; set; }
        public Color color { get; set; }
        public bool flipX { get; set; }
        public bool flipY { get; set; }
        public SpriteDrawMode drawMode { get; set; }
        public Vector2 size { get; set; }
    }

    public sealed class Camera : Behaviour
    {
        public static Camera main { get { return null; } }
        public bool orthographic { get; set; }
        public float orthographicSize { get; set; }
        public float aspect { get { return 0f; } }
        public Color backgroundColor { get; set; }
    }

    public sealed class AudioClip : Object
    {
        public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool stream) { return null; }
        public bool SetData(float[] data, int offsetSamples) { return true; }
        public float length { get { return 0f; } }
        public int samples { get { return 0; } }
    }

    public sealed class AudioSource : Behaviour
    {
        public AudioClip clip { get; set; }
        public bool loop { get; set; }
        public bool playOnAwake { get; set; }
        public float spatialBlend { get; set; }
        public float volume { get; set; }
        public bool mute { get; set; }
        public bool isPlaying { get { return false; } }
        public void Play() { }
        public void Stop() { }
        public void PlayOneShot(AudioClip clip, float volumeScale) { }
    }

    // ------------------------------------------------------------ atributos
    public enum RuntimeInitializeLoadType { AfterSceneLoad, BeforeSceneLoad, AfterAssembliesLoaded, BeforeSplashScreen, SubsystemRegistration }

    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute() { }
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType loadType) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeField : Attribute { }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HeaderAttribute : PropertyAttribute { public HeaderAttribute(string header) { } }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class TooltipAttribute : PropertyAttribute { public TooltipAttribute(string tooltip) { } }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class TextAreaAttribute : PropertyAttribute { public TextAreaAttribute() { } }

    public abstract class PropertyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireComponent : Attribute { public RequireComponent(Type requiredComponent) { } }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AddComponentMenu : Attribute { public AddComponentMenu(string menuName) { } }

    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int order) { } }

    // ------------------------------------------------------------ física 2D
    public enum RigidbodyType2D { Dynamic, Kinematic, Static }
    public enum RigidbodyInterpolation2D { None, Interpolate, Extrapolate }
    public enum CollisionDetectionMode2D { Discrete, Continuous }
    public enum RigidbodySleepMode2D { NeverSleep, StartAwake, StartAsleep }

    public sealed class Rigidbody2D : Component
    {
        public RigidbodyType2D bodyType { get; set; }
        public float gravityScale { get; set; }
        public bool freezeRotation { get; set; }
        public RigidbodyInterpolation2D interpolation { get; set; }
        public CollisionDetectionMode2D collisionDetectionMode { get; set; }
        public RigidbodySleepMode2D sleepMode { get; set; }
        public bool simulated { get; set; }
        public Vector2 linearVelocity { get; set; }
        public Vector2 position { get; set; }
        public float mass { get; set; }
        public float linearDamping { get; set; }
        public void MovePosition(Vector2 position) { }
    }

    public sealed class PhysicsMaterial2D : Object
    {
        public PhysicsMaterial2D() { }
        public PhysicsMaterial2D(string name) { }
        public float friction { get; set; }
        public float bounciness { get; set; }
    }

    public class Collider2D : Behaviour
    {
        public enum CompositeOperation { None, Merge, Intersect, Difference, Flip }
        public bool isTrigger { get; set; }
        public Vector2 offset { get; set; }
        public Bounds bounds { get { return default(Bounds); } }
        public PhysicsMaterial2D sharedMaterial { get; set; }
        public bool usedByEffector { get; set; }
        public CompositeOperation compositeOperation { get; set; }
        public Rigidbody2D attachedRigidbody { get { return null; } }
    }

    public sealed class BoxCollider2D : Collider2D
    {
        public Vector2 size { get; set; }
        public float edgeRadius { get; set; }
    }

    public sealed class CircleCollider2D : Collider2D
    {
        public float radius { get; set; }
    }

    public sealed class CompositeCollider2D : Collider2D
    {
        public enum GeometryType { Outlines, Polygons }
        public GeometryType geometryType { get; set; }
    }

    public class Effector2D : Behaviour { }

    public sealed class PlatformEffector2D : Effector2D
    {
        public bool useOneWay { get; set; }
        public float surfaceArc { get; set; }
    }

    public class Collision2D
    {
        public Collider2D collider { get { return null; } }
        public GameObject gameObject { get { return null; } }
        public Rigidbody2D rigidbody { get { return null; } }
    }

    public struct RaycastHit2D
    {
        public Collider2D collider { get { return null; } }
        public Vector2 point { get { return default(Vector2); } }
        public float distance { get { return 0f; } }
        public static implicit operator bool(RaycastHit2D hit) { return false; }
    }

    public class Physics2D
    {
        public static Collider2D OverlapBox(Vector2 point, Vector2 size, float angle, int layerMask) { return null; }
        public static Collider2D OverlapCircle(Vector2 point, float radius, int layerMask) { return null; }
        public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask) { return default(RaycastHit2D); }
        public static void IgnoreLayerCollision(int layer1, int layer2, bool ignore) { }
        public static void IgnoreCollision(Collider2D collider1, Collider2D collider2, bool ignore) { }
    }

    // ------------------------------------------------------------ grade
    public class GridLayout : Behaviour { }

    public sealed class Grid : GridLayout
    {
        public Vector3 cellSize { get; set; }
    }

    // ------------------------------------------------------------ interface (UIModule)
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }

    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }

    public sealed class Canvas : Behaviour
    {
        public RenderMode renderMode { get; set; }
        public int sortingOrder { get; set; }
    }

    public sealed class CanvasRenderer : Component { }
}

namespace UnityEngine.Tilemaps
{
    using UnityEngine;

    [Flags]
    public enum TileFlags { None = 0, LockColor = 1, LockTransform = 2, InstantiateGameObjectRuntimeOnly = 4, LockAll = 3 }

    public class ITilemap { }

    public struct TileData
    {
        public Sprite sprite { get; set; }
        public Color color { get; set; }
        public Matrix4x4 transform { get; set; }
        public TileFlags flags { get; set; }
        public Tile.ColliderType colliderType { get; set; }
    }

    public struct TileAnimationData
    {
        public Sprite[] animatedSprites { get; set; }
        public float animationSpeed { get; set; }
        public float animationStartTime { get; set; }
    }

    public abstract class TileBase : ScriptableObject
    {
        public virtual void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) { }
        public virtual bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData) { return false; }
    }

    public class Tile : TileBase
    {
        public enum ColliderType { None, Sprite, Grid }
        public Sprite sprite { get; set; }
        public ColliderType colliderType { get; set; }
    }

    public sealed class Tilemap : GridLayout
    {
        public void SetTile(Vector3Int position, TileBase tile) { }
    }

    public sealed class TilemapRenderer : Renderer { }

    public sealed class TilemapCollider2D : Collider2D { }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene
    {
        public string name { get { return ""; } }
        public string path { get { return ""; } }
        public bool isDirty { get { return false; } }
    }

    public class SceneManager
    {
        public static Scene GetActiveScene() { return default(Scene); }
        public static void LoadScene(string sceneName) { }
        public static AsyncOperation LoadSceneAsync(string sceneName) { return null; }
    }
}

namespace UnityEngine.Rendering
{
    public abstract class RenderPipelineAsset : ScriptableObject { }

    public sealed class GraphicsSettings : Object
    {
        public static RenderPipelineAsset defaultRenderPipeline { get; set; }
    }
}

namespace UnityEngine.Events
{
    public delegate void UnityAction();

    public class UnityEvent
    {
        public void AddListener(UnityAction call) { }
        public void RemoveAllListeners() { }
        public void Invoke() { }
    }
}

