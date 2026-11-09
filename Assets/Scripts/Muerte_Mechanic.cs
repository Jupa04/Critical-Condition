using System.Collections;
using UnityEngine;
using TMPro;

// La vida del jugador es su oxigeno (Mecanica_Oxigeno).
// Cada golpe le quita oxigeno; si llega a 0, ya sea por golpes o por el tiempo, muere.
public class Muerte_Mechanic : MonoBehaviour
{
    [SerializeField] float danoPorGolpe = 25f;   // Oxigeno que quita cada golpe
    [SerializeField] float tiempoInvulnerable = 1f;
    [SerializeField] float tiempoRespawn = 3f;
    [SerializeField] string tagDano = "Dano";
    [SerializeField] Transform puntoRespawn; // Si esta vacio usa la posicion inicial
    [SerializeField] TextMeshProUGUI textoEstado;
    [SerializeField] CharacterController controller;
    [SerializeField] Movimineto_Jugador movimiento;
    [SerializeField] Mecanica_Oxigeno oxigeno;

    Vector3 posicionInicial;
    Quaternion rotacionInicial;
    float ultimoDano = -999f;
    bool muerto;
    public bool Muerto => muerto;   // Lo lee Astronauta_Animador

    void Start()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
    }

    void Update()
    {
        if (!muerto && oxigeno.SinOxigeno) Morir();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagDano)) RecibirDano();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag(tagDano)) RecibirDano();
    }

    public void RecibirDano()
    {
        if (muerto || Time.time < ultimoDano + tiempoInvulnerable) return;

        ultimoDano = Time.time;
        oxigeno.PerderOxigeno(danoPorGolpe);
        if (oxigeno.SinOxigeno) Morir();
    }

    void Morir()
    {
        muerto = true;
        movimiento.enabled = false;
        oxigeno.enabled = false;
        if (textoEstado != null) textoEstado.text = "Connection Lost";
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(tiempoRespawn);

        // El CharacterController bloquea el teletransporte si esta activo
        controller.enabled = false;
        if (puntoRespawn != null) transform.SetPositionAndRotation(puntoRespawn.position, puntoRespawn.rotation);
        else transform.SetPositionAndRotation(posicionInicial, rotacionInicial);
        controller.enabled = true;

        oxigeno.Rellenar();
        movimiento.AgregarCombustible(100f);
        movimiento.enabled = true;
        oxigeno.enabled = true;
        muerto = false;
    }
}
