Shader "Farsite/TimeOfArrival"
{
	Properties
	{
		_MaskTex ("Normalized texture to use as mask using r and a channel", 2D) = "white" {}
		_DefaultTex("Texture to display if criteria not met.", 2D) = "white" {}
		_DisplayTex("Texture to display when critera met.", 2D) = "white" {}
		_normalizedMask("Normalized mask value.", Range(0.0, 1.0)) = 0.0
	}
	SubShader
	{
		Tags {"RenderType" = "Opaque" }
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MaskTex;
			sampler2D _DefaultTex;
			sampler2D _DisplayTex;
			float _normalizedMask;
			float4 _MaskTex_ST;
			float4 _DefaultTex_ST;
			float4 _DisplayTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MaskTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				half4 col = tex2D(_MaskTex, i.uv);
				if (col.r <= _normalizedMask && col.a == 1.0)
				{
					col = tex2D(_DisplayTex, (i.uv));
				}
				else
				{
					col = tex2D(_DefaultTex, i.uv); 
					col.a = 0.3;
				}
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
