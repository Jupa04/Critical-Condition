using UnityEngine;

// Al pisar el checkpoint, el jugador reaparece aqui si muere o cae al vacio.
// El anillo del piso cambia de color para mostrar que ya quedo activado.
public class Checkpoint_Nivel : MonoBehaviour
{
    [SerializeField] Transform puntoRespawn;          // Vacio = este mismo objeto
    [SerializeField] Renderer indicador;              // Anillo del piso
    [SerializeField] Color colorInactivo = new Color(1f, 0.55f, 0.2f);
    [SerializeField] Color colorActivo = new Color(0.3f, 1f, 0.9f);
    [SerializeField] float brillo = 1.3f;             // Poco mas de 1 para un Bloom suave
    [SerializeField] bool recargarCombustible = true;

    static readonly int idColor = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock bloque;
    bool activado;

    void Start()
    {
        Pintar(colorInactivo);
    }

    void OnTriggerEnter(Collider other)
    {
        Muerte_Mechanic muerte = other.GetComponent<Muerte_Mechanic>();
        if (muerte == null) return;

        muerte.EstablecerRespawn(puntoRespawn != null ? puntoRespawn : transform);
        if (recargarCombustible)
        {
            Movimineto_Jugador mov = other.GetComponent<Movimineto_Jugador>();
            if (mov != null) mov.AgregarCombustible(100f);
        }

        if (activado) return;
        activado = true;
        Pintar(colorActivo);
    }

    void Pintar(Color c)
    {
        if (indicador == null) return;
        if (bloque == null) bloque = new MaterialPropertyBlock();
        indicador.GetPropertyBlock(bloque);
        bloque.SetColor(idColor, c * brillo);
        indicador.SetPropertyBlock(bloque);
    }
}
