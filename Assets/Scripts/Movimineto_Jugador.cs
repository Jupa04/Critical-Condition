using UnityEngine;
using TMPro;

public class Movimineto_Jugador : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] GameObject activeChar;
    [SerializeField] CharacterController controller;
    [SerializeField] Vector3 VelocidadJugador;
    [SerializeField] bool ComprobadorPiso;
    [SerializeField] bool ComprobadorSalto;
    [SerializeField] float Gravedad;
    [SerializeField] float MaxVelocidadCaida = -3f; 
    [SerializeField] float MovimientoHorizontal;
    [SerializeField] float MovimientoVertical;
    [SerializeField] float Velocidad = 4;
    [SerializeField] float VelocidadSalto = 1.5f; // Altura del salto

    [Header("Jetpack")]
    [SerializeField] float CombustibleMax = 1.5f;      // Segundos de jetpack
    [SerializeField] float Combustible;
    [SerializeField] float FuerzaJetpack = 8f;         // Empuje hacia arriba
    [SerializeField] float MaxVelocidadSubida = 4f;
    [SerializeField] float MultiplicadorJetpack = 2f;  // Velocidad horizontal extra mientras esta activo
    [SerializeField] bool JetpackActivo;
    [SerializeField] TMP_Text TextoCombustible;       
    bool JetpackListo;                               

    void Start()
    {
        if (activeChar == null) activeChar = gameObject;
        Gravedad = -2.5f; 
        controller.minMoveDistance = 0.001f;
        Combustible = CombustibleMax;
    }

    void Update()
    {
        //  Deteccion de piso 
        ComprobadorPiso = controller.isGrounded;
        if (ComprobadorPiso && VelocidadJugador.y < 0)
        {
            VelocidadJugador.y = -0.5f; // Pegado al piso
            ComprobadorSalto = false;   // Aterrizo (insertar animacion de aterrizar)
        }

        //  WASD (insertar animacion de movimiento)
        Vector3 adelante = transform.forward * Input.GetAxis("Vertical");
        Vector3 lateral = transform.right * Input.GetAxis("Horizontal");
        Vector3 movimiento = Vector3.ClampMagnitude(adelante + lateral, 1f) * Velocidad; 

        //  Salto (insertar animacion de salto) 
        if (Input.GetButtonDown("Jump") && ComprobadorPiso)
        {
            ComprobadorSalto = true;
            VelocidadJugador.y = Mathf.Sqrt(VelocidadSalto * -2f * Gravedad);
            JetpackListo = false;
        }

        // Jetpack
        if (!Input.GetButton("Jump")) JetpackListo = true;
        JetpackActivo = JetpackListo && !ComprobadorPiso && Input.GetButton("Jump") && Combustible > 0;
        if (JetpackActivo)
        {
            Combustible -= Time.deltaTime;
            VelocidadJugador.y += FuerzaJetpack * Time.deltaTime;
            movimiento *= MultiplicadorJetpack; 
        }

        //  UI del combustible 
        if (TextoCombustible != null)
        {
            TextoCombustible.text = "Jetpack: " + Mathf.CeilToInt(Mathf.Max(Combustible, 0) / CombustibleMax * 100) + "%";
        }

        //  Gravedad (insertar animacion de caida) 
        VelocidadJugador.y += Gravedad * Time.deltaTime;
        VelocidadJugador.y = Mathf.Clamp(VelocidadJugador.y, MaxVelocidadCaida, MaxVelocidadSubida);

        //  Aplicar movimiento final 
        movimiento.y = VelocidadJugador.y;
        controller.Move(movimiento * Time.deltaTime);
    }

    // Recarga del jetpack con capsulas (porcentaje del combustible maximo)
    public void AgregarCombustible(float porcentaje)
    {
        Combustible = Mathf.Min(Combustible + CombustibleMax * porcentaje / 100f, CombustibleMax);
    }
}
