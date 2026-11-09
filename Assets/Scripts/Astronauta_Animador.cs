using UnityEngine;

// Lee el movimiento de MC y se lo pasa al Animator del astronauta.
// La direccion sale de la misma entrada que usa Movimineto_Jugador,
// asi no se confunde cuando el mouse gira al jugador (eso causaba el zig zag).
[RequireComponent(typeof(Animator))]
public class Astronauta_Animador : MonoBehaviour
{
    [Tooltip("Objeto que se mueve. Vacio = el padre (MC).")]
    [SerializeField] Transform raiz;

    [Tooltip("Igual a 'Velocidad' en Movimineto_Jugador.")]
    [SerializeField] float velocidadMaxima = 4f;

    [Header("Sensacion de traje pesado")]
    [Tooltip("Segundos que tarda en arrancar y frenar la animacion. Mas alto = mas pesado.")]
    [SerializeField] float inercia = 0.35f;

    [Tooltip("Segundos sin tocar el piso antes de pasar a la animacion de aire.")]
    [SerializeField] float margenAire = 0.15f;

    [Header("Giro en el sitio")]
    [Tooltip("Grados por segundo de giro con el mouse que equivalen a la animacion de giro completa.")]
    [SerializeField] float giroMaximo = 120f;
    [Tooltip("Por debajo de este giro (grados por segundo) se queda en idle.")]
    [SerializeField] float giroMinimo = 15f;
    [Tooltip("Segundos que tarda en entrar y salir de la animacion de giro.")]
    [SerializeField] float inerciaGiro = 0.2f;

    static readonly int idMoveX = Animator.StringToHash("MoveX");
    static readonly int idMoveZ = Animator.StringToHash("MoveZ");
    static readonly int idGrounded = Animator.StringToHash("Grounded");
    static readonly int idGiro = Animator.StringToHash("Giro");
    static readonly int idJetpack = Animator.StringToHash("Jetpack");
    static readonly int idMuerto = Animator.StringToHash("Muerto");

    Animator anim;
    CharacterController cc;
    Movimineto_Jugador movimiento;
    Muerte_Mechanic muerte;
    Vector3 posAnterior;
    float yawAnterior;
    float tiempoSinPiso;

    void Awake()
    {
        anim = GetComponent<Animator>();
        anim.applyRootMotion = false;
        if (raiz == null) raiz = transform.parent != null ? transform.parent : transform;
        cc = raiz.GetComponent<CharacterController>();
        movimiento = raiz.GetComponent<Movimineto_Jugador>();
        muerte = raiz.GetComponent<Muerte_Mechanic>();
        posAnterior = raiz.position;
        yawAnterior = raiz.eulerAngles.y;
    }

    // LateUpdate: la camara gira a MC en su LateUpdate, asi se mide el giro de este frame
    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        bool muerto = muerte != null && muerte.Muerto;
        anim.SetBool(idMuerto, muerto);

        // Cuanto se movio de verdad (si choca con una pared o esta muerto, frena la animacion)
        Vector3 delta = raiz.position - posAnterior;
        posAnterior = raiz.position;
        float rapidez = new Vector2(delta.x, delta.z).magnitude / dt;
        float factor = Mathf.Clamp01(rapidez / velocidadMaxima);

        // Cuanto giro MC (grados por segundo, positivo = a la derecha)
        float yaw = raiz.eulerAngles.y;
        float velocidadGiro = Mathf.DeltaAngle(yawAnterior, yaw) / dt;
        yawAnterior = yaw;

        // Direccion: la misma entrada que usa el script de movimiento
        Vector2 entrada = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (muerto) entrada = Vector2.zero;
        if (entrada.sqrMagnitude > 1f) entrada.Normalize();
        Vector2 dir = entrada.sqrMagnitude > 0.0001f ? entrada.normalized : Vector2.zero;
        Vector2 objetivo = dir * factor;

        bool piso = cc == null || cc.isGrounded;
        tiempoSinPiso = piso ? 0f : tiempoSinPiso + dt;
        bool enPiso = tiempoSinPiso < margenAire;

        // Giro en el sitio: solo quieto, en el piso y vivo
        float giro = 0f;
        bool quieto = entrada.sqrMagnitude < 0.01f && factor < 0.1f;
        if (quieto && enPiso && !muerto && Mathf.Abs(velocidadGiro) > giroMinimo)
            giro = Mathf.Clamp(velocidadGiro / giroMaximo, -1f, 1f);

        anim.SetFloat(idMoveX, objetivo.x, inercia, dt);
        anim.SetFloat(idMoveZ, objetivo.y, inercia, dt);
        anim.SetFloat(idGiro, giro, inerciaGiro, dt);
        anim.SetBool(idGrounded, enPiso);
        anim.SetBool(idJetpack, !muerto && movimiento != null && movimiento.JetpackEncendido);
    }
}
