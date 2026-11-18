using UnityEngine;

// Hace titilar las estrellas hijas de este objeto cambiando su escala suavemente.
// Las estrellas son mallas con material emisivo, el Bloom del Global Volume les da el brillo.
public class Estrellas_Titilar : MonoBehaviour
{
    [SerializeField] float velocidadMin = 0.6f;
    [SerializeField] float velocidadMax = 2.2f;
    [Range(0f, 0.9f)] [SerializeField] float amplitud = 0.35f;

    Transform[] estrellas;
    Vector3[] escalaBase;
    float[] fase;
    float[] velocidad;

    void Start()
    {
        int n = transform.childCount;
        estrellas = new Transform[n];
        escalaBase = new Vector3[n];
        fase = new float[n];
        velocidad = new float[n];
        for (int i = 0; i < n; i++)
        {
            estrellas[i] = transform.GetChild(i);
            escalaBase[i] = estrellas[i].localScale;
            fase[i] = Random.Range(0f, Mathf.PI * 2f);
            velocidad[i] = Random.Range(velocidadMin, velocidadMax);
        }
    }

    void Update()
    {
        float t = Time.time;
        for (int i = 0; i < estrellas.Length; i++)
        {
            float k = 1f + Mathf.Sin(t * velocidad[i] + fase[i]) * amplitud;
            estrellas[i].localScale = escalaBase[i] * k;
        }
    }
}
