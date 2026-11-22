using UnityEngine;
using UnityEngine.InputSystem;

namespace Xuixo.Escena1
{
    /// <summary>
    /// Lectura d'entrada comuna de l'escena 1 (teclat i comandament).
    /// El projecte fa servir el nou Input System, per tant no s'usa UnityEngine.Input.
    /// </summary>
    public static class EntradaJugador
    {
        /// <summary>
        /// Permet forcar una entrada concreta. Nomes l'utilitzen les proves automatitzades.
        /// </summary>
        public static Vector2? EntradaForcada;

        /// <summary>Retorna un vector en [-1,1] amb les fletxes (o WASD / stick esquerre).</summary>
        public static Vector2 Llegeix()
        {
            if (EntradaForcada.HasValue)
                return Vector2.ClampMagnitude(EntradaForcada.Value, 1f);

            Vector2 v = Vector2.zero;

            Keyboard teclat = Keyboard.current;
            if (teclat != null)
            {
                if (teclat.leftArrowKey.isPressed || teclat.aKey.isPressed) v.x -= 1f;
                if (teclat.rightArrowKey.isPressed || teclat.dKey.isPressed) v.x += 1f;
                if (teclat.upArrowKey.isPressed || teclat.wKey.isPressed) v.y += 1f;
                if (teclat.downArrowKey.isPressed || teclat.sKey.isPressed) v.y -= 1f;
            }

            Gamepad comandament = Gamepad.current;
            if (comandament != null)
            {
                v += comandament.leftStick.ReadValue();
                if (comandament.dpad.left.isPressed) v.x -= 1f;
                if (comandament.dpad.right.isPressed) v.x += 1f;
                if (comandament.dpad.up.isPressed) v.y += 1f;
                if (comandament.dpad.down.isPressed) v.y -= 1f;
            }

            return Vector2.ClampMagnitude(v, 1f);
        }

        /// <summary>Torna a comencar (R o boto sud del comandament).</summary>
        public static bool Reintentar()
        {
            Keyboard teclat = Keyboard.current;
            if (teclat != null && teclat.rKey.wasPressedThisFrame) return true;

            Gamepad comandament = Gamepad.current;
            return comandament != null && comandament.buttonSouth.wasPressedThisFrame;
        }

    }
}
