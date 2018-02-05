// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Snowbound/TransitionSprite"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0

        _MaskTexture("Mask Texture", 2D) = "white" {}
        _Elapsed("Elapsed Seconds", Range(0, 1)) = 0.0
        _SoftFudge("Percent Softness", Range(0, 1)) = 0.1
        _Invert("Invert", Range(0, 1)) = 0.0
        _FlipX("Flip X", Range(0, 1)) = 0.0
        _FlipY("Flip Y", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma shader_feature ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
#ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;

            sampler2D _MaskTexture;
            float _Elapsed;
            float _SoftFudge;
            int _Invert;
            int _FlipX;
            int _FlipY;

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);

                float2 adjustedCoord = uv;
                if (_FlipX > 0) {
                    adjustedCoord[0] = 1.0 - adjustedCoord[0];
                }
                if (_FlipY > 0) {
                    adjustedCoord[1] = 1.0 - adjustedCoord[1];
                }
                float maskValue = tex2D(_MaskTexture, adjustedCoord).a;

                // prevent rounding issues hack
                maskValue *= (1.0 - 1.0 / 255.0);

                // the leading edge is fully complete at the start at _SoftFudge, and starts completion at the other side at (1.0-_SoftFudge)
                float adjustedElapsed = 1.0 - (_Elapsed * (1.0 + _SoftFudge) - _SoftFudge / 2.0);
                float weightLow = (adjustedElapsed)-maskValue;
                float weightHigh = (adjustedElapsed + _SoftFudge) - maskValue;
                float weight = ((weightLow + weightHigh) / 2.0) / _SoftFudge;
                weight = clamp(weight, 0.0, 1.0);
                if (_Invert) {
                    weight = (1.0f - weight);
                }

                color.a = lerp(0.0f, color.a, weight);

                return color;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
