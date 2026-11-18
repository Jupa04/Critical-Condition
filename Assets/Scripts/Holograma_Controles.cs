using UnityEngine;
using UnityEngine.UI;

// Holograma de controles al inicio del nivel, con el mismo estilo del HUD.
// Mientras esta abierto pausa el juego, libera el mouse y bloquea camara y movimiento.
// Se cierra con la X, con el boton "Got it", con Enter o con Escape; con H se vuelve a abrir.
public class Holograma_Controles : MonoBehaviour
{
    [SerializeField] CanvasGroup grupo;
    [SerializeField] RectTransform panel;
    [SerializeField] Button botonCerrar;
    [SerializeField] Button botonEntendido;
    [Tooltip("Movimiento, camara y demas scripts que no deben leer el teclado/mouse mientras el holograma esta abierto")]
    [SerializeField] Behaviour[] bloquearMientrasAbierto;
    [SerializeField] bool mostrarAlIniciar = true;
    [SerializeField] KeyCode teclaMostrar = KeyCode.H;
    [SerializeField] float duracionAnimacion = 0.25f;

    bool abierto;
    float visible;
    float escalaTiempoAnterior = 1f;
    bool[] estadoAnterior;

    public bool Abierto => abierto;

    void Start()
    {
        if (botonCerrar != null) botonCerrar.onClick.AddListener(Cerrar);
        if (botonEntendido != null) botonEntendido.onClick.AddListener(Cerrar);
        Aplicar(0f);
        if (mostrarAlIniciar) Abrir();
    }

    void Update()
    {
        if (abierto)
        {
            // La camara bloquea el cursor en su Start: lo liberamos cada cuadro mientras se lee el holograma
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape)) Cerrar();
        }
        else if (Input.GetKeyDown(teclaMostrar)) Abrir();

        visible = Mathf.MoveTowards(visible, abierto ? 1f : 0f, Time.unscaledDeltaTime / Mathf.Max(duracionAnimacion, 0.01f));
        Aplicar(visible);
    }

    void Aplicar(float k)
    {
        if (grupo != null)
        {
            // Parpadeo leve de holograma
            float parpadeo = abierto && k >= 1f && Random.value < 0.04f ? Random.Range(0.75f, 0.92f) : 1f;
            grupo.alpha = k * parpadeo;
            grupo.interactable = abierto;
            grupo.blocksRaycasts = abierto;
        }
        if (panel != null)
        {
            // Se "proyecta": nace como una linea y se abre en vertical
            float e = 1f - (1f - k) * (1f - k);
            panel.localScale = new Vector3(Mathf.Lerp(0.85f, 1f, e), Mathf.Lerp(0.02f, 1f, e), 1f);
        }
    }

    public void Abrir()
    {
        if (abierto) return;
        abierto = true;
        escalaTiempoAnterior = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;

        estadoAnterior = new bool[bloquearMientrasAbierto.Length];
        for (int i = 0; i < bloquearMientrasAbierto.Length; i++)
        {
            if (bloquearMientrasAbierto[i] == null) continue;
            estadoAnterior[i] = bloquearMientrasAbierto[i].enabled;
            bloquearMientrasAbierto[i].enabled = false;
        }
    }

    public void Cerrar()
    {
        if (!abierto) return;
        abierto = false;
        Time.timeScale = escalaTiempoAnterior;

        for (int i = 0; i < bloquearMientrasAbierto.Length; i++)
            if (bloquearMientrasAbierto[i] != null) bloquearMientrasAbierto[i].enabled = estadoAnterior[i];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
