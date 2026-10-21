// Colocar este archivo dentro de una carpeta llamada "Editor" (por ejemplo Assets/Editor/ConvertirAToon.cs)
// Menú: Herramientas > Toon
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ConvertirAToon
{
    // Solo se convierten materiales que usen estos shaders 3D.
    // Todo lo demás (textos, UI, cielo, partículas, sprites) se deja quieto.
    static readonly string[] SHADERS_3D =
    {
        "Universal Render Pipeline/Lit",
        "Universal Render Pipeline/Simple Lit",
        "Universal Render Pipeline/Complex Lit",
        "Universal Render Pipeline/Baked Lit",
        "Standard",
        "Standard (Specular setup)"
    };

    // Sombras azuladas, como en Breath of the Wild, en vez de solo más oscuras
    static readonly Color TINTE_SOMBRA_1 = new Color(0.72f, 0.76f, 0.90f);
    static readonly Color TINTE_SOMBRA_2 = new Color(0.52f, 0.56f, 0.74f);

    const string CARPETA_AJUSTES = "Assets/Settings";
    const string NOMBRE_VOLUMEN = "Volumen Estilo BotW";

    // ------------------------------------------------------------------
    // 1. Convertir materiales 3D (URP Lit / Standard) al Toon Shader
    // ------------------------------------------------------------------
    [MenuItem("Herramientas/Toon/1. Convertir materiales 3D a Toon")]
    static void Convertir()
    {
        Shader toon = BuscarShaderToon();
        if (toon == null)
        {
            Aviso("No encontré ningún shader Toon. Revisa en Package Manager > In Project que \"Unity Toon Shader\" esté instalado.");
            return;
        }

        var lista = new List<Material>();
        foreach (Material m in MaterialesDelProyecto())
            if (System.Array.IndexOf(SHADERS_3D, m.shader.name) >= 0) lista.Add(m);

        if (lista.Count == 0) { Aviso("No hay materiales 3D por convertir."); return; }
        if (!Confirmar($"Se convertirán {lista.Count} materiales 3D al shader \"{toon.name}\" con estilo BotW. ¿Continuar?")) return;

        Undo.RecordObjects(lista.ToArray(), "Convertir a Toon");
        foreach (Material m in lista)
        {
            Texture tex = LeerTextura(m, "_BaseMap", "_MainTex");
            Color color = LeerColor(m, Color.white, "_BaseColor", "_Color");
            Texture normal = LeerTextura(m, "_BumpMap");
            Texture emisionTex = LeerTextura(m, "_EmissionMap");
            Color emision = m.IsKeywordEnabled("_EMISSION") ? LeerColor(m, Color.black, "_EmissionColor") : Color.black;

            m.shader = toon;

            PonerTextura(m, "_MainTex", tex);
            PonerTextura(m, "_1st_ShadeMap", tex);
            PonerTextura(m, "_2nd_ShadeMap", tex);
            PonerColor(m, "_BaseColor", color);
            PonerTextura(m, "_NormalMap", normal);
            PonerTextura(m, "_Emissive_Tex", emisionTex);
            PonerColor(m, "_Emissive_Color", emision);

            AplicarEstiloBotW(m);
            EditorUtility.SetDirty(m);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Convertidos {lista.Count} materiales a {toon.name}.");
    }

    // ------------------------------------------------------------------
    // 2. Ajustar al estilo BotW los materiales que ya son Toon
    // ------------------------------------------------------------------
    [MenuItem("Herramientas/Toon/2. Aplicar estilo BotW a materiales Toon")]
    static void AjustarExistentes()
    {
        var lista = new List<Material>();
        foreach (Material m in MaterialesDelProyecto())
            if (EsToon(m)) lista.Add(m);

        if (lista.Count == 0) { Aviso("No hay materiales Toon para ajustar."); return; }
        if (!Confirmar($"Se ajustarán {lista.Count} materiales Toon al estilo BotW. ¿Continuar?")) return;

        Undo.RecordObjects(lista.ToArray(), "Estilo BotW");
        foreach (Material m in lista)
        {
            AplicarEstiloBotW(m);
            EditorUtility.SetDirty(m);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Ajustados {lista.Count} materiales al estilo BotW.");
    }

    // ------------------------------------------------------------------
    // 3. Luz, niebla, cielo y post-procesado de la escena abierta
    // ------------------------------------------------------------------
    [MenuItem("Herramientas/Toon/3. Aplicar ambiente BotW a la escena")]
    static void AplicarAmbiente()
    {
        if (!Confirmar("Se ajustarán la luz, la niebla, el cielo y el post-procesado de la escena abierta. ¿Continuar?")) return;

        if (!AssetDatabase.IsValidFolder(CARPETA_AJUSTES)) AssetDatabase.CreateFolder("Assets", "Settings");

        // Sol cálido con sombras suaves y no totalmente negras
        Light sol = RenderSettings.sun;
        if (sol == null)
        {
            foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { sol = l; break; }
        }
        if (sol != null)
        {
            Undo.RecordObject(sol, "Ambiente BotW");
            sol.color = new Color(1f, 0.95f, 0.84f);
            sol.intensity = 1.3f;
            sol.shadows = LightShadows.Soft;
            sol.shadowStrength = 0.75f;
        }

        // Luz ambiental de tres colores: cielo azul, horizonte neutro, suelo cálido
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.62f, 0.74f, 0.92f);
        RenderSettings.ambientEquatorColor = new Color(0.58f, 0.63f, 0.62f);
        RenderSettings.ambientGroundColor = new Color(0.36f, 0.34f, 0.30f);

        // Niebla azulada de distancia, la "perspectiva atmosférica" típica de BotW
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.74f, 0.83f, 0.93f);
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 180f;

        // Cielo claro y luminoso
        Shader cieloShader = Shader.Find("Skybox/Procedural");
        if (cieloShader != null)
        {
            string rutaCielo = CARPETA_AJUSTES + "/CieloBotW.mat";
            Material cielo = AssetDatabase.LoadAssetAtPath<Material>(rutaCielo);
            if (cielo == null)
            {
                cielo = new Material(cieloShader);
                AssetDatabase.CreateAsset(cielo, rutaCielo);
            }
            cielo.SetFloat("_SunSize", 0.035f);
            cielo.SetFloat("_AtmosphereThickness", 0.7f);
            cielo.SetColor("_SkyTint", new Color(0.55f, 0.70f, 0.95f));
            cielo.SetColor("_GroundColor", new Color(0.55f, 0.60f, 0.62f));
            cielo.SetFloat("_Exposure", 1.25f);
            EditorUtility.SetDirty(cielo);
            RenderSettings.skybox = cielo;
        }

        // Perfil de post-procesado: brillo suave, colores vivos pero poco contraste
        string rutaPerfil = CARPETA_AJUSTES + "/PerfilEstiloBotW.asset";
        VolumeProfile perfil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(rutaPerfil);
        if (perfil == null)
        {
            perfil = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(perfil, rutaPerfil);

            Bloom bloom = perfil.Add<Bloom>(true);
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(0.6f);
            bloom.scatter.Override(0.75f);

            ColorAdjustments color = perfil.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.15f);
            color.contrast.Override(-8f);
            color.saturation.Override(12f);

            Tonemapping tono = perfil.Add<Tonemapping>(true);
            tono.mode.Override(TonemappingMode.Neutral);

            Vignette vineta = perfil.Add<Vignette>(true);
            vineta.intensity.Override(0.18f);

            foreach (VolumeComponent c in perfil.components) AssetDatabase.AddObjectToAsset(c, perfil);
            EditorUtility.SetDirty(perfil);
        }

        // Volumen global propio, con prioridad alta para que mande sobre los demás
        GameObject goVolumen = GameObject.Find(NOMBRE_VOLUMEN);
        if (goVolumen == null)
        {
            goVolumen = new GameObject(NOMBRE_VOLUMEN);
            Undo.RegisterCreatedObjectUndo(goVolumen, "Ambiente BotW");
        }
        Volume volumen = goVolumen.GetComponent<Volume>();
        if (volumen == null) volumen = Undo.AddComponent<Volume>(goVolumen);
        volumen.isGlobal = true;
        volumen.priority = 10f;
        volumen.sharedProfile = perfil;

        // Activar post-procesado en las cámaras
        foreach (Camera cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            UniversalAdditionalCameraData datos = cam.GetUniversalAdditionalCameraData();
            Undo.RecordObject(datos, "Ambiente BotW");
            datos.renderPostProcessing = true;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Aviso("Listo. Guarda la escena con Ctrl+S.");
    }

    // ------------------------------------------------------------------
    // Reparar los textos de TextMeshPro que la versión anterior convirtió por error
    // ------------------------------------------------------------------
    [MenuItem("Herramientas/Toon/Reparar textos de la UI")]
    static void RepararTextos()
    {
        Shader sdf = Shader.Find("TextMeshPro/Distance Field");
        Shader bitmap = Shader.Find("TextMeshPro/Bitmap");
        if (sdf == null) { Aviso("No encontré los shaders de TextMeshPro."); return; }

        var rutas = new HashSet<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
            rutas.Add(AssetDatabase.GUIDToAssetPath(guid));
        foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { "Assets" }))
            rutas.Add(AssetDatabase.GUIDToAssetPath(guid));

        int reparados = 0;
        foreach (string ruta in rutas)
        {
            System.Type tipo = AssetDatabase.GetMainAssetTypeAtPath(ruta);
            bool esTexto = ruta.Contains("TextMesh Pro") || (tipo != null && tipo.Name == "TMP_FontAsset");
            if (!esTexto) continue;

            foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(ruta))
            {
                if (o is Material m && EsToon(m))
                {
                    Undo.RecordObject(m, "Reparar textos");
                    m.shader = (m.name.Contains("Bitmap") && bitmap != null) ? bitmap : sdf;
                    m.shaderKeywords = new string[0];
                    EditorUtility.SetDirty(m);
                    reparados++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Aviso($"Materiales de texto reparados: {reparados}.");
    }

    // ------------------------------------------------------------------
    // Ajustes del material para el look de Breath of the Wild
    // ------------------------------------------------------------------
    static void AplicarEstiloBotW(Material m)
    {
        Color colorBase = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : Color.white;

        // Sombras teñidas de azul
        PonerColor(m, "_1st_ShadeColor", Multiplicar(colorBase, TINTE_SOMBRA_1));
        PonerColor(m, "_2nd_ShadeColor", Multiplicar(colorBase, TINTE_SOMBRA_2));

        // Paso de luz a sombra suave, no un corte duro
        PonerFloat(m, "_BaseColor_Step", 0.55f);
        PonerFloat(m, "_BaseShade_Feather", 0.08f);
        PonerFloat(m, "_ShadeColor_Step", 0.15f);
        PonerFloat(m, "_1st2nd_Shades_Feather", 0.12f);

        // Sin brillo especular de plástico
        PonerColor(m, "_HighColor", Color.black);

        // Borde de luz cálido en las siluetas
        PonerFloat(m, "_RimLight", 1f);
        PonerColor(m, "_RimLightColor", new Color(1f, 0.95f, 0.85f));
        PonerFloat(m, "_RimLight_Power", 0.25f);
        PonerFloat(m, "_RimLight_FeatherOff", 0f);
        PonerFloat(m, "_LightDirection_MaskOn", 1f);

        // Contorno fino y teñido con el color del objeto, no negro grueso
        PonerFloat(m, "_Outline_Width", 0.4f);
        PonerColor(m, "_Outline_Color", new Color(0.25f, 0.25f, 0.30f));
        PonerFloat(m, "_Is_BlendBaseColor", 1f);
    }

    // ------------------------------------------------------------------
    // Utilidades
    // ------------------------------------------------------------------

    // Solo archivos .mat propios: deja fuera fuentes, modelos FBX, ejemplos del paquete y TextMeshPro
    static List<Material> MaterialesDelProyecto()
    {
        var lista = new List<Material>();
        foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (!ruta.EndsWith(".mat")) continue;
            if (ruta.StartsWith("Assets/Samples") || ruta.Contains("TextMesh Pro")) continue;

            Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m != null && m.shader != null) lista.Add(m);
        }
        return lista;
    }

    static Shader BuscarShaderToon()
    {
        string[] candidatos = { "Toon", "Universal Render Pipeline/Toon", "Unity Toon Shader/Toon" };
        foreach (string nombre in candidatos)
        {
            Shader s = Shader.Find(nombre);
            if (s != null) return s;
        }

        foreach (ShaderInfo info in ShaderUtil.GetAllShaderInfo())
        {
            string n = info.name;
            if (n.Contains("Toon") && !n.StartsWith("Hidden") && !n.Contains("Tessellation") && !info.hasErrors)
                return Shader.Find(n);
        }
        return null;
    }

    static bool EsToon(Material m)
    {
        return m.shader != null && m.shader.name.Contains("Toon");
    }

    static Texture LeerTextura(Material m, params string[] nombres)
    {
        foreach (string n in nombres)
            if (m.HasProperty(n) && m.GetTexture(n) != null) return m.GetTexture(n);
        return null;
    }

    static Color LeerColor(Material m, Color porDefecto, params string[] nombres)
    {
        foreach (string n in nombres)
            if (m.HasProperty(n)) return m.GetColor(n);
        return porDefecto;
    }

    static void PonerTextura(Material m, string nombre, Texture valor)
    {
        if (m.HasProperty(nombre)) m.SetTexture(nombre, valor);
    }

    static void PonerColor(Material m, string nombre, Color valor)
    {
        if (m.HasProperty(nombre)) m.SetColor(nombre, valor);
    }

    static void PonerFloat(Material m, string nombre, float valor)
    {
        if (m.HasProperty(nombre)) m.SetFloat(nombre, valor);
    }

    static Color Multiplicar(Color a, Color b)
    {
        return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a);
    }

    static void Aviso(string texto)
    {
        EditorUtility.DisplayDialog("Toon", texto, "OK");
    }

    static bool Confirmar(string texto)
    {
        return EditorUtility.DisplayDialog("Toon", texto, "Sí", "Cancelar");
    }
}
