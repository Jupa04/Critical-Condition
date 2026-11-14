using UnityEngine;
using TMPro;

// Linterna de la antena del casco.
// Se prende y apaga con una tecla, gasta bateria mientras esta encendida
// y la recupera poco a poco apagada. El HUD muestra la bateria en el panel del jetpack.
public class Linterna_Casco : MonoBehaviour
{
    [Header("Piezas")]
    [SerializeField] Light luz;
    [SerializeField] Renderer antena;              // para encender las luces de la caja
    [SerializeField] GameObject bombillo;           // punta brillante de la antena
    [SerializeField] TextMeshProUGUI textoBateria;

    [Header("Control")]
    [SerializeField] KeyCode tecla = KeyCode.F;
    [SerializeField] bool encendidaAlIniciar = false;

    [Header("Bateria (%)")]
    [SerializeField] float consumoPorSegundo = 0.8f;   // encendida: unos 2 minutos de luz
    [SerializeField] float recargaPorSegundo = 0.5f;   // apagada: el traje la recarga
    [SerializeField] float minimoParaEncender = 5f;
    [SerializeField] float umbralParpadeo = 15f;       // por debajo, la luz titila

    [Header("Luz")]
    [SerializeField] float intensidad = 45f;
    [SerializeField] Color colorEmision = new Color(1.4f, 1.3f, 1.05f);

    [Header("Colores del HUD")]
    [SerializeField] Color colorEncendida = new Color(0.37f, 0.89f, 0.88f);
    [SerializeField] Color colorApagada = new Color(0.45f, 0.45f, 0.5f);
    [SerializeField] Color colorBaja = new Color(1f, 0.78f, 0.25f);
    [SerializeField] Color colorCritica = new Color(1f, 0.3f, 0.25f);

    static readonly int idEmision = Shader.PropertyToID("_Emissive_Color");
    MaterialPropertyBlock bloque;
    float bateria = 100f;
    bool encendida;
    Muerte_Mechanic muerte;

    public bool Encendida => encendida;
    public float Bateria => bateria;

    void Awake()
    {
        bloque = new MaterialPropertyBlock();
        muerte = GetComponentInParent<Muerte_Mechanic>();
        encendida = encendidaAlIniciar;
    }

    void Update()
    {
        bool muerto = muerte != null && muerte.Muerto;

        if (!muerto && Input.GetKeyDown(tecla))
        {
            if (encendida) encendida = false;
            else if (bateria >= minimoParaEncender) encendida = true;
        }
        if (muerto) encendida = false;

        // Bateria
        if (encendida)
        {
            bateria = Mathf.Max(bateria - consumoPorSegundo * Time.deltaTime, 0f);
            if (bateria <= 0f) encendida = false;
        }
        else bateria = Mathf.Min(bateria + recargaPorSegundo * Time.deltaTime, 100f);

        // Con poca bateria la luz titila de vez en cuando
        float brillo = 1f;
        if (encendida && bateria < umbralParpadeo && Random.value < 0.08f) brillo = Random.Range(0.15f, 0.5f);

        if (luz != null)
        {
            luz.enabled = encendida;
            luz.intensity = intensidad * brillo;
        }
        if (bombillo != null) bombillo.SetActive(encendida && brillo > 0.6f);
        if (antena != null)
        {
            antena.GetPropertyBlock(bloque);
            bloque.SetColor(idEmision, encendida ? colorEmision * brillo : Color.black);
            antena.SetPropertyBlock(bloque);
        }

        ActualizarHUD();
    }

    void ActualizarHUD()
    {
        if (textoBateria == null) return;
        int porcentaje = Mathf.CeilToInt(bateria);
        textoBateria.text = "LANTERN " + porcentaje + "%";

        Color c = colorApagada;
        if (encendida)
        {
            if (porcentaje <= umbralParpadeo) c = Color.Lerp(colorCritica * 0.5f, colorCritica, Mathf.Abs(Mathf.Sin(Time.time * 6f)));
            else if (porcentaje <= 30) c = colorBaja;
            else c = colorEncendida;
        }
        c.a = 1f;
        textoBateria.color = Color.Lerp(Color.white, c, encendida ? 0.6f : 0.85f);
    }
}
