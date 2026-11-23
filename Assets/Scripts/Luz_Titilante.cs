using UnityEngine;

// Hace titilar una luz de baja intensidad, como una alarma de pasillo.
[RequireComponent(typeof(Light))]
public class Luz_Titilante : MonoBehaviour
{
    [SerializeField] float intensidadMin = 0.2f;
    [SerializeField] float intensidadMax = 0.9f;
    [SerializeField] float velocidad = 6f;

    Light luz;
    float semilla;

    void Awake()
    {
        luz = GetComponent<Light>();
        semilla = Random.value * 10f;
    }

    void Update()
    {
        float r = Mathf.PerlinNoise(semilla, Time.time * velocidad);
        luz.intensity = Mathf.Lerp(intensidadMin, intensidadMax, r);
    }
}
