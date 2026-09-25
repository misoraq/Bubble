Shader "UI/GameOverVignette"
{
    Properties
    {
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Range(0, 1)) = 0.3
        _Softness ("Softness", Range(0.001, 0.5)) = 0.05
        _Alpha ("Alpha", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Blend SrcAlpha OneMinusSrcAlpha

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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Center;
            float _Radius;
            float _Softness;
            float _Alpha;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

float2 center = _Center.xy;

// âÊñ ÇÃècâ°î‰ÇéÊìæ
float aspect = _ScreenParams.x / _ScreenParams.y;

// â°ï˚å¸Çï‚ê≥ÇµÇƒê^â~Ç…Ç∑ÇÈ
float2 correctedUV = uv;
float2 correctedCenter = center;

correctedUV.x *= aspect;
correctedCenter.x *= aspect;

float distanceFromCenter =
    distance(correctedUV, correctedCenter);

              float edge = smoothstep(
    _Radius - _Softness,
    _Radius,
    distanceFromCenter
);

return fixed4(0, 0, 0, edge * _Alpha);
            }

            ENDCG
        }
    }
}