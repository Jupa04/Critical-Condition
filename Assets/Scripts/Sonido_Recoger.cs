using UnityEngine;

// Sonido al recoger un objeto: elige uno de los clips al azar, sin repetir el anterior,
// y lo reproduce en 2D (no depende de la distancia a la camara).
public static class Sonido_Recoger
{
    static AudioSource fuente;
    static int ultimo = -1;

    public static void Reproducir(AudioClip[] clips, float volumen)
    {
        if (clips == null || clips.Length == 0) return;
        if (fuente == null)
        {
            var go = new GameObject("Sonido_Recoger");
            go.hideFlags = HideFlags.HideInHierarchy;
            fuente = go.AddComponent<AudioSource>();
            fuente.playOnAwake = false;
            fuente.spatialBlend = 0f;
        }
        int i = Random.Range(0, clips.Length);
        if (clips.Length > 1 && i == ultimo) i = (i + 1) % clips.Length;
        ultimo = i;
        if (clips[i] == null) return;
        fuente.pitch = Random.Range(0.96f, 1.04f);
        fuente.PlayOneShot(clips[i], volumen);
    }
}
