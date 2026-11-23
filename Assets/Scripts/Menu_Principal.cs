using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Menu principal: aparece despues de elegir el idioma y antes del dialogo inicial.
// De fondo, la camara se mece despacio sobre la estacion con la Tierra debajo,
// los asteroides giran lentamente y la Tierra rota. Jugar sigue con la cinematica; Salir cierra el juego.
// Se maneja con raton o con W/S (o flechas) y Enter.
public class Menu_Principal : MonoBehaviour
{
    [Header("Fondo")]
    [SerializeField] Camera camara;
    [SerializeField] Transform centro;                 // lo que mira la camara
    [SerializeField] float radio = 260f;
    [SerializeField] float altura = 70f;
    [SerializeField] float bajarMirada = 30f;
    [SerializeField] float anguloBase = 180f;           // grados alrededor del centro
    [SerializeField] float vaiven = 28f;                // grados hacia cada lado
    [SerializeField] float periodo = 50f;               // segundos por ida y vuelta
    [SerializeField] float desvioMirada = -14f;         // giro extra para dejar la estacion a la derecha del logo
    [SerializeField] float campoVision = 50f;
    [SerializeField] Transform asteroides;              // sus hijos giran despacio
    [SerializeField] float giroAsteroides = 8f;         // grados por segundo
    [SerializeField] Transform tierra;
    [SerializeField] float giroTierra = 0.5f;

    [Header("Interfaz")]
    [SerializeField] CanvasGroup panel;
    [SerializeField] CanvasGroup negro;
    [SerializeField] Button[] botones;                  // 0 = Jugar, 1 = Salir
    [SerializeField] Color fondoNormal = new Color(0.02f, 0.07f, 0.09f, 0.72f);
    [SerializeField] Color fondoSeleccion = new Color(0.37f, 0.89f, 0.88f, 0.92f);
    [SerializeField] Color textoNormal = new Color(0.37f, 0.89f, 0.88f, 0.9f);
    [SerializeField] Color textoSeleccion = new Color(0.02f, 0.07f, 0.09f);
    [Tooltip("Piezas de cada boton (se buscan por nombre): Marco, Recorte/Brillo, Flecha_Izq, Flecha_Der, Texto")]
    [SerializeField] float separacionFlechas = 26f;
    [SerializeField] float escalaSeleccion = 1.06f;

    [Header("Musica (opcional)")]
    [SerializeField] AudioClip musica;
    [SerializeField] float volumen = 0.6f;

    AudioSource fuente;
    AudioSource fuenteUI;
    AudioClip clipMover, clipConfirmar;
    float[] mezcla;          // 0 = normal, 1 = seleccionado (suavizado)
    float pulso;             // destello al confirmar
    int seleccion;
    int elegido = -1;
    Vector3 ultimoRaton;
    Quaternion[] rotAsteroides;
    Quaternion rotTierra;
    float tiempo;

    void Awake()
    {
        if (panel != null) { panel.alpha = 0f; panel.interactable = false; panel.blocksRaycasts = false; panel.gameObject.SetActive(false); }
        for (int i = 0; i < botones.Length; i++)
        {
            int indice = i;
            if (botones[i] != null) botones[i].onClick.AddListener(() => { seleccion = indice; elegido = indice; });
        }
        mezcla = new float[botones.Length];
        fuenteUI = gameObject.AddComponent<AudioSource>();
        fuenteUI.playOnAwake = false; fuenteUI.spatialBlend = 0f; fuenteUI.volume = 0.5f;
        clipMover = Tono(new[] { 1320f }, 0.035f);
        clipConfirmar = Tono(new[] { 880f, 1320f, 1760f }, 0.05f);
    }

