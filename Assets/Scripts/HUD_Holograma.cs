using UnityEngine;
using TMPro;

// Holograma del HUD: gira el wireframe y cambia su color segun el estado.
// Vida: lee las palabras que escribe Muerte_Mechanic (Warning, Critical, Lost).
// Jetpack: si el texto trae un porcentaje, el color sale de ese porcentaje.
public class HUD_Holograma : MonoBehaviour
{
    [SerializeField] Transform pivote;
    [SerializeField] Renderer wireframe;
    [SerializeField] TextMeshProUGUI textoEstado;
    [SerializeField] float gradosPorSegundo = 45f;

    [Header("Colores por estado")]
    [SerializeField] Color estable = new Color(0.37f, 0.89f, 0.88f);
    [SerializeField] Color advertencia = new Color(1f, 0.78f, 0.25f);
    [SerializeField] Color critico = new Color(1f, 0.3f, 0.25f);
    [SerializeField] Color perdido = new Color(0.45f, 0.45f, 0.5f);

    [Header("Porcentaje (jetpack)")]
    [SerializeField] int limiteAdvertencia = 50;   // igual o menos: naranja
    [SerializeField] int limiteCritico = 20;       // igual o menos: rojo parpadeando; 0: gris

    static readonly int idColor = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock bloque;

    void Awake() { bloque = new MaterialPropertyBlock(); }

    void Update()
    {
        if (pivote != null) pivote.Rotate(0f, gradosPorSegundo * Time.deltaTime, 0f, Space.Self);

        string estado = textoEstado != null ? textoEstado.text : "";
        string nivel = Estado(estado);

        Color c = estable;
        float parpadeo = 1f;
        if (nivel == "Warning") c = advertencia;
        else if (nivel == "Critical") { c = critico; parpadeo = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 6f)); }
        else if (nivel == "Lost") { c = perdido; parpadeo = Random.value > 0.1f ? 0.6f : 0.1f; }

        c *= parpadeo; c.a = 1f;
        if (textoEstado != null) textoEstado.color = Color.Lerp(Color.white, c, 0.6f);
        if (wireframe != null)
        {
            wireframe.GetPropertyBlock(bloque);
            bloque.SetColor(idColor, c);
            wireframe.SetPropertyBlock(bloque);
        }
    }

    // Traduce el texto a un estado: palabras para la vida, porcentaje para el jetpack
    string Estado(string texto)
    {
        if (texto.Contains("Warning")) return "Warning";
        if (texto.Contains("Critical")) return "Critical";
        if (texto.Contains("Lost")) return "Lost";

        int signo = texto.IndexOf('%');
        if (signo < 0) return "";
        int inicio = signo;
        while (inicio > 0 && char.IsDigit(texto[inicio - 1])) inicio--;
        if (!int.TryParse(texto.Substring(inicio, signo - inicio), out int porcentaje)) return "";

        if (porcentaje <= 0) return "Lost";
        if (porcentaje <= limiteCritico) return "Critical";
        if (porcentaje <= limiteAdvertencia) return "Warning";
        return "";
    }
}
