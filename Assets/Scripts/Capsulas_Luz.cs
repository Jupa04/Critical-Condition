using UnityEngine;

// Luz de las capsulas de hibernacion: apagadas casi siempre, encendidas solo durante la grabacion de Chahin.
// Cambia el material de las franjas de luz y prende o apaga las luces asociadas.
public class Capsulas_Luz : MonoBehaviour
{
    [Tooltip("Franjas de luz de todas las capsulas")]
    [SerializeField] Renderer[] franjas;
    [SerializeField] Material encendida;
    [SerializeField] Material apagada;
    [Tooltip("Luces puntuales de las capsulas")]
    [SerializeField] Light[] luces;
    [SerializeField] bool empezarEncendidas = false;

    public bool Encendidas { get; private set; }

    void Awake() { Aplicar(empezarEncendidas); }

    public void Encender() { Aplicar(true); }
    public void Apagar() { Aplicar(false); }

    [ContextMenu("Ver encendidas")] void VerEncendidas() { Aplicar(true); }
    [ContextMenu("Ver apagadas")] void VerApagadas() { Aplicar(false); }

    void Aplicar(bool on)
    {
        Encendidas = on;
        Material m = on ? encendida : apagada;
        if (m != null) foreach (Renderer r in franjas) if (r != null) r.sharedMaterial = m;
        foreach (Light l in luces) if (l != null) l.gameObject.SetActive(on);
    }
}
