using UnityEngine;
using TMPro;

// Holograma del HUD: gira el wireframe y cambia su color segun el estado.
// El estado sale del texto principal: palabras clave (Warning, Critical, Lost)
// o un porcentaje (combustible). Si el porcentaje cae de golpe,
// el holograma destella en rojo para avisar que el jugador recibio dano.
public class HUD_Holograma : MonoBehaviour
{
    [SerializeField] Transform pivote;
    [SerializeField] Renderer wireframe;
    [SerializeField] TextMeshProUGUI textoEstado;
    [Tooltip("Opcional: texto con el porcentaje (ej. O2 85%). Se colorea igual y se usa para detectar golpes.")]
    [SerializeField] TextMeshProUGUI textoValor;
    [SerializeField] float gradosPorSegundo = 45f;

    [Header("Colores por estado")]
    [SerializeField] Color estable = new Color(0.37f, 0.89f, 0.88f);
    [SerializeField] Color advertencia = new Color(1f, 0.78f, 0.25f);
    [SerializeField] Color critico = new Color(1f, 0.3f, 0.25f);
    [SerializeField] Color perdido = new Color(0.45f, 0.45f, 0.5f);

    [Header("Porcentaje")]
    [SerializeField] int limiteAdvertencia = 50;   // igual o menos: naranja
    [SerializeField] int limiteCritico = 20;       // igual o menos: rojo parpadeando; 0: gris

    [Header("Destello al recibir dano")]
    [Tooltip("Caida minima del porcentaje en un solo frame para contar como golpe.")]
    [SerializeField] int caidaParaDestello = 5;
    [SerializeField] float duracionDestello = 0.35f;

    static readonly int idColor = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock bloque;
    int porcentajeAnterior = -1;
    float finDestello = -1f;

    void Awake() { bloque = new MaterialPropertyBlock(); }

    void Update()
    {
        if (pivote != null) pivote.Rotate(0f, gradosPorSegundo * Time.deltaTime, 0f, Space.Self);

        string estado = textoEstado != null ? textoEstado.text : "";
        string nivel = Estado(estado);
        DetectarGolpe(textoValor != null ? textoValor.text : estado, nivel);

        Color c = estable;
        float parpadeo = 1f;
        if (nivel == "Warning") c = advertencia;
        else if (nivel == "Critical") { c = critico; parpadeo = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 6f)); }
        else if (nivel == "Lost") { c = perdido; parpadeo = Random.value > 0.1f ? 0.6f : 0.1f; }

        // Destello rojo que se desvanece despues de un golpe
        if (Time.time < finDestello)
        {
            float t = (finDestello - Time.time) / duracionDestello;
            c = Color.Lerp(c, critico * 1.6f, t);
            parpadeo = Mathf.Max(parpadeo, 1f);
        }

        c *= parpadeo; c.a = 1f;
        Color colorTexto = Color.Lerp(Color.white, c, 0.6f);
        if (textoEstado != null) textoEstado.color = colorTexto;
        if (textoValor != null) textoValor.color = colorTexto;
        if (wireframe != null)
        {
            wireframe.GetPropertyBlock(bloque);
            bloque.SetColor(idColor, c);
            wireframe.SetPropertyBlock(bloque);
        }
    }

    // Traduce el texto a un estado: palabras clave o porcentaje
    string Estado(string texto)
    {
        if (texto.Contains("Warning")) return "Warning";
        if (texto.Contains("Critical")) return "Critical";
        if (texto.Contains("Lost")) return "Lost";

        int porcentaje = LeerPorcentaje(texto);
        if (porcentaje < 0) return "";
        if (porcentaje <= 0) return "Lost";
        if (porcentaje <= limiteCritico) return "Critical";
        if (porcentaje <= limiteAdvertencia) return "Warning";
        return "";
    }

    // Si el porcentaje cae de golpe en un frame, activa el destello
    void DetectarGolpe(string texto, string nivel)
    {
        int porcentaje = LeerPorcentaje(texto);
        if (nivel == "Lost" || porcentaje < 0) { porcentajeAnterior = -1; return; }
        if (porcentajeAnterior >= 0 && porcentajeAnterior - porcentaje >= caidaParaDestello)
            finDestello = Time.time + duracionDestello;
        porcentajeAnterior = porcentaje;
    }

    static int LeerPorcentaje(string texto)
    {
        int signo = texto.IndexOf('%');
        if (signo < 0) return -1;
        int inicio = signo;
        while (inicio > 0 && char.IsDigit(texto[inicio - 1])) inicio--;
        return int.TryParse(texto.Substring(inicio, signo - inicio), out int p) ? p : -1;
    }
}
