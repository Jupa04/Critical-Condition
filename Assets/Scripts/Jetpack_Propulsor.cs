using UnityEngine;

// Efecto toon del propulsor del jetpack.
// En el espacio no hay humo que suba ni fuego con oxigeno: el escape es un chorro
// de plasma cian con nucleo blanco, bocanadas que se encogen y chispas cortas.
// La llama "hierve" a pocos cuadros por segundo, como en la animacion 2D.
public class Jetpack_Propulsor : MonoBehaviour
{
    [Header("Referencias (vacio = se buscan solas)")]
    [SerializeField] Movimineto_Jugador movimiento;
    [SerializeField] Muerte_Mechanic muerte;

    [Header("Piezas del efecto")]
    [SerializeField] Transform[] llamas;            // conos exterior y nucleo
    [SerializeField] ParticleSystem[] bocanadas;     // nubecitas de plasma
    [SerializeField] ParticleSystem[] chispas;       // destellos cortos
    [SerializeField] Light luz;

    [Header("Ajustes")]
    [SerializeField] float tasaBocanadas = 55f;      // por tobera, por segundo
    [SerializeField] float tasaChispas = 14f;
    [SerializeField] float intensidadLuz = 2.5f;
    [Tooltip("Cuadros por segundo del 'hervor' de la llama. Bajo = mas caricatura.")]
    [SerializeField] float cuadrosHervor = 14f;
    [SerializeField] float tiempoEncendido = 0.08f;
    [SerializeField] float tiempoApagado = 0.12f;
    [SerializeField] int bocanadasAlEncender = 6;

    Vector3[] escalasBase;
    float intensidad;
    float siguienteHervor;
    Vector3 hervor = Vector3.one;
    bool encendidoAntes;

    void Awake()
    {
        if (movimiento == null) movimiento = GetComponentInParent<Movimineto_Jugador>();
        if (muerte == null) muerte = GetComponentInParent<Muerte_Mechanic>();
        escalasBase = new Vector3[llamas.Length];
        for (int i = 0; i < llamas.Length; i++) escalasBase[i] = llamas[i].localScale;
        Aplicar(0f);
    }

    void Update()
    {
        bool encendido = movimiento != null && movimiento.JetpackEncendido && (muerte == null || !muerte.Muerto);

        // Rampa corta de encendido y apagado
        float objetivo = encendido ? 1f : 0f;
        float tiempo = encendido ? tiempoEncendido : tiempoApagado;
        intensidad = Mathf.MoveTowards(intensidad, objetivo, Time.deltaTime / Mathf.Max(tiempo, 0.001f));

        // Al encender, una bocanada de golpe para que se sienta el arranque
        if (encendido && !encendidoAntes)
            foreach (var p in bocanadas) p.Emit(bocanadasAlEncender);
        encendidoAntes = encendido;

        // Hervor escalonado: cambia la forma de la llama a pocos cuadros por segundo
        if (Time.time >= siguienteHervor)
        {
            siguienteHervor = Time.time + 1f / cuadrosHervor;
            float largo = Random.Range(0.8f, 1.2f);
            float ancho = Random.Range(0.92f, 1.06f);
            hervor = new Vector3(ancho, largo, ancho);
        }

        Aplicar(intensidad);
    }

    void Aplicar(float k)
    {
        for (int i = 0; i < llamas.Length; i++)
        {
            Vector3 b = escalasBase[i];
            // La llama nace delgada y se alarga: el ancho crece mas rapido que el largo
            llamas[i].localScale = new Vector3(b.x * hervor.x * Mathf.Sqrt(k), b.y * hervor.y * k, b.z * hervor.z * Mathf.Sqrt(k));
            llamas[i].gameObject.SetActive(k > 0.01f);
        }
        foreach (var p in bocanadas) { var e = p.emission; e.rateOverTime = tasaBocanadas * k; }
        foreach (var p in chispas) { var e = p.emission; e.rateOverTime = tasaChispas * k; }
        if (luz != null)
        {
            luz.enabled = k > 0.01f;
            luz.intensity = intensidadLuz * k * hervor.y;
        }
    }
}
