//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

Shader "WUInity/Households" {

	Properties
	{
		_PaletteTex("Texture", 2D) = "white" {}
		_Scale("Scale", Range(0,30)) = 15.0
		_GroundOffset("Ground offset", Range(0,10)) = 1.0
	}

		SubShader{
			CGPROGRAM
			#pragma surface ConfigureSurface NoLighting noambient
			#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
			#pragma editor_sync_compilation
			#pragma target 4.5

			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			StructuredBuffer<float4> _PositionsAndState;
			#endif

			float _Scale, _GroundOffset;
			sampler2D _PaletteTex;

			void ConfigureProcedural() 
			{
				#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				float3 position = float3(_PositionsAndState[unity_InstanceID].x, _GroundOffset, _PositionsAndState[unity_InstanceID].y);
				unity_ObjectToWorld = 0.0;
				unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
				unity_ObjectToWorld._m00_m11_m22 = _Scale * _PositionsAndState[unity_InstanceID].z;
				#endif
			}

			struct Input 
			{
				float3 worldPos;
			};	

			fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
			{
				fixed4 c;
				c.rgb = s.Albedo;
				c.a = s.Alpha;
				return c;
			}

			void ConfigureSurface(Input input, inout SurfaceOutput surface)
			{
				#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				float2 uv = float2(_PositionsAndState[unity_InstanceID].w, 0.125);
				float4 col = tex2D(_PaletteTex, uv);
				surface.Albedo = col;
				#endif
			}
			ENDCG
	}

		FallBack "Unlit"
}