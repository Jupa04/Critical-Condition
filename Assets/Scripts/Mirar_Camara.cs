using UnityEngine;

// Hace que un letrero del mundo quede siempre de frente a la camara.
public class Mirar_Camara : MonoBehaviour
{
    Transform camara;

    void LateUpdate()
    {
        if (camara == null) { if (Camera.main == null) return; camara = Camera.main.transform; }
        transform.rotation = camara.rotation;
    }
}
