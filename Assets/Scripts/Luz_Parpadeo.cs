using UnityEngine;

// Luz que falla: pulsa despacio y de vez en cuando se apaga un instante.
[RequireComponent(typeof(Light))]
public class Luz_Parpadeo : MonoBehaviour
{
    [SerializeField] float intensidadMaxima = 1.2f;
    [SerializeField] float pulsosPorSegundo = 0.35f;
    [SerializeField, Range(0f, 1f)] float probabilidadFallo = 0.02f;
    Light luz;
    float apagadaHasta;

    void Awake() { luz = GetComponent<Light>(); }

    void Update()
    {
        if (Time.time < apagadaHasta) { luz.intensity = 0f; return; }
        if (Random.value < probabilidadFallo) apagadaHasta = Time.time + Random.Range(0.05f, 0.18f);
        float pulso = 0.55f + 0.45f * Mathf.Sin(Time.time * pulsosPorSegundo * Mathf.PI * 2f);
        luz.intensity = intensidadMaxima * pulso;
    }
}
