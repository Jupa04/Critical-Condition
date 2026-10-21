using System.Collections;
using UnityEngine;
using TMPro;

public class Muerte_Mechanic : MonoBehaviour
{
    [SerializeField] int vidaMaxima = 3;
    [SerializeField] int vidaActual;
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

    void Start()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        vidaActual = vidaMaxima;
        ActualizarHUD();
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
        vidaActual--;
        if (vidaActual <= 0) Morir();
        else ActualizarHUD();
    }

    void Morir()
    {
        muerto = true;
        vidaActual = 0;
        movimiento.enabled = false;
        oxigeno.enabled = false;
        ActualizarHUD();
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

        vidaActual = vidaMaxima;
        oxigeno.Rellenar();
        movimiento.AgregarCombustible(100f);
        movimiento.enabled = true;
        oxigeno.enabled = true;
        muerto = false;
        ActualizarHUD();
    }

    void ActualizarHUD()
    {
        if (textoEstado == null) return;

        if (vidaActual >= 3) textoEstado.text = "Stable";
        else if (vidaActual == 2) textoEstado.text = "Warning";
        else if (vidaActual == 1) textoEstado.text = "Critical Condition";
        else textoEstado.text = "Connection Lost";
    }
}