    // Pitidos cortos de onda cuadrada, al estilo de la interfaz del traje
    static AudioClip Tono(float[] notas, float porNota)
    {
        int n = Mathf.CeilToInt(44100 * porNota);
        var datos = new float[n * notas.Length];
        for (int k = 0; k < notas.Length; k++)
            for (int i = 0; i < n; i++)
            {
                float s = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * notas[k] * i / 44100f)) * 0.5f;
                datos[k * n + i] = s * (1f - i / (float)n) * 0.18f;
            }
        var clip = AudioClip.Create("TonoMenu", datos.Length, 1, 44100, false);
        clip.SetData(datos, 0);
        return clip;
    }

    // La musica del menu suena desde la eleccion de idioma
    public void EmpezarMusica()
    {
        if (musica == null) return;
        if (fuente == null)
        {
            fuente = gameObject.AddComponent<AudioSource>();
            fuente.playOnAwake = false; fuente.loop = true; fuente.spatialBlend = 0f;
        }
        fuente.clip = musica;
        fuente.volume = volumen;
        if (!fuente.isPlaying) fuente.Play();
    }

    public IEnumerator Mostrar()
    {
        EmpezarMusica();
        float fovAntes = camara != null ? camara.fieldOfView : 60f;
        Guardar();
        tiempo = 0f;
        Fondo(0f);

        panel.gameObject.SetActive(true);
        panel.alpha = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        seleccion = 0; elegido = -1; pulso = 0f;
        for (int i = 0; i < mezcla.Length; i++) mezcla[i] = i == seleccion ? 1f : 0f;
        AnimarBotones(0f);
        ultimoRaton = Input.mousePosition;

        // Aparece el fondo desde negro y luego el menu
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 1.6f)
        {
            Fondo(Time.unscaledDeltaTime);
            AnimarBotones(Time.unscaledDeltaTime);
            if (negro != null) negro.alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
            panel.alpha = Mathf.Clamp01(t * 2f - 1f);
            yield return null;
        }
        if (negro != null) negro.alpha = 0f;
        panel.alpha = 1f;
        panel.interactable = true; panel.blocksRaycasts = true;

        while (true)
        {
            Fondo(Time.unscaledDeltaTime);
            Navegar();
            AnimarBotones(Time.unscaledDeltaTime);
            if (elegido >= 0)
            {
                // Destello y pitido de confirmacion antes de actuar
                fuenteUI.PlayOneShot(clipConfirmar);
                pulso = 1f;
                int opcion = elegido;
                elegido = -1;
                for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime)
                {
                    Fondo(Time.unscaledDeltaTime);
                    AnimarBotones(Time.unscaledDeltaTime);
                    yield return null;
                }
                if (opcion == 1) { Salir(); continue; }
                break;
            }
            yield return null;
        }

        // Jugar: se funde a negro, se apaga la musica del menu y se devuelve todo como estaba
        panel.interactable = false; panel.blocksRaycasts = false;
        float vol = fuente != null ? fuente.volume : 0f;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.9f)
        {
            Fondo(Time.unscaledDeltaTime);
            AnimarBotones(Time.unscaledDeltaTime);
            if (negro != null) negro.alpha = t;
            panel.alpha = 1f - t;
            if (fuente != null) fuente.volume = vol * (1f - t);
            yield return null;
        }
        if (negro != null) negro.alpha = 1f;
        if (fuente != null) fuente.Stop();
        panel.alpha = 0f;
        panel.gameObject.SetActive(false);
        Restaurar();
        if (camara != null) camara.fieldOfView = fovAntes;
    }

    void Navegar()
    {
        int n = botones.Length;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) { seleccion = (seleccion + n - 1) % n; Resaltar(); }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) { seleccion = (seleccion + 1) % n; Resaltar(); }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) elegido = seleccion;

        // El raton selecciona al pasar por encima
        if ((Input.mousePosition - ultimoRaton).sqrMagnitude > 1f)
        {
            ultimoRaton = Input.mousePosition;
            for (int i = 0; i < n; i++)
            {
                var rt = botones[i].transform as RectTransform;
                if (RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, null) && seleccion != i) { seleccion = i; Resaltar(); }
            }
        }
    }

    // Cambio de seleccion: solo suena; la animacion la hace AnimarBotones
    void Resaltar()
    {
        if (fuenteUI != null && clipMover != null) fuenteUI.PlayOneShot(clipMover);
    }

    // El boton elegido crece, se llena de color, le aparecen flechas que laten a los lados
    // y un brillo lo recorre de vez en cuando; al confirmar destella
    void AnimarBotones(float dt)
    {
        pulso = Mathf.MoveTowards(pulso, 0f, dt * 3f);
        float reloj = Time.unscaledTime;
        for (int i = 0; i < botones.Length; i++)
        {
            Button b = botones[i];
            if (b == null) continue;
            mezcla[i] = Mathf.MoveTowards(mezcla[i], i == seleccion ? 1f : 0f, dt * 7f);
            float k = Mathf.SmoothStep(0f, 1f, mezcla[i]);
            float destello = i == seleccion ? pulso : 0f;
            RectTransform rt = (RectTransform)b.transform;

            float respira = i == seleccion ? Mathf.Sin(reloj * 3f) * 0.012f : 0f;
            float escala = Mathf.Lerp(1f, escalaSeleccion, k) + respira + destello * 0.06f;
            rt.localScale = new Vector3(escala, escala, 1f);

            var img = b.GetComponent<Image>();
            if (img != null) img.color = Color.Lerp(Color.Lerp(fondoNormal, fondoSeleccion, k), Color.white, destello * 0.8f);
            var txt = b.transform.Find("Texto")?.GetComponent<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.color = Color.Lerp(textoNormal, textoSeleccion, k);
                txt.characterSpacing = Mathf.Lerp(6f, 14f, k);
            }
            var marco = b.transform.Find("Marco")?.GetComponent<Image>();
            if (marco != null) marco.color = new Color(fondoSeleccion.r, fondoSeleccion.g, fondoSeleccion.b, Mathf.Lerp(0.55f, 1f, k));

            float mitad = rt.rect.width * 0.5f;
            float vaivenFlecha = Mathf.Sin(reloj * 5f) * 5f;
            Flecha(b.transform.Find("Flecha_Izq") as RectTransform, -(mitad + separacionFlechas + vaivenFlecha), k);
            Flecha(b.transform.Find("Flecha_Der") as RectTransform, mitad + separacionFlechas + vaivenFlecha, k);

            var brillo = b.transform.Find("Recorte/Brillo") as RectTransform;
            if (brillo != null)
            {
                float ciclo = Mathf.Repeat(reloj / 2.4f, 1f);
                float ancho = rt.rect.width + brillo.rect.width;
                brillo.anchoredPosition = new Vector2(-ancho * 0.5f + ciclo * ancho * 1.6f, 0f);
                var bi = brillo.GetComponent<Image>();
                if (bi != null) bi.color = new Color(1f, 1f, 1f, 0.45f * k);
            }
        }
    }

    static void Flecha(RectTransform f, float x, float k)
    {
        if (f == null) return;
        f.anchoredPosition = new Vector2(x, 0f);
        var img = f.GetComponent<Image>();
        if (img != null) { var c = img.color; c.a = k; img.color = c; }
    }

    void Fondo(float dt)
    {
        tiempo += dt;
        if (camara != null && centro != null)
        {
            float a = (anguloBase + Mathf.Sin(tiempo * 2f * Mathf.PI / periodo) * vaiven) * Mathf.Deg2Rad;
            float h = altura + Mathf.Sin(tiempo * 2f * Mathf.PI / (periodo * 0.63f)) * altura * 0.12f;
            Vector3 pos = centro.position + new Vector3(Mathf.Sin(a) * radio, h, Mathf.Cos(a) * radio);
            Quaternion mira = Quaternion.LookRotation(centro.position + Vector3.down * bajarMirada - pos);
            camara.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, desvioMirada, 0f) * mira);
            camara.fieldOfView = campoVision;
        }
        if (asteroides != null && rotAsteroides != null)
            for (int i = 0; i < asteroides.childCount; i++)
            {
                Transform t = asteroides.GetChild(i);
                Vector3 eje = new Vector3(Mathf.Sin(i * 1.7f), Mathf.Cos(i * 2.3f), Mathf.Sin(i * 0.9f + 1f)).normalized;
                t.Rotate(eje, giroAsteroides * (0.5f + (i % 5) * 0.25f) * dt, Space.World);
            }
        if (tierra != null) tierra.Rotate(Vector3.up, giroTierra * dt, Space.World);
    }

    void Guardar()
    {
        if (asteroides != null)
        {
            rotAsteroides = new Quaternion[asteroides.childCount];
            for (int i = 0; i < rotAsteroides.Length; i++) rotAsteroides[i] = asteroides.GetChild(i).localRotation;
        }
        if (tierra != null) rotTierra = tierra.localRotation;
    }

    // Los asteroides tienen colisiones en el nivel: vuelven a su posicion original
    void Restaurar()
    {
        if (asteroides != null && rotAsteroides != null)
            for (int i = 0; i < rotAsteroides.Length && i < asteroides.childCount; i++) asteroides.GetChild(i).localRotation = rotAsteroides[i];
        if (tierra != null) tierra.localRotation = rotTierra;
    }

    void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
