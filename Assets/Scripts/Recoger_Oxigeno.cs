using System.Collections;
using UnityEngine;

public class Recoger_Oxigeno : MonoBehaviour
{
    [SerializeField] float cantidadOxigeno = 25f;
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
        Mecanica_Oxigeno oxigeno = other.GetComponent<Mecanica_Oxigeno>();

        if (oxigeno != null)
        {
            oxigeno.AgregarOxigeno(cantidadOxigeno);
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
