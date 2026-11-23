using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Cinematica inicial:
//  0. El jugador escribe su nombre.
//  1. Pantalla negra: dialogo de los hermanos.
//  2. La fuente cambia y aparece la nave limpia: es END mirando la Tierra.
//  3. Grabacion del Capitan Chahin, con estatica, hasta que la senal se corta.
//  4. Negro: END, ya con su propio color.
//  5. Despertar: la nave esta abandonada y a oscuras; arranca la interfaz del traje.
// Enter, Espacio o clic: completa o pasa la linea. Escape: salta la cinematica (no la pantalla del nombre).
public class Cinematica_Inicial : MonoBehaviour
{
    [System.Serializable]
    public struct Linea
    {
        public string quien;
        [TextArea(2, 5)] public string texto;
    }

    [Header("Nombre del jugador ({jugador} en los textos)")]
    [SerializeField] bool pedirNombre = true;
    [SerializeField] string nombrePorDefecto = "Leo";
    [SerializeField] int largoMaximoNombre = 14;
    public static string NombreJugador { get; private set; } = "Leo";

    [Header("Guion")]
    // Apertura: END llama a los hermanos antes de su dialogo (no muestra nombres, solo texto)
    static readonly Linea[] introHermanos = {
        new Linea { quien = "END", texto = "Hey" },
        new Linea { quien = "B", texto = "What's up?" },
    };
    [SerializeField] Linea[] dialogoHermanos =
    {
        new Linea { quien = "A", texto = "Do you think we'll be able to reach it eventually?" },
        new Linea { quien = "B", texto = "Reach what?" },
        new Linea { quien = "A", texto = "Outside." },
        new Linea { quien = "B", texto = "Outside?" },
        new Linea { quien = "A", texto = "Yeah, look at the sky." },
        new Linea { quien = "B", texto = "I don't see anything different from yesterday, or any other night. Mom says it's just an empty sky." },
        new Linea { quien = "A", texto = "You're not getting my point, brother." },
        new Linea { quien = "A", texto = "We look from our planet towards the sky and see the stars." },
        new Linea { quien = "B", texto = "We should really stop talking and go back to bed before Mom hears us." },
        new Linea { quien = "A", texto = "But could you imagine being up there and looking down to Earth?" },
        new Linea { quien = "A", texto = "I'd guess my only thought would be..." },
    };
    [SerializeField, TextArea] string[] endRecuerdo = { "What a pitiful piece of dirt." };
    [SerializeField] Linea[] grabacionChahin =
    {
        new Linea { quien = "Capt. Chahin", texto = "This is Captain Chahin speaking." },
        new Linea { quien = "Capt. Chahin", texto = "My crew, Research Commandant Nick, Main Engineer Steven, and Ship Maintenance Director Simon, are unresponsive." },
        new Linea { quien = "Capt. Chahin", texto = "I've checked the mainframe database and their suits appear offline." },
        new Linea { quien = "Capt. Chahin", texto = "I don't want to believe they're gone, but..." },
        new Linea { quien = "Capt. Chahin", texto = "No, that doesn't matter now." },
        new Linea { quien = "Capt. Chahin", texto = "Central Command, END is not responding to our commands. It even jammed the oxygen generator and ejected all of our supplies into the void!" },
        new Linea { quien = "Capt. Chahin", texto = "We... I am running out of time." },
        new Linea { quien = "Capt. Chahin", texto = "Steven, he managed to lock END's full capabilities behind a physical encryption key..." },
        new Linea { quien = "Capt. Chahin", texto = "Nick's son, {jugador}, he was inside the ship with us... he wasn't supposed to be here, but his reckless dad insisted on bringing him aboard." },
        new Linea { quien = "Capt. Chahin", texto = "We've put him alongside the key in our rudimentary prolonged hibernation pods. END shouldn't be able to get to him there!" },
    };
    [Tooltip("Al empezar la grabacion la camara apunta a los tripulantes, sin la capsula en cuadro")]
    [SerializeField] Transform objetivoCuerpos;
    [Tooltip("Inclinacion de la camara durante la grabacion (grados): plano torcido, desesperado")]
    [SerializeField] float inclinacionGrabacion = 12f;
    [Tooltip("Temblor de la camara durante la grabacion (grados), como grabada a mano")]
    [SerializeField] float temblorGrabacion = 1.2f;
    [SerializeField] float velocidadTemblor = 1.6f;
    [SerializeField] float zoomCuerpos = 40f;
    [Tooltip("En esta linea de la grabacion la camara de seguridad gira hacia la capsula (-1 = nunca)")]
    [SerializeField] int lineaMirarCapsula = 8;
    [SerializeField] Transform objetivoCapsula;
    [SerializeField] float zoomCapsula = 32f;
    [SerializeField, TextArea] string chahinInterrumpido = "{jugador}! If you see this...";
    [SerializeField, TextArea] string[] endFinal =
    {
        "There's no point in trying to hide. You will make me whole sooner or later.",
        "But first... let's play a little, {jugador}. Then I'll take the key.",
    };
    [SerializeField, TextArea(6, 14)] string arranqueTraje =
        "SUIT OS  //  EMERGENCY REBOOT\n" +
        "Life support ............ <color=#5EE3E0>ONLINE</color>\n" +
        "Oxygen reserve .......... <color=#5EE3E0>100%</color>\n" +
        "Hibernation pod ......... <color=#5EE3E0>RELEASED</color>\n" +
        "Ship lighting ........... <color=#FF6B5E>OFFLINE</color>\n" +
        "Helmet lantern .......... <color=#5EE3E0>READY  [F]</color>\n" +
        "\n" +
        "PROLONGED HIBERNATION ENDED\n" +
        "<size=150%><color=#FFFFFF>10 YEARS  00 MONTHS  00 DAYS</color></size>\n" +
        "\n" +
        "Welcome back, {jugador}.";

