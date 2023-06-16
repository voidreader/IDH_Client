Shader "Unlit/Transparent Colored (Gray)"
{
    Properties
    {
        _MainTex("Base (RGB), Alpha (A)", 2D) = "black" {}
        _EffectAmount("Effect Amount", Range(0, 1)) = 0.0
        _Intensity("Intencity", float) = 1.0
    }
    SubShader
    {
        LOD 100
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Offset 1,1
        Blend SrcAlpha OneMinusSrcAlpha

    Pass
    {
        CGPROGRAM
        #pragma vertex vert 
        #pragma fragment frag 
        #include "UnityCG.cginc" 
               
        struct appdata_t
        {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
            fixed4 color : COLOR;
        };
        struct v2f
        {
            float4 vertex : SV_POSITION;
            half2 texcoord : TEXCOORD0;
            fixed4 color : COLOR;
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;
        fixed _EffectAmount;
        half _Intensity;

        v2f vert(appdata_t v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.color = v.color;
            return o;
        }

        fixed4 frag(v2f i) : COLOR    
        {
            fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
            col.rgb = lerp(col.rgb, dot(col.rgb, float3(0.3, 0.59, 0.11)), _EffectAmount) * _Intensity;
            return col;
        }
                
            ENDCG
    }

}

    SubShader
    {
        LOD 100
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
                                                         
        Pass
        {
            Cull Off
            Lighting Off
            ZWrite Off
            Fog { Mode Off }
            Offset -1, -1
            ColorMask RGB
            AlphaTest Greater .01
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMaterial AmbientAndDiffuse
                       
            SetTexture[_MainTex]
            {
                Combine Texture * Primary
            }
        }
    }
}