using UnityEngine;
using TMPro;

// Oxigeno y vida son el mismo valor: el traje pierde oxigeno con el tiempo
// y tambien cuando el jugador recibe dano. En 0 el jugador muere (Muerte_Mechanic).
public class Mecanica_Oxigeno : MonoBehaviour
{
    [SerializeField] float oxigenoMaximo = 100f;
    [SerializeField] float oxigenoActual = 100f;
    [SerializeField] float consumoPorSegundo = 0.5f;

    [Header("HUD")]
    [Tooltip("Muestra el porcentaje, ej. O2 85%")]
    [SerializeField] TextMeshProUGUI textoOxigeno;
    [Tooltip("Muestra la condicion del jugador: Stable, Warning, Critical Condition")]
    [SerializeField] TextMeshProUGUI textoEstado;
    [SerializeField] int limiteAdvertencia = 50;   // igual o menos: Warning
    [SerializeField] int limiteCritico = 25;       // igual o menos: Critical Condition

    public bool SinOxigeno => oxigenoActual <= 0f;
    public float Porcentaje => oxigenoActual / oxigenoMaximo * 100f;

    void Update()
    {
        oxigenoActual = Mathf.Max(oxigenoActual - consumoPorSegundo * Time.deltaTime, 0f);
        ActualizarHUD();
    }

    public void Rellenar()
    {
        oxigenoActual = oxigenoMaximo;
        ActualizarHUD();
    }

    public void AgregarOxigeno(float cantidad)
    {
        oxigenoActual = Mathf.Min(oxigenoActual + cantidad, oxigenoMaximo);
        ActualizarHUD();
    }

    // Un golpe rompe el traje: baja el oxigeno, que tambien es la vida
    public void PerderOxigeno(float cantidad)
    {
        oxigenoActual = Mathf.Max(oxigenoActual - cantidad, 0f);
        ActualizarHUD();
    }

    void ActualizarHUD()
    {
        int porcentaje = Mathf.CeilToInt(Porcentaje);
        if (textoOxigeno != null) textoOxigeno.text = "O2 " + porcentaje + "%";
        if (textoEstado == null) return;

        // "Connection Lost" lo escribe Muerte_Mechanic al morir
        if (porcentaje <= 0) return;
        if (porcentaje <= limiteCritico) textoEstado.text = "Critical Condition";
        else if (porcentaje <= limiteAdvertencia) textoEstado.text = "Warning";
        else textoEstado.text = "Stable";
    }
}
