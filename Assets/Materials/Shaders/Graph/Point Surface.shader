Shader "Graph/Point Surface"
{
    Properties{
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
    }
    SubShader{
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
        float3 worldPos;
        };

        float _Smoothness;
        void ConfigureSurface(Input i, inout SurfaceOutputStandard s) {
            s.Albedo = saturate(i.worldPos * 0.5 + 0.5);
            s.Smoothness = _Smoothness;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
