using UnityEngine;

// Volumen invisible debajo del nivel: si el jugador cae, pierde oxigeno y vuelve al ultimo checkpoint.
[RequireComponent(typeof(BoxCollider))]
public class Zona_Vacio : MonoBehaviour
{
    void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Muerte_Mechanic muerte = other.GetComponent<Muerte_Mechanic>();
        if (muerte != null) muerte.CaerAlVacio();
    }
}
