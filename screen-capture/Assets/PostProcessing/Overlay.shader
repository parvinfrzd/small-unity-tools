Shader "WC/PostProcessing/Overlay"
{
    HLSLINCLUDE
// StdLib.hlsl holds pre-configured vertex shaders (VertDefault), varying structs (VaryingsDefault), and most of the data you need to write common effects.
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_LogoTex, sampler_LogoTex);


        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float4 mainColor =  SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            float4 logoColor =  SAMPLE_TEXTURE2D(_LogoTex, sampler_LogoTex, i.texcoord);

            return lerp(mainColor,logoColor,logoColor.a);
        }

    ENDHLSL
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment Frag
            ENDHLSL
        }
    }
}
