// API simplificada do Unity 6: assembly UnityEngine.UI (pacote com.unity.ugui).
// Só as assinaturas que o jogo usa, para checar a compilação fora do Unity.
#pragma warning disable 0067, 0649, 0660, 0661, 0626, 0824
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    using UnityEngine;

    public class UIBehaviour : MonoBehaviour
    {
        protected virtual void Awake() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
#if UNITY_EDITOR
        protected virtual void OnValidate() { }
#endif
    }

    public class BaseEventData { }
    public class PointerEventData : BaseEventData { }

    public interface IEventSystemHandler { }
    public interface ISelectHandler : IEventSystemHandler { void OnSelect(BaseEventData eventData); }
    public interface IDeselectHandler : IEventSystemHandler { void OnDeselect(BaseEventData eventData); }
    public interface IPointerEnterHandler : IEventSystemHandler { void OnPointerEnter(PointerEventData eventData); }

    public class EventSystem : UIBehaviour
    {
        public static EventSystem current { get; set; }
        public void SetSelectedGameObject(GameObject selected) { }
        public GameObject currentSelectedGameObject { get { return null; } }
    }

    public abstract class BaseInputModule : UIBehaviour { }
}

namespace UnityEngine.UI
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Events;

    public class VertexHelper
    {
        public int currentVertCount { get { return 0; } }
        public void Clear() { }
        public void AddVert(Vector3 position, Color32 color, Vector4 uv0) { }
        public void AddTriangle(int idx0, int idx1, int idx2) { }
    }

    public abstract class Graphic : UIBehaviour
    {
        public virtual Color color { get; set; }
        public virtual bool raycastTarget { get; set; }
        public RectTransform rectTransform { get { return null; } }
        public virtual Texture mainTexture { get { return null; } }
        public virtual void SetVerticesDirty() { }
        protected virtual void OnPopulateMesh(VertexHelper vh) { }
#if UNITY_EDITOR
        protected override void OnValidate() { }
#endif
    }

    public abstract class MaskableGraphic : Graphic
    {
#if UNITY_EDITOR
        protected override void OnValidate() { }
#endif
    }

    public class Image : MaskableGraphic
    {
        public enum Type { Simple, Sliced, Tiled, Filled }
        public enum FillMethod { Horizontal, Vertical, Radial90, Radial180, Radial360 }
        public Sprite sprite { get; set; }
        public Type type { get; set; }
        public FillMethod fillMethod { get; set; }
        public int fillOrigin { get; set; }
        public float fillAmount { get; set; }
        public float pixelsPerUnitMultiplier { get; set; }
    }

    public struct ColorBlock
    {
        public Color normalColor { get; set; }
        public Color highlightedColor { get; set; }
        public Color pressedColor { get; set; }
        public Color selectedColor { get; set; }
        public Color disabledColor { get; set; }
        public float fadeDuration { get; set; }
    }

    public struct Navigation
    {
        public enum Mode { None = 0, Horizontal = 1, Vertical = 2, Automatic = 3, Explicit = 4 }
        public Mode mode { get; set; }
        public Selectable selectOnUp { get; set; }
        public Selectable selectOnDown { get; set; }
        public Selectable selectOnLeft { get; set; }
        public Selectable selectOnRight { get; set; }
    }

    public class Selectable : UIBehaviour
    {
        public bool interactable { get; set; }
        public ColorBlock colors { get; set; }
        public Navigation navigation { get; set; }
        public Graphic targetGraphic { get; set; }
    }

    public class Button : Selectable
    {
        public class ButtonClickedEvent : UnityEvent { }
        public ButtonClickedEvent onClick { get; set; }
    }

    public class CanvasScaler : UIBehaviour
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize }
        public enum ScreenMatchMode { MatchWidthOrHeight, Expand, Shrink }
        public ScaleMode uiScaleMode { get; set; }
        public Vector2 referenceResolution { get; set; }
        public ScreenMatchMode screenMatchMode { get; set; }
        public float matchWidthOrHeight { get; set; }
        public float referencePixelsPerUnit { get; set; }
    }

    public class GraphicRaycaster : BaseRaycaster { }
}

namespace UnityEngine.EventSystems
{
    public abstract class BaseRaycaster : UIBehaviour { }
}

