using UnityEngine;

// Sonidos del astronauta.
// Pasos: una pisada cada vez que un pie de la animacion baja al piso (no un bucle), asi coinciden con lo que se ve.
// Propulsor en bucle mientras el jetpack esta encendido. Aterrizaje tras estar en el aire.
// Golpe al recibir dano y radio al morir (los llama Muerte_Mechanic).
// Con poco oxigeno suenan la alarma del traje y la respiracion agitada.
public class Sonido_Jugador : MonoBehaviour
{
    [Header("Referencias (vacio = se buscan solas)")]
    [SerializeField] CharacterController controller;
    [SerializeField] Movimineto_Jugador movimiento;
    [SerializeField] Muerte_Mechanic muerte;
    [SerializeField] Mecanica_Oxigeno oxigeno;
    [SerializeField] Animator animator;

    [Header("Fuentes")]
    [SerializeField] AudioSource fuenteEfectos;      // pisadas, aterrizaje, golpe, radio
    [SerializeField] AudioSource fuenteJetpack;      // bucle
    [SerializeField] AudioSource fuenteAlarma;       // bucle
    [SerializeField] AudioSource fuenteRespiracion;  // bucle

    [Header("Pasos")]
    [SerializeField] AudioClip[] pisadas;
    [SerializeField] float volumenPasos = 0.55f;
    [Tooltip("Cuanto tiene que subir un pie sobre su punto de apoyo para que la bajada cuente como pisada")]
    [SerializeField] float alturaLevantar = 0.06f;
    [Tooltip("Margen sobre el punto de apoyo con el que el pie se considera de nuevo en el piso")]
    [SerializeField] float margenApoyo = 0.025f;
    [Tooltip("Que tan rapido el punto de apoyo se adapta si el pie queda mas alto (m/s)")]
    [SerializeField] float subidaApoyo = 0.1f;
    [SerializeField] float variacionTono = 0.08f;

    [Header("Aterrizaje")]
    [SerializeField] AudioClip aterrizaje;
    [SerializeField] float volumenAterrizaje = 0.8f;
    [SerializeField] float tiempoAireMinimo = 0.35f;   // saltitos o bordes no suenan

    [Header("Golpe y muerte")]
    [SerializeField] AudioClip[] golpes;
    [SerializeField] float volumenGolpe = 0.9f;
    [SerializeField] AudioClip radioMuerte;
    [SerializeField] float volumenRadio = 0.8f;

    [Header("Oxigeno bajo")]
    [SerializeField] float umbralAlarma = 25f;         // % igual o menos: alarma (igual a Critical Condition)
    [SerializeField] float umbralRespiracion = 25f;    // % igual o menos: respiracion agitada
    [SerializeField] float volumenAlarma = 0.45f;
    [SerializeField] float volumenRespiracion = 0.7f;

    [Header("Jetpack")]
    [SerializeField] float volumenJetpack = 0.7f;
    [SerializeField] float tiempoEncendido = 0.08f;
    [SerializeField] float tiempoApagado = 0.25f;

    Transform pieIzq, pieDer;
    bool izqArriba, derArriba;
    float izqApoyo = 99f, derApoyo = 99f;   // altura mas baja de cada pie en su ultimo apoyo
    float tiempoSinPiso, ultimaPisada = -1f;
    int ultimaVariante = -1;
    bool estabaMuerto;

