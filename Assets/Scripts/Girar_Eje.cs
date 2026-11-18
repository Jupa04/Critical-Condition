using UnityEngine;

// Gira el objeto sobre uno de sus ejes locales (rodillos, ventiladores, antenas).
public class Girar_Eje : MonoBehaviour
{
    [SerializeField] Vector3 eje = Vector3.right;
    [SerializeField] float gradosPorSegundo = 180f;

    void Update()
    {
        transform.Rotate(eje, gradosPorSegundo * Time.deltaTime, Space.Self);
    }
}
