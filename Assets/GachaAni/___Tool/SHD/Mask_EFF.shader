// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Mask_Eff" 
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_Mask ("Mask", 2D) = "white" {}		
		_TexScrollX("TexScrollX" , Float) = 0.0
		_TexScrollY("TexScrollY" , Float) = 0.0
        _Tiling("Tiling" , Color) = (1.0,1.0,1.0,1.0)
	}

	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Cull off
			Lighting Off
			ZWrite Off
			AlphaTest Off
			Fog { Mode Off }
			Offset -1, -1
			ColorMask RGB
			Blend SrcAlpha One
								
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
						
			sampler2D _MainTex;
			sampler2D _Mask;
			
			float4 _MainTex_ST;			
			float4 _TintColor;
			float _TexScrollX;
			float _TexScrollY;
            float4 _Tiling;

			struct appdata_t
			{
				float4 vertex : POSITION;		
				half4 color : COLOR;		
				float2 texcoord : TEXCOORD0;
				
			};

			struct v2f
			{
				float4 vertex : POSITION;			
					half4 color : COLOR;	
				float2 texcoord : TEXCOORD0;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);	
				o.color = v.color;			
				o.texcoord = v.texcoord;	
				
				return o;
			}

			half4 frag (v2f IN) : COLOR
			{
				float2 texoffset;
				texoffset.x = _TexScrollX;
				texoffset.y = _TexScrollY;
				half4 col = tex2D(_MainTex, _Tiling.xy*IN.texcoord+texoffset) * IN.color * (_TintColor*2);				
				half4 mask = tex2D(_Mask, _Tiling.zw*IN.texcoord) ;				
				
				col *= mask;

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
			ColorMask RGB
			AlphaTest Greater .01
			Blend SrcAlpha One
			ColorMaterial AmbientAndDiffuse
			
			SetTexture [_Mask] {combine texture * primary}
			SetTexture [_MainTex]
			{
				Combine Texture * previous
			}
		}
	}
}
	