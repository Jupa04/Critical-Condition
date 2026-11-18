using UnityEngine;

// Piston hidraulico: tiembla un momento como aviso, sale de golpe, se queda afuera y regresa despacio.
// Va en la cabeza del piston (Rigidbody cinematico + trigger al frente). Empuja al jugador que tenga delante.
[DefaultExecutionOrder(-50)]
public class Piston_Empujador : MonoBehaviour
{
    [Tooltip("Recorrido de la cabeza, en los ejes de su padre")]
    [SerializeField] Vector3 recorrido = new Vector3(11f, 0f, 0f);
    [SerializeField] float tiempoEspera = 1.6f;
    [SerializeField] float tiempoAviso = 0.5f;
    [SerializeField] float tiempoSalida = 0.45f;
    [SerializeField] float tiempoFuera = 0.5f;
    [SerializeField] float tiempoRegreso = 1.3f;
    [SerializeField] float desfase;
    [SerializeField] float temblorAviso = 0.08f;

    Vector3 inicio;
    Vector3 posicionAnterior;
    CharacterController pasajero;

    void Start()
    {
        inicio = transform.localPosition;
        posicionAnterior = transform.position;
    }

    void Update()
    {
        float ciclo = tiempoEspera + tiempoAviso + tiempoSalida + tiempoFuera + tiempoRegreso;
        float t = Mathf.Repeat(Time.time + desfase, ciclo);
        float k = 0f;
        Vector3 temblor = Vector3.zero;
        bool saliendo = false;

        if (t < tiempoEspera) k = 0f;
        else if ((t -= tiempoEspera) < tiempoAviso) temblor = recorrido.normalized * Mathf.Sin(Time.time * 60f) * temblorAviso;
        else if ((t -= tiempoAviso) < tiempoSalida) { k = t / tiempoSalida; saliendo = true; }
        else if ((t -= tiempoSalida) < tiempoFuera) k = 1f;
        else k = 1f - (t - tiempoFuera) / tiempoRegreso;

        transform.localPosition = inicio + recorrido * k + temblor;

        Vector3 delta = transform.position - posicionAnterior;
        posicionAnterior = transform.position;
        if (saliendo && pasajero != null && pasajero.enabled) pasajero.Move(delta * 1.1f);
    }

    void OnTriggerEnter(Collider other)
    {
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null) pasajero = cc;
    }

    void OnTriggerExit(Collider other)
    {
        if (pasajero != null && other.gameObject == pasajero.gameObject) pasajero = null;
    }
}
