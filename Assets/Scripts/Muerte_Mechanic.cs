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
    Camara_TerceraPersona camara;
    Sonido_Jugador sonido;
    public bool Muerto => muerto;   // Lo lee Astronauta_Animador

    void Start()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        sonido = GetComponent<Sonido_Jugador>();
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
        SacudirCamara();
        if (sonido != null) sonido.Golpe();
        if (oxigeno.SinOxigeno) Morir();
    }

    void SacudirCamara()
    {
        if (camara == null && Camera.main != null) camara = Camera.main.GetComponent<Camara_TerceraPersona>();
        if (camara != null) camara.Sacudir();
    }

    // Lo llaman los checkpoints del nivel para mover el punto de reaparicion
    public void EstablecerRespawn(Transform punto)
    {
        if (punto != null) puntoRespawn = punto;
    }

    // Lo llama Zona_Vacio: caer fuera del nivel quita oxigeno y regresa al ultimo checkpoint
    public void CaerAlVacio()
    {
        if (muerto) return;

        ultimoDano = Time.time;
        oxigeno.PerderOxigeno(danoPorGolpe);
        if (sonido != null) sonido.Golpe();
        if (oxigeno.SinOxigeno) { Morir(); return; }

        controller.enabled = false;
        if (puntoRespawn != null) transform.SetPositionAndRotation(puntoRespawn.position, puntoRespawn.rotation);
        else transform.SetPositionAndRotation(posicionInicial, rotacionInicial);
        controller.enabled = true;
    }

    void Morir()
    {
        muerto = true;
        movimiento.enabled = false;
        oxigeno.enabled = false;
        if (textoEstado != null) textoEstado.text = Idioma.T("Connection Lost");
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
