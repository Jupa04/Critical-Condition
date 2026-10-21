using UnityEngine;
using TMPro;

public class Mecanica_Oxigeno : MonoBehaviour
{
    [SerializeField] float oxigenoMaximo = 100f;
    [SerializeField] float oxigenoActual = 100f;
    [SerializeField] float consumoPorSegundo = 1f;
    [SerializeField] TextMeshProUGUI textoOxigeno;

    public bool SinOxigeno => oxigenoActual <= 0f;

    void Update()
    {
        oxigenoActual = Mathf.Max(oxigenoActual - consumoPorSegundo * Time.deltaTime, 0f);
        textoOxigeno.text = "Oxigeno: " + Mathf.CeilToInt(oxigenoActual) + "%";
    }

    public void Rellenar()
    {
        oxigenoActual = oxigenoMaximo;
        textoOxigeno.text = "Oxigeno: " + Mathf.CeilToInt(oxigenoActual) + "%";
    }

    public void AgregarOxigeno(float cantidad)
    {
        oxigenoActual = Mathf.Min(oxigenoActual + cantidad, oxigenoMaximo);
    }
}
