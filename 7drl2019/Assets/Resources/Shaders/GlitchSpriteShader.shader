// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Erebus/GlitchSpriteShader" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [PerRendererData] _Color ("Tint", Color) = (1,1,1,1)
        [PerRendererData] _Flash ("Flash", Color) = (1,1,1,0)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        [PerRendererData] _Alpha("Alpha", Float) = 1.0
        [PerRendererData] _Desaturation("Desaturation", Range(0, 1)) = 0.0
        _Cutoff("Base Alpha cutoff", Range(0,.9)) = .5
        _ResolutionX("Resolution X (px)", Float) = 1066
        _ResolutionY("Resolution Y (px)", Float) = 600
        
        _Elapsed("Elapsed Seconds", Float) = 1.0

    }
    
	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
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
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
            float _ResolutionX;
            float _ResolutionY;
            float _Alpha;
            float _Desaturation;
            fixed4 _Flash;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{ 
                float2 xy = IN.texcoord;
                fixed4 c = tex2D(_MainTex, xy) * IN.color;
                float avg = (c[0] + c[1] + c[2]) / 3.0;
                float4 desat = float4(avg / 2.0, avg / 2.0, avg / 2.0, c.a);
                fixed3 result = c.rgb * (1.0 - _Desaturation) + desat.rgb * (_Desaturation);
                result = result * (1.0 - _Flash.a) + _Flash.rgb * _Flash.a;
                result.rgb *= c.a;
				return fixed4(result.r, result.g, result.b, c.a);
			}
		ENDCG
		}
	}
}
