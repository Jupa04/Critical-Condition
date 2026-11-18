using UnityEngine;

// Sonidos del astronauta: pasos mientras camina en el piso y propulsor mientras el jetpack esta encendido.
// Los dos son clips en bucle; aqui solo se sube y baja su volumen para que no corten de golpe.
public class Sonido_Jugador : MonoBehaviour
{
    [Header("Referencias (vacio = se buscan solas)")]
    [SerializeField] CharacterController controller;
    [SerializeField] Movimineto_Jugador movimiento;
    [SerializeField] Muerte_Mechanic muerte;

    [Header("Pasos")]
    [SerializeField] AudioSource fuentePasos;
    [SerializeField] float volumenPasos = 0.55f;
    [SerializeField] float entradaMinima = 0.2f;       // Cuanto hay que empujar WASD para que suenen
    [SerializeField] float tiempoFundidoPasos = 0.12f;

    [Header("Jetpack")]
    [SerializeField] AudioSource fuenteJetpack;
    [SerializeField] float volumenJetpack = 0.7f;
    [SerializeField] float tiempoEncendido = 0.08f;
    [SerializeField] float tiempoApagado = 0.25f;

    float tiempoSinPiso;

    void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (movimiento == null) movimiento = GetComponent<Movimineto_Jugador>();
        if (muerte == null) muerte = GetComponent<Muerte_Mechanic>();
        Preparar(fuentePasos);
        Preparar(fuenteJetpack);
    }

    void Preparar(AudioSource fuente)
    {
        if (fuente == null) return;
        fuente.loop = true;
        fuente.playOnAwake = false;
        fuente.volume = 0f;
    }

    void Update()
    {
        bool activo = Time.timeScale > 0f && movimiento != null && movimiento.enabled && (muerte == null || !muerte.Muerto);

        // isGrounded parpadea al bajar bordes: se da un pequeno margen antes de cortar los pasos
        if (controller.isGrounded) tiempoSinPiso = 0f; else tiempoSinPiso += Time.deltaTime;
        // Se mira lo que pide el jugador y no la velocidad real: una plataforma movil tambien lo desplaza
        float entrada = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;
        bool caminando = activo && tiempoSinPiso < 0.15f && entrada > entradaMinima;
        bool volando = activo && movimiento.JetpackEncendido;

        Ajustar(fuentePasos, caminando ? volumenPasos : 0f, tiempoFundidoPasos);
        Ajustar(fuenteJetpack, volando ? volumenJetpack : 0f, volando ? tiempoEncendido : tiempoApagado);
    }

    void Ajustar(AudioSource fuente, float objetivo, float tiempo)
    {
        if (fuente == null || fuente.clip == null) return;
        float maximo = Mathf.Max(objetivo, fuente.volume, 0.01f);
        fuente.volume = Mathf.MoveTowards(fuente.volume, objetivo, Time.unscaledDeltaTime / Mathf.Max(tiempo, 0.01f) * maximo);

        if (objetivo > 0f && !fuente.isPlaying) fuente.Play();
        else if (objetivo <= 0f && fuente.volume <= 0.001f && fuente.isPlaying) fuente.Pause();
    }
}
