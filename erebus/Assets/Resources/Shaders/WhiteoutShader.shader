// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Snowbound/WhiteoutShader" {

Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _SnowTexture ("Snow Texture", 2D) = "white" {}
    _Color ("Main Color", COLOR) = (1,1,1,1)
    _Elapsed ("Elapsed", float) = 0.0
    _Offset ("Offset", Range(0, 1)) = 0.0
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
                float4 color : COLOR;
				UNITY_FOG_COORDS(1)
			};

			sampler2D _MainTex;
            sampler2D _SnowTexture;
            fixed4 _Color;
			float4 _MainTex_ST;
            float _Offset;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                fixed2 texcoord1 = i.texcoord;
                fixed2 texcoord2 = i.texcoord;
                fixed2 texcoord3 = i.texcoord;
                fixed2 texcoord4 = i.texcoord;
                
                texcoord1[0] -= _Offset + _Time[1] * 1.0;
                texcoord2[0] -= _Offset + _Time[1] * 2.0;
                texcoord3[0] -= _Offset + _Time[1] * 3.0;
                texcoord4[0] -= _Offset + _Time[1] * 7.0;
                
                texcoord1[1] += _SinTime[3] * 0.1 + _Time[1] * 0.1;
                texcoord2[1] += _SinTime[3] * 0.2 + _Time[1] * 0.2;
                texcoord3[1] += _SinTime[3] * 0.3 + _Time[1] * 0.4;
                texcoord4[1] += _SinTime[3] * 0.4 + _Time[1] * 0.8;
                
                fixed4 sample1 = tex2D(_SnowTexture, texcoord1);
                fixed4 sample2 = tex2D(_SnowTexture, texcoord2);
                fixed4 sample3 = tex2D(_SnowTexture, texcoord3);
                fixed4 sample4 = tex2D(_SnowTexture, texcoord4);
                
                float combined = (sample1.a + sample2.a + sample3.a + sample4.a) / 4.0 * 0.9;
                float mult = (_SinTime[3] * 0.4 + 0.1);
                if (mult > 0.8) {
                    mult = 0.8;
                }
                if (mult < 0.0) {
                    mult = 0.0;
                }
                combined -= (i.texcoord[1] * i.texcoord[1] * mult);
                combined += (1.0 - i.texcoord[1]) * (1.0 - i.texcoord[1]) * 0.15;
                
                
				fixed4 result = fixed4(_Color.r, _Color.g, _Color.b, combined * _Color.a * i.color.a);
				return result;
			}
		ENDCG
	}
}

}