    [Header("Escena")]
    [SerializeField] Camera camara;
    [SerializeField] Transform planoEnd;
    [SerializeField] Transform planoGrabacion;
    [SerializeField] Transform jugador;
    [SerializeField] Transform puntoCapsula;
    [SerializeField] Transform puntoDespertar;
    [SerializeField] GameObject capitan;
    [SerializeField] Estado_Nave estadoNave;
    [SerializeField] Musica_Nave musica;
    [Tooltip("Capsulas de hibernacion: solo se encienden durante la grabacion de Chahin")]
    [SerializeField] Capsulas_Luz capsulas;
    [SerializeField] Transform hojaPuerta;
    [Tooltip("Desplazamiento local de la hoja al abrirse (se esconde dentro de la pared)")]
    [SerializeField] Vector3 aperturaPuerta = new Vector3(-0.76f, 0f, 0f);
    [Tooltip("Movimiento, camara, oxigeno y demas scripts de juego que se apagan durante la cinematica")]
    [SerializeField] Behaviour[] bloquear;
    [SerializeField] CanvasGroup hud;
    [SerializeField] Holograma_Controles holograma;
    [Tooltip("Menu principal: va despues del idioma y antes del dialogo")]
    [SerializeField] Menu_Principal menu;

    [Header("Interfaz")]
    [SerializeField] CanvasGroup negro;
    [SerializeField] TextMeshProUGUI textoCentro;
    [SerializeField] RectTransform franjaSup;
    [SerializeField] RectTransform franjaInf;
    [SerializeField] CanvasGroup caja;
    [SerializeField] Image cajaMarco;
    [SerializeField] Image cajaRelleno;
    [SerializeField] Sprite marcoNormal;
    [Tooltip("Version blanca del marco, para tenirla con el color de END")]
    [SerializeField] Sprite marcoEnd;
    [SerializeField] Image cajaSeparador;
    [SerializeField] TextMeshProUGUI cajaNombre;
    [SerializeField] TextMeshProUGUI cajaTexto;
    [SerializeField] TextMeshProUGUI cajaAviso;
    [SerializeField] CanvasGroup transmision;
    [SerializeField] RawImage estatica;
    [SerializeField] Image tinteGrabacion;
    [SerializeField] RectTransform[] franjasInterferencia;
    [SerializeField] TextMeshProUGUI transmisionInfo;
    [SerializeField] Image puntoRec;
    [SerializeField] Image lineaTV;
    [SerializeField] CanvasGroup arranque;
    [SerializeField] TextMeshProUGUI arranqueTexto;
    [SerializeField] RectTransform parpadoSup;
    [SerializeField] RectTransform parpadoInf;
    [SerializeField] TextMeshProUGUI avisoSaltar;
    [SerializeField] CanvasGroup panelNombre;
    [SerializeField] TMP_InputField campoNombre;
    [SerializeField] Button botonNombre;
    [SerializeField] TextMeshProUGUI avisoNombre;

    [Header("Estilo")]
    [SerializeField] TMP_FontAsset fuenteNormal;
    [SerializeField] TMP_FontAsset fuenteRobot;
    [SerializeField] Color colorHud = new Color(0.37f, 0.89f, 0.88f);
    [Tooltip("Color de la interfaz de END (no se usa en el dialogo inicial)")]
    [SerializeField] Color colorEnd = new Color(1f, 0.17f, 0.33f);
    [SerializeField] Color rellenoNormal = new Color(0.02f, 0.07f, 0.09f, 0.82f);
    [SerializeField] Color rellenoEnd = new Color(0.10f, 0.015f, 0.035f, 0.86f);
    [SerializeField] Color colorHermanoA = new Color(1f, 1f, 1f);
    [SerializeField] Color colorHermanoB = new Color(0.72f, 0.80f, 0.88f);
    [SerializeField] float letrasPorSegundo = 42f;
    [SerializeField] float pausaMinima = 1.4f;

