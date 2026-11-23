using UnityEngine;
using TMPro;

// Punto de evacuacion al final del nivel: muestra el aviso de llegada con el tiempo que tardo el jugador.
public class Meta_Nivel : MonoBehaviour
{
    [SerializeField] GameObject aviso;              // Panel del HUD que se enciende al llegar
    [SerializeField] TextMeshProUGUI textoTiempo;
    [SerializeField] Mecanica_Oxigeno oxigeno;      // Se detiene el consumo al llegar
    [Tooltip("Opcional: cinematica que se reproduce antes del aviso")]
    [SerializeField] Cinematica_Escape cinematica;

    float inicio;
    bool terminado;

    void Start()
    {
        inicio = Time.time;
        if (aviso != null) aviso.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (terminado || other.GetComponent<Muerte_Mechanic>() == null) return;
        terminado = true;

        float t = Time.time - inicio;
        if (textoTiempo != null) textoTiempo.text = Idioma.T("TIME  {0}", Mathf.FloorToInt(t / 60f).ToString("00") + ":" + Mathf.FloorToInt(t % 60f).ToString("00"));
        if (oxigeno != null) oxigeno.enabled = false;
        if (cinematica != null) cinematica.Iniciar(() => { if (aviso != null) aviso.SetActive(true); });
        else if (aviso != null) aviso.SetActive(true);
    }
}
