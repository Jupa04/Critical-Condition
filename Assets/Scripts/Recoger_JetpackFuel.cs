using System.Collections;
using UnityEngine;

public class Recoger_JetpackFuel : MonoBehaviour
{
    [SerializeField] float cantidadFuel = 25f; // Porcentaje del combustible maximo
    [SerializeField] float tiempoRespawn = 5f;
    [SerializeField] float velocidadRotacion = 100f;
    [SerializeField] bool sentidoHorario = true;

    Renderer[] renderers;
    Collider col;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        float direccion = sentidoHorario ? 1f : -1f;
        transform.Rotate(Vector3.up * velocidadRotacion * direccion * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        Movimineto_Jugador jetpack = other.GetComponent<Movimineto_Jugador>();

        if (jetpack != null)
        {
            jetpack.AgregarCombustible(cantidadFuel);
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        Mostrar(false);
        yield return new WaitForSeconds(tiempoRespawn);
        Mostrar(true);
    }

    void Mostrar(bool visible)
    {
        col.enabled = visible;
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }
    }
}