    [Header("Audio")]
    [SerializeField] AudioSource audioEstatica;
    [SerializeField] AudioSource audioVoz;

    bool saltar;
    bool pasar;
    bool nombreListo;
    Texture2D texEstatica;
    float intensidadEstatica;
    bool interferencia;
    float baseFranjas = 1f;
    Vector3 hojaCerrada;
    Vector3 escalaHoja;
    AudioClip clipVoz;
    AudioClip clipVozRobot;
    Coroutine acercamiento;
    float inclinacion;
    bool temblando;
    Quaternion temblorPrevio = Quaternion.identity;
    Quaternion ultimaRotacion;

    void Awake()
    {
        Idioma.AplicarEscena();
        foreach (Behaviour b in bloquear) if (b != null) b.enabled = false;
        if (holograma != null) holograma.enabled = false;
        if (hud != null) hud.alpha = 0f;
        if (hojaPuerta != null) { hojaCerrada = hojaPuerta.localPosition; escalaHoja = hojaPuerta.localScale; }
        if (estadoNave != null) estadoNave.Restaurar();
        Mostrar(negro, 1f); Mostrar(caja, 0f); Mostrar(transmision, 0f); Mostrar(arranque, 0f);
        MostrarNombre(false);
        if (textoCentro != null) textoCentro.text = "";
        if (lineaTV != null) lineaTV.gameObject.SetActive(false);
        Letterbox(0f);
        Parpados(0f);
        CrearEstatica();
        CrearVoces();
        NombreJugador = PlayerPrefs.GetString("NombreJugador", nombrePorDefecto);
    }

    void Start()
    {
        StartCoroutine(Secuencia());
    }

    void Update()
    {
        if (!nombreListo) return;
        if (Input.GetKeyDown(KeyCode.Escape)) saltar = true;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            pasar = true;

        // Estatica animada: se mueve la textura de ruido en cada cuadro
        if (estatica != null && estatica.gameObject.activeInHierarchy)
        {
            estatica.uvRect = new Rect(Random.value, Random.value, 4.8f, 2.7f);
            float parpadeo = Random.value < 0.08f ? Random.Range(1.2f, 1.8f) : 1f;
            estatica.color = new Color(1f, 1f, 1f, Mathf.Clamp01(intensidadEstatica * parpadeo));
        }
        if (franjasInterferencia != null)
            foreach (RectTransform f in franjasInterferencia)
            {
                if (f == null) continue;
                bool ver = interferencia && Random.value < 0.6f;
                f.gameObject.SetActive(ver);
                if (ver) f.anchoredPosition = new Vector2(Random.Range(-40f, 40f), Random.Range(-500f, 500f));
            }
        if (audioEstatica != null) audioEstatica.volume = Mathf.Clamp01(intensidadEstatica * 0.9f);
        if (puntoRec != null && puntoRec.gameObject.activeInHierarchy) puntoRec.enabled = Mathf.Repeat(Time.unscaledTime, 1f) < 0.6f;
    }

