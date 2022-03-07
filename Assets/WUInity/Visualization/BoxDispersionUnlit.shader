Shader "WUInity/BoxDispersionUnlit"
{
    Properties
    {
        _Transparency("Transparency", float) = 0.8
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
            uniform float _MinValue, _MaxValue, _Transparency;
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
                float value = 10440.0 * _Data[index]; // 1.2 * 8700.0 = 10440.0

                //clip exits shader here if below 0
                clip(value - _MinValue);

                float valueNorm = saturate((value - _MinValue) / (_MaxValue - _MinValue));
                float4 color = tex2D(_ScaleGradient, float2(valueNorm, 0.125));
                color.a = _Transparency;

                return color;
            }
            ENDCG
        }
    }
}
