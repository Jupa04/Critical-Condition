using UnityEngine;
using TMPro;

// Holograma de vida del HUD: gira el wireframe del astronauta y
// cambia su color segun el estado que escribe Muerte_Mechanic.
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

    static readonly int idColor = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock bloque;

    void Awake() { bloque = new MaterialPropertyBlock(); }

    void Update()
    {
        if (pivote != null) pivote.Rotate(0f, gradosPorSegundo * Time.deltaTime, 0f, Space.Self);

        string estado = textoEstado != null ? textoEstado.text : "";
        Color c = estable;
        float parpadeo = 1f;
        if (estado.Contains("Warning")) c = advertencia;
        else if (estado.Contains("Critical")) { c = critico; parpadeo = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 6f)); }
        else if (estado.Contains("Lost")) { c = perdido; parpadeo = Random.value > 0.1f ? 0.6f : 0.1f; }

        c *= parpadeo; c.a = 1f;
        if (textoEstado != null) textoEstado.color = Color.Lerp(Color.white, c, 0.6f);
        if (wireframe != null)
        {
            wireframe.GetPropertyBlock(bloque);
            bloque.SetColor(idColor, c);
            wireframe.SetPropertyBlock(bloque);
        }
    }
}