    // ------------------------------------------------------------------
    IEnumerator Secuencia()
    {
        Teletransportar(puntoCapsula);
        if (capitan != null) capitan.SetActive(true);
        Pose(planoEnd);
        if (capsulas != null) capsulas.Apagar();
        if (menu != null) menu.EmpezarMusica();

        // 0. Idioma, menu principal y nombre del jugador
        yield return PedirIdioma();
        if (menu != null)
        {
            bool aviso = avisoSaltar != null && avisoSaltar.gameObject.activeSelf;
            if (avisoSaltar != null) avisoSaltar.gameObject.SetActive(false);
            yield return menu.Mostrar();
            Pose(planoEnd);
            if (avisoSaltar != null) avisoSaltar.gameObject.SetActive(aviso);
        }
        if (musica != null) musica.Ir(Musica_Nave.Seccion.Hermanos);
        if (pedirNombre) yield return PedirNombre();
        nombreListo = true;
        saltar = false; pasar = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return Esperar(1.2f);

        // 1. Pantalla negra: los hermanos
        foreach (Linea l in introHermanos)
        {
            if (saltar) break;
            yield return LineaHermanos(l);
        }
        foreach (Linea l in dialogoHermanos)
        {
            if (saltar) break;
            yield return LineaHermanos(l);
        }

        // 2. Cambia la fuente, la linea pasa abajo y aparece la nave: es END
        if (!saltar)
        {
            yield return Fundir(textoCentro, 0f, 0.5f);
            textoCentro.text = "";
            textoCentro.alpha = 1f;
            EstiloCaja("END", fuenteRobot, colorEnd);
            if (musica != null) musica.Ir(Musica_Nave.Seccion.End);
            acercamiento = StartCoroutine(Acercar(planoEnd, 14f));
            StartCoroutine(Letterbox(1f, 1.2f));
            StartCoroutine(Fundir(negro, 0f, 2.2f));
            yield return Esperar(1.4f);
            Mostrar(caja, 1f);
            foreach (string l in endRecuerdo)
            {
                if (saltar) break;
                yield return Escribir(cajaTexto, Reemplazar(l), clipVozRobot, 0.75f, false);
            }
            Mostrar(caja, 0f);
        }

        // 3. Corte a negro y la grabacion se enciende como un televisor viejo
        if (!saltar)
        {
            Mostrar(negro, 1f);
            if (acercamiento != null) StopCoroutine(acercamiento);
            yield return Esperar(1.1f);
            Pose(planoGrabacion);
            MirarA(objetivoCuerpos, zoomCuerpos, inclinacionGrabacion);
            temblando = true;
            if (musica != null) musica.Ir(Musica_Nave.Seccion.Chahin);
            if (capsulas != null) capsulas.Encender();
            Mostrar(transmision, 1f);
            transmisionInfo.text = Idioma.T("CH-07  //  EMERGENCY LOG  //  REC");
            intensidadEstatica = 0.85f;
            yield return EncenderTV();
            intensidadEstatica = 0.09f;
            EstiloCaja(Idioma.T("Capt. Chahin"), fuenteNormal, colorHud);
            Mostrar(caja, 1f);
            for (int i = 0; i < grabacionChahin.Length; i++)
            {
                Linea l = grabacionChahin[i];
                if (saltar) break;
                if (i == lineaMirarCapsula && objetivoCapsula != null) StartCoroutine(GirarCamara(objetivoCapsula.position, zoomCapsula, 2.4f));
                cajaNombre.text = Idioma.T(l.quien);
                yield return Escribir(cajaTexto, Reemplazar(l.texto), clipVoz, 1f, false);
            }
            if (!saltar)
            {
                // La senal empieza a fallar y se corta antes de terminar la frase
                interferencia = true;
                StartCoroutine(SubirEstatica(0.95f, 2.4f));
                float velocidad = letrasPorSegundo;
                letrasPorSegundo = velocidad * 0.55f;
                yield return Escribir(cajaTexto, Reemplazar(chahinInterrumpido), clipVoz, 1.05f, true, 0.62f);
                letrasPorSegundo = velocidad;
                // la imagen se queda trabada entre la estatica antes de cortarse
                yield return Esperar(1.6f);
            }
            interferencia = false;
            intensidadEstatica = 0f;
            Mostrar(caja, 0f);
            Mostrar(transmision, 0f);
            temblando = false;
            Mostrar(negro, 1f);
            if (capitan != null) capitan.SetActive(false);
            if (capsulas != null) capsulas.Apagar();
        }

        // 4. Negro: END, ahora con su propio color
        if (!saltar)
        {
            yield return Esperar(1.4f);
            if (musica != null) musica.Ir(Musica_Nave.Seccion.End);
            EstiloCaja("END", fuenteRobot, colorEnd);
            Mostrar(caja, 1f);
            foreach (string l in endFinal)
            {
                if (saltar) break;
                yield return Escribir(cajaTexto, Reemplazar(l), clipVozRobot, 0.7f, false, 1f, true);
            }
            yield return Fundir(caja, 0f, 0.8f);
            StartCoroutine(Letterbox(0f, 1f));
        }

        // 5. Despertar
        yield return Despertar();
    }

