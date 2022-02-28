Shader "WUInity/Household" {

	Properties
	{
		_Scale("Scale", Range(0,30)) = 15.0
		_GroundOffset("Ground offset", Range(0,10)) = 1.0
	}

		SubShader{
			CGPROGRAM
			#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
			#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
			#pragma editor_sync_compilation
			#pragma target 4.5

			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			StructuredBuffer<float4> _PositionsAndState;
			#endif

			float _Scale, _GroundOffset;

			void ConfigureProcedural() 
			{
				#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				float3 position = _PositionsAndState[unity_InstanceID].xyz;
				position.y += _GroundOffset;
				unity_ObjectToWorld = 0.0;
				unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
				unity_ObjectToWorld._m00_m11_m22 = _Scale;
				#endif
			}

			struct Input 
			{
				float3 worldPos;
			};			

			void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) 
			{
				#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				float4 color = float4(1, 0, 0, 1);
				if (_PositionsAndState[unity_InstanceID].w >= 1.0)
				{
					color = float4(0, 1, 0, 1);
				}
				else if (_PositionsAndState[unity_InstanceID].w >= 0.5)
				{
					color = float4(0, 0, 1, 1);
				}

				surface.Albedo = color;
				#endif
			}
			ENDCG
	}

		FallBack "Diffuse"
}