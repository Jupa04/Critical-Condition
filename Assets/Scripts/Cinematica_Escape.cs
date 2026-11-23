using System.Collections;
using UnityEngine;
using TMPro;

// Cinematica final del nivel: el astronauta cruza el puente, sube la escalera de la capsula,
// entra, recibe una llamada de la colonia y la capsula se cierra y despega.
// La inicia Meta_Nivel; al terminar llama a 'alTerminar' (el aviso de nivel completado).
// ESC la salta.
public class Cinematica_Escape : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] Transform jugador;
    [SerializeField] CharacterController controlador;
    [SerializeField] Behaviour movimiento;
    [SerializeField] Behaviour animadorJugador;      // Astronauta_Animador (se apaga para manejar el Animator a mano)
    [SerializeField] Animator anim;
    [SerializeField] string estadoQuieto = "Locomocion";
    [SerializeField] string estadoCaminar = "Caminar";
    [Tooltip("Clip de caminar: su velocidad propia fija la del avance, para que los pies no patinen")]
    [SerializeField] AnimationClip clipCaminar;
    [SerializeField] string estadoEscalar = "Escalar";
    [SerializeField] string estadoTeclear = "Teclear";
    [Tooltip("Capa del Animator que teclea solo con los brazos (de pie)")]
    [SerializeField] string capaTeclear = "Teclear_Brazos";
    [Tooltip("0 = se calcula con el clip de caminar")]
    [SerializeField] float velocidadCaminar = 0f;
    [Tooltip("Subida por segundo de la animacion de escalar (se calcula sola si es 0)")]
    [SerializeField] float velocidadEscalar = 0f;
    [SerializeField] AnimationClip clipEscalar;
    [Tooltip("Igual a la velocidad del estado Escalar en el Animator")]
    [SerializeField] float velocidadAnimEscalar = 1.4f;
    [Tooltip("La pose de escalar sube el cuerpo sobre la raiz: se compensa para que los pies queden en los peldanos")]
    [SerializeField] float alturaPoseEscalar = 2.1f;
    [Tooltip("Distancia a la escalera en la que se corta del plano de caminar al de escalar")]
    [SerializeField] float corteCaminar = 2.2f;
    [Tooltip("Altura (sobre puntoEscalera) a la que ya va subiendo cuando entra el plano de escalar")]
    [SerializeField] float inicioEscalar = 0.97f;
    [Tooltip("Cuanto antes del final de la escalera se corta al interior")]
    [SerializeField] float corteEscalar = 0.6f;

    [Header("Camara")]
    [SerializeField] Camera camara;
    [SerializeField] Behaviour camaraControl;
    [SerializeField] Transform planoCaminar;
    [SerializeField] Transform planoEscalar;
    [SerializeField] Transform planoLlamada;
    [Tooltip("Planos que se alternan durante la llamada (cada dos lineas)")]
    [SerializeField] Transform[] planosLlamada;
    [SerializeField] Transform planoDespegue;

    [Header("Capsula")]
    [SerializeField] Transform capsula;
    [SerializeField] Transform bisagraVidrio;
    [SerializeField] Vector3 vidrioAbierto = new Vector3(-105f, 0f, 0f);
    [SerializeField] Transform puntoInicio;          // en el puente, mirando a la capsula
    [SerializeField] Transform puntoEscalera;        // pies del jugador frente a la escalera
    [SerializeField] float alturaFinEscalera = 5f;   // altura de los pies al terminar de subir (sobre puntoEscalera)
    [SerializeField] Transform puntoBorde;           // pies sobre el borde, antes de bajar
    [SerializeField] Transform puntoInterior;        // pies en el piso de la capsula, mirando al frente
    [SerializeField] GameObject[] propulsores;
    [SerializeField] float aceleracion = 7f;
    [SerializeField] float duracionDespegue = 5f;
    [SerializeField] AudioSource sonidoDespegue;

    [Header("Interfaz")]
    [SerializeField] CanvasGroup negro;
    [SerializeField] RectTransform franjaSup;
    [SerializeField] RectTransform franjaInf;
    [SerializeField] GameObject[] ocultarHud;
    [SerializeField] GameObject panelLlamada;
    [SerializeField] TextMeshProUGUI estadoLlamada;
    [SerializeField] TextMeshProUGUI avisoComunicacion;
    [SerializeField] CanvasGroup caja;
    [SerializeField] TextMeshProUGUI cajaTitulo;
    [SerializeField] TextMeshProUGUI cajaTexto;
    [SerializeField] TextMeshProUGUI cajaAviso;
    [SerializeField] TMP_FontAsset fuente;
    [SerializeField] Color colorTitulo = new Color(1f, 0.72f, 0.3f);
    [SerializeField] float letrasPorSegundo = 42f;
    [SerializeField] UnityEngine.UI.Image cajaMarco;
    [SerializeField] UnityEngine.UI.Image cajaRelleno;
    [SerializeField] UnityEngine.UI.Image cajaSeparador;
    [Tooltip("Margen entre la franja negra superior y el panel de la llamada")]
    [SerializeField] float margenPanel = 22f;

    [Header("END en negro")]
    [SerializeField] TMP_FontAsset fuenteEnd;
    [SerializeField] Color colorEnd = new Color(1f, 0.17f, 0.33f);
    [SerializeField] Sprite marcoEnd;
    [SerializeField] Color rellenoEnd = new Color(0.1f, 0.02f, 0.04f, 0.85f);
    [SerializeField, TextArea(2, 4)] string[] lineasEnd =
    {
        "So that's where he's headed, huh... Then I'd better not disappoint them.",
        "Keep watching, brother.",
    };

    [Header("Musica (cancion de BeepBox: 96 BPM, compases de 2,5 s)")]
    [SerializeField] AudioClip musicaFinal;
    [SerializeField] float volumenMusica = 0.55f;
    [Tooltip("Se silencia la musica del nivel al empezar")]
    [SerializeField] Musica_Nave musicaNivel;
    [Tooltip("Segundos donde empieza cada parte: tension (se repite mientras dura la llamada), despegue, END y creditos")]
    [SerializeField] float segTension = 30f;
    [SerializeField] float segDespegue = 50f;
    [SerializeField] float segEnd = 55f;
    [SerializeField] float segCreditos = 70f;

    [Header("Cierre")]
    [SerializeField] CanvasGroup creditos;
    [SerializeField] TextMeshProUGUI textoGracias;
    [SerializeField] TextMeshProUGUI textoNombres;
    [SerializeField] string gracias = "Thanks for playing";
    [SerializeField] string nombres = "Juan Nieto  ·  Keyner Moises  ·  Juan Castillo";

    [Header("Llamada (en ingles; se traduce con Idioma)")]
    [SerializeField] string quienLlama = "Colony  //  Survivor";
    [SerializeField, TextArea(2, 4)] string[] lineas =
    {
        "{jugador}? Can you hear me? This is the colony. We picked up your suit's signal.",
        "Listen carefully. END took control of the main ship orbiting Saturn.",
        "It used the technology of the nearby ships and satellites to build platforms like the one you just crossed.",
        "That pod doesn't have enough power to reach Saturn. You'll have to stop at other platforms along the way.",
        "END most likely knows that... expect more obstacles to get through.",
        "The colony survivors are fine, for now. We don't know for how long.",
        "We need the encryption key you're carrying to shut END down. Good luck.",
    };

    System.Action alTerminar;
    AudioSource[] fuentesMusica;
    int musicaActiva;
    float fundidoMusica = 0.5f;
    bool enLlamada;
    bool corriendo, saltar, pasar, bloqueado;
    float alturaRaiz;
    Quaternion vidrioCerrado;
    AudioSource fuenteVoz;
    AudioClip clipVoz, clipVozEnd;

    static readonly int idMoveX = Animator.StringToHash("MoveX");
    static readonly int idMoveZ = Animator.StringToHash("MoveZ");
    static readonly int idGrounded = Animator.StringToHash("Grounded");
    static readonly int idGiro = Animator.StringToHash("Giro");
    static readonly int idJetpack = Animator.StringToHash("Jetpack");

    void Awake()
    {
        if (bisagraVidrio != null)
        {
            vidrioCerrado = bisagraVidrio.localRotation;
            bisagraVidrio.localRotation = vidrioCerrado * Quaternion.Euler(vidrioAbierto);
        }
        foreach (var p in propulsores) if (p != null) p.SetActive(false);
        if (panelLlamada != null) panelLlamada.SetActive(false);
        if (avisoComunicacion != null) avisoComunicacion.enabled = false;
        fuenteVoz = gameObject.AddComponent<AudioSource>();
        fuenteVoz.playOnAwake = false; fuenteVoz.spatialBlend = 0f; fuenteVoz.volume = 0.6f;
        clipVoz = Pitido(0.035f, 520f);
        clipVozEnd = Pitido(0.05f, 220f, true);
        if (creditos != null) { creditos.alpha = 0f; creditos.gameObject.SetActive(false); }
    }

    public void Iniciar(System.Action fin)
    {
        if (corriendo) return;
        alTerminar = fin;
        StartCoroutine(Secuencia());
    }

    void Update()
    {
        ActualizarMusica();
        if (!corriendo && !bloqueado) return;
        // Otros sistemas (como el holograma de controles) pueden devolver el control: se mantiene apagado
        if (movimiento != null && movimiento.enabled) movimiento.enabled = false;
        if (animadorJugador != null && animadorJugador.enabled) animadorJugador.enabled = false;
        if (camaraControl != null && camaraControl.enabled) camaraControl.enabled = false;
        if (controlador != null && controlador.enabled) controlador.enabled = false;
        if (!corriendo) return;
        if (Input.GetKeyDown(KeyCode.Escape)) saltar = true;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) pasar = true;
    }

    // ------------------------------------------------------------------
    IEnumerator Secuencia()
    {
        corriendo = true;
        bloqueado = true;
        if (movimiento != null) movimiento.enabled = false;
        if (animadorJugador != null) animadorJugador.enabled = false;
        if (camaraControl != null) camaraControl.enabled = false;
        if (controlador != null)
        {
            alturaRaiz = controlador.height * 0.5f * jugador.lossyScale.y + controlador.skinWidth - controlador.center.y * jugador.lossyScale.y;
            controlador.enabled = false;
        }
        Animar(0f);

        if (musicaNivel != null) musicaNivel.Ir(Musica_Nave.Seccion.Ninguna);
        MusicaIr(0f, 0.01f);
        yield return Fundir(1f, 0.6f);
        // El HUD de juego se oculta ya en negro, para que las franjas nunca lo corten
        foreach (var g in ocultarHud) if (g != null) g.SetActive(false);
        Franjas(1f);

        // 1. Cruza el puente hasta la escalera
        // Los cambios de animacion ocurren siempre en un corte de camara, nunca a la vista:
        // camina en un plano, el siguiente plano ya lo muestra subiendo, y el interior ya tecleando.
        Colocar(puntoInicio.position, puntoInicio.rotation);
        Plano(planoCaminar);
        anim.Play(estadoCaminar, 0, 0f);
        Animar(0f);
        float vCaminar = velocidadCaminar;
        if (vCaminar <= 0f && clipCaminar != null) vCaminar = clipCaminar.averageSpeed.z * anim.humanScale * anim.transform.lossyScale.y;
        if (vCaminar <= 0f) vCaminar = 2.5f;
        Vector3 destino = puntoEscalera.position + Vector3.up * alturaRaiz;
        bool fundido = false;
        IEnumerator entrada = Fundir(0f, 0.6f);
        while (!saltar && (jugador.position - destino).magnitude > corteCaminar)
        {
            if (!fundido) fundido = !entrada.MoveNext();
            jugador.position = Vector3.MoveTowards(jugador.position, destino, vCaminar * Time.deltaTime);
            Seguir(jugador.position + Vector3.up * 1.2f, 3f);
            yield return null;
        }
        if (negro != null) negro.alpha = 0f;

        // 2. Corte: ya va subiendo la escalera
        if (!saltar)
        {
            Plano(planoEscalar);
            anim.Play(estadoEscalar, 0, 0f);
            anim.Update(0f);
            float v = velocidadEscalar;
            if (v <= 0f && clipEscalar != null) v = clipEscalar.averageSpeed.y * anim.humanScale * anim.transform.lossyScale.y * velocidadAnimEscalar;
            if (v <= 0f) v = 1.5f;
            Colocar(puntoEscalera.position + Vector3.up * (inicioEscalar - alturaPoseEscalar), puntoEscalera.rotation);
            float yFin = puntoEscalera.position.y + alturaFinEscalera + alturaRaiz - alturaPoseEscalar - corteEscalar;
            while (!saltar && jugador.position.y < yFin)
            {
                jugador.position += Vector3.up * v * Time.deltaTime;
                Seguir(jugador.position + Vector3.up * (alturaPoseEscalar + 1.2f), 2.5f);
                yield return null;
            }
        }

        // 3. Corte: ya esta de pie en la capsula, tecleando en el panel
        if (!saltar)
        {
            Colocar(puntoInterior.position, puntoInterior.rotation);
            Animar(0f);
            anim.Play(estadoQuieto, 0, 0f);
            Brazos(true);
            anim.Update(0f);
            Plano(planosLlamada != null && planosLlamada.Length > 0 ? planosLlamada[0] : planoLlamada);
        }

        // 4. Llamada de la colonia
        if (!saltar)
        {
            yield return Esperar(0.6f);
            if (panelLlamada != null) { BajarPanel(); panelLlamada.SetActive(true); }
            enLlamada = true;
            if (estadoLlamada != null) estadoLlamada.text = Idioma.T("Connecting...");
            float t = 0f;
            while (!saltar && t < 2.4f)
            {
                t += Time.deltaTime;
                if (avisoComunicacion != null)
                {
                    avisoComunicacion.enabled = Mathf.Repeat(t, 0.6f) < 0.4f;
                    avisoComunicacion.text = Idioma.T("COMMUNICATION DETECTED");
                }
                yield return null;
            }
            if (avisoComunicacion != null) avisoComunicacion.enabled = false;
            if (estadoLlamada != null) estadoLlamada.text = Idioma.T("Connected");
            if (cajaTitulo != null) { cajaTitulo.text = Idioma.T(quienLlama); cajaTitulo.color = colorTitulo; if (fuente != null) cajaTitulo.font = fuente; }
            if (fuente != null) cajaTexto.font = fuente;
            cajaTexto.color = Color.white;
            caja.alpha = 1f;
            for (int i = 0; i < lineas.Length; i++)
            {
                if (saltar) break;
                if (planosLlamada != null && planosLlamada.Length > 0 && i % 2 == 0) Plano(planosLlamada[(i / 2) % planosLlamada.Length]);
                yield return Escribir(Idioma.T(lineas[i]).Replace("{jugador}", Cinematica_Inicial.NombreJugador), clipVoz, 0.9f);
            }
            caja.alpha = 0f;
            if (estadoLlamada != null) estadoLlamada.text = Idioma.T("Call ended");
            enLlamada = false;
            yield return Esperar(0.8f);
            if (panelLlamada != null) panelLlamada.SetActive(false);
        }

        // 5. Se cierra el vidrio
        if (!saltar && bisagraVidrio != null)
        {
            Quaternion desde = bisagraVidrio.localRotation;
            float t = 0f;
            while (!saltar && t < 1f)
            {
                t += Time.deltaTime / 1.4f;
                bisagraVidrio.localRotation = Quaternion.Slerp(desde, vidrioCerrado, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            bisagraVidrio.localRotation = vidrioCerrado;
        }

        // 6. Despegue
        if (!saltar)
        {
            Plano(planoDespegue);
            if (!MusicaEntre(segDespegue - 1f, segEnd)) MusicaIr(segDespegue, 0.4f);
            Vector3 local = capsula.InverseTransformPoint(jugador.position);
            foreach (var p in propulsores) if (p != null) p.SetActive(true);
            if (sonidoDespegue != null) sonidoDespegue.Play();
            Vector3 basePos = capsula.position;
            float t = 0f, vel = 0f;
            Vector3 camBase = camara.transform.position;
            while (!saltar && t < duracionDespegue)
            {
                t += Time.deltaTime;
                if (t > 1.1f)
                {
                    vel += aceleracion * Time.deltaTime;
                    capsula.position += Vector3.up * vel * Time.deltaTime;
                }
                float temblor = Mathf.Clamp01(1.4f - t * 0.25f) * 0.12f;
                camara.transform.position = camBase + Random.insideUnitSphere * temblor;
                jugador.position = capsula.TransformPoint(local);
                Seguir(capsula.position + Vector3.up * 5f, 4f);
                if (t > duracionDespegue - 1.1f && negro != null) negro.alpha = Mathf.Max(negro.alpha, (t - (duracionDespegue - 1.1f)) / 1.1f);
                yield return null;
            }
        }

        // Fin (tambien al saltar con ESC)
        bool saltado = saltar;
        enLlamada = false;
        if (negro != null && negro.alpha < 1f) yield return Fundir(1f, 0.4f, true);
        caja.alpha = 0f;
        if (avisoComunicacion != null) avisoComunicacion.enabled = false;
        if (panelLlamada != null) panelLlamada.SetActive(false);
        foreach (var g in ocultarHud) if (g != null) g.SetActive(false);
        if (bisagraVidrio != null) bisagraVidrio.localRotation = vidrioCerrado;
        // La capsula ya se fue: se ocultan sus piezas (sin apagar este objeto, que corre la cinematica)
        if (capsula != null) foreach (var r in capsula.GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var p in propulsores) if (p != null) p.SetActive(false);
        if (jugador != null) foreach (var r in jugador.GetComponentsInChildren<Renderer>()) r.enabled = false;
        Franjas(0f);
        Brazos(false);
        if (panelLlamada != null && panelOriginal.HasValue) ((RectTransform)panelLlamada.transform).anchoredPosition = panelOriginal.Value;
        alTerminar?.Invoke();

        // 7. END habla en la pantalla negra
        if (!saltado)
        {
            saltar = false;
            float vol = sonidoDespegue != null ? sonidoDespegue.volume : 0f;
            for (float t = 0f; t < 1f && sonidoDespegue != null; t += Time.deltaTime / 1.2f) { sonidoDespegue.volume = vol * (1f - t); yield return null; }
            if (sonidoDespegue != null) sonidoDespegue.Stop();
            yield return Esperar(0.6f);
            if (!MusicaEntre(segEnd - 0.5f, segCreditos)) MusicaIr(segEnd, 0.6f);
            EstiloEnd();
            caja.alpha = 1f;
            foreach (string l in lineasEnd)
            {
                if (saltar) break;
                yield return Escribir(Idioma.T(l), clipVozEnd, 0.7f);
            }
            caja.alpha = 0f;
            yield return Esperar(0.8f);
        }

        // 8. Portada y agradecimiento
        if (creditos != null)
        {
            saltar = false;
            if (textoGracias != null) textoGracias.text = "";
            if (textoNombres != null) textoNombres.text = "";
            creditos.gameObject.SetActive(true);
            if (!MusicaEntre(segCreditos - 1.5f, 999f)) MusicaIr(segCreditos, 0.8f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 2f) { creditos.alpha = Mathf.SmoothStep(0f, 1f, t); yield return null; }
            creditos.alpha = 1f;
            yield return new WaitForSeconds(0.5f);
            if (textoNombres != null) yield return Teclear(textoNombres, nombres, clipVoz, 1.05f);
            yield return new WaitForSeconds(0.3f);
            if (textoGracias != null) yield return Teclear(textoGracias, Idioma.T(gracias), clipVoz, 0.9f);
        }
        corriendo = false;
    }

    void EstiloEnd()
    {
        if (cajaTitulo != null) { cajaTitulo.text = "END"; cajaTitulo.color = new Color(colorEnd.r, colorEnd.g, colorEnd.b, 0.85f); if (fuenteEnd != null) cajaTitulo.font = fuenteEnd; }
        if (fuenteEnd != null) cajaTexto.font = fuenteEnd;
        cajaTexto.color = colorEnd;
        if (cajaMarco != null) { if (marcoEnd != null) cajaMarco.sprite = marcoEnd; cajaMarco.color = colorEnd; }
        if (cajaRelleno != null) cajaRelleno.color = rellenoEnd;
        if (cajaSeparador != null) cajaSeparador.color = new Color(colorEnd.r, colorEnd.g, colorEnd.b, 0.35f);
        if (cajaAviso != null) cajaAviso.color = new Color(colorEnd.r, colorEnd.g, colorEnd.b, 0.6f);
    }

    // Escribe un texto letra por letra sin esperar al jugador
    IEnumerator Teclear(TextMeshProUGUI destino, string texto, AudioClip voz, float tono)
    {
        destino.text = texto;
        destino.maxVisibleCharacters = 0;
        destino.ForceMeshUpdate();
        int total = destino.textInfo.characterCount;
        float visibles = 0f; int anterior = -1;
        while (visibles < total)
        {
            visibles += Time.deltaTime * letrasPorSegundo * 0.6f;
            int n = Mathf.Min(total, Mathf.FloorToInt(visibles));
            destino.maxVisibleCharacters = n;
            if (n != anterior && n % 2 == 0 && voz != null) { fuenteVoz.pitch = tono; fuenteVoz.PlayOneShot(voz); }
            anterior = n;
            yield return null;
        }
        destino.maxVisibleCharacters = total;
    }

    // ------------------------------------------------------------------
    // Musica final: dos fuentes para saltar entre partes de la cancion con un fundido corto
    void MusicaIr(float segundo, float fundido)
    {
        if (musicaFinal == null) return;
        if (fuentesMusica == null)
        {
            fuentesMusica = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                fuentesMusica[i] = gameObject.AddComponent<AudioSource>();
                fuentesMusica[i].playOnAwake = false; fuentesMusica[i].loop = false; fuentesMusica[i].spatialBlend = 0f;
                fuentesMusica[i].volume = 0f;
            }
        }
        musicaActiva = 1 - musicaActiva;
        AudioSource f = fuentesMusica[musicaActiva];
        f.clip = musicaFinal;
        f.time = Mathf.Clamp(segundo, 0f, musicaFinal.length - 0.05f);
        f.volume = fundido <= 0.02f ? volumenMusica : 0f;
        f.Play();
        fundidoMusica = Mathf.Max(fundido, 0.01f);
    }

    bool MusicaEntre(float desde, float hasta)
    {
        if (fuentesMusica == null) return false;
        AudioSource f = fuentesMusica[musicaActiva];
        return f.isPlaying && f.time >= desde && f.time < hasta;
    }

    void ActualizarMusica()
    {
        if (fuentesMusica == null) return;
        // Mientras siga la llamada, la parte de tension se repite en vez de pasar al despegue
        AudioSource activa = fuentesMusica[musicaActiva];
        if (enLlamada && activa.isPlaying && activa.time >= segDespegue - 0.05f) MusicaIr(segTension, 0.8f);
        float paso = volumenMusica * Time.unscaledDeltaTime / fundidoMusica;
        for (int i = 0; i < 2; i++)
        {
            AudioSource f = fuentesMusica[i];
            float objetivo = i == musicaActiva ? volumenMusica : 0f;
            f.volume = Mathf.MoveTowards(f.volume, objetivo, paso);
            if (i != musicaActiva && f.volume <= 0f && f.isPlaying) f.Stop();
        }
    }

    void Colocar(Vector3 pies, Quaternion rot)
    {
        jugador.SetPositionAndRotation(pies + Vector3.up * alturaRaiz, rot);
    }

    void Animar(float avance)
    {
        if (anim == null) return;
        anim.SetFloat(idMoveX, 0f);
        anim.SetFloat(idMoveZ, avance);
        anim.SetFloat(idGiro, 0f);
        anim.SetBool(idGrounded, true);
        anim.SetBool(idJetpack, false);
    }

    // Capa de brazos tecleando sobre la pose de pie
    void Brazos(bool si)
    {
        if (anim == null) return;
        int capa = anim.GetLayerIndex(capaTeclear);
        if (capa < 0) { if (si) anim.Play(estadoTeclear, 0, 0f); return; }
        anim.SetLayerWeight(capa, si ? 1f : 0f);
        if (si) anim.Play(estadoTeclear, capa, 0f);
    }

    // El panel de la llamada se baja para que la franja negra no lo corte
    Vector2? panelOriginal;
    void BajarPanel()
    {
        var rt = panelLlamada.transform as RectTransform;
        if (rt == null || franjaSup == null) return;
        if (!panelOriginal.HasValue) panelOriginal = rt.anchoredPosition;
        float franja = franjaSup.rect.height * franjaSup.localScale.y;
        float y = Mathf.Min(panelOriginal.Value.y, -(franja + margenPanel));
        rt.anchoredPosition = new Vector2(panelOriginal.Value.x, y);
    }

    void Plano(Transform p)
    {
        if (camara == null || p == null) return;
        camara.transform.SetPositionAndRotation(p.position, p.rotation);
    }

    void Seguir(Vector3 objetivo, float suavidad)
    {
        if (camara == null) return;
        Quaternion q = Quaternion.LookRotation(objetivo - camara.transform.position);
        camara.transform.rotation = Quaternion.Slerp(camara.transform.rotation, q, 1f - Mathf.Exp(-suavidad * Time.deltaTime));
    }

    void Franjas(float k)
    {
        if (franjaSup != null) franjaSup.localScale = new Vector3(1f, k, 1f);
        if (franjaInf != null) franjaInf.localScale = new Vector3(1f, k, 1f);
    }

    IEnumerator Girar(Quaternion hacia, float dur)
    {
        Quaternion desde = jugador.rotation;
        for (float t = 0f; t < 1f && !saltar; t += Time.deltaTime / dur)
        {
            jugador.rotation = Quaternion.Slerp(desde, hacia, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        jugador.rotation = hacia;
    }

    // Movimiento con un pequeno arco hacia arriba (para pasar sobre el borde)
    IEnumerator Mover(Vector3 hasta, Quaternion rot, float dur, bool arco)
    {
        Vector3 desde = jugador.position; Quaternion r0 = jugador.rotation;
        for (float t = 0f; t < 1f && !saltar; t += Time.deltaTime / dur)
        {
            float e = Mathf.SmoothStep(0f, 1f, t);
            Vector3 p = Vector3.Lerp(desde, hasta, e);
            if (arco) p += Vector3.up * Mathf.Sin(e * Mathf.PI) * 0.6f;
            jugador.position = p;
            jugador.rotation = Quaternion.Slerp(r0, rot, e);
            Seguir(jugador.position + Vector3.up * 1.5f, 3f);
            yield return null;
        }
        jugador.SetPositionAndRotation(hasta, rot);
    }

    IEnumerator Esperar(float s)
    {
        for (float t = 0f; t < s && !saltar; t += Time.deltaTime) yield return null;
    }

    IEnumerator Fundir(float hasta, float dur, bool aunqueSalte = false)
    {
        if (negro == null) yield break;
        float desde = negro.alpha;
        for (float t = 0f; t < 1f && (aunqueSalte || !saltar); t += Time.unscaledDeltaTime / dur)
        {
            negro.alpha = Mathf.Lerp(desde, hasta, t);
            yield return null;
        }
        negro.alpha = hasta;
    }

    IEnumerator Escribir(string texto, AudioClip voz, float tono)
    {
        pasar = false;
        cajaTexto.text = texto;
        cajaTexto.maxVisibleCharacters = 0;
        cajaTexto.ForceMeshUpdate();
        int total = cajaTexto.textInfo.characterCount;
        if (cajaAviso != null) cajaAviso.enabled = false;
        float visibles = 0f; int anterior = -1;
        while (visibles < total && !saltar)
        {
            if (pasar) { pasar = false; break; }
            visibles += Time.deltaTime * letrasPorSegundo;
            int n = Mathf.Min(total, Mathf.FloorToInt(visibles));
            cajaTexto.maxVisibleCharacters = n;
            if (n != anterior && n % 2 == 0) { fuenteVoz.pitch = tono + Random.Range(-0.05f, 0.05f); fuenteVoz.PlayOneShot(voz); }
            anterior = n;
            yield return null;
        }
        cajaTexto.maxVisibleCharacters = total;
        if (cajaAviso != null) cajaAviso.enabled = true;
        float espera = 1.4f + texto.Length * 0.03f;
        for (float t = 0f; t < espera && !pasar && !saltar; t += Time.deltaTime) yield return null;
        pasar = false;
        if (cajaAviso != null) cajaAviso.enabled = false;
    }

    static AudioClip Pitido(float segundos, float frecuencia, bool cuadrada = false)
    {
        int n = Mathf.CeilToInt(44100 * segundos);
        var clip = AudioClip.Create("VozLlamada", n, 1, 44100, false);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float s = Mathf.Sin(2f * Mathf.PI * frecuencia * i / 44100f);
            if (cuadrada) s = Mathf.Sign(s) * 0.6f;
            d[i] = s * (1f - i / (float)n) * 0.16f;
        }
        clip.SetData(d, 0);
        return clip;
    }
}
