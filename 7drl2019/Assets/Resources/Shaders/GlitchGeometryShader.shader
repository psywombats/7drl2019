Shader "Erebus/GlitchGeometryShader" {
    Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        [PerRendererData] _Alpha("Alpha", Float) = 1.0
        _Cutoff("Base Alpha cutoff", Range(0,.9)) = .5
        _ResolutionX("Resolution X (px)", Float) = 1280
        _ResolutionY("Resolution Y (px)", Float) = 720
        
        [PerRendererData]_Elapsed("Elapsed Seconds", Float) = 1.0
        
        [PerRendererData]_HeroPos("Hero position", Vector) = (0, 0, 0, 0)
        [PerRendererData]_OldHeroPos("Old hero position", Vector) = (0, 0, 0, 0)
        [PerRendererData]_SightRange("Sight range", Float) = 100.0
        [PerRendererData]_CellResolutionX("Cell count x", Float) = 32.0
        [PerRendererData]_CellResolutionY("Cell count y", Float) = 32.0
        [PerRendererData]_VisibilityTex("Visibility", 2D) = "white" {}
        [PerRendererData]_OldVisibilityTex("Old Visibility", 2D) = "white" {}
        [PerRendererData]_VisibilityBlend("Vis Blend", Float) = 1.0
    }
    
    SubShader {
    
        Tags {
            "Queue"="Geometry"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Lighting Off
        Blend SrcAlpha One
        Offset 10, -1

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
        #pragma multi_compile _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #include "Glitch.cginc"
        
        float _ResolutionX;
        float _ResolutionY;
        float _Alpha;
        float _CellResolutionX;
        float _CellResolutionY;
        sampler2D _VisibilityTex;
        sampler2D _OldVisibilityTex;
        float _VisibilityBlend;
        float _SightRange;
        float4 _HeroPos;
        float4 _OldHeroPos;

        struct Input {
            float2 uv_MainTex;
            fixed4 color;
            float2 texcoord : TEXCOORD0;
            float2 cellUV;
            float4 vertex;
        };

        void vert(inout appdata_full v, out Input o) {
            v.vertex.xy *= _Flip.xy;

            #if defined(PIXELSNAP_ON)
            v.vertex = UnityPixelSnap (v.vertex);
            #endif

            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = v.color * _Color * _RendererColor;
            
            float2 cell = float2(v.vertex.x, v.vertex.z);
            if (v.normal.x > 0.0) {
                cell.x += 0.5;
            }
            if (v.normal.z > 0.0) {
                cell.y += 0.5;
            }
            if (v.normal.x < 0.0) {
                cell.x -= 0.5;
            }
            if (v.normal.z < 0.0) {
                cell.y -= 0.5;
            }
            cell.x = (round(cell.x * 2.0)) / 2.0;
            cell.y = (round(cell.y * 2.0)) / 2.0;
            o.cellUV = float2(cell.x / _CellResolutionX, cell.y / _CellResolutionY);
            o.vertex = v.vertex;
        }
        
        float sqdist(float4 a, float4 b) {
            return  (a.x - b.x) * (a.x - b.x) + 
                    (a.y - b.y) * (a.y - b.y) +
                    (a.z - b.z) * (a.z - b.z);
        }
        
        float adjustAlpha(float4 a, float4 b, float sight) {
            float dist = sqdist(a, b);
            dist = dist - (sight * sight);
            float blur = 12.0;
            if (dist > blur) dist = blur;
            if (dist < 0.0) dist = 0.0;
            dist /= blur;
            return dist;
        }

        void surf(Input IN, inout SurfaceOutput o) {
            float2 xy = IN.uv_MainTex;
            // float4 pxXY = float4(xy[0] * (float)_ResolutionX, xy[1] * (float)_ResolutionY, 0.0, 0.0);
            fixed4 c1 = SampleSpriteTexture(xy) * IN.color;
            fixed4 c2 = fixed4(c1[0], c1[1], c1[2], c1[3]);
            fixed4 data1 = tex2D(_VisibilityTex, IN.cellUV);
            fixed4 unknown1 = fixed4((c1.r + c1.g + c1.b) * 0.09375, 0.0, (c1.r + c1.g + c1.b) *0.28, 1.0);
            if (data1.b < 1.0) { unknown1.b = 0.0; unknown1.r = 0.0; }
            float fader1 = adjustAlpha(_HeroPos, IN.vertex, _SightRange);
            if (data1.r < 1.0) fader1 = 1.0;
            c1 = fader1 * unknown1 + (1.0 - fader1) * c1;
            
            fixed4 data2 = tex2D(_OldVisibilityTex, IN.cellUV);
            fixed4 unknown2 = fixed4((c2.r + c2.g + c2.b) * 0.09375, 0.0, (c2.r + c2.g + c2.b) *0.28, 1.0);
            if (data2.b < 1.0) { unknown2.b = 0.0; unknown2.r = 0.0; }
            float fader2 = adjustAlpha(_OldHeroPos, IN.vertex, _SightRange);
            if (data2.r < 1.0) fader2 = 1.0;
            c2 = fader2 * unknown2 + (1.0 - fader2) * c2;
            
            fixed4 c = _VisibilityBlend * c1 + (1.0 - _VisibilityBlend) * c2;
            
            o.Albedo = c.rgb * c.a;
            o.Alpha = c.a * _Alpha;
        }
        ENDCG
    }

Fallback "Transparent/VertexLit"
}