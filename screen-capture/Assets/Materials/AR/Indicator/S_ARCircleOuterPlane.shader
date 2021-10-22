Shader "WC/AR/Unlit/S_ARGrassOuterPlane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _circleRadius ("CircleRadius", Range(0,1)) = 0.5
        _Shape ("Shape", Range(0,1)) = 0.002
        _ShapeWave ("ShapeWave", Range(0,50)) = 0.002
        _circleRadiusEdge ("CircleRadiusEdge", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Assets/Libraries/Jam3Components/Shaders/MathUtils.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            uniform float _circleRadius;
            uniform float _circleRadiusEdge;

            #include "Assets/Materials/AR/Indicator/ARCirclePlane.cginc"

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 circleUv = i.uv;
                float radius = ARCirclePlaneShapeRadius(i.uv);

                fixed2 circleCenter = fixed2(radius, radius);
                circleUv -= circleCenter;
                float dist = sqrt(dot(circleUv, circleUv));

                if (dist < _circleRadius) {
                    discard;
                }

                float alpha = clamp(mapLinear(dist, _circleRadius, _circleRadiusEdge, 1.0, 0.0), 0.0, 1.0);
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * fixed4(1.0, 1.0, 1.0, alpha);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
