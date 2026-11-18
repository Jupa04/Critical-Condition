using UnityEngine;

// Plataforma de carga que va y viene sobre sus rieles y lleva encima al jugador.
// Necesita un Rigidbody cinematico y un trigger sobre la cubierta para saber quien va montado.
[DefaultExecutionOrder(-50)]   // Antes que el jugador, para que su propio Move decida si esta en el piso
public class Plataforma_Movil : MonoBehaviour
{
    [Tooltip("Hasta donde viaja, relativo a su posicion inicial (ejes del padre)")]
    [SerializeField] Vector3 desplazamiento = new Vector3(0f, 0f, 18f);
    [SerializeField] float duracionViaje = 4f;
    [SerializeField] float pausaEnExtremos = 1f;
    [SerializeField] float desfase;

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
        float ciclo = 2f * (duracionViaje + pausaEnExtremos);
        float t = Mathf.Repeat(Time.time + desfase, ciclo);
        float k;
        if (t < duracionViaje) k = t / duracionViaje;
        else if (t < duracionViaje + pausaEnExtremos) k = 1f;
        else if (t < 2f * duracionViaje + pausaEnExtremos) k = 1f - (t - duracionViaje - pausaEnExtremos) / duracionViaje;
        else k = 0f;

        transform.localPosition = inicio + desplazamiento * Mathf.SmoothStep(0f, 1f, k);

        Vector3 delta = transform.position - posicionAnterior;
        posicionAnterior = transform.position;
        if (pasajero != null && pasajero.enabled) pasajero.Move(delta);
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
