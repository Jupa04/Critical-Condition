using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Objeto que se puede revisar con la tecla E: registros de la tripulacion, capsulas, etc.
// Al entrar en su zona aparece el aviso "[E] ..."; al presionar E se abre una caja de texto
// que se escribe letra por letra. E, Enter o clic completan el texto y luego cierran la caja.
public class Registro_Interactivo : MonoBehaviour
{
    [Header("Zona (en coordenadas de este objeto)")]
    [SerializeField] Vector3 zona = new Vector3(3f, 4f, 3f);
    [SerializeField] Transform jugador;
    [Tooltip("Solo se puede usar mientras este componente este activo (ej. el movimiento del jugador; apagado en cinematicas)")]
    [SerializeField] Behaviour requiereActivo;

    [Header("Contenido (en ingles; se traduce con Idioma)")]
    [SerializeField] string accion = "Read log";
    [SerializeField] string titulo = "Simon";
    [SerializeField, TextArea(2, 5)] string texto = "";
    [SerializeField] TMP_FontAsset fuente;
    [SerializeField] Color colorTitulo = new Color(0.37f, 0.89f, 0.88f);
    [SerializeField] Color colorTexto = Color.white;

    [Header("Interfaz compartida")]
    [SerializeField] TextMeshProUGUI aviso;
    [SerializeField] CanvasGroup caja;
    [SerializeField] TextMeshProUGUI cajaTitulo;
    [SerializeField] TextMeshProUGUI cajaTexto;
    [SerializeField] TextMeshProUGUI cajaAviso;
    [SerializeField] KeyCode tecla = KeyCode.E;
    [SerializeField] float letrasPorSegundo = 45f;
    [SerializeField] float tonoVoz = 1f;

    static readonly List<Registro_Interactivo> todos = new List<Registro_Interactivo>();
    static Registro_Interactivo abierto;
    static int ultimoCuadro = -1;
    static AudioSource fuenteVoz;
    static AudioClip clipVoz;

    float visibles;
    int total;
    bool completo;

    void OnEnable() { todos.Add(this); }
    void OnDisable()
    {
        todos.Remove(this);
        if (abierto == this) Cerrar();
    }

    void Start()
    {
        if (caja != null && abierto == null) caja.alpha = 0f;
        if (aviso != null && abierto == null) aviso.enabled = false;
        if (clipVoz == null) clipVoz = Pitido(0.035f, 600f);
        if (fuenteVoz == null)
        {
            var go = new GameObject("Voz_Registros");
            fuenteVoz = go.AddComponent<AudioSource>();
            fuenteVoz.playOnAwake = false;
            fuenteVoz.spatialBlend = 0f;
            fuenteVoz.volume = 0.6f;
        }
    }

    bool Dentro()
    {
        if (jugador == null) return false;
        Vector3 p = transform.InverseTransformPoint(jugador.position);
        Vector3 m = zona * 0.5f;
        return Mathf.Abs(p.x) <= m.x && Mathf.Abs(p.y) <= m.y && Mathf.Abs(p.z) <= m.z;
    }

    bool Disponible()
    {
        if (Time.timeScale <= 0f) return false;
        if (requiereActivo != null && !requiereActivo.isActiveAndEnabled) return false;
        return Dentro();
    }

    // Uno solo de los registros maneja el aviso cada cuadro
    void Update()
    {
        if (ultimoCuadro == Time.frameCount) return;
        ultimoCuadro = Time.frameCount;

        if (abierto != null) { abierto.Leer(); return; }

        Registro_Interactivo cerca = null;
        float mejor = float.MaxValue;
        foreach (var r in todos)
        {
            if (r == null || !r.Disponible()) continue;
            float d = (r.transform.position - r.jugador.position).sqrMagnitude;
            if (d < mejor) { mejor = d; cerca = r; }
        }

        TextMeshProUGUI av = cerca != null ? cerca.aviso : aviso;
        if (av != null)
        {
            av.enabled = cerca != null;
            if (cerca != null) av.text = "[" + cerca.tecla + "]  " + Idioma.T(cerca.accion);
        }
        if (cerca != null && Input.GetKeyDown(cerca.tecla)) cerca.Abrir();
    }

    void Abrir()
    {
        abierto = this;
        if (aviso != null) aviso.enabled = false;
        if (caja == null || cajaTexto == null) { abierto = null; return; }
        if (cajaTitulo != null)
        {
            cajaTitulo.text = Idioma.T(titulo);
            cajaTitulo.color = colorTitulo;
            if (fuente != null) cajaTitulo.font = fuente;
        }
        if (fuente != null) cajaTexto.font = fuente;
        cajaTexto.color = colorTexto;
        cajaTexto.text = Idioma.T(texto);
        cajaTexto.maxVisibleCharacters = 0;
        cajaTexto.ForceMeshUpdate();
        total = cajaTexto.textInfo.characterCount;
        visibles = 0f;
        completo = false;
        if (cajaAviso != null) cajaAviso.enabled = false;
        caja.alpha = 1f;
    }

    void Leer()
    {
        bool pasar = Input.GetKeyDown(tecla) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0);

        // Si el jugador se aleja o vuelve una cinematica, se cierra
        if (!Disponible()) { Cerrar(); return; }

        if (!completo)
        {
            int antes = Mathf.FloorToInt(visibles);
            visibles = pasar ? total : visibles + Time.unscaledDeltaTime * letrasPorSegundo;
            int n = Mathf.Min(total, Mathf.FloorToInt(visibles));
            cajaTexto.maxVisibleCharacters = n;
            if (!pasar && n != antes && n % 2 == 0 && fuenteVoz != null)
            {
                fuenteVoz.pitch = tonoVoz + Random.Range(-0.05f, 0.05f);
                fuenteVoz.PlayOneShot(clipVoz);
            }
            if (n >= total)
            {
                completo = true;
                if (cajaAviso != null) cajaAviso.enabled = true;
            }
            return;
        }
        if (pasar) Cerrar();
    }

    void Cerrar()
    {
        if (abierto == this) abierto = null;
        if (caja != null) caja.alpha = 0f;
        if (cajaAviso != null) cajaAviso.enabled = false;
    }

    static AudioClip Pitido(float segundos, float frecuencia)
    {
        int n = Mathf.CeilToInt(44100 * segundos);
        var clip = AudioClip.Create("VozRegistro", n, 1, 44100, false);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float s = Mathf.Sin(2f * Mathf.PI * frecuencia * i / 44100f);
            d[i] = s * (1f - i / (float)n) * 0.18f;
        }
        clip.SetData(d, 0);
        return clip;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.37f, 0.89f, 0.88f, 0.35f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, zona);
    }
}
