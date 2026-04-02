//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

Shader "WUInity/HouseholdsURP"
{
    Properties
    {
        _PaletteTex("Texture", 2D) = "white" {}
        _Scale("Scale", Range(0,30)) = 15.0
        _GroundOffset("Ground Offset", Range(0,10)) = 1.0
        _MaxVisualHouseholdSize("Max", Range(0,100)) = 10
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            // Vertex + fragment program
            #pragma vertex Vert
            #pragma fragment Frag

            // Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:ConfigureProcedural assumeuniformscaling

            // Includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Textures
            TEXTURE2D(_PaletteTex);
            SAMPLER(sampler_PaletteTex);

            float _Scale;
            float _GroundOffset;
            uint _MaxVisualHouseholdSize;

            // Structured buffer: x = pos.x, y = pos.z, z = household size, w = palette index
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            StructuredBuffer<float4> _PositionsAndState;
            #endif

            // ---------------------------------------------------------
            // Per-instance transform (row-based). DX11-safe.
            // ---------------------------------------------------------
            UNITY_INSTANCING_BUFFER_START(PerInstance)
                UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceRow0)
                UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceRow1)
                UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceRow2)
            UNITY_INSTANCING_BUFFER_END(PerInstance)

            // ---------------------------------------------------------
            // Build per-instance transform matrix (3 rows)
            // ---------------------------------------------------------
            void ConfigureProcedural()
            {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)

                float4 data = _PositionsAndState[unity_InstanceID];
                float3 posWS = float3(data.x, _GroundOffset, data.y);
                float s = _Scale * min(data.z, _MaxVisualHouseholdSize);

                float4 row0 = float4(s, 0, 0, posWS.x);
                float4 row1 = float4(0, s, 0, posWS.y);
                float4 row2 = float4(0, 0, s, posWS.z);

                UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceRow0) = row0;
                UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceRow1) = row1;
                UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceRow2) = row2;

            #endif
            }

            // ---------------------------------------------------------
            // Vertex → Fragment structures
            // ---------------------------------------------------------
            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // ---------------------------------------------------------
            // Vertex Shader
            // ---------------------------------------------------------
            Varyings Vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float4 r0 = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceRow0);
                float4 r1 = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceRow1);
                float4 r2 = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceRow2);

                float3 local = v.positionOS.xyz;

                float3 worldPos = float3(
                    dot(local, r0.xyz) + r0.w,
                    dot(local, r1.xyz) + r1.w,
                    dot(local, r2.xyz) + r2.w
                );

                o.positionHCS = TransformWorldToHClip(worldPos);

            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                float4 data = _PositionsAndState[unity_InstanceID];
                o.uv = float2(data.w, 0.125);
            #else
                o.uv = float2(0.0, 0.125);
            #endif

                return o;
            }

            // ---------------------------------------------------------
            // Fragment Shader (sample palette here)
            // ---------------------------------------------------------
            float4 Frag(Varyings i) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_PaletteTex, sampler_PaletteTex, i.uv);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
