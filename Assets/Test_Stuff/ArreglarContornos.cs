// Colocar en Assets/Editor/ArreglarContornos.cs
// Menú: Herramientas > Toon > Arreglar contornos
//
// Toma como referencia un material que SÍ tiene buen contorno (el del tanque de oxígeno)
// y copia su configuración de contorno a los materiales Toon que no lo tienen o lo tienen
// demasiado delgado. Compensa la escala de cada objeto para que el grosor se vea igual.
//
// Referencia: si tienes seleccionado un material Toon en la ventana Project, usa ese.
// Si no, busca el material llamado "Oxygen_Mat".
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class ArreglarContornos
{
    const string NOMBRE_REFERENCIA = "Oxygen_Mat";
    const string PASE_CONTORNO = "SRPDefaultUnlit";
    const string ANCHO = "_Outline_Width";

    // Propiedades del contorno que no llevan "outline" en el nombre
    static readonly HashSet<string> EXTRAS = new HashSet<string>
    {
        "_Farthest_Distance", "_Nearest_Distance", "_Offset_Z", "_Is_BlendBaseColor"
    };

    [MenuItem("Herramientas/Toon/Arreglar contornos")]
    static void Arreglar()
    {
        Material referencia = BuscarReferencia();
        if (referencia == null)
        {
            Aviso($"No encontré un material de referencia. Selecciona en la ventana Project un material Toon que SÍ tenga buen contorno, como \"{NOMBRE_REFERENCIA}\", y vuelve a ejecutar.");
            return;
        }
        if (!referencia.HasProperty(ANCHO))
        {
            Aviso($"El material de referencia \"{referencia.name}\" no tiene propiedades de contorno. Elige otro.");
            return;
        }

        float anchoRef = referencia.GetFloat(ANCHO);
        float escalaRef = EscalaEnEscena(referencia);
        if (escalaRef <= 0f) escalaRef = 1f;
        float grosorRef = anchoRef * escalaRef;

        // Materiales Toon del proyecto que necesitan arreglo
        var arreglar = new List<Material>();
        foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (!ruta.EndsWith(".mat")) continue;
            if (ruta.StartsWith("Assets/Samples") || ruta.Contains("TextMesh Pro")) continue;

            Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m == null || m == referencia || m.shader == null) continue;
            if (!m.shader.name.Contains("Toon") || !m.HasProperty(ANCHO)) continue;

            bool paseApagado = !m.GetShaderPassEnabled(PASE_CONTORNO);
            float escala = EscalaEnEscena(m);
            float grosor = m.GetFloat(ANCHO) * (escala > 0f ? escala : 1f);
            bool muyDelgado = grosor < grosorRef * 0.5f;

            if (paseApagado || muyDelgado) arreglar.Add(m);
        }

        if (arreglar.Count == 0)
        {
            Aviso("Todos los materiales Toon ya tienen contorno parecido al de la referencia.");
            return;
        }

        string lista = string.Join("\n", arreglar.Take(12).Select(m => "• " + m.name));
        if (arreglar.Count > 12) lista += $"\n… y {arreglar.Count - 12} más";
        if (!EditorUtility.DisplayDialog("Arreglar contornos",
                $"Referencia: \"{referencia.name}\"\n\nSe arreglará el contorno de {arreglar.Count} materiales:\n{lista}\n\n¿Continuar?",
                "Sí", "Cancelar"))
            return;

        Undo.RecordObjects(arreglar.ToArray(), "Arreglar contornos");

        foreach (Material m in arreglar)
        {
            CopiarPropiedadesDeContorno(referencia, m);
            CopiarPalabrasClave(referencia, m);

            // Encender el pase que dibuja el contorno
            m.SetShaderPassEnabled(PASE_CONTORNO, true);

            // Grosor compensado por la escala del objeto en la escena
            float escala = EscalaEnEscena(m);
            float ancho = escala > 0f ? grosorRef / escala : anchoRef;
            m.SetFloat(ANCHO, ancho);

            EditorUtility.SetDirty(m);
        }

        AssetDatabase.SaveAssets();
        Aviso($"Listo. Contorno arreglado en {arreglar.Count} materiales.\n\nSi en algún objeto el contorno sale con huecos en las esquinas, en su material cambia \"Outline Mode\" entre Normal Direction y Position Scaling.");
    }

    static Material BuscarReferencia()
    {
        // 1. Material Toon seleccionado en la ventana Project
        if (Selection.activeObject is Material sel && sel.shader != null && sel.shader.name.Contains("Toon"))
            return sel;

        // 2. Material con el nombre de referencia
        foreach (string guid in AssetDatabase.FindAssets(NOMBRE_REFERENCIA + " t:Material"))
        {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (m != null && m.name == NOMBRE_REFERENCIA && m.shader != null && m.shader.name.Contains("Toon"))
                return m;
        }
        return null;
    }

    static void CopiarPropiedadesDeContorno(Material desde, Material hacia)
    {
        Shader shader = desde.shader;
        int total = shader.GetPropertyCount();
        for (int i = 0; i < total; i++)
        {
            string nombre = shader.GetPropertyName(i);
            if (nombre == ANCHO) continue;
            bool esDeContorno = nombre.ToLower().Contains("outline") || EXTRAS.Contains(nombre);
            if (!esDeContorno || !hacia.HasProperty(nombre)) continue;

            switch (shader.GetPropertyType(i))
            {
                case ShaderPropertyType.Color:
                    hacia.SetColor(nombre, desde.GetColor(nombre));
                    break;
                case ShaderPropertyType.Vector:
                    hacia.SetVector(nombre, desde.GetVector(nombre));
                    break;
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    hacia.SetFloat(nombre, desde.GetFloat(nombre));
                    break;
                case ShaderPropertyType.Texture:
                    hacia.SetTexture(nombre, desde.GetTexture(nombre));
                    break;
                case ShaderPropertyType.Int:
                    hacia.SetInteger(nombre, desde.GetInteger(nombre));
                    break;
            }
        }
    }

    static void CopiarPalabrasClave(Material desde, Material hacia)
    {
        var deReferencia = new HashSet<string>(desde.shaderKeywords.Where(k => k.Contains("OUTLINE")));
        foreach (string k in hacia.shaderKeywords.Where(k => k.Contains("OUTLINE")).ToArray())
            if (!deReferencia.Contains(k)) hacia.DisableKeyword(k);
        foreach (string k in deReferencia)
            hacia.EnableKeyword(k);
    }

    // Escala promedio de los objetos de la escena abierta que usan este material (-1 si ninguno)
    static float EscalaEnEscena(Material m)
    {
        float suma = 0f;
        int cuenta = 0;
        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!r.sharedMaterials.Contains(m)) continue;
            Vector3 s = r.transform.lossyScale;
            suma += (Mathf.Abs(s.x) + Mathf.Abs(s.y) + Mathf.Abs(s.z)) / 3f;
            cuenta++;
        }
        return cuenta > 0 ? suma / cuenta : -1f;
    }

    static void Aviso(string texto)
    {
        EditorUtility.DisplayDialog("Arreglar contornos", texto, "OK");
    }
}
