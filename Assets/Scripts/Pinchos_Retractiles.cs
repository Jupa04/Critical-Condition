using UnityEngine;

// Puas que se esconden bajo la rejilla del piso: asoman las puntas como aviso, salen de golpe,
// se quedan arriba un momento y bajan. Con varios en fila y distinto desfase se forma una oleada.
public class Pinchos_Retractiles : MonoBehaviour
{
    [SerializeField] Transform pinchos;            // Lo que sube y baja
    [SerializeField] Collider zonaDano;            // Trigger con tag Dano, activo solo con las puas afuera
    [SerializeField] float alturaOculto = -1.05f;  // Posicion local Y escondidos
    [SerializeField] float alturaArriba = -0.27f;  // Posicion local Y afuera
    [Range(0f, 1f)] [SerializeField] float asomoAviso = 0.2f;
    [SerializeField] float tiempoOculto = 1.6f;
    [SerializeField] float tiempoAviso = 0.6f;
    [SerializeField] float tiempoSubida = 0.12f;
    [SerializeField] float tiempoArriba = 1f;
    [SerializeField] float tiempoBajada = 0.4f;
    [SerializeField] float desfase;

    void Update()
    {
        float ciclo = tiempoOculto + tiempoAviso + tiempoSubida + tiempoArriba + tiempoBajada;
        float t = Mathf.Repeat(Time.time + desfase, ciclo);
        float k;
        if (t < tiempoOculto) k = 0f;
        else if ((t -= tiempoOculto) < tiempoAviso) k = asomoAviso + Mathf.Sin(Time.time * 50f) * 0.03f;
        else if ((t -= tiempoAviso) < tiempoSubida) k = Mathf.Lerp(asomoAviso, 1f, t / tiempoSubida);
        else if ((t -= tiempoSubida) < tiempoArriba) k = 1f;
        else k = 1f - (t - tiempoArriba) / tiempoBajada;

        Vector3 p = pinchos.localPosition;
        p.y = Mathf.Lerp(alturaOculto, alturaArriba, k);
        pinchos.localPosition = p;
        if (zonaDano != null) zonaDano.enabled = k > 0.5f;
    }
}
