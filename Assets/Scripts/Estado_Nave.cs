using UnityEngine;

// Cambia el modulo de la nave entre su version limpia (cinematica) y abandonada (al despertar):
// materiales gastados, luces del techo apagadas, oxido visible y luz de emergencia.
public class Estado_Nave : MonoBehaviour
{
    [Tooltip("Materiales limpios y su version abandonada, en el mismo orden")]
    [SerializeField] Material[] limpios;
    [SerializeField] Material[] abandonados;
    [Tooltip("Se encienden solo con la nave limpia (luces del techo)")]
    [SerializeField] GameObject[] soloLimpia;
    [Tooltip("Se encienden solo con la nave abandonada (oxido, luz de emergencia)")]
    [SerializeField] GameObject[] soloAbandonada;
    [Tooltip("Piezas que conservan su material limpio aunque la nave este abandonada (ej. la puerta exterior)")]
    [SerializeField] Transform[] conservarLimpios;
    [Tooltip("Piezas dentro de las anteriores que si se oxidan (ej. la cara interior de la puerta)")]
    [SerializeField] Transform[] oxidarIgual;
    [SerializeField] bool empezarAbandonada = false;

    public bool Abandonada { get; private set; }

    void Awake() { Aplicar(empezarAbandonada); }

    public void Abandonar() { Aplicar(true); }
    public void Restaurar() { Aplicar(false); }

    [ContextMenu("Ver abandonada")] void VerAbandonada() { Aplicar(true); }
    [ContextMenu("Ver limpia")] void VerLimpia() { Aplicar(false); }

    bool Conservar(Transform t)
    {
        if (conservarLimpios == null) return false;
        if (oxidarIgual != null) foreach (Transform o in oxidarIgual) if (o != null && t.IsChildOf(o)) return false;
        foreach (Transform c in conservarLimpios) if (c != null && t.IsChildOf(c)) return true;
        return false;
    }

    void Aplicar(bool abandonada)
    {
        Abandonada = abandonada;
        Material[] desde = abandonada ? limpios : abandonados;
        Material[] hacia = abandonada ? abandonados : limpios;
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            if (Conservar(r.transform)) continue;
            Material[] mats = r.sharedMaterials;
            bool cambio = false;
            for (int i = 0; i < mats.Length; i++)
            {
                int k = System.Array.IndexOf(desde, mats[i]);
                if (k >= 0 && k < hacia.Length && hacia[k] != null) { mats[i] = hacia[k]; cambio = true; }
            }
            if (cambio) r.sharedMaterials = mats;
        }
        foreach (GameObject g in soloLimpia) if (g != null) g.SetActive(!abandonada);
        foreach (GameObject g in soloAbandonada) if (g != null) g.SetActive(abandonada);
    }
}
