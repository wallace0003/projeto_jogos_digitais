using UnityEngine;
using UnityEngine.InputSystem;

namespace CincoEstrelas
{
    /// <summary>
    /// Leitura dos controles com o Input System (Project Settings &gt; Player &gt;
    /// Active Input Handling = Input System Package). Teclas da pág. 4 do GDD:
    /// A/D ou setas andam, W/↑ pula, S/↓ embaixadinhas ou desce da tábua,
    /// Shift corre, X chuta (segurar carrega), C cavadinha, Z pedalada,
    /// Q companheiro, Espaço poder/especial, E/Enter interage.
    /// Também funciona com controle (gamepad).
    /// </summary>
    public static class Entrada
    {
        static Keyboard Teclado
        {
            get { return Keyboard.current; }
        }

        static Gamepad Controle
        {
            get { return Gamepad.current; }
        }

        /// <summary>-1 (esquerda) a 1 (direita).</summary>
        public static float Horizontal
        {
            get
            {
                float v = 0f;
                Keyboard kb = Teclado;
                if (kb != null)
                {
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v -= 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v += 1f;
                }
                Gamepad gp = Controle;
                if (gp != null)
                {
                    float x = gp.leftStick.ReadValue().x;
                    if (Mathf.Abs(x) > 0.35f) v += Mathf.Sign(x);
                    if (gp.dpad.left.isPressed) v -= 1f;
                    if (gp.dpad.right.isPressed) v += 1f;
                }
                return Mathf.Clamp(v, -1f, 1f);
            }
        }

        /// <summary>W ou ↑ (pular).</summary>
        public static bool PuloApertado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)) ||
                       (gp != null && gp.buttonSouth.wasPressedThisFrame);
            }
        }

        public static bool PuloSegurado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && (kb.wKey.isPressed || kb.upArrowKey.isPressed)) ||
                       (gp != null && gp.buttonSouth.isPressed);
            }
        }

        /// <summary>S ou ↓ segurado: embaixadinhas (parado) ou descer da tábua.</summary>
        public static bool BaixoSegurado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && (kb.sKey.isPressed || kb.downArrowKey.isPressed)) ||
                       (gp != null && (gp.dpad.down.isPressed || gp.leftStick.ReadValue().y < -0.6f));
            }
        }

        public static bool BaixoApertado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)) ||
                       (gp != null && gp.dpad.down.wasPressedThisFrame);
            }
        }

        /// <summary>Shift: correr.</summary>
        public static bool CorrerSegurado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed)) ||
                       (gp != null && gp.rightTrigger.isPressed);
            }
        }

        /// <summary>X: chutar (segurar carrega a força).</summary>
        public static bool ChuteApertado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && kb.xKey.wasPressedThisFrame) || (gp != null && gp.buttonWest.wasPressedThisFrame);
            }
        }

        public static bool ChuteSegurado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && kb.xKey.isPressed) || (gp != null && gp.buttonWest.isPressed);
            }
        }

        /// <summary>C: cavadinha.</summary>
        public static bool CavadinhaApertada
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && kb.cKey.wasPressedThisFrame) || (gp != null && gp.buttonNorth.wasPressedThisFrame);
            }
        }

        /// <summary>Z: drible (pedalada), que prepara o superchute.</summary>
        public static bool DribleApertado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && kb.zKey.wasPressedThisFrame) || (gp != null && gp.rightShoulder.wasPressedThisFrame);
            }
        }

        /// <summary>Q: chamar o companheiro.</summary>
        public static bool CompanheiroApertado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && kb.qKey.wasPressedThisFrame) || (gp != null && gp.leftShoulder.wasPressedThisFrame);
            }
        }

        /// <summary>Espaço: usar a carta de poder ou o especial da torcida.</summary>
        public static bool PoderApertado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && kb.spaceKey.wasPressedThisFrame) || (gp != null && gp.buttonEast.wasPressedThisFrame);
            }
        }

        /// <summary>E ou Enter: interagir, confirmar e responder.</summary>
        public static bool ConfirmarApertado
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame ||
                                       kb.numpadEnterKey.wasPressedThisFrame)) ||
                       (gp != null && (gp.buttonSouth.wasPressedThisFrame || gp.startButton.wasPressedThisFrame));
            }
        }

        public static bool PausaApertada
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                return (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame)) ||
                       (gp != null && gp.startButton.wasPressedThisFrame);
            }
        }

        /// <summary>Liga/desliga o som (tecla M).</summary>
        public static bool MudoApertado
        {
            get
            {
                Keyboard kb = Teclado;
                return kb != null && kb.mKey.wasPressedThisFrame;
            }
        }

        /// <summary>Setas para cima/baixo nos menus e no quiz (-1 = cima, 1 = baixo, 0 = nada).</summary>
        public static int NavegacaoVertical
        {
            get
            {
                Keyboard kb = Teclado;
                Gamepad gp = Controle;
                if ((kb != null && (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)) ||
                    (gp != null && gp.dpad.up.wasPressedThisFrame))
                    return -1;
                if ((kb != null && (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)) ||
                    (gp != null && gp.dpad.down.wasPressedThisFrame))
                    return 1;
                return 0;
            }
        }

        /// <summary>Tecla 1 a 4 apertada neste quadro (0 a 3), ou -1.</summary>
        public static int AlternativaApertada
        {
            get
            {
                Keyboard kb = Teclado;
                if (kb == null)
                    return -1;
                if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) return 0;
                if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) return 1;
                if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame) return 2;
                if (kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame) return 3;
                return -1;
            }
        }
    }
}
