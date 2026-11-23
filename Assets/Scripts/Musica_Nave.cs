using UnityEngine;

// Musica del prologo, hecha en BeepBox, dividida en secciones por compases:
//   Hermanos  compases 1-2   (pantalla negra)
//   End       compases 3-6   (la nave limpia y END)
//   Chahin    compases 7-10  (la grabacion)
//   Despertar compases 11-12 (despertar y la nave oxidada)
//   Pasillo   compases 13-16 y sigue sin corte desde el 3 (el bucle de la cancion)
// Cada seccion se repite mientras dure su escena; al cambiar de escena salta a la nueva seccion
// justo al terminar el compas que suena, con dos fuentes programadas al mismo instante para que no haya cortes.
// Fuera de la nave y el pasillo suena la musica normal del nivel.
public class Musica_Nave : MonoBehaviour
{
    public enum Seccion { Ninguna, Hermanos, End, Chahin, Despertar, Pasillo, Nivel }

    [Header("Cancion")]
    [SerializeField] AudioClip tema;
    [SerializeField] float pulsosPorMinuto = 112f;
    [SerializeField] int pulsosPorCompas = 8;
    [SerializeField] float volumen = 0.55f;

    [Header("Musica del nivel")]
    [SerializeField] AudioSource musicaNivel;
    [SerializeField] float volumenNivel = 1f;
    [SerializeField] float fundido = 1.5f;

    [Header("Zonas (despues de la cinematica)")]
    [SerializeField] Transform jugador;
    [SerializeField] Renderer zonaNave;      // la sala de hibernacion
    [SerializeField] Renderer zonaPasillo;   // el tubo de acople

    AudioSource[] fuentes = new AudioSource[2];
    int activa;
    Seccion pedida = Seccion.Ninguna;
    bool sonando;
    int compas;              // compas que suena ahora (1..16)
    double finCompas;        // instante dsp en que termina
    bool decidido;           // ya se programo lo que sigue a este compas
    int siguiente;
    bool porZonas;
    float volTema, volNivel;

    double DuracionCompas => pulsosPorCompas * 60.0 / pulsosPorMinuto;
    int Compases => tema == null ? 16 : Mathf.Max(1, Mathf.RoundToInt((float)(tema.length / DuracionCompas)));

