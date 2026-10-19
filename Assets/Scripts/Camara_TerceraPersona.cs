using UnityEngine;

public class Camara_TerceraPersona : MonoBehaviour
{
    [SerializeField] Transform objetivo;                 
    [SerializeField] Vector3 offset = new Vector3(0, 2f, -5f);
    [SerializeField, Range(30f, 120f)] float CampoVision = 60f; // FOV ajustable en tiempo real
    [SerializeField] float SensibilidadMouse = 3f;
    [SerializeField] float LimiteInferior = -20f;           
    [SerializeField] float LimiteSuperior = 60f;               
    [SerializeField] float SuavizadoSeguimiento = 10f;        
    float rotacionVertical;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
    }

    void LateUpdate()
    {
        Camera.main.fieldOfView = CampoVision;

        objetivo.Rotate(0, Input.GetAxis("Mouse X") * SensibilidadMouse, 0);
        rotacionVertical -= Input.GetAxis("Mouse Y") * SensibilidadMouse;
        rotacionVertical = Mathf.Clamp(rotacionVertical, LimiteInferior, LimiteSuperior);

        
        Quaternion rotacion = Quaternion.Euler(rotacionVertical, objetivo.eulerAngles.y, 0);
        Vector3 posicionDeseada = objetivo.position + rotacion * offset;
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, SuavizadoSeguimiento * Time.deltaTime);

        transform.LookAt(objetivo.position + Vector3.up * 1.5f);
    }
}
