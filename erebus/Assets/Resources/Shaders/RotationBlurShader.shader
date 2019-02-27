Shader "Sprites/Smear" {

    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [PerRendererData] _Color ("Tint", Color) = (1,1,1,1)
        // _SwingArcExponent ("Swing Arc Exponent", Range(0, 10)) = 1
    }
    
    SubShader {
    
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
        #pragma multi_compile _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #include "UnityShaderVariables.cginc"
        #include "UnitySprites.cginc"
        
        int _PivotX;
        int _PivotY;
        int _SmearLows[32];
        int _SmearHighs[32];
        float _SwingArcSize;
        float _SwingArcExponent;
        int _TipRow;

        struct Input {
            float2 uv_MainTex;
            fixed4 color;
            float2 texcoord : TEXCOORD0;
            float2 screenPos: TEXCOORD2;
        };

        void vert(inout appdata_full v, out Input o) {
            #if defined(PIXELSNAP_ON)
            v.vertex = UnityPixelSnap (v.vertex);
            #endif

            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = v.color * _Color;
        }

        void surf(Input IN, inout SurfaceOutput o) {
            float2 xy = IN.uv_MainTex;
            float4 pxXY = float4(int(xy[0] * 32.0), int(xy[1] * 32.0), 0.0, 0.0);
            fixed4 c;
            if (pxXY.y > _PivotY && pxXY.x < _SmearHighs[pxXY.y] - 1 && _SmearLows[pxXY.y] > _PivotX) {
                float dx = pxXY.x - _PivotX;
                float dy = pxXY.y - _PivotY;
                float distPivot = sqrt(dx*dx + dy*dy);
                int row = int(distPivot + _PivotY);
                float a = atan2(dy, dx);
                float aMin = atan2(row - _PivotY, _SmearHighs[row] - _PivotX);
                float rx = (a - aMin) / (0.5 * 3.141);
                float ry = float(row - _PivotY) / float((32 - _TipRow) - _PivotY);
                float maxArc;
                if (_SwingArcExponent > 0.0) {
                    maxArc = pow(ry, _SwingArcExponent);
                } else {
                    maxArc = 1.0;
                }
                maxArc *= _SwingArcSize;
                if (rx <= maxArc) {
                    float smearRange = _SmearHighs[row] - _SmearLows[row];
                    int col = int((1.0 - rx / maxArc) * smearRange + _SmearLows[row]);
                    xy.x = float(col) / 32.0;
                    xy.y = float(row) / 32.0;
                }
            }
            c = tex2D(_MainTex, xy);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Albedo *= o.Alpha;
        }
        ENDCG
    }

Fallback "Transparent/VertexLit"
}