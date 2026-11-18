using System.Collections;
using UnityEngine;

// Barredora giratoria al estilo Fall Guys: gira sobre su eje y, si golpea al jugador,
// lo empuja hacia afuera y le quita oxigeno (los brazos llevan el tag Dano).
// El objeto necesita un Rigidbody cinematico para que sus triggers detecten al CharacterController.
public class Obstaculo_Giratorio : MonoBehaviour
{
    [SerializeField] float gradosPorSegundo = 60f;
    [SerializeField] float fuerzaEmpuje = 9f;
    [SerializeField] float alturaEmpuje = 3f;
    [SerializeField] float duracionEmpuje = 0.35f;

    Rigidbody cuerpo;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Quaternion giro = Quaternion.Euler(0f, gradosPorSegundo * Time.fixedDeltaTime, 0f);
        if (cuerpo != null) cuerpo.MoveRotation(cuerpo.rotation * giro);
        else transform.rotation *= giro;
    }

    void OnTriggerEnter(Collider other)
    {
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc == null) return;

        // Direccion en la que barre el brazo en el punto del jugador
        Vector3 radio = other.transform.position - transform.position;
        radio.y = 0f;
        Vector3 tangente = Vector3.Cross(Vector3.up, radio).normalized * Mathf.Sign(gradosPorSegundo);
        Vector3 empuje = (tangente + radio.normalized * 0.5f).normalized * fuerzaEmpuje + Vector3.up * alturaEmpuje;
        StartCoroutine(Empujar(cc, empuje));
    }

    IEnumerator Empujar(CharacterController cc, Vector3 velocidad)
    {
        float t = 0f;
        while (t < duracionEmpuje && cc != null && cc.enabled)
        {
            float k = 1f - t / duracionEmpuje;
            cc.Move(velocidad * k * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
    }
}
