Shader "WUInity/BoxDispersionUnlit"
{
    Properties
    {
        _MinValue("MinValue", float) = 0.6 //5 meters at C = 3
        _MaxValue("MaxValue", float) = 1.0 // 30 meters at C = 3
        _ScaleGradient("ScaleGradient", 2D) = "green" {}
    }
    SubShader
    {
        Tags {"Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}
        LOD 100
        Lighting Off

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
            uniform float _MinValue, _MaxValue;
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
                float value = _Data[index];

                clip(value - _MinValue);

                float valueNorm = saturate((value - _MinValue) / (_MaxValue - _MinValue));
                float4 color = tex2D(_ScaleGradient, float2(valueNorm, 0.125));
                color.a = 1.0;              

                return color;
            }
            ENDCG
        }
    }
}
