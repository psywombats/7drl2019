Shader "Erebus/AutomataShader" {
    Properties {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _BufferTex("Accum Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        
        _Elapsed("Elapsed Seconds", Float) = 1.0
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
            
            float _Elapsed;
            
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
			
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
			
			fixed4 _Color;

			v2f vert(appdata_t IN) {
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _BufferTex;
			sampler2D _AlphaTex;
			float4 _MainTex_TexelSize;

			fixed4 frag(v2f IN) : SV_Target {
                float2 xy = IN.texcoord;
                float2 pxXY = IN.vertex;
                float t = _Elapsed + 500.0;
                
                fixed4 c = tex2D(_BufferTex, IN.texcoord) * IN.color;

                
				return c;
			}
		ENDCG
		}
	}
}
