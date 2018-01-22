Shader "Unlit/GlitchShader" {
    Properties {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        
        _Elapsed("Elapsed Seconds", Float) = 1.0
        
        [Space(25)][MaterialToggle] _HDispEnabled(" === Horizontal Displacement === ", Float) = 0.0
        [MaterialToggle] _HDispSloppyPower("Sloppy power", Float) = 0
        _HDispChance("Chance", Range(0, 1)) = 0.5
        _HDispPower("Power", Range(0, 1)) = 0.5
        _HDispPowerVariance("Power variance", Range(0, 1)) = 0.5
        _HDispChunking("Chunk size", Range(0, 1)) = 0.5
        _HDispChunkingVariance("Chunk size variance", Range(0, 1)) = 0.5
        
        [Space(25)][MaterialToggle] _HBleedEnabled(" === Horizontal Streaking === ", Float) = 0
        [MaterialToggle] _HBleedAlphaRestrict("Restrict to alpha", Float) = 0
        _HBleedChance("Chance", Range(0, 1)) = 0.5
        _HBleedChunking("Chunk size", Range(0, 1)) = 0.5
        _HBleedChunkingVariance("Chunk size variance", Range(0, 1)) = 0.5
        _HBleedTailing("Tail length", Range(0, 1)) = 0.5
        _HBleedTailingVariance("Tail length variance", Range(0, 1)) = 0.5
        
        [Space(25)][MaterialToggle] _SFrameEnabled(" === Static Frames === ", Float) = 0
        [MaterialToggle] _SFrameAlphaIncluded("Include alpha regions", Float) = 0
        _SFrameChance("Chance", Range(0, 1)) = 0.5
        _SFrameChunking("Chunk size", Range(0, 1)) = 0.5
        _SFrameChunkingVariance("Chunk size variance", Range(0, 1)) = 0.5
        
        [Space(25)][MaterialToggle] _PDistEnabled(" === Palette Distortion === ", Float) = 0
        [MaterialToggle] _PDistAlphaIncluded("Include alpha regions", Float) = 0
        [MaterialToggle] _PDistSimultaneousInvert("Synchronized inversion", Float) = 0
        _PDistInvertR("R inversion chance", Range(0, 1)) = 0.0
        _PDistInvertG("G inversion chance", Range(0, 1)) = 0.0
        _PDistInvertB("B inversion chance", Range(0, 1)) = 0.0
        _PDistMaxR("R max chance", Range(0, 1)) = 0.0
        _PDistMaxG("G max chance", Range(0, 1)) = 0.0
        _PDistMaxB("B max chance", Range(0, 1)) = 0.0
        _PDistMonocolorChance("Monocolor chance", Range(0, 1)) = 0.0
        _PDistMonocolor("Monocolor", Color) = (1.0, 1.0, 1.0, 1.0)
        
        [Space(25)][MaterialToggle] _RDispEnabled(" === Rectangular Displacement === ", Float) = 0.0
        [MaterialToggle] _RDispCopyOnly("Keep source region intact", Range(0, 1)) = 0.0
        [MaterialToggle] _RDispInvertSource("Invert source background", Range(0, 1)) = 0.0
        [MaterialToggle] _RDispKeepAlpha("Preserve source alpha", Range(0, 1)) = 1.0
        _RDispTex("Background texture", 2D) = "black" {}
        _RDispChance("Chance", Range(0, 1)) = 0.5
        _RDispChunkXSize("Chunk X Size", Range(0, 1)) = 0.5
        _RDispChunkYSize("Chunk Y size", Range(0, 1)) = 0.5
        _RDispChunkVariance("Chunk size variance", Range(0, 1)) = 0.5
        _RDispMinPowerX("Displacement min dist X", Range(-1, 1)) = -0.5
        _RDispMaxPowerX("Displacement max dist X", Range(-1, 1)) = 0.5
        _RDispMinPowerY("Displacement min dist Y", Range(-1, 1)) = -0.5
        _RDispMaxPowerY("Displacement max dist Y", Range(-1, 1)) = 0.5
        
        [Space(25)][MaterialToggle] _VSyncEnabled(" === VSync === ", Float) = 0.0
        _VSyncPowerMin("Min jitter power", Range(-1, 1)) = -0.5
        _VSyncPowerMax("Max jitter power", Range(-1, 1)) = 0.5
        _VSyncJitterChance("Jitter chance", Range(0, 1)) = 0.5
        _VSyncJitterDuration("Jitter duration", Range(0, 1)) = 0.5
        _VSyncChance("Loop chance", Range(0, 1)) = 0.5
        _VSyncDuration("Loop duration", Range(0, 1)) = 0.5
        
        [Space(25)][MaterialToggle] _SShiftEnabled(" === Scanline Shift === ", Float) = 0.0
        _SShiftChance("Chance", Range(0, 1)) = .5
        _SShiftPowerMin("Min power", Range(0, 1)) = 0.25
        _SShiftPowerMax("Max power", Range(0, 1)) = 0.5
        
        [Space(25)][MaterialToggle] _TDistEnabled(" === Traveling Distortion === ", Float) = 0.0
        [MaterialToggle] _TDistTailoff("Linear tailoff", Range(0, 1)) = 1
        [MaterialToggle] _TDistExcludeAlpha("Exclude alpha regions", Range(0, 1)) = 0
        _TDistChance("Chance", Range(0, 1)) = .5
        _TDistDuration("Duration", Range(0, 1)) = .5
        _TDistChunking("Chunk height", Range(0, 1)) = .5
        _TDistStaticBarSize("Static effect height", Range(0, 1)) = .5
        _TDistStaticSize("Static chunk size", Range(0, 1)) = .5
        [MaterialToggle] _TDistHDisp("Horizontal displacement enabled", Range(0, 1)) = 0
        _TDistHDispPower("Displacement power", Range(0, 1)) = .5
        _TDistHDispPowerVariance("Displacement power variance", Range(0, 1)) = .5
        _TDistColorBarSize("Color effect height", Range(0, 1)) = 0
        [MaterialToggle] _TDispPreserveBrightness("Color preserve brightness", Range(0, 1)) = 0
        [MaterialToggle] _TDistInvertR("Color invert R", Range(0, 1)) = 0
        [MaterialToggle] _TDistInvertG("Color invert G", Range(0, 1)) = 0
        [MaterialToggle] _TDistInvertB("Color invert B", Range(0, 1)) = 0
        
        [Space(25)][MaterialToggle] _SColorEnabled(" === Scanline Coloring === ", Float) = 0.0
        [MaterialToggle] _SColorExcludeAlpha("Exclude alpha regions", Range(0, 1)) = 1.0
        _SColorChance("Chance",  Range(0, 1)) = 0.5
        _SColorVelocity("Vertical velocity",  Range(-1, 1)) = 0.0
        _SColorGap("Gap",  Range(0, 1)) = 0.05
        _SColorBrightness("Brightness change",  Range(-1, 1)) = 0.0
        [MaterialToggle] _SColorBleed("Full bleed", Range(0, 1)) = 0.0
        [MaterialToggle] _SColorStatic("Scanline static", Range(0, 1)) = 0.0
        
        [Space(25)][MaterialToggle] _CClampEnabled(" === Color Channel Clamping === ", Float) = 0.0
		_CClampBrightness("Pre-brightness boost",  Range(-1, 1)) = 0.0
        [MaterialToggle] _CClampBlack("Always include true black/white", Float) = 1.0
        _CClampR("R shades allowed",  Range(0, 1)) = 1.0
        _CClampG("G shades allowed",  Range(0, 1)) = 1.0
        _CClampB("B shades allowed",  Range(0, 1)) = 1.0
        [MaterialToggle] _CClampDither("Dithering enabled", Float) = 0.0
        [MaterialToggle] _CClampDitherVary("Varied dithering enabled", Float) = 0.0
        _CClampDitherChunk("Dithering chunk width", Range(0, 1)) = 0.5
        _CClampJitterR("R colors jitter power",  Range(0, 1)) = 0.0
        _CClampJitterG("G colors jitter power",  Range(0, 1)) = 0.0
        _CClampJitterB("B colors jitter power",  Range(0, 1)) = 0.0

		[Space(25)][MaterialToggle] _PEdgeEnabled(" === Pulsing Edge === ", Float) = 0.0
		_PEdgeDuration("Duration",  Range(0.1, 10)) = 0.5
        _PEdgeDepthMin("Depth Min",  Range(0, 1)) = 0.0
		_PEdgeDepthMax("Depth Max",  Range(0, 1)) = 0.5
		_PEdgePower("Power",  Range(0, 1)) = 1.0
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
            
            float _HDispEnabled;
            float _HDispChance;
            float _HDispPower;
            float _HDispPowerVariance;
            float _HDispChunking;
            float _HDispChunkingVariance;
            float _HDispSloppyPower;
            
            float _HBleedEnabled;
            float _HBleedChance;
            float _HBleedChunking;
            float _HBleedChunkingVariance;
            float _HBleedTailing;
            float _HBleedTailingVariance;
            float _HBleedAlphaRestrict;
            
            float _SFrameEnabled;
            float _SFrameAlphaIncluded;
            float _SFrameChance;
            float _SFrameChunking;
            float _SFrameChunkingVariance;
            
            float _PDistEnabled;
            float _PDistAlphaIncluded;
            float _PDistInvertR;
            float _PDistInvertG;
            float _PDistInvertB;
            float _PDistSimultaneousInvert;
            float _PDistMaxR;
            float _PDistMaxG;
            float _PDistMaxB;
            float _PDistMonocolorChance;
            float4 _PDistMonocolor;
            
            float _RDispEnabled;
            sampler2D _RDispTex;
            float _RDispInvertSource;
            float _RDispChunkXSize;
            float _RDispChunkYSize;
            float _RDispChunkVariance;
            float _RDispSquareDisp;
            float _RDispMinPowerX;
            float _RDispMaxPowerX;
            float _RDispMinPowerY;
            float _RDispMaxPowerY;
            float _RDispChance;
            float _RDispCopyOnly;
            float _RDispKeepAlpha;
            
            float _VSyncEnabled;
            float _VSyncPowerMin;
            float _VSyncPowerMax;
            float _VSyncChance;
            float _VSyncDuration;
            float _VSyncJitterChance;
            float _VSyncJitterDuration;
            
            float _SShiftEnabled;
            float _SShiftChance;
            float _SShiftPowerMin;
            float _SShiftPowerMax;
            
            float _TDistEnabled;
            float _TDistChance;
            float _TDistDuration;
            float _TDistColorBarSize;
            float _TDistStaticBarSize;
            float _TDistInvertR;
            float _TDistInvertG;
            float _TDistInvertB;
            float _TDistExcludeAlpha;
            float _TDistStaticSize;
            float _TDistTailoff;
            float _TDispPreserveBrightness;
            float _TDistHDisp;
            float _TDistHDispPower;
            float _TDistChunking;
            float _TDistHDispPowerVariance;
            
            float _SColorEnabled;
            float _SColorBleed;
            float _SColorStatic;
            float _SColorBrightness;
            float _SColorGap;
            float _SColorChance;
            float _SColorVelocity;
            float _SColorExcludeAlpha;
            
            float _CClampEnabled;
            float _CClampSynch;
            float _CClampDither;
            float _CClampDitherVary;
            float _CClampR;
            float _CClampG;
            float _CClampB;
            float _CClampVariance;
            float _CClampDitherChunk;
            float _CClampBlack;
            float _CClampJitterR;
            float _CClampJitterG;
            float _CClampJitterB;
			float _CClampBrightness;

			float _PEdgeEnabled;
			float _PEdgeDepthMin;
			float _PEdgeDuration;
			float _PEdgeDepthMax;
			float _PEdgePower;
            
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
			sampler2D _AlphaTex;
            
            // for when 0.0001 and 0.1 are equally valid
            // source is from a slider, usually 0-1
            float cubicEase(float source,  float newMax) {
                return (source * source * source) * newMax;
            }
            
            // simple remap from source to a new max scale
            float ease(float source, float newMax) {
                return source * newMax;
            }
            
            float rand2(float seed1, float seed2) {
                return frac(sin(dot(float2(seed1, seed2), float2(12.9898, 78.233))) * 43758.5453);
            }
            
            float rand3(float seed1, float seed2, float seed3) {
                return frac(sin(dot(float3(seed1, seed2, seed3), float3(45.5432, 12.9898, 78.233))) * 43758.5453);
            }
            
            float lerp(float a, float b, float r) {
                return r * a + (1.0 - r) * b;
            }
            
            // returns a result between rangeMin and rangeMax, eased
            float randRange(float rangeMin, float rangeMax, float easedMax, float3 seed) {
                float base = rangeMin;
                base = base + (rangeMax - rangeMin) * rand3(seed[0], seed[1], seed[2]);
                return cubicEase(base, easedMax);
            }
            
            // varies the source value by a percentage
            // seed: seed value to pass to rand
            // source: the value to modify
            // variance: from 0-1 how much variance is allowed (from slider)
            // varianceRange: at max variance, the percent that source varies
            float variance3(float source, float variance, float varianceRange, float3 seed) {
                float v = variance * varianceRange;
                float v2 = v * rand3(seed[0], seed[1], seed[2]);
                return source + (source * v2);
            }
            
            // same as interval but no covariance and not clamped (for time)
            float intervalT(float interval) {
                return ((float)((int)(_Elapsed * (1.0/interval)))) * interval;
            }
            
            // same as interval, except it should covary based on a given seed
            float intervalR(float source, float interval, float seed) {
                float stagger = rand2(seed, _Elapsed);
                float result = ((float)((int)((source + stagger) * (1.0/interval)))) * interval - stagger;
                return clamp(result, 0.0, 1.0);
            }
            
            // fixed interval, won't vary frame to frame
            float intervalF(float source, float interval) {
                return ((float)((int)(source * (1.0/interval)))) * interval;
            }
            
            // argument is in range 0-1
            // but we need to clamp it to say, 0.0, 0.2, 0.4 etc for 1/5 chunks
            float interval(float source, float interval) {
                return intervalR(source, interval, 12.34);
            }
            
            // clamps a color to a fixed pallette
            // source: original color value
            // shadesAllowed: 0-1, will ease this to get final shade count
            // dither: true to weighted-random pick between two nearest shades
            // vary: dithering choices will vary with time too
            // seed: how to get the seed for dithering
            float clampShade(float source, float shadesAllowed, bool dither, bool vary, float2 seed) {
                float interval = 1.0 - ease(shadesAllowed, 1.0);
                if (interval == 0.0) {
                    return source;
                }
                float low = intervalF(source, interval);
                float high = intervalF(source, interval) + interval;
                //return high;
                if (_CClampBlack > 0.0 && low < interval / 2.0) {
                    low = 0.0;
                }
                if (_CClampBlack > 0.0 && 1.0 - high < interval / 2.0) {
                    high = 1.0;
                }
                if (!dither) {
                    if (source - low < high - source) {
                        return low;
                    } else {
                        return high;
                    }
                }
                float chance = (high - source) / interval;
                float roll;
                if (vary) {
                    roll = rand3(_Elapsed, seed[0] * 34.0, seed[1] * 35.0);
                } else {
                    roll = rand2(seed[0] * 34.0, seed[1] * 35.0);
                }
                if (roll > 1.0 - chance) {
                    return low;
                } else {
                    return high;
                }
            }
            
            // returns a jitter from -1 to 1 scaled by jitter
            float jitter(float jitter, fixed2 seed) {
                if (jitter > 0.0) {
                    float add = rand2(seed[0] * 36.0, seed[1] * 17.0) * 2.0 - 1.0;
                    add *= jitter;
                    return add;
                }
                return 0.0;
            }
            
            // return c2 with the brightness of c1
            fixed4 preserveBrightness(fixed4 c1, fixed4 c2) {
                fixed4 result = c2;
                float brightness = (c1[0] + c1[1] + c1[2]);
                float newBrightness = (c2[0] + c2[1] + c2[2]);
                float ratio = brightness / newBrightness;
                result[0] *= ratio;
                result[1] *= ratio;
                result[2] *= ratio;
                return result;
            }
            
            // inverts a given color channel
            // source: this is the source color that will be inverted
            // channelIndex: the index to flip (r/g/b 0/1/2)
            // chance: will be cubicly eased to 0-1 range
            // seed: covariant
            fixed4 invertChannel(fixed4 source, int channelIndex, float chance, float seed) {
                fixed4 result = source;
                float roll = rand2(_Elapsed, seed);
                float invertChance = cubicEase(chance, 1.0);
                if ((roll > 1.0 - invertChance) && (source.a > 0.02 || _PDistAlphaIncluded > 0.0)) {
                    result[channelIndex] = 1.0 - result[channelIndex];
                }
                return result;
            }
            fixed4 maxChannel(fixed4 source, int channelIndex, float chance, float seed) {
                fixed4 result = source;
                float roll = rand2(_Elapsed, seed);
                float invertChance = cubicEase(chance, 1.0);
                if ((roll > 1.0 - invertChance) && (source.a > 0.02 || _PDistAlphaIncluded > 0.0)) {
                    result[channelIndex] = 1.0;
                }
                return result;
            }

			fixed4 SampleSpriteTexture (float2 uv) {
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				//color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target {
                float2 xy = IN.texcoord;
                float2 pxXY = IN.vertex;
                float t = _Elapsed + 500.0;
                
                // horizontal chunk displacement
                if (_HDispEnabled > 0.0) {
                    float hdispChunkSize = variance3(cubicEase(_HDispChunking, 0.2), _HDispChunkingVariance, 1.0, float3(0.0, 0.0, t));
                    float hdispChance = cubicEase(_HDispChance, 0.05);
                    float hdispRoll = rand3(0.1, interval(xy[1], hdispChunkSize), t);
                    if (hdispRoll > 1.0 - hdispChance) {
                        float powerSeed = _HDispSloppyPower < 1.0 ? interval(xy[1], hdispChunkSize) : xy[1];
                        xy[0] += variance3(cubicEase(_HDispPower, 0.15), _HDispPowerVariance, 1.0, float3(0.2, powerSeed, t));
                    }
                }
                
                // v-sync
                if (_VSyncEnabled > 0.0 ) {
                    float syncDuration = cubicEase(_VSyncDuration, 0.5);
                    float syncChunk = intervalT(syncDuration);
                    float syncChance = cubicEase(_VSyncChance, 1.0);
                    float syncRoll = rand2(syncChunk, 20.0);
                    if (syncRoll > 1.0 - syncChance) {
                        float syncElapsed = (_Elapsed - syncChunk) / syncDuration;
                        xy[1] -= syncElapsed;
                    } else {
                        float jitterDuration = cubicEase(_VSyncJitterDuration, 0.4);
                        float jitterChunk = intervalT(jitterDuration);
                        float jitterChance = cubicEase(_VSyncJitterChance, 1.0);
                        float jitterRoll = rand2(jitterChunk, 21.0);
                        if (jitterRoll > 1.0 - jitterChance) {
                            float jitterElapsed = (_Elapsed - jitterChunk) / jitterDuration;
                            float power = randRange(_VSyncPowerMin, _VSyncPowerMax, 0.4, float3(jitterChunk, 22.0, 22.0));
                            if (jitterElapsed < 0.5) {
                                power *= (jitterElapsed * 2.0);
                            } else {
                                power *= (1.0 - ((jitterElapsed - 0.5) * 2.0));
                            }
                            xy[1] += power;
                        }
                    }
                    if (xy[1] < 0.0) {
                        xy[1] += 1.0;
                    }
                    if (xy[1] > 1.0) {
                        xy[1] -= 1.0;
                    }
                }
                
                // scanline shift
                if (_SShiftEnabled > 0.0) {
                    float chance = cubicEase(_SShiftChance, 1.0);
                    float roll = rand2(23.0, t);
                    if (roll > 1.0 - chance) {
                        int remain = 0;
                        if (rand2(t, 24.0) > 0.5) {
                            remain = 1;
                        }
                        int mod = pxXY[1] % 2;
                        if (mod == remain) {
                            float power = randRange(_SShiftPowerMin, _SShiftPowerMax, 0.2, float3(24.0, t, 0.0));
                            if (rand2(t, 25.0) > 0.5) {
                                power *= -1;
                            }
                            xy[0] += power;
                        }
                    }
                }
                
                fixed4 c = SampleSpriteTexture(xy) * IN.color;
				c.rgb *= c.a;
                
                // rectangular displacement
                if (_RDispEnabled > 0.0) {
                    float chunkSizeX = variance3(cubicEase(_RDispChunkXSize, 0.3), _RDispChunkVariance, 1.0, float3(11.0, 0.0, t));
                    float chunkSizeY = variance3(cubicEase(_RDispChunkYSize, 0.3), _RDispChunkVariance, 1.0, float3(13.0, 0.0, t));
                    float chance = cubicEase(_RDispChance, 1.0);
                    
                    // source (we are the source coords)
                    if (_RDispCopyOnly == 0.0) {
                        float sourceChunkX = intervalR(xy[0], chunkSizeX, 12.0);
                        float sourceChunkY = intervalR(xy[1], chunkSizeY, 14.0);
                        float sourceRoll = rand3(t, sourceChunkX, sourceChunkY);
                        if ((sourceRoll > 1.0 - chance) && (!_RDispKeepAlpha || (c.a > 0.01))) {
                            if (_RDispInvertSource > 0.0) {
                                fixed4 cOrig = c;
                                c[0] = (1.0f - c[0]);
                                c[1] = (1.0f - c[1]);
                                c[2] = (1.0f - c[2]);
                                c = preserveBrightness(cOrig, c);
                            } else {
                                c = tex2D(_RDispTex, xy);
                                c.rgb *= c.a;
                            }
                        }
                    }

                    // destination (we are the destination coords)
                    float offX = randRange(_RDispMinPowerX, _RDispMaxPowerX, 0.2, float3(15.0, 0.0, t));
                    float offY = randRange(_RDispMinPowerY, _RDispMaxPowerY, 0.2, float3(16.0, 0.0, t));
                    float sourceX = xy[0] + offX;
                    float sourceY = xy[1] + offY;
                    float sourceChunkX = intervalR(sourceX, chunkSizeX, 12.0);
                    float sourceChunkY = intervalR(sourceY, chunkSizeY, 14.0);
                    float sourceRoll = rand3(t, sourceChunkX, sourceChunkY);
                    if (sourceRoll > 1.0 - chance) {
                        c = tex2D(_MainTex, float2(sourceX, sourceY));
                        c.rgb *= c.a;
                    }
                }
                
                // traveling distortion
                if (_TDistEnabled > 0.0) {
                    float duration = cubicEase(_TDistDuration, 2.0);
                    float chunk = intervalT(duration);
                    float chance = cubicEase(_TDistChance, 1.0);
                    float roll = rand2(chunk, 26.0);
                    if (roll > 1.0 - chance) {
                        float elapsed = (_Elapsed - chunk) / duration;
                        float maxSize = cubicEase(max(_TDistColorBarSize, _TDistStaticBarSize), 1.0);
                        if (_TDistColorBarSize > 0.0) {
                            float colorSize = cubicEase(_TDistColorBarSize, 1.0);
                            float y1 = lerp(-maxSize, 1.0, elapsed);
                            float y2 = lerp(0.0, 1.0 + maxSize, elapsed) - (maxSize - colorSize);
                            if (xy[1] > y1 && xy[1] <= y2) {
                                float posRatio = 1.0 - ((xy[1] - y1) / (y2 - y1));
                                float chunkedRatio = posRatio;
                                if (_TDistChunking > 0.0) {
                                    float chunkSize = ease(_TDistChunking, .5);
                                    chunkedRatio = intervalR(posRatio, chunkSize, 28.0);
                                }
                                if (_TDistHDisp > 0.0) {
                                    float power = variance3(cubicEase(_TDistHDispPower, 0.5), _TDistHDispPowerVariance, 1.0, float3(29.0, t, 0.0));
                                    power *= chunkedRatio;
                                    c = tex2D(_MainTex, float2(xy[0] + power, xy[1]));
                                    c.rgb *= c.a;
                                }
                                if (!_TDistExcludeAlpha || (c.a > 0.01)) {
                                    fixed4 cOrig = c;
                                    if (_TDistInvertR > 0.0) {
                                        c[0] = 1.0 - c[0];
                                    }
                                    if (_TDistInvertG > 0.0) {
                                        c[1] = 1.0 - c[1];
                                    }
                                    if (_TDistInvertB > 0.0) {
                                        c[2] = 1.0 - c[2];
                                    }
                                    if (_TDispPreserveBrightness) {
                                        c = preserveBrightness(cOrig, c);
                                    }
                                    if (_TDistTailoff > 0.0) {
                                        c[0] = lerp(c[0], cOrig[0], chunkedRatio);
                                        c[1] = lerp(c[1], cOrig[1], chunkedRatio);
                                        c[2] = lerp(c[2], cOrig[2], chunkedRatio);
                                    }
                                    float fuzzy = rand3(t, xy[0], posRatio);
                                    if (fuzzy > 0.5) {
                                        fuzzy = (fuzzy - 0.5) * 2.0;
                                        float fuzzFactor = 0.08;
                                        if (fuzzy < 0.33) {
                                            c[0] += fuzzFactor;
                                            c[1] += fuzzFactor;
                                        } else if (fuzzy < 0.66) { 
                                            c[0] += fuzzFactor;
                                            c[2] += fuzzFactor;
                                        } else {
                                            c[1] += fuzzFactor;
                                            c[2] += fuzzFactor;
                                        }
                                    }
                                }
                            }
                        }
                        if (_TDistStaticBarSize > 0.0) {
                            float staticSize = cubicEase(_TDistStaticBarSize, 1.0);
                            float y1 = lerp(-maxSize, 1.0, elapsed);
                            float y2 = lerp(0.0, 1.0 + maxSize, elapsed) - (maxSize - staticSize);
                            if (xy[1] > y1 && xy[1] <= y2) {
                                float posRatio = 1.0 - ((xy[1] - y1) / (y2 - y1));
                                float chunkedRatio = posRatio;
                                fixed4 cOrig = c;
                                float staticSize = cubicEase(_TDistStaticSize, 0.2);
                                float staticChunk = intervalR(xy[0], staticSize, 27.0);
                                float staticRoll = rand3(staticChunk, t, xy[1]);
                                float newValue = 0.0;
                                if (staticRoll > 0.5) {
                                    newValue = 1.0;
                                }
                                c[0] = newValue;
                                c[1] = newValue;
                                c[2] = newValue;
                                if (_TDistTailoff > 0.0) {
                                    c[0] = lerp(c[0], cOrig[0], posRatio);
                                    c[1] = lerp(c[1], cOrig[1], posRatio);
                                    c[2] = lerp(c[2], cOrig[2], posRatio);
                                }
                            }
                        }
                    }
                }
                
                // horizontal color bleed
                if (_HBleedEnabled > 0.0) {
                    float hbleedTailSize = variance3(cubicEase(_HBleedTailing, 0.5), _HBleedTailingVariance, 1.0, float3(0.0, 0.0, t));
                    float hbleedChunkSize = variance3(cubicEase(_HBleedChunking, 0.2), _HBleedChunkingVariance, 1.0, float3(0.0, 0.0, t));
                    float hbleedChance = cubicEase(_HBleedChance, 1.0);
                    float hbleedYInterval = interval(xy[1], 0.1);
                    float hbleedXInterval = intervalR(xy[0], abs(hbleedTailSize), hbleedChance * 100);
                    float hbleedRoll = rand3(hbleedXInterval * 100, hbleedYInterval * 200, t);
                    if (hbleedRoll > 1.0 - hbleedChance) {
                        float r = (xy[0] - hbleedXInterval) / abs(hbleedTailSize);
                        fixed4 c2 = SampleSpriteTexture(float2(hbleedXInterval, xy[1])) * IN.color;
                        c2.rgb *= c2.a;
                        
                        fixed4 smear;
                        smear.r = lerp(c.r, c2.r, r);
                        smear.g = lerp(c.g, c2.g, r);
                        smear.b = lerp(c.b, c2.b, r);
                        if (c.a < 0.02 || _HBleedAlphaRestrict < 1.0) {
                            c.r = (smear.r > c.r) ? smear.r : c.r;
                            c.g = (smear.g > c.g) ? smear.g : c.g;
                            c.b = (smear.b > c.b) ? smear.b : c.b;
                        }
                        if (c.a < 0.02) {
                            c.a = c2.a;
                        }
                    }
                }

				// pulsing edge
				if (_PEdgeEnabled > 0.0) {
					float dx = xy[0] - .5;
					float dy = xy[1] - .5;
					float dist = dx * dx + dy * dy;
					float offset = dist + _PEdgeDepthMin + (_PEdgeDepthMax - _PEdgeDepthMin) * ((.5 + sin(t / _PEdgeDuration)) / 2.0);
					offset = offset * _PEdgePower;
					c[0] -= offset;
					c[1] -= offset;
					c[2] -= offset;
					c[0] = clamp(c[0], 0.0, 1.0);
					c[1] = clamp(c[1], 0.0, 1.0);
					c[2] = clamp(c[2], 0.0, 1.0);
				}
                
                // scanline recolorations
                if (_SColorEnabled > 0.0 && (!_SColorExcludeAlpha || (c.a > 0.01))) {
                    float chance = cubicEase(_SColorChance, 1.0);
                    float roll = rand3(31.0, t, 0.0); // dunno if they should turn off independently
                    if (roll >= 1.0 - chance) {
                        uint chunkSize = floor(cubicEase(_SColorGap, 512.0));
                        if (chunkSize < 2) {
                            chunkSize = 2;
                        }
                        uint y = pxXY[1];
                        uint scanlineY = intervalF(y, chunkSize);
                        uint off = (t * cubicEase(_SColorVelocity, 512.0)) % chunkSize;
                        if (y == scanlineY + off) {
                            if (_SColorStatic > 0.0) {
                                float brightness = 0.0;
                                if (rand3(t, xy[0] * 31.0, xy[1] * 32.0) > 0.5) {
                                    brightness = 1.0;
                                }
                                c[0] = brightness;
                                c[1] = brightness;
                                c[2] = brightness;
                            }
                            c[0] += _SColorBrightness;
                            c[1] += _SColorBrightness;
                            c[2] += _SColorBrightness;
                            c[0] = clamp(c[0], 0.0, 1.0);
                            c[1] = clamp(c[1], 0.0, 1.0);
                            c[2] = clamp(c[2], 0.0, 1.0);
                        }
                    }
                }
                
                // channel clamping
                if (_CClampEnabled > 0.0 && c.a > 0.01) {
                    fixed2 seed = xy;
                    if (_CClampDitherChunk > 0.0) {
                        if (_CClampDitherVary > 0.0) {
                            seed[0] = intervalR(seed[0], cubicEase(_CClampDitherChunk, 0.5), t);
                        } else {
                            seed[0] = intervalF(seed[0], cubicEase(_CClampDitherChunk, 0.5));
                        }
                    }
                    float shadesR = _CClampR + jitter(cubicEase(_CClampJitterR, 1.0), fixed2(t, 10.0));
                    float shadesG = _CClampG + jitter(cubicEase(_CClampJitterG, 1.0), fixed2(t, 20.0));
                    float shadesB = _CClampB + jitter(cubicEase(_CClampJitterB, 1.0), fixed2(t, 30.0));
                    c[0] = clampShade(c[0] + _CClampBrightness, shadesR, _CClampDither > 0.0, _CClampDitherVary > 0.0, seed);
                    c[1] = clampShade(c[1] + _CClampBrightness, shadesG, _CClampDither > 0.0, _CClampDitherVary > 0.0, seed);
                    c[2] = clampShade(c[2] + _CClampBrightness, shadesB, _CClampDither > 0.0, _CClampDitherVary > 0.0, seed);
                }
                
                // static frames
                if (_SFrameEnabled > 0.0) {
                    float sframeChance = cubicEase(_SFrameChance, 1.0) + 0.01;
                    float sframeRoll = rand2(0.6, t);
                    if ((_SFrameAlphaIncluded || c.a >= 0.02) && (sframeRoll > 1.0 - sframeChance)) {
                        float sframeChunkSize = variance3(cubicEase(_SFrameChunking, 0.2), _SFrameChunkingVariance, 1.0, float3(0.0, 0.0, t));
                        if (sframeChunkSize < 0.001) {
                            sframeChunkSize = 0.001;
                        }
                        float sframeInterval = intervalR(xy[0], sframeChunkSize, t);
                        float sframeSubroll = rand3(sframeInterval * 200.0, xy[1] * 1000.0, t);
                        if (sframeSubroll > 0.5) {
                            c = float4(0.0, 0.0, 0.0, 1.0);
                        } else {
                            c = float4(1.0, 1.0, 1.0, 1.0);
                        }
                    }
                }
                
				return c;
			}
		ENDCG
		}
	}
}
