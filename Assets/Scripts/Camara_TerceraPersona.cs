using UnityEngine;

public class Camara_TerceraPersona : MonoBehaviour
{
    [SerializeField] Transform objetivo;                 
    [SerializeField] Vector3 offset = new Vector3(0, 2f, -5f);
    [SerializeField, Range(30f, 120f)] float CampoVision = 60f; // FOV ajustable en tiempo real
    [SerializeField] float SensibilidadMouse = 3f;
    [SerializeField] float LimiteInferior = -20f;           
    [SerializeField] float LimiteSuperior = 60f;               
    [SerializeField] float SuavizadoSeguimiento = 10f;        
    [Header("Colision con paredes")]
    [SerializeField] float radioColision = 0.3f;         // La camara se acerca al jugador si choca con algo
    [SerializeField] float alturaPivote = 1.5f;
    [Header("Sacudida al recibir dano")]
    [SerializeField] float intensidadSacudida = 0.35f;   // Metros que se mueve la camara en el golpe
    [SerializeField] float duracionSacudida = 0.3f;
    float rotacionVertical;
    Vector3 posicionBase;          // Posicion de la camara sin la sacudida
    float tiempoSacudida;
    Muerte_Mechanic muerte;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        posicionBase = transform.position;
        if (objetivo != null) muerte = objetivo.GetComponent<Muerte_Mechanic>();
    }

    // Pone la camara de una vez en su lugar detras del jugador (sin el arrastre del suavizado)
    public void Ubicar(float inclinacionInicial = 8f)
    {
        if (objetivo == null) return;
        rotacionVertical = inclinacionInicial;
        Quaternion rotacion = Quaternion.Euler(rotacionVertical, objetivo.eulerAngles.y, 0);
        Vector3 deseada = EvitarParedes(objetivo.position + Vector3.up * alturaPivote, objetivo.position + rotacion * offset);
        posicionBase = deseada;
        transform.position = deseada;
        transform.LookAt(objetivo.position + Vector3.up * 1.5f);
        Camera.main.fieldOfView = CampoVision;
    }

    void LateUpdate()
    {
        Camera.main.fieldOfView = CampoVision;

        // Durante la animacion de muerte el personaje no gira con el mouse
        bool muerto = muerte != null && muerte.Muerto;
        if (!muerto) objetivo.Rotate(0, Input.GetAxis("Mouse X") * SensibilidadMouse, 0);
        rotacionVertical -= Input.GetAxis("Mouse Y") * SensibilidadMouse;
        rotacionVertical = Mathf.Clamp(rotacionVertical, LimiteInferior, LimiteSuperior);

        
        Quaternion rotacion = Quaternion.Euler(rotacionVertical, objetivo.eulerAngles.y, 0);
        Vector3 posicionDeseada = objetivo.position + rotacion * offset;
        posicionDeseada = EvitarParedes(objetivo.position + Vector3.up * alturaPivote, posicionDeseada);
        posicionBase = Vector3.Lerp(posicionBase, posicionDeseada, SuavizadoSeguimiento * Time.deltaTime);
        transform.position = posicionBase;

        transform.LookAt(objetivo.position + Vector3.up * 1.5f);

        // Sacudida: se suma al final para que no arrastre el seguimiento suave, y se va apagando
        if (tiempoSacudida > 0f)
        {
            tiempoSacudida -= Time.deltaTime;
            float fuerza = intensidadSacudida * Mathf.Clamp01(tiempoSacudida / duracionSacudida);
            Vector2 desvio = Random.insideUnitCircle * fuerza;
            transform.position += transform.right * desvio.x + transform.up * desvio.y;
        }
    }

    // Si hay una pared entre el jugador y la camara, la camara se pone delante de ella
    Vector3 EvitarParedes(Vector3 pivote, Vector3 deseada)
    {
        Vector3 dir = deseada - pivote;
        float dist = dir.magnitude;
        if (dist < 0.01f) return deseada;
        dir /= dist;
        float cerca = dist;
        foreach (var h in Physics.SphereCastAll(pivote, radioColision, dir, dist, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.collider.transform.IsChildOf(objetivo)) continue;
            if (h.distance < cerca) cerca = h.distance;
        }
        return pivote + dir * Mathf.Max(cerca, 0.2f);
    }

    // Lo llama Muerte_Mechanic cuando el jugador recibe dano
    public void Sacudir()
    {
        tiempoSacudida = duracionSacudida;
    }
}
