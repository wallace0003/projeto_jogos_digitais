using UnityEngine;

namespace CincoEstrelas
{
    /// <summary>Bola que sobe e desce sozinha (embaixadinhas das telas de menu).</summary>
    public class BolaQuicando : MonoBehaviour
    {
        public Vector3 pe;
        public float altura = 1.5f;
        public float periodo = 0.45f;

        void Update()
        {
            float f = Mathf.Repeat(Time.unscaledTime / periodo, 1f);
            transform.position = pe + Vector3.up * (Mathf.Sin(f * Mathf.PI) * altura);
            transform.Rotate(0f, 0f, -360f * Time.unscaledDeltaTime);
        }
    }
}
