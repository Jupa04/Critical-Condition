// Brillo de atmosfera para la Tierra: borde luminoso (fresnel) con mezcla aditiva.
// Modo Borde: esfera un poco mas grande que la Tierra, caras de frente.
// Modo Halo: esfera mas grande, caras traseras; el brillo se desvanece hacia afuera.
Shader "Nave/Atmosfera"
{
    Properties
    {
        _Color ("Color", Color) = (0.35, 0.65, 1, 1)
        _Potencia ("Potencia", Range(0.3, 10)) = 3
        _Intensidad ("Intensidad", Range(0, 6)) = 1.5
        [Enum(Borde,0,Halo,1)] _Modo ("Modo", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Atmosfera"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Potencia;
                half _Intensidad;
                half _Modo;
                half _Cull;
            CBUFFER_END

            struct Atributos { float4 posicion : POSITION; float3 normal : NORMAL; };
            struct Varyings { float4 posicionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 posicionWS : TEXCOORD1; };

            Varyings vert (Atributos i)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(i.posicion.xyz);
                o.posicionCS = p.positionCS;
                o.posicionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(i.normal);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 v = normalize(GetWorldSpaceViewDir(i.posicionWS));
                float d = dot(normalize(i.normalWS), v);
                float f = _Modo < 0.5 ? pow(saturate(1.0 - d), _Potencia) : pow(saturate(-d), _Potencia);
                return half4(_Color.rgb * f * _Intensidad, 1.0);
            }
            ENDHLSL
        }
    }
}
