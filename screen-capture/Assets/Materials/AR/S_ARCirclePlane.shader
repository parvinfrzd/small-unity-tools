Shader "WC/AR/Unlit/S_ARCirclePlane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _circleRadius ("CircleRadius", Range(0,1)) = 0.5
        _circleRadiusEdge ("CircleRadiusEdge", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float _circleRadius;
            uniform float _circleRadiusEdge;

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
                // https://www.geeks3d.com/20130705/shader-library-circle-disc-fake-sphere-in-glsl-opengl-glslhacker/
                fixed2 circleUv = i.uv;
                fixed2 circleCenter = fixed2(0.5, 0.5);
                circleUv -= circleCenter;
                float dist = sqrt(dot(circleUv, circleUv));

                if ((dist > (_circleRadius + _circleRadiusEdge)) || (dist < (_circleRadius - _circleRadiusEdge))) {
                    discard;
                }
                fixed4 col = tex2D(_MainTex, i.uv);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
