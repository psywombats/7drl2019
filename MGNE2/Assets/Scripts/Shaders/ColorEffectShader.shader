Shader "Hidden/Color Effect" {

Properties {
    _MainTex("Base (RGB)", 2D) = "white" {}
    _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
}

SubShader {
    Pass {
        ZTest Always Cull Off ZWrite Off
				
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform fixed4 _Color;

fixed4 frag (v2f_img i) : SV_Target {
	fixed4 output = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
	output.r *= _Color.r;
    output.g *= _Color.g;
    output.b *= _Color.b;
	return output;
}
ENDCG

    }
}

Fallback off

}
