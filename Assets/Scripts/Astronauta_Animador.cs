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

    static readonly int idMoveX = Animator.StringToHash("MoveX");
    static readonly int idMoveZ = Animator.StringToHash("MoveZ");
    static readonly int idGrounded = Animator.StringToHash("Grounded");

    Animator anim;
    CharacterController cc;
    Vector3 posAnterior;
    float tiempoSinPiso;

    void Awake()
    {
        anim = GetComponent<Animator>();
        anim.applyRootMotion = false;
        if (raiz == null) raiz = transform.parent != null ? transform.parent : transform;
        cc = raiz.GetComponent<CharacterController>();
        posAnterior = raiz.position;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Cuanto se movio de verdad (si choca con una pared o esta muerto, frena la animacion)
        Vector3 delta = raiz.position - posAnterior;
        posAnterior = raiz.position;
        float rapidez = new Vector2(delta.x, delta.z).magnitude / dt;
        float factor = Mathf.Clamp01(rapidez / velocidadMaxima);

        // Direccion: la misma entrada que usa el script de movimiento
        Vector2 entrada = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (entrada.sqrMagnitude > 1f) entrada.Normalize();
        Vector2 dir = entrada.sqrMagnitude > 0.0001f ? entrada.normalized : Vector2.zero;
        Vector2 objetivo = dir * factor;

        bool piso = cc == null || cc.isGrounded;
        tiempoSinPiso = piso ? 0f : tiempoSinPiso + dt;

        anim.SetFloat(idMoveX, objetivo.x, inercia, dt);
        anim.SetFloat(idMoveZ, objetivo.y, inercia, dt);
        anim.SetBool(idGrounded, tiempoSinPiso < margenAire);
    }
}
