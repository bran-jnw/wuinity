//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

Shader "WUInity/GeneralDataDisplay"
{
    Properties
    {
        _Transparency("Transparency", float) = 0.8
        _LowerCutOff("Lower cut-off", float) = 0.0
        _MinValue("MinValue", float) = 0.6 //5 meters at C = 3
        _MaxValue("MaxValue", float) = 1.0 // 30 meters at C = 3
        _ScaleGradient("ScaleGradient", 2D) = "green" {}
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            uniform sampler2D _ScaleGradient;
            float4 _ScaleGradient_ST;
            uniform float _MinValue, _MaxValue, _Transparency, _LowerCutOff, _DataMultiplier;
            uniform int _CellsX, _CellsY;
            uniform StructuredBuffer<float> _Data;

            v2f vert(appdata_base v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _ScaleGradient);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                int xIndex = i.texcoord.x * _CellsX;
                int yIndex = i.texcoord.y * _CellsY;
                int index = xIndex + yIndex * _CellsX;
                float value = _Data[index] * _DataMultiplier;

                //clip exits shader here if below 0
                clip(value - _LowerCutOff);

                float valueNorm = saturate((value - _MinValue) / (_MaxValue - _MinValue)); //(value - _MinValue) / (_MaxValue - _MinValue);
                float4 color = tex2D(_ScaleGradient, float2(valueNorm, 0.125));
                color.a = _Transparency;

                return color;
            }
            ENDCG
        }
    }
}