    void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (movimiento == null) movimiento = GetComponent<Movimineto_Jugador>();
        if (muerte == null) muerte = GetComponent<Muerte_Mechanic>();
        if (oxigeno == null) oxigeno = GetComponent<Mecanica_Oxigeno>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.isHuman)
        {
            pieIzq = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            pieDer = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }
        if (fuenteEfectos != null) { fuenteEfectos.loop = false; fuenteEfectos.playOnAwake = false; fuenteEfectos.clip = null; fuenteEfectos.volume = 1f; }
        Bucle(fuenteJetpack);
        Bucle(fuenteAlarma);
        Bucle(fuenteRespiracion);
    }

    void Bucle(AudioSource fuente)
    {
        if (fuente == null) return;
        fuente.loop = true;
        fuente.playOnAwake = false;
        fuente.volume = 0f;
    }

    bool Activo => Time.timeScale > 0f && movimiento != null && movimiento.enabled && (muerte == null || !muerte.Muerto);

    void Update()
    {
        bool activo = Activo;

        // Aterrizaje: isGrounded parpadea al bajar bordes, por eso se exige un tiempo minimo en el aire
        if (controller.isGrounded)
        {
            if (activo && tiempoSinPiso >= tiempoAireMinimo) Aterrizar();
            tiempoSinPiso = 0f;
        }
        else tiempoSinPiso += Time.deltaTime;

        bool volando = activo && movimiento.JetpackEncendido;
        Ajustar(fuenteJetpack, volando ? volumenJetpack : 0f, volando ? tiempoEncendido : tiempoApagado);

        // Oxigeno bajo: alarma del traje y respiracion agitada
        float o2 = oxigeno != null ? oxigeno.Porcentaje : 100f;
        bool vivo = muerte == null || !muerte.Muerto;
        bool juego = Time.timeScale > 0f && movimiento != null && movimiento.enabled;
        Ajustar(fuenteAlarma, juego && vivo && o2 <= umbralAlarma ? volumenAlarma : 0f, 0.3f);
        float agitacion = juego && vivo && o2 <= umbralRespiracion ? 1f : 0f;
        Ajustar(fuenteRespiracion, agitacion * volumenRespiracion, agitacion > 0f ? 1.2f : 0.6f);

        // Muerte: radio una sola vez
        bool muerto = muerte != null && muerte.Muerto;
        if (muerto && !estabaMuerto) Morir();
        estabaMuerto = muerto;
    }

    // Los pies se leen despues de que el Animator los movio en este cuadro
    void LateUpdate()
    {
        if (pieIzq == null || pieDer == null) return;
        bool enPiso = tiempoSinPiso < 0.12f;
        float baseY = transform.position.y + (controller.center.y - controller.height * 0.5f) * transform.lossyScale.y;
        Pie(pieIzq, ref izqArriba, ref izqApoyo, baseY, enPiso);
        Pie(pieDer, ref derArriba, ref derApoyo, baseY, enPiso);
    }

    // Cada pie se compara con su propio punto de apoyo: la altura exacta del pie en el piso
    // cambia segun la mezcla de animaciones, asi que un umbral fijo se pierde pisadas.
    void Pie(Transform pie, ref bool arriba, ref float apoyo, float baseY, bool enPiso)
    {
        float altura = pie.position.y - baseY;
        // El apoyo sigue lo mas bajo que ha estado el pie: baja al instante y sube despacio,
        // asi se olvida de poses viejas (dormido en la capsula, teletransportes)
        apoyo = Mathf.Min(altura, apoyo + subidaApoyo * Time.deltaTime);
        if (!arriba)
        {
            if (altura > apoyo + alturaLevantar) arriba = true;
        }
        else if (altura < apoyo + margenApoyo)
        {
            arriba = false;
            if (enPiso && Activo) Pisada();
        }
    }

    void Pisada()
    {
        if (Time.time - ultimaPisada < 0.12f) return;   // los dos pies casi juntos al frenar: una sola pisada
        ultimaPisada = Time.time;
        AudioClip clip = Variante(pisadas);
        if (clip == null || fuenteEfectos == null) return;
        fuenteEfectos.pitch = 1f + Random.Range(-variacionTono, variacionTono);
        fuenteEfectos.PlayOneShot(clip, volumenPasos);
    }

    void Aterrizar()
    {
        ultimaPisada = Time.time;   // el pie que toca el piso al caer no suma otra pisada
        izqArriba = derArriba = false;
        izqApoyo = derApoyo = 99f;
        if (aterrizaje == null || fuenteEfectos == null) return;
        fuenteEfectos.pitch = 1f;
        fuenteEfectos.PlayOneShot(aterrizaje, volumenAterrizaje);
    }

    // Lo llama Muerte_Mechanic al recibir un golpe o caer al vacio
    public void Golpe()
    {
        AudioClip clip = Variante(golpes);
        if (clip == null || fuenteEfectos == null) return;
        fuenteEfectos.pitch = 1f + Random.Range(-0.05f, 0.05f);
        fuenteEfectos.PlayOneShot(clip, volumenGolpe);
    }

    void Morir()
    {
        if (fuenteAlarma != null) { fuenteAlarma.volume = 0f; fuenteAlarma.Pause(); }
        if (fuenteRespiracion != null) { fuenteRespiracion.volume = 0f; fuenteRespiracion.Pause(); }
        if (radioMuerte == null || fuenteEfectos == null) return;
        fuenteEfectos.pitch = 1f;
        fuenteEfectos.PlayOneShot(radioMuerte, volumenRadio);
    }

    AudioClip Variante(AudioClip[] lista)
    {
        if (lista == null || lista.Length == 0) return null;
        int i = Random.Range(0, lista.Length);
        if (lista.Length > 1 && i == ultimaVariante) i = (i + 1) % lista.Length;   // nunca la misma dos veces seguidas
        ultimaVariante = i;
        return lista[i];
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
