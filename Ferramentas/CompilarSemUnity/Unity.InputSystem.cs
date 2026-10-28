// API simplificada do Unity 6: assembly Unity.InputSystem (pacote com.unity.inputsystem).
// Só as assinaturas que o jogo usa, para checar a compilação fora do Unity.
#pragma warning disable 0067, 0649, 0660, 0661, 0626, 0824
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.InputSystem.Controls
{
    using UnityEngine;

    public class ButtonControl
    {
        public bool isPressed { get { return false; } }
        public bool wasPressedThisFrame { get { return false; } }
        public bool wasReleasedThisFrame { get { return false; } }
    }

    public class KeyControl : ButtonControl { }

    public class StickControl
    {
        public Vector2 ReadValue() { return default(Vector2); }
    }

    public class DpadControl
    {
        public ButtonControl up { get { return null; } }
        public ButtonControl down { get { return null; } }
        public ButtonControl left { get { return null; } }
        public ButtonControl right { get { return null; } }
    }
}

namespace UnityEngine.InputSystem
{
    using UnityEngine.InputSystem.Controls;

    public class Keyboard
    {
        public static Keyboard current { get { return null; } }
        public KeyControl aKey { get { return null; } }
        public KeyControl cKey { get { return null; } }
        public KeyControl dKey { get { return null; } }
        public KeyControl eKey { get { return null; } }
        public KeyControl mKey { get { return null; } }
        public KeyControl pKey { get { return null; } }
        public KeyControl qKey { get { return null; } }
        public KeyControl sKey { get { return null; } }
        public KeyControl wKey { get { return null; } }
        public KeyControl xKey { get { return null; } }
        public KeyControl zKey { get { return null; } }
        public KeyControl spaceKey { get { return null; } }
        public KeyControl enterKey { get { return null; } }
        public KeyControl numpadEnterKey { get { return null; } }
        public KeyControl escapeKey { get { return null; } }
        public KeyControl leftShiftKey { get { return null; } }
        public KeyControl rightShiftKey { get { return null; } }
        public KeyControl leftArrowKey { get { return null; } }
        public KeyControl rightArrowKey { get { return null; } }
        public KeyControl upArrowKey { get { return null; } }
        public KeyControl downArrowKey { get { return null; } }
        public KeyControl digit1Key { get { return null; } }
        public KeyControl digit2Key { get { return null; } }
        public KeyControl digit3Key { get { return null; } }
        public KeyControl digit4Key { get { return null; } }
        public KeyControl numpad1Key { get { return null; } }
        public KeyControl numpad2Key { get { return null; } }
        public KeyControl numpad3Key { get { return null; } }
        public KeyControl numpad4Key { get { return null; } }
    }

    public class Gamepad
    {
        public static Gamepad current { get { return null; } }
        public ButtonControl buttonSouth { get { return null; } }
        public ButtonControl buttonNorth { get { return null; } }
        public ButtonControl buttonEast { get { return null; } }
        public ButtonControl buttonWest { get { return null; } }
        public ButtonControl startButton { get { return null; } }
        public ButtonControl leftShoulder { get { return null; } }
        public ButtonControl rightShoulder { get { return null; } }
        public ButtonControl rightTrigger { get { return null; } }
        public ButtonControl leftTrigger { get { return null; } }
        public StickControl leftStick { get { return null; } }
        public DpadControl dpad { get { return null; } }
    }
}

namespace UnityEngine.InputSystem.UI
{
    public class InputSystemUIInputModule : UnityEngine.EventSystems.BaseInputModule { }
}

