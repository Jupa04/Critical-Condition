using System.IO;
using UnityEditor;
using UnityEngine;

// Materiales del modulo de la nave en dos estados:
//  - Limpio: para la cinematica (colores planos, sin oxido).
//  - Abandonado: al despertar (colores apagados y desgaste suave; el oxido va aparte, como calcomanias).
// Las texturas se generan por codigo. Menu: Herramientas > Nave > Generar materiales
public static class Nave_Materiales
{
    const string CARPETA_TEX = "Assets/3DModels/Nave/Texturas";
    const string CARPETA_MAT = "Assets/Materials/Nave";

    static readonly Color TINTE_SOMBRA_1 = new Color(0.72f, 0.76f, 0.90f);
    static readonly Color TINTE_SOMBRA_2 = new Color(0.52f, 0.56f, 0.74f);
    public static readonly Color OXIDO_OSCURO = new Color(0.42f, 0.22f, 0.11f);
    public static readonly Color OXIDO_MEDIO = new Color(0.66f, 0.38f, 0.19f);
    public static readonly Color OXIDO_CLARO = new Color(0.86f, 0.62f, 0.40f);

    [MenuItem("Herramientas/Nave/Generar materiales")]
    public static void Generar()
    {
        Carpeta(CARPETA_TEX); Carpeta(CARPETA_MAT);
        Texture2D desgaste = Desgaste("T_Nave_Desgaste", 7);

        Par("M_Nave_Casco", new Color(0.70f, 0.74f, 0.80f), desgaste);
        Par("M_Nave_MetalOscuro", new Color(0.24f, 0.27f, 0.33f), desgaste);
        Par("M_Nave_Puerta", new Color(0.44f, 0.50f, 0.58f), desgaste);
        Par("M_Nave_Acento", new Color(0.93f, 0.50f, 0.16f), desgaste);
        Par("M_Nave_Blanco", new Color(0.90f, 0.92f, 0.94f), desgaste);
        Par("M_Nave_Tuberia", new Color(0.62f, 0.66f, 0.72f), desgaste);
        Par("M_Nave_TuberiaOscura", new Color(0.36f, 0.38f, 0.42f), desgaste);

        Luz("M_Nave_LuzCian", new Color(0.37f, 0.89f, 0.88f) * 2.6f);
        Luz("M_Nave_LuzCian_Abandonado", new Color(0.37f, 0.89f, 0.88f) * 0.9f);
        Luz("M_Nave_LuzTecho", new Color(1f, 0.95f, 0.85f) * 2.2f);
        Luz("M_Nave_LuzTecho_Abandonado", new Color(0.10f, 0.10f, 0.11f));

        Calcomania("M_Oxido_Chorreo", Chorreo("T_Oxido_Chorreo", 11));
        Calcomania("M_Oxido_Banda", Banda("T_Oxido_Banda", 23));
        Calcomania("M_Oxido_Mancha", Mancha("T_Oxido_Mancha", 31));
        AssetDatabase.SaveAssets();
    }

    // ------------------------------------------------------------------
    static void Par(string nombre, Color color, Texture2D desgaste)
    {
        Toon(nombre, color, null);
        // Abandonado: menos saturado y mas oscuro, con desgaste suave
        Color.RGBToHSV(color, out float h, out float s, out float v);
        Color apagado = Color.HSVToRGB(h, s * 0.75f, v * 0.78f);
        Toon(nombre + "_Abandonado", apagado, desgaste, 0.13f, 0.06f);
    }

    static void Toon(string nombre, Color color, Texture2D tex, float sombra1 = -1f, float sombra2 = -1f)
    {
        string ruta = CARPETA_MAT + "/" + nombre + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Nivel/M_Nivel_Metal.mat"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.SetTexture("_MainTex", tex);
        m.SetTexture("_1st_ShadeMap", tex);
        m.SetTexture("_2nd_ShadeMap", tex);
        m.SetColor("_BaseColor", color);
        // En la nave abandonada lo que no ilumina la linterna queda casi negro
        m.SetColor("_1st_ShadeColor", sombra1 < 0f ? color * TINTE_SOMBRA_1 : color * TINTE_SOMBRA_1 * sombra1);
        m.SetColor("_2nd_ShadeColor", sombra2 < 0f ? color * TINTE_SOMBRA_2 : color * TINTE_SOMBRA_2 * sombra2);
        // En la version abandonada la sombra del sistema es completa (si no, las paredes que miran al sol quedan a medias)
        if (sombra1 >= 0f) m.SetFloat("_Tweak_SystemShadowsLevel", -0.5f);
        // Sin contorno: en las paredes vistas desde adentro el contorno se ve como un borde negro
        m.SetFloat("_Outline_Width", 0f);
        m.SetShaderPassEnabled("SRPDefaultUnlit", false);
        m.SetShaderPassEnabled("Outline", false);
        EditorUtility.SetDirty(m);
    }

    static void Luz(string nombre, Color c)
    {
        string ruta = CARPETA_MAT + "/" + nombre + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Nivel/M_Nivel_LuzGuia.mat"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        c.a = 1f;
        m.SetColor("_BaseColor", c);
        EditorUtility.SetDirty(m);
    }