    void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            fuentes[i] = gameObject.AddComponent<AudioSource>();
            fuentes[i].clip = tema;
            fuentes[i].playOnAwake = false;
            fuentes[i].loop = false;
            fuentes[i].spatialBlend = 0f;
            fuentes[i].volume = 0f;
        }
        if (musicaNivel != null) { musicaNivel.playOnAwake = false; musicaNivel.Stop(); musicaNivel.volume = 0f; }
    }

    // ---------- lo que llama la cinematica ----------
    public void Ir(Seccion s)
    {
        porZonas = false;
        Pedir(s);
    }

    // Termina la cinematica: desde aqui manda donde esta el jugador
    public void Juego() { porZonas = true; }

    void Pedir(Seccion s)
    {
        if (s == pedida) return;
        pedida = s;
        if (s == Seccion.Nivel || s == Seccion.Ninguna) return;
        if (!sonando) Empezar(Entrada(s));
    }

    // ---------- secciones ----------
    static int Entrada(Seccion s)
    {
        switch (s)
        {
            case Seccion.Hermanos: return 1;
            case Seccion.End: return 3;
            case Seccion.Chahin: return 7;
            case Seccion.Despertar: return 11;
            case Seccion.Pasillo: return 13;
        }
        return 1;
    }

    // Rango de compases que pertenece a cada seccion y a donde vuelve al terminarlo
    static void Rango(Seccion s, out int ini, out int fin, out int vuelta)
    {
        switch (s)
        {
            case Seccion.Hermanos: ini = 1; fin = 2; vuelta = 1; break;
            case Seccion.End: ini = 3; fin = 6; vuelta = 3; break;
            case Seccion.Chahin: ini = 7; fin = 10; vuelta = 7; break;
            case Seccion.Despertar: ini = 11; fin = 12; vuelta = 11; break;
            default: ini = 3; fin = 16; vuelta = 3; break;   // Pasillo: entra en 13 y sigue el bucle 3-16
        }
    }

    int Despues(int actual)
    {
        Rango(pedida, out int ini, out int fin, out int vuelta);
        if (actual >= ini && actual <= fin) return actual < fin ? actual + 1 : vuelta;
        return Entrada(pedida);
    }

    void Empezar(int desde)
    {
        if (tema == null) return;
        double t = AudioSettings.dspTime + 0.05;
        AudioSource f = fuentes[activa];
        f.Stop();
        f.timeSamples = Muestra(desde);
        f.PlayScheduled(t);
        compas = desde;
        finCompas = t + DuracionCompas;
        decidido = false;
        sonando = true;
    }

    int Muestra(int c) => Mathf.Clamp(Mathf.RoundToInt((float)((c - 1) * DuracionCompas * tema.frequency)), 0, tema.samples - 1);

    void Update()
    {
        if (porZonas) Pedir(Zona());

        bool enNivel = pedida == Seccion.Nivel || pedida == Seccion.Ninguna;
        float dt = Time.unscaledDeltaTime / Mathf.Max(fundido, 0.01f);
        volTema = Mathf.MoveTowards(volTema, enNivel ? 0f : 1f, dt);
        volNivel = Mathf.MoveTowards(volNivel, pedida == Seccion.Nivel ? 1f : 0f, dt);
        foreach (AudioSource f in fuentes) f.volume = volumen * volTema;

        if (musicaNivel != null)
        {
            musicaNivel.volume = volumenNivel * volNivel;
            if (volNivel > 0f && !musicaNivel.isPlaying) musicaNivel.Play();
            else if (volNivel <= 0f && musicaNivel.isPlaying) musicaNivel.Pause();
        }

        if (!sonando) return;
        if (enNivel && volTema <= 0f)
        {
            foreach (AudioSource f in fuentes) f.Stop();
            sonando = false;
            return;
        }
        Programar();
    }

    // Medio segundo antes de que termine cada compas se decide cual sigue
    void Programar()
    {
        double ahora = AudioSettings.dspTime;
        if (!decidido && ahora >= finCompas - 0.5)
        {
            siguiente = Despues(compas);
            if (siguiente > Compases) siguiente = 1;
            if (siguiente != compas + 1)
            {
                // Salto: la otra fuente empieza en el compas nuevo en el instante exacto en que esta termina
                AudioSource actual = fuentes[activa];
                AudioSource otra = fuentes[1 - activa];
                otra.Stop();
                otra.timeSamples = Muestra(siguiente);
                otra.PlayScheduled(finCompas);
                actual.SetScheduledEndTime(finCompas);
                activa = 1 - activa;
            }
            decidido = true;
        }
        if (ahora >= finCompas)
        {
            compas = siguiente;
            finCompas += DuracionCompas;
            decidido = false;
        }
    }

    Seccion Zona()
    {
        if (jugador == null) return Seccion.Despertar;
        Vector3 p = jugador.position;
        if (zonaPasillo != null && Dentro(zonaPasillo.bounds, p, 0.3f)) return Seccion.Pasillo;
        if (zonaNave != null && Dentro(zonaNave.bounds, p, 0.3f)) return Seccion.Despertar;
        // en el umbral entre zonas (la puerta) se mantiene lo que sonaba
        if (zonaPasillo != null && Dentro(zonaPasillo.bounds, p, 1.5f)) return pedida == Seccion.Nivel ? Seccion.Nivel : Seccion.Pasillo;
        if (zonaNave != null && Dentro(zonaNave.bounds, p, 1.5f)) return pedida;
        return Seccion.Nivel;
    }

    static bool Dentro(Bounds b, Vector3 p, float margen)
    {
        b.Expand(margen * 2f);
        return b.Contains(p);
    }
}
