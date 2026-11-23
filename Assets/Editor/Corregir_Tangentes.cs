using UnityEditor;
using UnityEngine;

// Al importar un modelo, corrige las tangentes que quedan paralelas a la normal (o en cero).
// Con esas tangentes el shader Toon calcula una bitangente nula, la normaliza y da NaN;
// un solo pixel NaN se convierte con el bloom en una mancha blanca enorme en pantalla.
public class Corregir_Tangentes : AssetPostprocessor
{
    public override uint GetVersion() => 1;

    void OnPostprocessModel(GameObject raiz)
    {
        foreach (var mf in raiz.GetComponentsInChildren<MeshFilter>(true)) Corregir(mf.sharedMesh);
        foreach (var smr in raiz.GetComponentsInChildren<SkinnedMeshRenderer>(true)) Corregir(smr.sharedMesh);
    }

    public static int Corregir(Mesh malla)
    {
        if (malla == null) return 0;
        Vector3[] normales = malla.normals;
        Vector4[] tangentes = malla.tangents;
        if (normales.Length == 0 || tangentes.Length != normales.Length) return 0;
        int corregidas = 0;
        for (int i = 0; i < normales.Length; i++)
        {
            Vector3 n = normales[i].normalized;
            Vector3 t = new Vector3(tangentes[i].x, tangentes[i].y, tangentes[i].z);
            Vector3 orto = t - n * Vector3.Dot(n, t);
            bool signoValido = Mathf.Abs(tangentes[i].w) > 0.5f;
            if (orto.sqrMagnitude > 1e-4f && t.sqrMagnitude > 1e-4f && signoValido) continue;
            if (orto.sqrMagnitude > 1e-4f && t.sqrMagnitude > 1e-4f)
            {
                tangentes[i].w = 1f;
                corregidas++;
                continue;
            }
            // Tangente invalida: cualquier direccion perpendicular a la normal sirve (no hay mapa de normales)
            Vector3 eje = Mathf.Abs(n.y) < 0.9f ? Vector3.up : Vector3.right;
            orto = Vector3.Cross(eje, n).normalized;
            float w = signoValido ? Mathf.Sign(tangentes[i].w) : 1f;
            tangentes[i] = new Vector4(orto.x, orto.y, orto.z, w);
            corregidas++;
        }
        if (corregidas > 0) malla.tangents = tangentes;
        return corregidas;
    }
}
