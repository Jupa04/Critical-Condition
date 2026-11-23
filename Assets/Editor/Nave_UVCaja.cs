using UnityEditor;
using UnityEngine;

// UV por proyeccion de caja para los modelos de la nave exportados de Maya.
// Las primitivas y los booleanos de Maya traen UV desordenadas; con esto las
// texturas de oxido quedan en escala real (una repeticion cada 'metrosPorRepeticion').
public class Nave_UVCaja : AssetPostprocessor
{
    const string CARPETA = "Assets/3DModels/Nave/";
    const float ESCALA_ESCENA = 5.5f;          // escala con la que se coloca el modulo en la escena
    const float METROS_POR_REPETICION = 4f;

    void OnPostprocessModel(GameObject raiz)
    {
        if (!assetPath.StartsWith(CARPETA)) return;
        foreach (MeshFilter mf in raiz.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh m = mf.sharedMesh;
            if (m == null) continue;
            Matrix4x4 aRaiz = raiz.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix;
            Vector3[] v = m.vertices;
            Vector3[] n = m.normals;
            var uv = new Vector2[v.Length];
            float k = ESCALA_ESCENA / METROS_POR_REPETICION;
            for (int i = 0; i < v.Length; i++)
            {
                Vector3 p = aRaiz.MultiplyPoint3x4(v[i]) * k;
                Vector3 nn = n.Length == v.Length ? aRaiz.MultiplyVector(n[i]).normalized : Vector3.up;
                Vector3 a = new Vector3(Mathf.Abs(nn.x), Mathf.Abs(nn.y), Mathf.Abs(nn.z));
                if (a.x >= a.y && a.x >= a.z) uv[i] = new Vector2(p.z, p.y);
                else if (a.y >= a.z) uv[i] = new Vector2(p.x, p.z);
                else uv[i] = new Vector2(p.x, p.y);
            }
            m.uv = uv;
            m.RecalculateTangents();
        }
    }
}
