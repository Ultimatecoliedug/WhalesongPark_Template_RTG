Shader "Unlit/WindTrailShader"
{
    Properties
    {
        _Colour("Grayscale Colour", Color) = (.5, .5, .5, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _WindPercent;
            fixed4 _Colour;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float ForwardWind = _WindPercent * 2.0f;
                ForwardWind = min(ForwardWind, 1.0f);

                float BackWind = _WindPercent - 0.5f;

                if (_WindPercent < i.uv.x || BackWind > i.uv.x)
                {
                    discard;
                }

                return _Colour;
            }
            ENDCG
        }
    }
}