    // El oxido se multiplica sobre la superficie: la tiñe sin agregar luz, asi en la oscuridad no brilla
    static void Calcomania(string nombre, Texture2D tex)
    {
        string ruta = CARPETA_MAT + "/" + nombre + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (m == null) { m = new Material(unlit); AssetDatabase.CreateAsset(m, ruta); }
        m.shader = unlit;
        m.SetTexture("_BaseMap", tex);
        m.SetColor("_BaseColor", Color.white);
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 3f);
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.DstColor);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
        if (m.HasProperty("_SrcBlendAlpha")) { m.SetFloat("_SrcBlendAlpha", 0f); m.SetFloat("_DstBlendAlpha", 1f); }
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_Cull", 2f);
        m.shaderKeywords = new string[0];
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.EnableKeyword("_ALPHAMODULATE_ON");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = 2990;
        m.SetShaderPassEnabled("DepthOnly", false);
        m.SetShaderPassEnabled("ShadowCaster", false);
        m.SetShaderPassEnabled("DepthNormals", false);
        EditorUtility.SetDirty(m);
    }

    // ------------------------------------------------------------------
    // Desgaste: variacion muy suave y grande (polvo, zonas mas gastadas), sin manchas
    static Texture2D Desgaste(string nombre, int semilla)
    {
        const int T = 256;
        var px = new Color[T * T];
        for (int y = 0; y < T; y++)
            for (int x = 0; x < T; x++)
            {
                float u = x / (float)T, v = y / (float)T;
                float grande = Fbm(u, v, 2, 3, semilla);
                float fino = Fbm(u, v, 32, 2, semilla + 5);
                float k = Mathf.Lerp(0.86f, 1.0f, grande) - (fino - 0.5f) * 0.04f;
                px[y * T + x] = new Color(k, k, k * 0.985f, 1f);
            }
        return Guardar(nombre, T, T, px, TextureWrapMode.Repeat);
    }

    // Chorreo: borde irregular arriba (de donde sale el agua), un lavado tenue que baja
    // y gotas con un halo de mancha alrededor, cada una de distinto largo y ancho
    static Texture2D Chorreo(string nombre, int semilla)
    {
        const int W = 256, H = 512;
        var a = new float[W * H];
        var rnd = new System.Random(semilla);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float u = x / (float)W, v = 1f - y / (float)H;   // v=0 arriba
                float lado = Mathf.SmoothStep(0f, 1f, Mathf.Min(u, 1f - u) / 0.18f);
                float borde = 0.035f + 0.07f * Fbm(u * 1.5f, 0.3f, 5, 3, semilla) * (0.6f + 0.4f * Fbm(u, 0.7f, 3, 2, semilla + 2));
                float origen = Mathf.Clamp01((borde - v) / 0.04f + 0.4f) * 0.7f;
                float lavado = Mathf.Pow(Mathf.Clamp01(1f - v / 0.75f), 2.2f) * 0.22f * (0.7f + 0.6f * Fbm(u * 3f, v * 0.5f, 4, 3, semilla + 3));
                a[y * W + x] = Mathf.Max(origen, lavado) * lado;
            }
        int gotas = 7;
        for (int g = 0; g < gotas; g++)
        {
            float cx = 0.16f + 0.68f * (float)rnd.NextDouble();
            float ancho = 0.018f + 0.035f * (float)rnd.NextDouble();
            float largo = 0.3f + 0.65f * (float)rnd.NextDouble();
            float fuerza = 0.45f + 0.35f * (float)rnd.NextDouble();
            float fase = (float)rnd.NextDouble() * 10f;
            for (int y = 0; y < H; y++)
            {
                float v = 1f - y / (float)H;
                if (v > largo + 0.03f) continue;
                float t = Mathf.Clamp01(v / largo);
                float desvio = 0.008f * Mathf.Sin(v * 23f + fase) + 0.004f * Mathf.Sin(v * 61f + fase * 2f);
                float w = ancho * (1f - 0.45f * t);
                float gota = t > 0.9f ? 1f + 0.8f * Mathf.Sin((t - 0.9f) / 0.1f * Mathf.PI) : 1f;   // la gota se engorda al final
                for (int x = 0; x < W; x++)
                {
                    float u = x / (float)W;
                    float d = Mathf.Abs(u - cx - desvio);
                    float nucleo = Mathf.Clamp01(1f - d / (w * gota)) ;
                    float halo = Mathf.Exp(-Mathf.Pow(d / (w * 3.2f), 2f)) * 0.35f;
                    float caida = Mathf.Pow(1f - t * 0.85f, 1.2f) * (v > largo ? Mathf.Clamp01(1f - (v - largo) / 0.03f) : 1f);
                    float valor = Mathf.Max(Mathf.SmoothStep(0f, 1f, nucleo), halo) * caida * fuerza;
                    int i = y * W + x;
                    a[i] = 1f - (1f - a[i]) * (1f - valor);
                }
            }
        }
        return Colorear(nombre, W, H, a, semilla, TextureWrapMode.Clamp, 1.25f);
    }

    // Banda: oxido acumulado en la union del piso con la pared, con borde superior irregular
    static Texture2D Banda(string nombre, int semilla)
    {
        const int W = 512, H = 128;
        var a = new float[W * H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float u = x / (float)W, v = y / (float)H;   // v=0 abajo
                float altura = 0.35f + 0.45f * Fbm(u * 2f, 0.5f, 3, 3, semilla) + 0.15f * Fbm(u * 6f, 0.2f, 5, 2, semilla + 1);
                float k = Mathf.Clamp01((altura - v) / 0.18f);
                float lados = Mathf.SmoothStep(0f, 1f, Mathf.Min(u, 1f - u) / 0.08f);
                a[y * W + x] = k * lados * 0.85f;
            }
        return Colorear(nombre, W, H, a, semilla, TextureWrapMode.Clamp, 1f);
    }

    // Mancha: charco seco con marca de borde (donde el agua se evaporo) y centro mas tenue
    static Texture2D Mancha(string nombre, int semilla)
    {
        const int T = 256;
        var a = new float[T * T];
        for (int y = 0; y < T; y++)
            for (int x = 0; x < T; x++)
            {
                float u = x / (float)T - 0.5f, v = y / (float)T - 0.5f;
                float ang = Mathf.Atan2(v, u);
                float r = Mathf.Sqrt(u * u + v * v) * 2f;
                float radio = 0.62f + 0.22f * Fbm(Mathf.Cos(ang) * 0.5f + 0.5f, Mathf.Sin(ang) * 0.5f + 0.5f, 3, 3, semilla);
                float dentro = Mathf.Clamp01((radio - r) / 0.06f);
                float marca = Mathf.Exp(-Mathf.Pow((r - radio * 0.93f) / 0.05f, 2f));
                a[y * T + x] = Mathf.Clamp01(dentro * 0.38f + marca * 0.55f) * (0.85f + 0.3f * Fbm(x / (float)T, y / (float)T, 12, 2, semilla + 4));
            }
        return Colorear(nombre, T, T, a, semilla, TextureWrapMode.Clamp, 1f);
    }

    // Convierte la intensidad en color de oxido: donde hay mas, mas oscuro; en los bordes, anaranjado
    static Texture2D Colorear(string nombre, int w, int h, float[] a, int semilla, TextureWrapMode modo, float contraste)
    {
        var px = new Color[w * h];
        for (int i = 0; i < a.Length; i++)
        {
            float k = Mathf.Clamp01(a[i] * contraste);
            int x = i % w, y = i / w;
            float grano = Fbm(x / (float)Mathf.Max(w, h), y / (float)Mathf.Max(w, h), 32, 2, semilla + 9);
            Color c = k > 0.6f ? Color.Lerp(OXIDO_MEDIO, OXIDO_OSCURO, (k - 0.6f) / 0.4f) : Color.Lerp(OXIDO_CLARO, OXIDO_MEDIO, k / 0.6f);
            c *= Mathf.Lerp(0.9f, 1.08f, grano);
            c.a = Mathf.Clamp01(k * Mathf.Lerp(0.8f, 1.1f, grano));
            px[i] = c;
        }
        return Guardar(nombre, w, h, px, modo);
    }

    static Texture2D Guardar(string nombre, int w, int h, Color[] px, TextureWrapMode modo)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        tex.SetPixels(px); tex.Apply();
        string ruta = CARPETA_TEX + "/" + nombre + ".png";
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(ruta);
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        imp.wrapMode = modo;
        imp.alphaIsTransparency = true;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
    }

    static void Carpeta(string ruta)
    {
        if (AssetDatabase.IsValidFolder(ruta)) return;
        string padre = Path.GetDirectoryName(ruta).Replace('\\', '/');
        Carpeta(padre);
        AssetDatabase.CreateFolder(padre, Path.GetFileName(ruta));
    }

    static float Fbm(float u, float v, int frecuencia, int octavas, int semilla)
    {
        float suma = 0f, amp = 0.5f, total = 0f;
        for (int o = 0; o < octavas; o++)
        {
            suma += amp * Ruido(u * frecuencia, v * frecuencia, frecuencia, semilla + o * 101);
            total += amp; amp *= 0.5f; frecuencia *= 2;
        }
        return suma / total;
    }

    static float Ruido(float x, float y, int p, int semilla)
    {
        int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
        float fx = x - x0, fy = y - y0;
        fx = fx * fx * (3f - 2f * fx); fy = fy * fy * (3f - 2f * fy);
        float a = Hash(x0, y0, p, semilla), b = Hash(x0 + 1, y0, p, semilla);
        float c = Hash(x0, y0 + 1, p, semilla), d = Hash(x0 + 1, y0 + 1, p, semilla);
        return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
    }

    static float Hash(int x, int y, int p, int semilla)
    {
        x = ((x % p) + p) % p; y = ((y % p) + p) % p;
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + semilla * 982451653);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0xFFFFFF;
        }
    }
}
