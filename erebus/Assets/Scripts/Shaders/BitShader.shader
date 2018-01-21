Shader "Unlit/BitShader" {
	
    Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [MaterialToggle] _TriColorMode("Foreground? (ignores white color)", Float) = 0
		[Space(20)] _SourceBlackColor ("Source Black Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _SourceDarkColor ("Source Dark Color", Color) = (0.33, 0.33, 0.33, 1.0)
        _SourceLightColor ("Source Light Color", Color) = (0.66, 0.66, 0.66, 1.0)
        _SourceWhiteColor ("Source White Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [Space(20)] _OutBlackColor ("Out Black Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _OutDarkColor ("Out Dark Color", Color) = (0.33, 0.33, 0.33, 1.0)
        _OutLightColor ("Out Light Color", Color) = (0.66, 0.66, 0.66, 1.0)
        _OutWhiteColor ("Out White Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader {
    
		Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass {
            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
            
            fixed4 _SourceBlackColor;
            fixed4 _SourceDarkColor;
            fixed4 _SourceLightColor;
            fixed4 _SourceWhiteColor;
            fixed4 _OutBlackColor;
            fixed4 _OutDarkColor;
            fixed4 _OutLightColor;
            fixed4 _OutWhiteColor;
            float _TriColorMode;
			
			struct appdata_t {
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_t IN) {
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;

			fixed4 SampleSpriteTexture (float2 uv) {
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}
            
            float colorDelta(float3 color, float3 reference) {
                return abs(color.r - reference.r) + abs(color.g - reference.g) + abs(color.b - reference.b);
            }

			fixed4 frag(v2f IN) : SV_Target {
				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				c.rgb *= c.a;
                
                float deltaBlack = colorDelta(c, _SourceBlackColor);
                float deltaDark = colorDelta(c, _SourceDarkColor);
                float deltaLight = colorDelta(c, _SourceLightColor);
                float deltaWhite = colorDelta(c, _SourceWhiteColor);
                
                if (_TriColorMode) {
                    deltaWhite = 256.0;
                    if (c.a == 0.0) {
                        return float4(0.0, 0.0, 0.0, 0.0);
                    }
                }
                
                if (deltaBlack <= deltaDark && deltaBlack <= deltaLight && deltaBlack <= deltaWhite) {
                    return _OutBlackColor;
                }
                if (deltaDark <= deltaBlack && deltaDark <= deltaLight && deltaDark <= deltaWhite) {
                    return _OutDarkColor;
                }
                if (deltaLight <= deltaDark && deltaLight <= deltaBlack && deltaLight <= deltaWhite) {
                    return _OutLightColor;
                }     
                if (deltaWhite <= deltaDark && deltaWhite <= deltaLight && deltaWhite <= deltaBlack) {
                    return _OutWhiteColor;
                }
                
                //return float4(deltaWhite * .33, deltaWhite * .33, deltaWhite * .33, 1.0);
                
				return c;
			}
		ENDCG
		}
	}
}