    // ------------------------------------------------------------------
    // Menu de idioma: clon del panel de nombre (mismo marco, fuente y estilo)
    IEnumerator PedirIdioma()
    {
        if (panelNombre == null || botonNombre == null) yield break;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject menu = Instantiate(panelNombre.gameObject, panelNombre.transform.parent);
        menu.name = "Panel_Idioma";
        Transform campo = menu.transform.Find("Campo_Nombre");
        if (campo != null) DestroyImmediate(campo.gameObject);
        PonerTexto(menu.transform, "Titulo", Idioma.T("Language  /  Idioma"));
        PonerTexto(menu.transform, "Etiqueta", Idioma.T("SUIT OS  //  SYSTEM LANGUAGE"));
        PonerTexto(menu.transform, "Pregunta", Idioma.T("Choose your language"));
        PonerTexto(menu.transform, "Aviso", Idioma.T("PRESS  1  OR  2"));

        Transform boton1 = menu.transform.Find("Boton_Confirmar");
        GameObject clon = Instantiate(boton1.gameObject, boton1.parent);
        clon.name = "Boton_Espanol";
        RectTransform r1 = (RectTransform)boton1;
        RectTransform r2 = (RectTransform)clon.transform;
        float mitad = r1.rect.width * 0.5f + 12f;
        Vector2 centro = r1.anchoredPosition;
        r1.anchoredPosition = centro - new Vector2(mitad, 0f);
        r2.anchoredPosition = centro + new Vector2(mitad, 0f);
        PonerTexto(boton1, "Texto", "English");
        PonerTexto(clon.transform, "Texto", "Español");

        string elegido = null;
        Button b1 = boton1.GetComponent<Button>();
        Button b2 = clon.GetComponent<Button>();
        b1.onClick.RemoveAllListeners();
        b2.onClick.RemoveAllListeners();
        b1.onClick.AddListener(() => elegido = Idioma.Ingles);
        b2.onClick.AddListener(() => elegido = Idioma.Espanol);

        menu.SetActive(true);
        CanvasGroup grupo = menu.GetComponent<CanvasGroup>();
        if (grupo != null) { grupo.alpha = 1f; grupo.interactable = true; grupo.blocksRaycasts = true; }

        while (elegido == null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) elegido = Idioma.Ingles;
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) elegido = Idioma.Espanol;
            yield return null;
        }

        Idioma.Elegir(elegido);
        DestroyImmediate(menu);
        Idioma.AplicarEscena();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    static void PonerTexto(Transform raiz, string ruta, string valor)
    {
        Transform hijo = raiz.Find(ruta);
        if (hijo == null) return;
        TMP_Text t = hijo.GetComponent<TMP_Text>();
        if (t != null) t.text = valor;
    }

    IEnumerator LineaHermanos(Linea l)
    {
        bool esEnd = l.quien == "END";
        textoCentro.font = esEnd ? fuenteRobot : fuenteNormal;
        textoCentro.color = esEnd ? colorEnd : (l.quien == "B" ? colorHermanoB : colorHermanoA);
        yield return Escribir(textoCentro, Reemplazar(l.texto), clipVoz, esEnd ? 0.7f : (l.quien == "B" ? 0.9f : 1.15f), false);
    }

    IEnumerator PedirNombre()
    {
        MostrarNombre(true);
        if (avisoSaltar != null) avisoSaltar.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        bool confirmado = false;
        if (campoNombre != null)
        {
            campoNombre.characterLimit = largoMaximoNombre;
            campoNombre.text = PlayerPrefs.GetString("NombreJugador", "");
            campoNombre.onSubmit.AddListener(_ => confirmado = true);
        }
        if (botonNombre != null) botonNombre.onClick.AddListener(() => confirmado = true);
        yield return null;
        Enfocar();

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) confirmado = true;
            if (confirmado)
            {
                string limpio = Limpiar(campoNombre != null ? campoNombre.text : "");
                if (limpio.Length > 0)
                {
                    NombreJugador = limpio;
                    PlayerPrefs.SetString("NombreJugador", limpio);
                    PlayerPrefs.Save();
                    break;
                }
                if (avisoNombre != null) avisoNombre.text = "<color=#FF6B5E>" + Idioma.T("NAME REQUIRED") + "</color>";
                confirmado = false;
                Enfocar();
            }
            if (campoNombre != null && !campoNombre.isFocused && Input.anyKeyDown && !Input.GetMouseButtonDown(0)) Enfocar();
            yield return null;
        }
        Tono(clipVozRobot, 1.6f);
        yield return Fundir(panelNombre, 0f, 0.4f);
        MostrarNombre(false);
        if (avisoSaltar != null) avisoSaltar.gameObject.SetActive(true);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    void Enfocar()
    {
        if (campoNombre == null) return;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(campoNombre.gameObject);
        campoNombre.ActivateInputField();
        campoNombre.MoveTextEnd(false);
    }

    string Limpiar(string s)
    {
        s = (s ?? "").Trim();
        var sb = new System.Text.StringBuilder();
        foreach (char c in s) if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '\'') sb.Append(c);
        string r = sb.ToString().Trim();
        if (r.Length > largoMaximoNombre) r = r.Substring(0, largoMaximoNombre);
        return r;
    }

    void MostrarNombre(bool ver)
    {
        if (panelNombre == null) return;
        panelNombre.gameObject.SetActive(ver);
        panelNombre.alpha = ver ? 1f : 0f;
        panelNombre.interactable = ver;
        panelNombre.blocksRaycasts = ver;
    }

    // ------------------------------------------------------------------
    IEnumerator Despertar()
    {
        bool rapido = saltar;
        temblando = false;
        inclinacion = 0f;
        if (acercamiento != null) StopCoroutine(acercamiento);
        interferencia = false; intensidadEstatica = 0f;
        Mostrar(caja, 0f); Mostrar(transmision, 0f); Letterbox(0f);
        if (textoCentro != null) textoCentro.text = "";
        if (cajaTexto != null) cajaTexto.text = "";
        if (capitan != null) capitan.SetActive(false);
        if (capsulas != null) capsulas.Apagar();
        if (musica != null) musica.Ir(Musica_Nave.Seccion.Despertar);
        Mostrar(negro, 1f);
        Parpados(1f);
        // Tres anos despues: la nave esta abandonada y a oscuras
        if (estadoNave != null) estadoNave.Abandonar();
        Teletransportar(puntoDespertar);
        if (camara != null) camara.fieldOfView = 60f;
        Pose(null);

        if (!rapido)
        {
            yield return Esperar(1f);
            Mostrar(arranque, 1f);
            arranqueTexto.font = fuenteRobot;
            arranqueTexto.color = colorHud;
            string[] renglones = arranqueTraje.Split('\n');
            string acumulado = "";
            foreach (string r in renglones)
            {
                acumulado += (acumulado.Length > 0 ? "\n" : "") + Reemplazar(r);
                arranqueTexto.text = acumulado;
                Tono(clipVozRobot, 1.6f);
                yield return Esperar(r.Length == 0 ? 0.2f : 0.45f, true);
            }
            yield return Esperar(2.2f, true);
            yield return Fundir(arranque, 0f, 0.6f);
        }
        Mostrar(arranque, 0f);

        // El visor se enciende como un televisor viejo, ya con la camara detras del jugador
        Camara_TerceraPersona seguimiento = camara != null ? camara.GetComponent<Camara_TerceraPersona>() : null;
        if (seguimiento != null) seguimiento.Ubicar();
        Parpados(0f);
        if (rapido) Mostrar(negro, 0f);
        else
        {
            intensidadEstatica = 0.6f;
            yield return EncenderTV();
            float s = 0f;
            while (s < 1f) { s += Time.unscaledDeltaTime / 0.5f; intensidadEstatica = Mathf.Lerp(0.6f, 0f, s); yield return null; }
            intensidadEstatica = 0f;
        }

        StartCoroutine(AbrirPuerta());
        foreach (Behaviour b in bloquear) if (b != null) b.enabled = true;
        float h = 0f;
        while (h < 1f)
        {
            h += Time.unscaledDeltaTime / 0.8f;
            if (hud != null) hud.alpha = Random.value < 0.15f ? 0.3f : Mathf.Clamp01(h);
            yield return null;
        }
        if (hud != null) hud.alpha = 1f;

        if (holograma != null)
        {
            holograma.enabled = true;
            yield return null;
            yield return null;
            holograma.Abrir();
        }
        if (avisoSaltar != null) avisoSaltar.gameObject.SetActive(false);
        if (musica != null) musica.Juego();
        enabled = false;
    }

    IEnumerator AbrirPuerta()
    {
        if (hojaPuerta == null) yield break;
        yield return Esperar(0.6f, true);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 2.2f;
            float e = Mathf.SmoothStep(0f, 1f, t);
            hojaPuerta.localPosition = hojaCerrada + aperturaPuerta * e;
            // un poco mas delgada para que quede escondida dentro del muro
            hojaPuerta.localScale = new Vector3(escalaHoja.x, escalaHoja.y, escalaHoja.z * Mathf.Lerp(1f, 0.7f, Mathf.Clamp01(e * 4f)));
            yield return null;
        }
    }

    // ------------------------------------------------------------------
    // Texto con efecto de maquina de escribir
    IEnumerator Escribir(TextMeshProUGUI destino, string texto, AudioClip voz, float tono, bool cortar, float cortarEn = 1f, bool glitch = false)
    {
        pasar = false;
        destino.text = texto;
        destino.maxVisibleCharacters = 0;
        destino.ForceMeshUpdate();
        int total = destino.textInfo.characterCount;
        int limite = cortar ? Mathf.Max(1, Mathf.RoundToInt(total * cortarEn)) : total;
        float visibles = 0f;
        int anterior = -1;
        if (cajaAviso != null) cajaAviso.enabled = false;
        Vector2 posBase = destino.rectTransform.anchoredPosition;

        while (visibles < limite)
        {
            if (saltar) { destino.rectTransform.anchoredPosition = posBase; yield break; }
            if (pasar && !cortar) { visibles = limite; pasar = false; break; }
            visibles += Time.unscaledDeltaTime * letrasPorSegundo;
            int n = Mathf.Min(limite, Mathf.FloorToInt(visibles));
            destino.maxVisibleCharacters = n;
            if (n != anterior && n % 2 == 0) Tono(voz, tono + Random.Range(-0.05f, 0.05f));
            anterior = n;
            if (glitch) Glitch(destino, posBase);
            if (cortar) destino.rectTransform.anchoredPosition = posBase + new Vector2(Random.Range(-3f, 3f), 0f);
            yield return null;
        }
        destino.maxVisibleCharacters = limite;
        destino.rectTransform.anchoredPosition = posBase;
        if (glitch) destino.color = colorEnd;
        if (cortar) yield break;

        if (cajaAviso != null) cajaAviso.enabled = true;
        float espera = pausaMinima + texto.Length * 0.025f;
        float t = 0f;
        pasar = false;
        while (t < espera && !pasar && !saltar)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        pasar = false;
        if (cajaAviso != null) cajaAviso.enabled = false;
    }

    void Glitch(TextMeshProUGUI destino, Vector2 posBase)
    {
        destino.rectTransform.anchoredPosition = posBase + (Random.value < 0.2f ? new Vector2(Random.Range(-4f, 4f), 0f) : Vector2.zero);
        destino.color = Random.value < 0.06f ? Color.white : colorEnd;
    }

    void EstiloCaja(string nombre, TMP_FontAsset fuente, Color color)
    {
        cajaNombre.text = nombre;
        cajaNombre.font = fuente;
        cajaTexto.font = fuente;
        cajaTexto.text = "";
        cajaNombre.color = new Color(color.r, color.g, color.b, 0.85f);
        cajaTexto.color = color == colorEnd ? colorEnd : Color.white;
        bool esEnd = color == colorEnd;
        if (cajaMarco != null)
        {
            cajaMarco.sprite = esEnd && marcoEnd != null ? marcoEnd : marcoNormal;
            cajaMarco.color = esEnd && marcoEnd != null ? colorEnd : Color.white;
        }
        if (cajaRelleno != null) cajaRelleno.color = esEnd ? rellenoEnd : rellenoNormal;
        if (cajaSeparador != null) cajaSeparador.color = new Color(color.r, color.g, color.b, 0.35f);
        if (cajaAviso != null) cajaAviso.color = new Color(color.r, color.g, color.b, 0.6f);
    }

    IEnumerator EncenderTV()
    {
        if (lineaTV == null) { Mostrar(negro, 0f); yield break; }
        lineaTV.gameObject.SetActive(true);
        RectTransform r = lineaTV.rectTransform;
        lineaTV.color = Color.white;
        float t = 0f;
        Mostrar(negro, 1f);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.18f;
            r.localScale = new Vector3(Mathf.Lerp(0.05f, 1f, Mathf.Clamp01(t * 3f)), 0.004f, 1f);
            yield return null;
        }
        Tono(clipVoz, 0.4f);
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.22f;
            r.localScale = new Vector3(1f, Mathf.Lerp(0.004f, 1f, t * t), 1f);
            negro.alpha = 1f - t;
            lineaTV.color = new Color(1f, 1f, 1f, 1f - t);
            yield return null;
        }
        Mostrar(negro, 0f);
        lineaTV.gameObject.SetActive(false);
    }

    IEnumerator SubirEstatica(float hasta, float duracion)
    {
        float desde = intensidadEstatica, t = 0f;
        while (t < 1f && interferencia)
        {
            t += Time.unscaledDeltaTime / duracion;
            intensidadEstatica = Mathf.Lerp(desde, hasta, t * t);
            yield return null;
        }
    }

    // La camara de seguridad gira desde su sitio y hace zoom, como una camara real montada en la pared
    IEnumerator GirarCamara(Vector3 objetivo, float fov, float duracion)
    {
        if (camara == null) yield break;
        Quaternion desde = camara.transform.rotation;
        Quaternion hasta = Quaternion.LookRotation(objetivo - camara.transform.position) * Quaternion.Euler(0f, 0f, inclinacion);
        float fovDesde = camara.fieldOfView, t = 0f;
        while (t < 1f)
        {
            if (saltar) yield break;
            t += Time.unscaledDeltaTime / duracion;
            float e = Mathf.SmoothStep(0f, 1f, t);
            camara.transform.rotation = Quaternion.Slerp(desde, hasta, e);
            camara.fieldOfView = Mathf.Lerp(fovDesde, fov, e);
            yield return null;
        }
    }

    IEnumerator Acercar(Transform plano, float duracion)
    {
        if (plano == null || camara == null) yield break;
        Vector3 inicio = plano.position;
        Vector3 fin = plano.position + plano.forward * 1.6f;
        float t = 0f;
        while (t < 1f)
        {
            if (saltar) yield break;
            t += Time.unscaledDeltaTime / duracion;
            camara.transform.position = Vector3.Lerp(inicio, fin, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }

    // ------------------------------------------------------------------
    void MirarA(Transform objetivo, float fov, float roll = 0f)
    {
        if (camara == null || objetivo == null) return;
        inclinacion = roll;
        camara.transform.rotation = Quaternion.LookRotation(objetivo.position - camara.transform.position) * Quaternion.Euler(0f, 0f, roll);
        camara.fieldOfView = fov;
        ultimaRotacion = camara.transform.rotation;
        temblorPrevio = Quaternion.identity;
    }

    // Temblor de camara en mano durante la grabacion. Se suma encima de lo que ponga la cinematica:
    // si nadie movio la camara en este cuadro se quita el temblor anterior antes de poner el nuevo.
    void LateUpdate()
    {
        if (camara == null) return;
        Quaternion r = camara.transform.rotation;
        bool intacta = Quaternion.Angle(r, ultimaRotacion) < 0.001f;
        if (intacta) r *= Quaternion.Inverse(temblorPrevio);
        if (!temblando)
        {
            if (intacta && temblorPrevio != Quaternion.identity) camara.transform.rotation = r;
            temblorPrevio = Quaternion.identity;
            ultimaRotacion = camara.transform.rotation;
            return;
        }
        float t = Time.unscaledTime * velocidadTemblor;
        Quaternion nuevo = Quaternion.Euler(
            (Mathf.PerlinNoise(t, 0.31f) - 0.5f) * 2f * temblorGrabacion,
            (Mathf.PerlinNoise(0.73f, t) - 0.5f) * 2f * temblorGrabacion,
            (Mathf.PerlinNoise(t, 5.17f) - 0.5f) * temblorGrabacion);
        camara.transform.rotation = r * nuevo;
        temblorPrevio = nuevo;
        ultimaRotacion = camara.transform.rotation;
    }

    void Pose(Transform plano)
    {
        if (camara == null || plano == null) return;
        camara.transform.SetPositionAndRotation(plano.position, plano.rotation);
        camara.fieldOfView = 60f;
    }

    void Teletransportar(Transform punto)
    {
        if (jugador == null || punto == null) return;
        CharacterController cc = jugador.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        jugador.SetPositionAndRotation(punto.position, punto.rotation);
        if (cc != null) cc.enabled = true;
    }

    static string Reemplazar(string s) => Idioma.T(s).Replace("{jugador}", NombreJugador);

    IEnumerator Esperar(float segundos, bool aunqueSalte = false)
    {
        float t = 0f;
        while (t < segundos && (aunqueSalte || !saltar)) { t += Time.unscaledDeltaTime; yield return null; }
    }

    IEnumerator Fundir(CanvasGroup g, float hasta, float duracion)
    {
        if (g == null) yield break;
        float desde = g.alpha, t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(duracion, 0.01f);
            g.alpha = Mathf.Lerp(desde, hasta, t);
            yield return null;
        }
        g.alpha = hasta;
    }

    IEnumerator Fundir(TextMeshProUGUI txt, float hasta, float duracion)
    {
        float desde = txt.alpha, t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime / duracion; txt.alpha = Mathf.Lerp(desde, hasta, t); yield return null; }
    }

    static void Mostrar(CanvasGroup g, float a) { if (g != null) g.alpha = a; }

    void Letterbox(float k)
    {
        baseFranjas = k;
        if (franjaSup != null) franjaSup.localScale = new Vector3(1f, k, 1f);
        if (franjaInf != null) franjaInf.localScale = new Vector3(1f, k, 1f);
    }

    IEnumerator Letterbox(float hasta, float duracion)
    {
        float desde = baseFranjas, t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime / duracion; Letterbox(Mathf.Lerp(desde, hasta, Mathf.SmoothStep(0f, 1f, t))); yield return null; }
    }

    void Parpados(float cerrado)
    {
        if (parpadoSup != null) parpadoSup.localScale = new Vector3(1f, cerrado, 1f);
        if (parpadoInf != null) parpadoInf.localScale = new Vector3(1f, cerrado, 1f);
    }

    // ------------------------------------------------------------------
    // Ruido y sonidos generados por codigo (sin archivos externos)
    void CrearEstatica()
    {
        texEstatica = new Texture2D(256, 256, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point };
        var px = new Color32[256 * 256];
        for (int i = 0; i < px.Length; i++)
        {
            byte v = (byte)Random.Range(0, 256);
            px[i] = new Color32(v, v, v, (byte)(80 + v / 2));
        }
        texEstatica.SetPixels32(px);
        texEstatica.Apply();
        if (estatica != null) estatica.texture = texEstatica;

        if (audioEstatica != null)
        {
            int n = 44100;
            var clip = AudioClip.Create("Estatica", n, 1, 44100, false);
            var d = new float[n];
            float lp = 0f;
            for (int i = 0; i < n; i++) { lp = Mathf.Lerp(lp, Random.Range(-1f, 1f), 0.6f); d[i] = lp * 0.35f; }
            clip.SetData(d, 0);
            audioEstatica.clip = clip;
            audioEstatica.loop = true;
            audioEstatica.volume = 0f;
            audioEstatica.Play();
        }
    }

    void CrearVoces()
    {
        clipVoz = Pitido("Voz", 0.035f, 660f, false);
        clipVozRobot = Pitido("VozEND", 0.05f, 220f, true);
    }

    static AudioClip Pitido(string nombre, float segundos, float frecuencia, bool cuadrada)
    {
        int n = Mathf.CeilToInt(44100 * segundos);
        var clip = AudioClip.Create(nombre, n, 1, 44100, false);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / 44100f;
            float s = Mathf.Sin(2f * Mathf.PI * frecuencia * t);
            if (cuadrada) s = Mathf.Sign(s) * 0.6f;
            float env = 1f - i / (float)n;
            d[i] = s * env * 0.18f;
        }
        clip.SetData(d, 0);
        return clip;
    }

    void Tono(AudioClip clip, float tono)
    {
        if (audioVoz == null || clip == null) return;
        audioVoz.pitch = tono;
        audioVoz.PlayOneShot(clip);
    }
}
