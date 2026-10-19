using UnityEngine;
using TMPro;

public class Mecanica_Oxigeno : MonoBehaviour
{
    [SerializeField] float oxigenoMaximo = 100f;
    [SerializeField] float oxigenoActual = 100f;
    [SerializeField] float consumoPorSegundo = 1f;
    [SerializeField] TextMeshProUGUI textoOxigeno;

    void Update()
    {
        oxigenoActual -= consumoPorSegundo * Time.deltaTime;

        if (oxigenoActual <= 0f)
        {
            oxigenoActual = 0f;
            Debug.Log("Sin oxigeno: el jugador pierde");
        }

        textoOxigeno.text = "Oxigeno: " + Mathf.CeilToInt(oxigenoActual) + "%";
    }

    public void AgregarOxigeno(float cantidad)
    {
        oxigenoActual = Mathf.Min(oxigenoActual + cantidad, oxigenoMaximo);
    }
}
