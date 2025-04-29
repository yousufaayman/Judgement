Shader "Judgment/LoginDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DistortionTex ("Distortion Texture", 2D) = "gray" {}
        _DistortionAmount ("Distortion Amount", Range(0, 0.1)) = 0.02
        _SinColor ("Sin Color", Color) = (1,0,0,1)
        _SinIntensity ("Sin Color Intensity", Range(0, 1)) = 0.3
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1
        _VignetteIntensity ("Vignette Intensity", Range(0, 2)) = 1.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            sampler2D _DistortionTex;
            float4 _MainTex_ST;
            float _DistortionAmount;
            float4 _SinColor;
            float _SinIntensity;
            float _PulseSpeed;
            float _VignetteIntensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Get distortion texture value and convert it to displacement
                float2 distortion = tex2D(_DistortionTex, i.uv + float2(_Time.y * 0.05, _Time.y * 0.07)).rg;
                distortion = ((distortion * 2) - 1) * _DistortionAmount;
                
                // Apply pulsing effect
                float pulse = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5) * 0.3 + 0.7;
                distortion *= pulse;
                
                // Sample the texture with distortion
                fixed4 col = tex2D(_MainTex, i.uv + distortion);
                
                // Create a vignette effect
                float2 uv = i.uv * 2 - 1;
                float vignette = 1 - dot(uv, uv) * _VignetteIntensity;
                vignette = saturate(vignette);
                
                // Add sin color with pulsing intensity
                float sinColorPulse = sin(_Time.y * _PulseSpeed * 0.5) * 0.5 + 0.5;
                col.rgb = lerp(col.rgb, _SinColor.rgb, _SinIntensity * sinColorPulse * (1-vignette));
                
                // Apply vignette
                col.rgb *= vignette;
                
                // Add subtle noise
                float noise = frac(sin(dot(i.uv, float2(12.9898, 78.233)) * _Time.y) * 43758.5453);
                col.rgb += noise * 0.03;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}